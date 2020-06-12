using DinoShare.Helpers;
using DinoShare.Helpers.EmailServiceFactory;
using DinoShare.Models;
using DinoShare.ViewModels;
using DinoShare.ViewModels.EmailTemplateViewModelFactory;
using DinoShare.ViewModels.SystemConfigViewModelFactory;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DinoShare.Controllers
{
    public class MasterDataController : Controller
    {
        private readonly AppDBContext _context;
        private readonly SecurityOptions _securityOptions;
        private readonly IEmailService _emailService;

        public MasterDataController(AppDBContext context, IOptions<SecurityOptions> securityOptions, IEmailService emailService)
        {
            _context = context;
            _securityOptions = securityOptions.Value;
            _emailService = emailService;
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR)]
        public async Task<IActionResult> EmailTemplateList()
        {
            EmailTemplateListViewModel model = new EmailTemplateListViewModel();

            try
            {
                model._context = _context;
                model._emailService = _emailService;
                model._securityOptions = _securityOptions;

                await model.PopulateLists();
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.MasterDataController.EmailTemplateList", ex.Message, User, ex);
                ViewBag.Error = "An error occurred while loading data";
            }

            ViewData.Model = model;

            return View();
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR)]
        [HttpPost]
        public async Task<JsonResult> EmailTemplateList(EmailTemplateListViewModel model)
        {
            try
            {
                model._context = _context;
                model._emailService = _emailService;
                model._securityOptions = _securityOptions;

                await model.PopulateLists();

                return Json(new { result = true, data = model });
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.MasterDataController.EmailTemplateList", ex.Message, User, ex);
                ViewBag.Error = "An error occurred while loading data";
            }

            return Json(new { result = false, message = "An error occurred. Please try again later." });
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR)]
        public async Task<IActionResult> EmailTemplateDetails(Guid ID, bool Success = false)
        {
            EmailTemplateViewModel model = new EmailTemplateViewModel();

            try
            {
                model._context = _context;
                model._emailService = _emailService;
                model._securityOptions = _securityOptions;
                model.EmailTemplateID = ID;
                model._user = User;

                await model.PopulateDetails();
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.MasterDataController.EmailTemplateDetails", ex.Message, User, ex);
                ViewBag.Error = "An error occurred while loading data";
            }

            //Set message if redirected from save
            if (Success)
            {
                ViewBag.Success = "Email Template updated successfully";
            }

            ViewData.Model = model;

            return View();
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmailTemplateDetails(EmailTemplateViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model._context = _context;
                    model._emailService = _emailService;
                    model._securityOptions = _securityOptions;
                    model._user = User;

                    await model.Save();

                    return RedirectToAction("EmailTemplateDetails", new { ID = model.EmailTemplateID, Success = true });
                }
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.MasterDataController.EmailTemplateDetails", ex.Message, User, ex);
                ViewBag.Error = "An error occurred while loading data";
            }

            return View(model);
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR)]
        public async Task<IActionResult> SystemConfigList()
        {
            SystemConfigListViewModel model = new SystemConfigListViewModel();

            try
            {
                model._context = _context;
                model.SearchValue = "";

                await model.PopulateModel();
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.MasterDataController.SystemConfigList", ex.Message, User, ex);
                ViewBag.Error = "An error occurred. Please try again later.";
            }

            ViewData.Model = model;
            return View();
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR)]
        [HttpPost]
        public async Task<JsonResult> SystemConfigList(SystemConfigListViewModel model)
        {
            try
            {
                model._context = _context;
                await model.PopulateModel();

                return Json(new { result = true, data = model });
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.MasterDataController.SystemConfigList", ex.Message, User, ex);
                ViewBag.Error = "An error occurred. Please try again later.";
            }

            ViewData.Model = model;
            return Json(new { result = false, message = "An error occurred. Please try again later." });
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR)]
        public async Task<IActionResult> SystemConfigDetails(Guid ID, bool Success = false)
        {
            SystemConfigViewModel model = new SystemConfigViewModel();

            try
            {
                model._context = _context;
                model._securityOptions = _securityOptions;
                model.SystemConfigurationID = ID;
                model._user = User;
                await model.PopulateModel();
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.MasterDataController.SystemConfigDetails", ex.Message, User, ex);
                ViewBag.Error = "An error occurred. Please try again later.";
            }

            //Set message if redirected from save
            if (Success)
            {
                ViewBag.Success = "Configuration updated successfully";
            }

            ViewData.Model = model;
            return View();
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SystemConfigDetails(SystemConfigViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model._context = _context;
                    model._securityOptions = _securityOptions;
                    model._user = User;

                    Guid configID = await model.Save();
                    if (configID != Guid.Empty)
                    {
                        return RedirectToAction("SystemConfigDetails", new { ID = configID, Success = true });
                    }
                    else
                    {
                        ViewBag.Error = model.errorMessage;
                    }
                }
                catch (Exception ex)
                {
                    HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.MasterDataController.SystemConfigDetails", ex.Message, User, ex);
                    ViewBag.Error = "An error occurred. Please try again later.";
                }
            }

            return View(model);
        }
    }
}
