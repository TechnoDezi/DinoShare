using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DinoShare.Helpers;
using DinoShare.Helpers.EmailServiceFactory;
using DinoShare.Models;
using DinoShare.ViewModels;
using DinoShare.ViewModels.UsersViewModelFactory;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DinoShare.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDBContext _context;
        private readonly SecurityOptions _securityOptions;
        private readonly IEmailService _emailService;
        private readonly JwtIssuerOptions _jwtOptions;

        public AccountController(AppDBContext context, IOptions<SecurityOptions> securityOptions, IEmailService emailService, IOptions<JwtIssuerOptions> jwtOptions)
        {
            _context = context;
            _securityOptions = securityOptions.Value;
            _emailService = emailService;
            _jwtOptions = jwtOptions.Value;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Login(string ReturnUrl)
        {
            LoginViewModel model = new LoginViewModel();

            try
            {
                model.ReturnUrl = ReturnUrl;
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.AccountController.Login", ex.Message, User, ex);
            }

            ViewData.Model = model;

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var authService = new Helpers.AuthenticationService
                    {
                        _context = _context,
                        _httpContext = HttpContext,
                        _securityOptions = _securityOptions
                    };

                    var authenticationResult = await authService.SignIn(model.Username, model.Password);

                    if (authenticationResult.IsSuccess)
                    {
                        return RedirectToLocal(model.ReturnUrl);
                    }

                    ViewBag.Error = authenticationResult.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.AccountController.Login", ex.Message, User, ex);
            }

            return View(model);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Logoff()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model._context = _context;
                    model._securityOptions = _securityOptions;
                    model._user = User;
                    model._emailService = _emailService;

                    if (await model.SendForgotPasswordLink())
                    {
                        return RedirectToAction("ForgotPasswordConfirmation");
                    }
                    else
                    {
                        ViewBag.Error = "Unable to send password reset link. Please verify Email address correct. Note: Usernames without an email must be reset by an Administrator.";
                    }
                }
                catch (Exception ex)
                {
                    HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.AccountController.ForgotPassword", ex.Message, User, ex);
                }
            }

            return View(model);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string T)
        {
            ResetPasswordViewModel model = new ResetPasswordViewModel
            {
                _context = _context,
                _securityOptions = _securityOptions,
                _user = User,
                _emailService = _emailService,
                T = T
            };

            try
            {
                if (await model.ValidateToken() == false)
                {
                    ViewBag.Error = "The supplied token is not valid.";
                }
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.AccountController.ResetPassword", ex.Message, User, ex);
            }

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            model._context = _context;
            model._securityOptions = _securityOptions;
            model._user = User;
            model._emailService = _emailService;

            if (ModelState.IsValid)
            {
                try
                {
                    if (await model.ChangePassword())
                    {
                        return RedirectToAction("ResetPasswordConfirmation");
                    }
                    else
                    {
                        ViewBag.Error = "Unable to Reset Password";
                    }
                }
                catch (Exception ex)
                {
                    HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.AccountController.ResetPassword", ex.Message, User, ex);
                }
            }

            return View(model);
        }

        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Profile(bool Success = false)
        {
            UserDetailsViewModel model = new UserDetailsViewModel();

            try
            {
                Guid loggedInUserID = Guid.Parse(User.Claims.Where(x => x.Type == "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/UserID").First().Value);

                model._context = _context;
                model._emailService = _emailService;
                model._securityOptions = _securityOptions;
                model._user = User;
                model.UserID = loggedInUserID;

                await model.PopulateUserDetails();
                await model.PopulateLists();
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.AccountController.Profile", ex.Message, User, ex);
                ViewBag.Error = "An error occurred while loading profile details";
            }

            ViewData.Model = model;

            if (Success)
            {
                ViewBag.Success = "Profile updated successfully";
            }


            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Profile(UserDetailsViewModel model)
        {

            if (ModelState.IsValid)
            {
                Guid loggedInUserID = Guid.Parse(User.Claims.Where(x => x.Type == "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/UserID").First().Value);

                try
                {
                    var user = _context.Users.Where(x => x.UserID == loggedInUserID).FirstOrDefault();

                    model._context = _context;
                    model._securityOptions = _securityOptions;
                    model.UserID = loggedInUserID;
                    model._user = User;
                    //await model.UpdateUserProfile();
                    if (ModelState.IsValid)
                    {
                        if (model.UserID == await model.UpdateUserProfile())
                        {
                            ViewBag.Success = "Profile updated successfully";

                            return RedirectToAction(nameof(Profile), new { Success = true });
                        }
                        else
                        {
                            ViewBag.Error = model.errorMessage;
                        }
                    }
                }
                catch (Exception ex)
                {
                    HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.AccountController.Profile", ex.Message, User, ex);
                    ViewBag.Error = "An error occurred while updating your profile";
                }
            }
            else
            {
                ViewBag.Error = "Error Updating Profile";
            }

            return View(model);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Register(string ReturnUrl)
        {
            RegisterViewModel model = new RegisterViewModel();

            try
            {

            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.AccountController.Register", ex.Message, User, ex);
            }

            ViewData.Model = model;

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            try
            {
                model._context = _context;
                model._securityOptions = _securityOptions;
                model._emailService = _emailService;
                model._errorMessage = "";

                if (await model.Register())
                {
                    return RedirectToAction("RegisterConfirmation");
                }
                else
                {
                    ViewBag.Error = model._errorMessage;
                }
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Controllers.AccountController.Register", ex.Message, User, ex);
            }

            return View(model);
        }

        [AllowAnonymous]
        public IActionResult RegisterConfirmation()
        {
            return View();
        }
    }
}
