using DinoShare.Helpers;
using DinoShare.Helpers.EmailServiceFactory;
using DinoShare.Models;
using DinoShare.Models.FolderDataModelFactory;
using Hangfire;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DinoShare.ViewModels.FolderViewModelFactory
{
    public class FolderListViewModel
    {
        internal AppDBContext _context;
        internal IEmailService _emailService;
        internal SecurityOptions _securityOptions;
        internal ClaimsPrincipal _user;

        public string SearchValue { get; set; }

        public PaginationViewModel Pagination { get; set; }

        public List<FolderListViewModelData> ItemList { get; set; }

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

            var list = (from u in _context.Folders
                        where (!string.IsNullOrEmpty(SearchValue) && (u.Description.ToLower().Contains(SearchValue)) || string.IsNullOrEmpty(SearchValue))
                        select new FolderListViewModelData
                        {
                            Description = u.Description,
                            FolderID = u.FolderID
                        });

            Pagination.TotalRecords = list.Count();
            if (!string.IsNullOrEmpty(Pagination.SortBy))
            {
                list = list.OrderByName(Pagination.SortBy, Pagination.Descending);
            }
            ItemList = list.Skip(Pagination.Skip).Take(Pagination.Top).ToList();
        }
    }

    public class FolderListViewModelData
    {
        public Guid FolderID { get; set; }
        public string Description { get; set; }
    }

    public class FolderDetailsViewModel
    {
        internal AppDBContext _context;
        internal ClaimsPrincipal _user;
        internal SecurityOptions _securityOptions;
        internal IEmailService _emailService;
        internal string errorMessage;

        public Guid FolderID { get; set; }
        public string Description { get; set; }

        internal async Task<bool> Remove()
        {
            bool returnValue = false;

            UserHelperFunctions userHelper = new UserHelperFunctions()
            {
                _context = _context,
                _securityOptions = _securityOptions,
                _user = _user
            };
            userHelper.Populate();

            var folder = _context.Folders.Where(x => x.FolderID == FolderID).FirstOrDefault();

            if (folder != null)
            {
                _context.Folders.Remove(folder);

                await _context.SaveChangesAsync();

                returnValue = true;
            }

            return returnValue;
        }

        internal async Task<Guid> Save()
        {
            UserHelperFunctions userHelper = new UserHelperFunctions()
            {
                _context = _context,
                _emailService = _emailService,
                _securityOptions = _securityOptions,
                _user = _user
            };
            userHelper.Populate();

            bool isNew = false;

            var folder = _context.Folders.Where(x => x.FolderID == FolderID).FirstOrDefault();
            if (folder == null)
            {
                folder = new Folder();
                isNew = true;
                folder.FolderID = Guid.NewGuid();
            }

            folder.Description = Description;

            if (isNew)
            {
                _context.Add(folder);

                //Add logged in user as a share user
                var folderUsr = new FolderUser()
                {
                    AllowDelete = true,
                    AllowEdit = true,
                    FolderID = folder.FolderID,
                    FolderUserID = Guid.NewGuid(),
                    UserID = userHelper.loggedInUserID
                };

                _context.Add(folderUsr);
            }
            else
            {
                _context.Update(folder);
            }

            await _context.SaveChangesAsync();
            FolderID = folder.FolderID;

            return FolderID;
        }

        internal async Task PopulateLists()
        {
            
        }

        internal async Task PopulateDetails()
        {
            if (FolderID != Guid.Empty)
            {
                var folder = _context.Folders.Where(x => x.FolderID == FolderID).FirstOrDefault();

                if (folder != null)
                {
                    Description = folder.Description;
                }
            }
            else
            {
                
            }
        }
    }

    public class FolderAccessListViewModel
    {
        internal AppDBContext _context;
        internal IEmailService _emailService;
        internal SecurityOptions _securityOptions;
        internal ClaimsPrincipal _user;

        public string SearchValue { get; set; }
        public Guid FolderID { get; internal set; }

        public PaginationViewModel Pagination { get; set; }

        public List<FolderAccessListViewModelData> ItemList { get; set; }

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

            var list = (from u in _context.FolderUsers.Include(x => x.Folder).Include(x => x.User)
                        where u.FolderID == FolderID
                        && (!string.IsNullOrEmpty(SearchValue) && (u.Folder.Description.ToLower().Contains(SearchValue) || u.User.DisplayName.ToLower().Contains(SearchValue) || u.User.EmailAddress.ToLower().Contains(SearchValue)) || string.IsNullOrEmpty(SearchValue))
                        select new FolderAccessListViewModelData
                        {
                            FolderUserID = u.FolderUserID,
                            Description = u.Folder.Description,
                            FolderID = u.FolderID,
                            AllowDelete = u.AllowDelete,
                            AllowEdit = u.AllowEdit,
                            UserDisplayName = u.User.DisplayName,
                            UserID = u.UserID
                        });

            Pagination.TotalRecords = list.Count();
            if (!string.IsNullOrEmpty(Pagination.SortBy))
            {
                list = list.OrderByName(Pagination.SortBy, Pagination.Descending);
            }
            ItemList = list.Skip(Pagination.Skip).Take(Pagination.Top).ToList();
        }
    }

    public class FolderAccessListViewModelData
    {
        public Guid FolderUserID { get; set; }
        public Guid FolderID { get; set; }
        public string Description { get; set; }
        public Guid UserID { get; set; }
        public string UserDisplayName { get; set; }
        public bool AllowEdit { get; set; }
        public bool AllowDelete { get; set; }
    }

    public class FolderAccessDetailsViewModel
    {
        internal AppDBContext _context;
        internal ClaimsPrincipal _user;
        internal SecurityOptions _securityOptions;
        internal IEmailService _emailService;
        internal string errorMessage;

        public Guid FolderUserID { get; set; }
        public Guid FolderID { get; set; }
        public string Description { get; set; }
        public string SelectedUserID { get; set; }
        public bool AllowEdit { get; set; }
        public bool AllowDelete { get; set; }

        public List<SelectListItem> UserList { get; set; }

        internal async Task<bool> Remove()
        {
            bool returnValue = false;

            UserHelperFunctions userHelper = new UserHelperFunctions()
            {
                _context = _context,
                _securityOptions = _securityOptions,
                _user = _user
            };
            userHelper.Populate();

            var folder = _context.FolderUsers.Where(x => x.FolderUserID == FolderUserID).FirstOrDefault();

            if (folder != null)
            {
                _context.FolderUsers.Remove(folder);

                await _context.SaveChangesAsync();

                returnValue = true;
            }

            return returnValue;
        }

        internal async Task PopulateDetails()
        {
            if (FolderUserID != Guid.Empty)
            {
                var folderUser = _context.FolderUsers.Include(x => x.Folder).Where(x => x.FolderUserID == FolderUserID).FirstOrDefault();

                if (folderUser != null)
                {
                    Description = folderUser.Folder.Description;
                    SelectedUserID = folderUser.UserID.ToString();
                    AllowEdit = folderUser.AllowEdit;
                    AllowDelete = folderUser.AllowDelete;
                }
            }
            else
            {
                SelectedUserID = Guid.Empty.ToString();
            }
        }

        internal async Task PopulateLists()
        {
            UserList = (from u in _context.Users
                        where u.IsRemoved == false
                        select new SelectListItem
                        {
                            Text = $"{u.DisplayName} ({u.EmailAddress})",
                            Value = u.UserID.ToString()
                        }).ToList();
        }

        internal async Task<Guid> Save()
        {
            UserHelperFunctions userHelper = new UserHelperFunctions()
            {
                _context = _context,
                _emailService = _emailService,
                _securityOptions = _securityOptions,
                _user = _user
            };
            userHelper.Populate();

            bool isNew = false;

            var folderUser = _context.FolderUsers.Where(x => x.FolderUserID == FolderUserID).FirstOrDefault();
            if (folderUser == null)
            {
                folderUser = new FolderUser();
                isNew = true;
                folderUser.FolderUserID = Guid.NewGuid();
                folderUser.FolderID = FolderID;
            }

            folderUser.UserID = Guid.Parse(SelectedUserID);
            folderUser.AllowDelete = AllowDelete;
            folderUser.AllowEdit = AllowEdit;

            if (isNew)
            {
                _context.Add(folderUser);
            }
            else
            {
                _context.Update(folderUser);
            }

            await _context.SaveChangesAsync();
            FolderUserID = folderUser.FolderUserID;

            return FolderUserID;
        }
    }

    public class FolderDirectoryListViewModel
    {
        internal AppDBContext _context;
        internal IEmailService _emailService;
        internal SecurityOptions _securityOptions;
        internal ClaimsPrincipal _user;

        public string SearchValue { get; set; }
        public Guid FolderID { get; internal set; }

        public PaginationViewModel Pagination { get; set; }

        public List<FolderDirectoryListViewModelData> ItemList { get; set; }

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

            var list = (from u in _context.FolderDirectories.Include(x => x.Folder)
                        where u.FolderID == FolderID
                        && (!string.IsNullOrEmpty(SearchValue) && (u.Folder.Description.ToLower().Contains(SearchValue) || u.FolderPath.ToLower().Contains(SearchValue)) || string.IsNullOrEmpty(SearchValue))
                        select new FolderDirectoryListViewModelData
                        {
                            FolderDirectoryID = u.FolderDirectoryID,
                            Description = u.Folder.Description,
                            FolderID = u.FolderID,
                            FolderPath = u.FolderPath,
                        });

            Pagination.TotalRecords = list.Count();
            if (!string.IsNullOrEmpty(Pagination.SortBy))
            {
                list = list.OrderByName(Pagination.SortBy, Pagination.Descending);
            }
            ItemList = list.Skip(Pagination.Skip).Take(Pagination.Top).ToList();
        }
    }

    public class FolderDirectoryListViewModelData
    {
        public Guid FolderDirectoryID { get; set; }
        public Guid FolderID { get; set; }
        public string Description { get; set; }
        public string FolderPath { get; set; }
    }

    public class FolderDirectoryDetailsViewModel
    {
        internal AppDBContext _context;
        internal ClaimsPrincipal _user;
        internal SecurityOptions _securityOptions;
        internal IEmailService _emailService;
        internal string errorMessage;

        public Guid FolderDirectoryID { get; set; }
        public Guid FolderID { get; set; }
        public string Description { get; set; }
        public string FolderPath { get; set; }
        public bool IsUploadDirectory { get; set; }

        internal async Task<bool> Remove()
        {
            bool returnValue = false;

            UserHelperFunctions userHelper = new UserHelperFunctions()
            {
                _context = _context,
                _securityOptions = _securityOptions,
                _user = _user
            };
            userHelper.Populate();

            var folder = _context.FolderDirectories.Where(x => x.FolderDirectoryID == FolderDirectoryID).FirstOrDefault();

            if (folder != null)
            {
                _context.FolderDirectories.Remove(folder);

                await _context.SaveChangesAsync();

                returnValue = true;
            }

            return returnValue;
        }

        internal async Task PopulateDetails()
        {
            if (FolderDirectoryID != Guid.Empty)
            {
                var folderDir = _context.FolderDirectories.Include(x => x.Folder).Where(x => x.FolderDirectoryID == FolderDirectoryID).FirstOrDefault();

                if (folderDir != null)
                {
                    Description = folderDir.Folder.Description;
                    FolderPath = folderDir.FolderPath;
                    IsUploadDirectory = folderDir.IsUploadDirectory;
                }
            }
            else
            {
                
            }
        }

        internal async Task PopulateLists()
        {
            
        }

        internal async Task<Guid> Save()
        {
            UserHelperFunctions userHelper = new UserHelperFunctions()
            {
                _context = _context,
                _emailService = _emailService,
                _securityOptions = _securityOptions,
                _user = _user
            };
            userHelper.Populate();

            bool isNew = false;

            if(IsUploadDirectory == true)
            {
                var directories = _context.FolderDirectories.Where(x => x.FolderID == FolderID).ToList();
                foreach(var dir in directories)
                {
                    dir.IsUploadDirectory = false;
                    _context.Update(dir);
                }

                await _context.SaveChangesAsync();
            }

            var folderDir = _context.FolderDirectories.Where(x => x.FolderDirectoryID == FolderDirectoryID).FirstOrDefault();
            if (folderDir == null)
            {
                folderDir = new FolderDirectory();
                isNew = true;
                folderDir.FolderDirectoryID = Guid.NewGuid();
                folderDir.FolderID = FolderID;
            }

            folderDir.FolderPath = FolderPath;
            folderDir.IsUploadDirectory = IsUploadDirectory;

            if (isNew)
            {
                _context.Add(folderDir);
            }
            else
            {
                _context.Update(folderDir);
            }

            await _context.SaveChangesAsync();
            FolderDirectoryID = folderDir.FolderDirectoryID;

            BackgroundJob.Enqueue<BackgroundJobHelper>(x => x.subRescanDirectories(FolderDirectoryID));

            return FolderDirectoryID;
        }
    }
}
