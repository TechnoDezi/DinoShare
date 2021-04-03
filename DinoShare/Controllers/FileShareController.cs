using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using DinoShare.Helpers;
using DinoShare.Helpers.EmailServiceFactory;
using DinoShare.Models;
using DinoShare.ViewModels;
using DinoShare.ViewModels.FileListViewModelFactory;
using DinoShare.ViewModels.FolderViewModelFactory;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DinoShare.Controllers
{
    public class FileShareController : Controller
    {
        private readonly AppDBContext _context;
        private readonly SecurityOptions _securityOptions;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _env;

        public FileShareController(AppDBContext context, IOptions<SecurityOptions> securityOptions, IEmailService emailService, IWebHostEnvironment env)
        {
            _context = context;
            _securityOptions = securityOptions.Value;
            _emailService = emailService;
            _env = env;
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR)]
        public async Task<IActionResult> FolderList(bool Removed = false)
        {
            try
            {
                FolderListViewModel model = new FolderListViewModel();
                model._context = _context;
                model._emailService = _emailService;
                model._securityOptions = _securityOptions;
                model._user = User;

                await model.PopulateList();

                ViewData.Model = model;
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.FileShareController.Index", ex.Message, User, ex);
            }

            if (Removed)
            {
                ViewBag.Success = "Folder removed successfully";
            }

            return View();
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR)]
        public async Task<JsonResult> FolderList(FolderListViewModel model)
        {
            try
            {
                model._context = _context;
                model._emailService = _emailService;
                model._securityOptions = _securityOptions;
                model._user = User;

                await model.PopulateList();

                return Json(new { result = true, data = model });
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Checklist.Controllers.FileShareController.Index", ex.Message, User, ex);
            }

            return Json(new { result = false, message = "An error occurred. Please try again later." });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR)]
        public async Task<JsonResult> RemoveFolder(Guid ID)
        {
            try
            {
                FolderDetailsViewModel model = new FolderDetailsViewModel();
                model._context = _context;
                model._user = User;
                model.FolderID = ID;
                model._securityOptions = _securityOptions;

                bool removed = await model.Remove();

                if (removed)
                {
                    return Json(new { Result = true, Message = "Folder removed successfully" });
                }
                else
                {
                    return Json(new { Result = false, Message = "Unable to remove folder" });
                }
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.FileShareController.RemoveFolder", ex.Message, User, ex);
                return Json(new { Result = false, Message = "Unable to remove folder" });
            }
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR)]
        public async Task<IActionResult> FolderDetails(Guid ID, bool Success = false)
        {
            try
            {
                FolderDetailsViewModel model = new FolderDetailsViewModel();
                model._context = _context;
                model.FolderID = ID;
                model._user = User;
                await model.PopulateDetails();
                await model.PopulateLists();

                ViewData.Model = model;
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.FileShareController.FolderDetails", ex.Message, User, ex);
                ViewBag.Error = "An error occurred. Please try again later.";
            }

            //Set message if redirected from save
            if (Success)
            {
                ViewBag.Success = "Folder updated successfully";
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR)]
        public async Task<IActionResult> FolderDetails(FolderDetailsViewModel model)
        {
            model._context = _context;
            model._securityOptions = _securityOptions;
            model._emailService = _emailService;
            model._user = User;

            try
            {
                if (ModelState.IsValid)
                {
                    Guid folderID = Guid.Empty;

                    folderID = await model.Save();
                    if (folderID != Guid.Empty)
                    {
                        return RedirectToAction("FolderDetails", new { ID = folderID, Success = true });
                    }
                    else
                    {
                        ViewBag.Error = model.errorMessage;
                    }
                }
                else
                {
                    ViewBag.Error = "Please make sure the required fields are added";
                }
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.FileShareController.FolderDetails", ex.Message, User, ex);
                ViewBag.Error = "An error occurred. Please try again later.";
            }
            finally
            {
                await model.PopulateLists();
            }
            return View(model);
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR)]
        public async Task<IActionResult> FolderAccessList(Guid ID, bool Removed = false)
        {
            try
            {
                FolderAccessListViewModel model = new FolderAccessListViewModel();
                model._context = _context;
                model._emailService = _emailService;
                model._securityOptions = _securityOptions;
                model._user = User;
                model.FolderID = ID;

                await model.PopulateList();

                ViewData.Model = model;
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.FileShareController.FolderAccessList", ex.Message, User, ex);
            }

            if (Removed)
            {
                ViewBag.Success = "Folder removed successfully";
            }

            return View();
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR)]
        public async Task<JsonResult> FolderAccessList(FolderAccessListViewModel model)
        {
            try
            {
                model._context = _context;
                model._emailService = _emailService;
                model._securityOptions = _securityOptions;
                model._user = User;

                await model.PopulateList();

                return Json(new { result = true, data = model });
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Checklist.Controllers.FileShareController.FolderAccessList", ex.Message, User, ex);
            }

            return Json(new { result = false, message = "An error occurred. Please try again later." });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR)]
        public async Task<JsonResult> RemoveFolderAccess(Guid ID)
        {
            try
            {
                FolderAccessDetailsViewModel model = new FolderAccessDetailsViewModel();
                model._context = _context;
                model._user = User;
                model.FolderUserID = ID;
                model._securityOptions = _securityOptions;

                bool removed = await model.Remove();

                if (removed)
                {
                    return Json(new { Result = true, Message = "Folder access removed successfully" });
                }
                else
                {
                    return Json(new { Result = false, Message = "Unable to remove folder access" });
                }
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.FileShareController.RemoveFolderAccess", ex.Message, User, ex);
                return Json(new { Result = false, Message = "Unable to remove folder access" });
            }
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR)]
        public async Task<IActionResult> FolderAccessDetails(Guid ID, Guid FolderID, bool Success = false)
        {
            try
            {
                FolderAccessDetailsViewModel model = new FolderAccessDetailsViewModel();
                model._context = _context;
                model.FolderID = FolderID;
                model.FolderUserID = ID;
                model._user = User;
                await model.PopulateDetails();
                await model.PopulateLists();

                ViewData.Model = model;
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.FileShareController.FolderAccessDetails", ex.Message, User, ex);
                ViewBag.Error = "An error occurred. Please try again later.";
            }

            //Set message if redirected from save
            if (Success)
            {
                ViewBag.Success = "Folder updated successfully";
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR)]
        public async Task<IActionResult> FolderAccessDetails(FolderAccessDetailsViewModel model)
        {
            model._context = _context;
            model._securityOptions = _securityOptions;
            model._emailService = _emailService;
            model._user = User;

            try
            {
                if (ModelState.IsValid)
                {
                    Guid folderUserID = Guid.Empty;

                    folderUserID = await model.Save();
                    if (folderUserID != Guid.Empty)
                    {
                        return RedirectToAction("FolderAccessDetails", new { ID = folderUserID, FolderID = model.FolderID, Success = true });
                    }
                    else
                    {
                        ViewBag.Error = model.errorMessage;
                    }
                }
                else
                {
                    ViewBag.Error = "Please make sure the required fields are added";
                }
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.FileShareController.FolderAccessDetails", ex.Message, User, ex);
                ViewBag.Error = "An error occurred. Please try again later.";
            }
            finally
            {
                await model.PopulateLists();
            }
            return View(model);
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR)]
        public async Task<IActionResult> FolderDirectoryList(Guid ID, bool Removed = false)
        {
            try
            {
                FolderDirectoryListViewModel model = new FolderDirectoryListViewModel();
                model._context = _context;
                model._emailService = _emailService;
                model._securityOptions = _securityOptions;
                model._user = User;
                model.FolderID = ID;

                await model.PopulateList();

                ViewData.Model = model;
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.FileShareController.FolderDirectoryList", ex.Message, User, ex);
            }

            if (Removed)
            {
                ViewBag.Success = "Folder Directory removed successfully";
            }

            return View();
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR)]
        public async Task<JsonResult> FolderDirectoryList(FolderDirectoryListViewModel model)
        {
            try
            {
                model._context = _context;
                model._emailService = _emailService;
                model._securityOptions = _securityOptions;
                model._user = User;

                await model.PopulateList();

                return Json(new { result = true, data = model });
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Checklist.Controllers.FileShareController.FolderDirectoryList", ex.Message, User, ex);
            }

            return Json(new { result = false, message = "An error occurred. Please try again later." });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR)]
        public async Task<JsonResult> RemoveFolderDirectory(Guid ID)
        {
            try
            {
                FolderDirectoryDetailsViewModel model = new FolderDirectoryDetailsViewModel();
                model._context = _context;
                model._user = User;
                model.FolderDirectoryID = ID;
                model._securityOptions = _securityOptions;

                bool removed = await model.Remove();

                if (removed)
                {
                    return Json(new { Result = true, Message = "Folder directory removed successfully" });
                }
                else
                {
                    return Json(new { Result = false, Message = "Unable to remove folder directory" });
                }
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.FileShareController.RemoveFolderDirectory", ex.Message, User, ex);
                return Json(new { Result = false, Message = "Unable to remove folder directory" });
            }
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR)]
        public async Task<IActionResult> FolderDirectoryDetails(Guid ID, Guid FolderID, bool Success = false)
        {
            try
            {
                FolderDirectoryDetailsViewModel model = new FolderDirectoryDetailsViewModel();
                model._context = _context;
                model.FolderID = FolderID;
                model.FolderDirectoryID = ID;
                model._user = User;
                await model.PopulateDetails();
                await model.PopulateLists();

                ViewData.Model = model;
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.FileShareController.FolderDirectoryDetails", ex.Message, User, ex);
                ViewBag.Error = "An error occurred. Please try again later.";
            }

            //Set message if redirected from save
            if (Success)
            {
                ViewBag.Success = "Folder directory updated successfully";
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR)]
        public async Task<IActionResult> FolderDirectoryDetails(FolderDirectoryDetailsViewModel model)
        {
            model._context = _context;
            model._securityOptions = _securityOptions;
            model._emailService = _emailService;
            model._user = User;

            try
            {
                if (ModelState.IsValid)
                {
                    Guid folderDirectoryID = Guid.Empty;

                    folderDirectoryID = await model.Save();
                    if (folderDirectoryID != Guid.Empty)
                    {
                        return RedirectToAction("FolderDirectoryDetails", new { ID = folderDirectoryID, FolderID = model.FolderID, Success = true });
                    }
                    else
                    {
                        ViewBag.Error = model.errorMessage;
                    }
                }
                else
                {
                    ViewBag.Error = "Please make sure the required fields are added";
                }
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.FileShareController.FolderDirectoryDetails", ex.Message, User, ex);
                ViewBag.Error = "An error occurred. Please try again later.";
            }
            finally
            {
                await model.PopulateLists();
            }
            return View(model);
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR + "," + PublicEnums.UserRoleList.ROLE_USER)]
        public async Task<IActionResult> FileList()
        {
            try
            {
                FileListViewModel model = new FileListViewModel();
                model._context = _context;
                model._emailService = _emailService;
                model._securityOptions = _securityOptions;
                model._user = User;

                await model.PopulateDetail();

                ViewData.Model = model;
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.FileShareController.FileList", ex.Message, User, ex);
            }

            return View();
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR + "," + PublicEnums.UserRoleList.ROLE_USER)]
        public async Task<JsonResult> FolderFileList(FolderFileListViewModel model)
        {
            try
            {
                model._context = _context;
                model._emailService = _emailService;
                model._securityOptions = _securityOptions;
                model._user = User;

                await model.PopulateList();

                return Json(new { result = true, data = model });
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Checklist.Controllers.FileShareController.FolderFileList", ex.Message, User, ex);
            }

            return Json(new { result = false, message = "An error occurred. Please try again later." });
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR + "," + PublicEnums.UserRoleList.ROLE_USER)]
        public async Task<IActionResult> DownloadFile(Guid ID)
        {
            UserHelperFunctions userHelper = new UserHelperFunctions()
            {
                _context = _context,
                _emailService = _emailService,
                _securityOptions = _securityOptions,
                _user = User
            };
            userHelper.Populate();

            var file = (from fdf in _context.FolderDirectoryFiles
                        join fd in _context.FolderDirectories on fdf.FolderDirectoryID equals fd.FolderDirectoryID
                        join fu in _context.FolderUsers on fd.FolderID equals fu.FolderID
                        where fdf.FolderDirectoryFileID == ID && fu.UserID == userHelper.loggedInUserID
                        select fdf).FirstOrDefault();

            if (file != null)
            {
                if (file.IsDirectory == false)
                {
                    var stream = System.IO.File.OpenRead(file.FullPath);
                    return new FileStreamResult(stream, file.FileType)
                    {
                        FileDownloadName = file.FileName
                    };
                }
                else
                {
                    var zipFileBasePath = Path.Combine(_env.ContentRootPath, "App_Data", file.FileName + "_" + Guid.NewGuid().ToString() + ".zip");

                    ZipFile.CreateFromDirectory(file.FullPath, zipFileBasePath);

                    var stream = System.IO.File.OpenRead(zipFileBasePath);
                    return new FileStreamResult(stream, "application/zip")
                    {
                        FileDownloadName = file.FileName + ".zip"
                    };
                }
            }
            else
            {
                return new EmptyResult();
            }
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR + "," + PublicEnums.UserRoleList.ROLE_USER)]
        public async Task<IActionResult> RemoveFile(Guid ID)
        {
            try
            {
                FolderFileListViewModel model = new FolderFileListViewModel();
                model._context = _context;
                model._emailService = _emailService;
                model._securityOptions = _securityOptions;
                model._user = User;

                await model.RemoveFile(ID);

                return Json(new { result = true, message = "File removed successfully" });
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Checklist.Controllers.FileShareController.RemoveFile", ex.Message, User, ex);
            }

            return Json(new { result = false, message = "An error occurred. Please try again later." });
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR + "," + PublicEnums.UserRoleList.ROLE_USER)]
        public async Task<IActionResult> AddNewFolder(string NewFolderName, Guid FolderID, Guid? ParentFolderDirectoryFileID)
        {
            try
            {
                FolderFileListViewModel model = new FolderFileListViewModel();
                model._context = _context;
                model._emailService = _emailService;
                model._securityOptions = _securityOptions;
                model._user = User;

                await model.AddNewFolder(NewFolderName, FolderID, ParentFolderDirectoryFileID);

                return Json(new { result = true, message = "Folder added successfully" });
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Checklist.Controllers.FileShareController.AddNewFolder", ex.Message, User, ex);
            }

            return Json(new { result = false, message = "An error occurred. Please try again later." });
        }
    }
}
