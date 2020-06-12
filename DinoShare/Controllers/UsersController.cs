using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DinoShare.Helpers;
using DinoShare.Helpers.EmailServiceFactory;
using DinoShare.Models;
using DinoShare.ViewModels;
using DinoShare.ViewModels.UsersViewModelFactory;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DinoShare.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR)]
    public class UsersController : Controller
    {
        private readonly AppDBContext _context;
        private readonly SecurityOptions _securityOptions;
        private readonly IEmailService _emailService;

        public UsersController(AppDBContext context, IOptions<SecurityOptions> securityOptions, IEmailService emailService)
        {
            _context = context;
            _securityOptions = securityOptions.Value;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index(bool Removed = false)
        {
            try
            {
                UsersListViewModel model = new UsersListViewModel();
                model._context = _context;
                model._emailService = _emailService;
                model._securityOptions = _securityOptions;
                model._user = User;

                await model.PopulateList();

                ViewData.Model = model;
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.UsersController.Index", ex.Message, User, ex);
            }

            if (Removed)
            {
                ViewBag.Success = "User removed successfully";
            }

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> Index(UsersListViewModel model)
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
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Checklist.Controllers.UsersController.Index", ex.Message, User, ex);
            }

            return Json(new { result = false, message = "An error occurred. Please try again later." });
        }

        [HttpPost]
        public async Task<JsonResult> RemoveUser(Guid ID)
        {
            try
            {
                UserDetailsViewModel model = new UserDetailsViewModel();
                model._context = _context;
                model._user = User;
                model.UserID = ID;
                bool removed = await model.RemoveUser();

                if (removed)
                {
                    return Json(new { Result = true, Message = "User removed successfully" });
                }
                else
                {
                    return Json(new { Result = false, Message = "Unable to remove user" });
                }
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.UsersController.RemoveUser", ex.Message, User, ex);
                return Json(new { Result = false, Message = "Unable to remove user" });
            }
        }

        public async Task<IActionResult> Details(Guid ID, bool Success = false)
        {
            try
            {
                UserDetailsViewModel model = new UserDetailsViewModel();
                model._context = _context;
                model.UserID = ID;
                model._user = User;
                await model.PopulateUserDetails();
                await model.PopulateLists();

                ViewData.Model = model;
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.UsersController.Details", ex.Message, User, ex);
                ViewBag.Error = "An error occurred. Please try again later.";
            }

            //Set message if redirected from save
            if (Success)
            {
                ViewBag.Success = "User account updated successfully";
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(UserDetailsViewModel model)
        {
            model._context = _context;
            model._securityOptions = _securityOptions;
            model._emailService = _emailService;
            model._user = User;

            try
            {
                if (ModelState.IsValid)
                {
                    if (model.UserRoles.Any(x => x.Selected == true) == false)
                    {
                        ViewBag.Error = "At least one user role must be specified";
                    }
                    else if (string.IsNullOrEmpty(model.EmailAddress))
                    {
                        ViewBag.Error = "Email address must be specified";
                    }
                    else
                    {
                        Guid userID = Guid.Empty;

                        userID = await model.Save();
                        if (userID != Guid.Empty)
                        {
                            if (!string.IsNullOrEmpty(model._tmpPassword))
                            {
                                TempData["tmpPassword"] = model._tmpPassword;
                            }
                            else
                            {
                                TempData["tmpPassword"] = null;
                            }
                            return RedirectToAction("Details", new { ID = userID, Success = true });
                        }
                        else
                        {
                            ViewBag.Error = model.errorMessage;
                        }
                    }
                }
                else
                {
                    ViewBag.Error = "Please make sure the required fields are added including the login type.";
                }
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.UsersController.Details", ex.Message, User, ex);
                ViewBag.Error = "An error occurred. Please try again later.";
            }
            finally
            {
                await model.PopulateLists();
            }
            return View(model);
        }
    }
}
