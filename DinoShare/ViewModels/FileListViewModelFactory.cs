using DinoShare.Helpers;
using DinoShare.Helpers.EmailServiceFactory;
using DinoShare.Models;
using DinoShare.Models.FolderDataModelFactory;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DinoShare.ViewModels.FileListViewModelFactory
{
    public class FileListViewModel
    {
        internal AppDBContext _context;
        internal IEmailService _emailService;
        internal SecurityOptions _securityOptions;
        internal ClaimsPrincipal _user;

        public List<FileListViewModelData> FolderList { get; set; }

        internal async Task PopulateDetail()
        {
            UserHelperFunctions userHelper = new UserHelperFunctions()
            {
                _context = _context,
                _emailService = _emailService,
                _securityOptions = _securityOptions,
                _user = _user
            };
            userHelper.Populate();

            FolderList = LoadFolderList(userHelper, null);
        }

        private List<FileListViewModelData> LoadFolderList(UserHelperFunctions userHelper, Guid? parentFolderID = null)
        {
            return (from f in _context.Folders
                    join fu in _context.FolderUsers on f.FolderID equals fu.FolderID
                    where fu.UserID == userHelper.loggedInUserID && f.ParentFolderID == parentFolderID
                    orderby f.Description
                    select new { f, fu }).ToList().Select(x => new FileListViewModelData
                    {
                        Description = x.f.Description,
                        AllowDelete = x.fu.AllowDelete,
                        AllowEdit = x.fu.AllowEdit,
                        FolderID = x.f.FolderID,
                        FolderList = LoadFolderList(userHelper, x.f.FolderID)
                    }).ToList();
        }
    }

    public class FileListViewModelData
    {
        public Guid FolderID { get; set; }
        public string Description { get; set; }
        public bool AllowEdit { get; set; }
        public bool AllowDelete { get; set; }
        public bool IsActive { get; set; }

        public List<FileListViewModelData> FolderList { get; set; }
    }

    public class FolderFileListViewModel
    {
        internal AppDBContext _context;
        internal IEmailService _emailService;
        internal SecurityOptions _securityOptions;
        internal ClaimsPrincipal _user;

        public string SearchValue { get; set; }
        public Guid FolderID { get; set; }
        public Guid? ParentFolderDirectoryFileID { get; set; }

        public PaginationViewModel Pagination { get; set; }

        public List<FolderFileListViewModelData> ItemList { get; set; }

        internal async Task PopulateList()
        {
            UserHelperFunctions userHelper = new UserHelperFunctions()
            {
                _context = _context,
                _emailService = _emailService,
                _securityOptions = _securityOptions,
                _user = _user
            };
            userHelper.Populate();

            if (Pagination == null)
            {
                Pagination = new PaginationViewModel();
                Pagination.Top = 10;
            }

            var list = (from u in _context.FolderDirectories
                        join fu in _context.FolderUsers on u.FolderID equals fu.FolderID
                        join fd in _context.FolderDirectoryFiles on u.FolderDirectoryID equals fd.FolderDirectoryID
                        where u.FolderID == FolderID && fu.UserID == userHelper.loggedInUserID
                        && fd.ParentFolderDirectoryFileID == ParentFolderDirectoryFileID
                        && (!string.IsNullOrEmpty(SearchValue) && (fd.FileName.ToLower().Contains(SearchValue)) || string.IsNullOrEmpty(SearchValue))
                        orderby fd.IsDirectory descending, fd.FileName
                        select new FolderFileListViewModelData
                        {
                            FileName = fd.FileName,
                            CreatedDate = fd.CreatedDate.ToString("yyyy/MM/dd HH:mm"),
                            FolderDirectoryFileID = fd.FolderDirectoryFileID,
                            SizeMB = fd.SizeMB,
                            IsUploadDirectory = u.IsUploadDirectory,
                            IsDirectory = fd.IsDirectory
                        });

            Pagination.TotalRecords = list.Count();
            if (!string.IsNullOrEmpty(Pagination.SortBy))
            {
                list = list.OrderByName(Pagination.SortBy, Pagination.Descending);
            }
            ItemList = list.Skip(Pagination.Skip).Take(Pagination.Top).ToList();
        }

        internal async Task RemoveFile(Guid iD)
        {
            UserHelperFunctions userHelper = new UserHelperFunctions()
            {
                _context = _context,
                _emailService = _emailService,
                _securityOptions = _securityOptions,
                _user = _user
            };
            userHelper.Populate();

            var file = (from u in _context.FolderDirectories
                        join fu in _context.FolderUsers on u.FolderID equals fu.FolderID
                        join fd in _context.FolderDirectoryFiles on u.FolderDirectoryID equals fd.FolderDirectoryID
                        where fd.FolderDirectoryFileID == iD && fu.UserID == userHelper.loggedInUserID &&
                        fu.AllowDelete == true
                        select fd).FirstOrDefault();

            if(file != null)
            {
                if (file.IsDirectory == false)
                {
                    if (System.IO.File.Exists(file.FullPath))
                    {
                        System.IO.File.Delete(file.FullPath);
                    }
                }
                else
                {
                    if (System.IO.Directory.Exists(file.FullPath))
                    {
                        System.IO.Directory.Delete(file.FullPath, true);
                    }
                }

                _context.Remove(file);

                await _context.SaveChangesAsync();
            }
        }

        internal async Task AddNewFolder(string newFolderName, Guid folderID, Guid? parentFolderDirectoryFileID)
        {
            UserHelperFunctions userHelper = new UserHelperFunctions()
            {
                _context = _context,
                _emailService = _emailService,
                _securityOptions = _securityOptions,
                _user = _user
            };
            userHelper.Populate();

            var folder = _context.FolderUsers.FirstOrDefault(x => x.FolderID == folderID);
            if(folder != null)
            {
                var parentFolder = _context.FolderDirectoryFiles.Include(x => x.FolderDirectory).FirstOrDefault(x => x.FolderDirectoryFileID == parentFolderDirectoryFileID);
                if (parentFolder != null)
                {
                    var dirFile = new FolderDirectoryFile()
                    {
                        FileExtention = "",
                        FileName = newFolderName,
                        FolderDirectoryFileID = Guid.NewGuid(),
                        FolderDirectoryID = parentFolder.FolderDirectoryID,
                        FullPath = Path.Combine(parentFolder.FullPath, newFolderName),
                        CreatedDate = DateTime.UtcNow,
                        IsDirectory = true,
                        ParentFolderDirectoryFileID = parentFolderDirectoryFileID
                    };

                    _context.Add(dirFile);

                    await _context.SaveChangesAsync();

                    if(Directory.Exists(Path.Combine(parentFolder.FullPath, newFolderName)) == false)
                    {
                        Directory.CreateDirectory(Path.Combine(parentFolder.FullPath, newFolderName));
                    }
                }
            }
        }
    }

    public class FolderFileListViewModelData
    {
        public Guid FolderDirectoryFileID { get; set; }
        public string FileName { get; set; }
        public string SizeMB { get; set; }
        public string CreatedDate { get; set; }
        public bool IsUploadDirectory { get; set; }
        public bool IsDirectory { get; set; }
    }
}
