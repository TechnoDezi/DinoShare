using DinoShare.Helpers;
using DinoShare.Helpers.EmailServiceFactory;
using DinoShare.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DinoShare.ViewModels
{
    public class ForgotPasswordViewModel
    {
        internal AppDBContext _context;
        internal SecurityOptions _securityOptions;
        internal ClaimsPrincipal _user;
        internal IEmailService _emailService;

        [Required]
        [Display(Name = "Email address")]
        public string Username { get; set; }

        internal async Task<bool> SendForgotPasswordLink()
        {
            bool returnValue = false;

            UserHelperFunctions userHelper = new UserHelperFunctions();
            userHelper._context = _context;
            userHelper._emailService = _emailService;
            userHelper._securityOptions = _securityOptions;
            userHelper._user = _user;
            userHelper.Populate();

            returnValue = await userHelper.SendForgotPasswordLink(Username);

            return returnValue;
        }
    }
}
