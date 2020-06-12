using DinoShare.Helpers;
using DinoShare.Helpers.EmailServiceFactory;
using DinoShare.Models;
using DinoShare.Models.AccountDataModelFactory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DinoShare.ViewModels
{
    public class RegisterViewModel
    {
        internal AppDBContext _context;
        internal SecurityOptions _securityOptions;
        internal IEmailService _emailService;
        internal string _errorMessage;

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Surname")]
        public string Surname { get; set; }
        [Required]
        [Display(Name = "Display Name / Nickname")]
        public string DisplayName { get; set; }
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        internal async Task<bool> Register()
        {
            bool isNew = false;

            if (string.IsNullOrEmpty(FirstName))
            {
                _errorMessage = "Please enter a Name";
                return false;
            }
            else if (string.IsNullOrEmpty(Surname))
            {
                _errorMessage = "Please enter a Surname";
                return false;
            }
            else if (string.IsNullOrEmpty(EmailAddress))
            {
                _errorMessage = "Please enter a Email";
                return false;
            }
            else if (string.IsNullOrEmpty(Password))
            {
                _errorMessage = "Please enter a Password";
                return false;
            }
            else if (Password != ConfirmPassword)
            {
                _errorMessage = "The password and Confirm Password must match";
                return false;
            }

            var user = _context.Users.FirstOrDefault(x => ((x.EmailAddress == EmailAddress && x.EmailAddress != null)));
            if (user == null)
            {
                user = new User();
                isNew = true;
                user.UserID = Guid.NewGuid();
                user.IsSuspended = false;
                user.LoginTries = 0;
                user.CreatedUserID = user.UserID;
                user.CreatedDateTime = DateTime.UtcNow;
                user.IsRemoved = false;

                user.Password = HashProvider.ComputeHash(Password, HashProvider.HashAlgorithmList.SHA256, _securityOptions.PasswordSalt);
            }
            else
            {
                _errorMessage = "The user email address already exists. Find the existing user first and edit their details";
                return false;
            }

            user.DisplayName = DisplayName;
            user.EmailAddress = EmailAddress;
            user.IsSuspended = false;
            user.LoginTries = 0;
            user.EditUserID = user.UserID;
            user.EditDateTime = DateTime.UtcNow;
            user.FirstName = FirstName;
            user.Surname = Surname;
            user.Timezone = _context.SystemConfiguration.First(x => x.EventCode == PublicEnums.SystemConfigurationList.KEY_DEFAULT_TIME_ZONE.ToString()).ConfigValue;

            if (isNew)
            {
                _context.Add(user);

                //Add default student user role
                LinkUserRole link = new LinkUserRole();
                link.LinkUserRoleID = Guid.NewGuid();
                link.UserID = user.UserID;
                link.UserRoleID = _context.UserRoles.First(x => x.EventCode == PublicEnums.UserRoleList.ROLE_USER).UserRoleID;
                link.CreatedUserID = user.UserID;
                link.EditUserID = user.UserID;
                _context.Add(link);
            }
            else
            {
                _context.Update(user);
            }

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
