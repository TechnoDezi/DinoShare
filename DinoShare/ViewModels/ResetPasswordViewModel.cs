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
    public class ResetPasswordViewModel
    {
        internal AppDBContext _context;
        internal SecurityOptions _securityOptions;
        internal ClaimsPrincipal _user;
        internal IEmailService _emailService;

        public string T { get; set; }
        [Display(Name = "Password")]
        [Required]
        public string Password { get; set; }
        [Display(Name = "Confirm Password")]
        [Required]
        public string ConfirmPassword { get; set; }

        internal async Task<bool> ValidateToken()
        {
            bool returnValue = false;

            if (!string.IsNullOrEmpty(T))
            {
                if (Guid.TryParse(T, out Guid tResult))
                {
                    var userToken = _context.UserTemporaryToken.Where(x => x.UserTemporaryTokenID == tResult).FirstOrDefault();
                    if (userToken != null)
                    {
                        if (userToken.TokenExpiryDate >= DateTime.Now)
                        {
                            returnValue = true;
                        }
                    }
                }
            }

            return returnValue;
        }

        internal async Task<bool> ChangePassword()
        {
            UserHelperFunctions userHelper = new UserHelperFunctions()
            {
                _context = _context,
                _securityOptions = _securityOptions,
                _user = _user
            };
            userHelper.Populate();

            bool returnValue = false;

            if (Guid.TryParse(T, out Guid tResult))
            {
                var userToken = _context.UserTemporaryToken.Where(x => x.UserTemporaryTokenID == tResult).FirstOrDefault();
                if (userToken != null)
                {
                    var user = _context.Users.Where(x => x.UserID == userToken.UserID).FirstOrDefault();
                    if (user != null)
                    {
                        var hashedPassword = HashProvider.ComputeHash(Password, HashProvider.HashAlgorithmList.SHA256, _securityOptions.PasswordSalt);

                        user.Password = hashedPassword;
                        user.IsSuspended = false;
                        user.LoginTries = 0;
                        user.EditUserID = user.UserID;
                        user.EditDateTime = DateTime.UtcNow;
                        user.EditUserID = userHelper.loggedInUserID;
                        _context.Update(user);

                        await _context.SaveChangesAsync();

                        returnValue = true;
                    }
                }
            }

            return returnValue;
        }
    }
}
