using Microsoft.Extensions.Options;
using DinoShare.Helpers.EmailServiceFactory;
using DinoShare.Models;
using DinoShare.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using DinoShare.Models.FolderDataModelFactory;
using Microsoft.AspNetCore.StaticFiles;
using Hangfire;
using System.Web.Http;
using DinoShare.ViewModels.DropzoneModelFactory;
using Microsoft.AspNetCore.Hosting;
using System.Web;
using System.Net.Http;

namespace DinoShare.Helpers
{
    public class BackgroundJobHelper
    {
        internal AppDBContext _context;
        internal SecurityOptions _securityOptions;
        internal IEmailService _emailService;
        private IWebHostEnvironment _env;

        public BackgroundJobHelper(IWebHostEnvironment env, AppDBContext context, IOptions<SecurityOptions> securityOptions, IEmailService emailService)
        {
            _env = env;
            _context = context;
            _securityOptions = securityOptions.Value;
            _emailService = emailService;
        }

        public async Task CleanApplicationLog()
        {
            int cleanDays = int.Parse(_context.SystemConfiguration.First(x => x.EventCode == PublicEnums.SystemConfigurationList.KEY_CLEAN_APP_LOG_DAYS.ToString()).ConfigValue);

            var logs = _context.ApplicationLog.Where(x => x.LogDate < DateTime.Now.AddDays(cleanDays * -1)).ToList();

            if (logs != null && logs.Count() > 0)
            {
                foreach (var log in logs)
                {
                    _context.ApplicationLog.Remove(log);
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task RescanDirectories()
        {
            var folderDirectories = _context.FolderDirectories.ToList();
            foreach (var directory in folderDirectories)
            {
                await subRescanDirectories(directory.FolderDirectoryID);
            }
        }

        public async Task subRescanDirectories(Guid folderDirectoryID)
        {
            if (folderDirectoryID != null)
            {
                var folderDir = _context.FolderDirectories.Where(x => x.FolderDirectoryID == folderDirectoryID).FirstOrDefault();
                if (folderDir != null)
                {
                    var files = _context.FolderDirectoryFiles.Where(x => x.FolderDirectoryID == folderDirectoryID).ToList();
                    foreach (var file in files)
                    {
                        _context.Remove(file);
                    }

                    var fileScan = Directory.GetFiles(folderDir.FolderPath);
                    foreach (var file in fileScan)
                    {
                        FileInfo info = new FileInfo(file);

                        var dirFile = new FolderDirectoryFile()
                        {
                            FileExtention = info.Extension,
                            FileName = info.Name,
                            FolderDirectoryFileID = Guid.NewGuid(),
                            FolderDirectoryID = folderDir.FolderDirectoryID,
                            FullPath = info.FullName,
                            SizeMB = (Convert.ToDouble(info.Length) / 1024.00 / 1024.00).ToString("0.00"),
                            CreatedDate = info.CreationTime
                        };

                        var provider = new FileExtensionContentTypeProvider();
                        string contentType;
                        if (!provider.TryGetContentType(info.FullName, out contentType))
                        {
                            contentType = "application/octet-stream";
                        }

                        dirFile.FileType = contentType;

                        _context.Add(dirFile);
                    }
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteFolder(string path)
        {
            //Sleep a bit to give time for resources to release
            await Task.Delay(20000);

            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        public async Task CommitFileChunks(DzCommit model)
        {
            string path = "";

            try
            {
                var chunkDirBasePath = Path.Combine(_env.ContentRootPath, "App_Data");
                path = string.Format(@"{0}\{1}", chunkDirBasePath, model.dzIdentifier);
                var dest = Path.Combine(path, HttpUtility.UrlDecode(model.fileName));
                FileInfo info = null;

                // Get all files in directory and combine in filestream
                var files = Directory.EnumerateFiles(path).Where(s => !s.Equals(dest)).OrderBy(s => s);

                // Check that the number of chunks is as expected
                if (files.Count() != model.totalChunks)
                {
                    throw new Exception(string.Format("Total number of chunks: {0}. Expected: {1}!", files.Count(), model.totalChunks));
                }

                // Merge chunks into one file
                using (var fStream = new FileStream(dest, FileMode.Create))
                {
                    foreach (var file in files)
                    {
                        using (var sourceStream = System.IO.File.OpenRead(file))
                        {
                            sourceStream.CopyTo(fStream);
                        }
                    }
                    fStream.Flush();
                }

                // Check that merged file length is as expected.
                info = new FileInfo(dest);
                if (info != null)
                {
                    if (info.Length == model.expectedBytes)
                    {
                        if (model.folderID != Guid.Empty)
                        {
                            var uploadDirectory = _context.FolderDirectories.FirstOrDefault(x => x.FolderID == model.folderID && x.IsUploadDirectory == true);
                            if (uploadDirectory != null)
                            {
                                var finaldest = Path.Combine(uploadDirectory.FolderPath, HttpUtility.UrlDecode(model.fileName));

                                if (!Directory.Exists(uploadDirectory.FolderPath))
                                {
                                    Directory.CreateDirectory(uploadDirectory.FolderPath);
                                }

                                System.IO.File.Move(dest, finaldest);

                                BackgroundJob.Enqueue<BackgroundJobHelper>(x => x.DeleteFolder(path));
                                BackgroundJob.Enqueue<BackgroundJobHelper>(x => x.subRescanDirectories(uploadDirectory.FolderDirectoryID));
                            }
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("Total file size: {0}. Expected: {1}!", info.Length, model.expectedBytes));
                    }
                }
                else
                {
                    throw new Exception("Chunks failed to merge and file not saved!");
                }
            }
            catch (HttpResponseException ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.ApiCourseManagementController.CommitChunks", ex.Message, null, ex);
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.ApiCourseManagementController.CommitChunks", ex.Message, null, ex);
            }
            finally
            {
                BackgroundJob.Enqueue<BackgroundJobHelper>(x => x.DeleteFolder(path));
            }
        }
    }
}
