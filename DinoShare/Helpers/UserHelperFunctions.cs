using DinoShare.Helpers.EmailServiceFactory;
using DinoShare.Models;
using DinoShare.Models.AccountDataModelFactory;
using DinoShare.TemplateParser;
using DinoShare.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DinoShare.Helpers
{
    public class UserHelperFunctions
    {
        internal AppDBContext _context;
        internal SecurityOptions _securityOptions;
        internal IEmailService _emailService;
        internal ClaimsPrincipal _user;
        internal string _errorResult;

        internal Guid loggedInUserID;
        internal User user;

        public void Populate()
        {
            if (_user != null && _user.Identity.IsAuthenticated)
            {
                loggedInUserID = Guid.Parse(_user.Claims.Where(x => x.Type == "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/UserID").First().Value);

                user = _context.Users.First(x => x.UserID == loggedInUserID);
            }
        }

        internal async Task<bool> ChangePassword(string currentPassword, string newPassword)
        {
            bool returnValue = false;
            _errorResult = "";

            Guid loggedInUserID = Guid.Parse(_user.Claims.Where(x => x.Type == "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/UserID").First().Value);

            //Save user details
            var user = _context.Users.FirstOrDefault(x => x.UserID == loggedInUserID);
            if (user != null)
            {
                var currentPasswordHashed = HashProvider.ComputeHash(currentPassword, HashProvider.HashAlgorithmList.SHA256, _securityOptions.PasswordSalt);

                if (user.Password == currentPasswordHashed)
                {
                    user.Password = HashProvider.ComputeHash(newPassword, HashProvider.HashAlgorithmList.SHA256, _securityOptions.PasswordSalt);
                    user.EditUserID = user.UserID;
                    user.EditUserID = loggedInUserID;
                    user.EditDateTime = DateTime.UtcNow;

                    _context.Update(user);

                    await _context.SaveChangesAsync();

                    #region  Send change notification email

                    if (!string.IsNullOrEmpty(user.EmailAddress))
                    {
                        var variables = new Dictionary<string, PropertyMetaData>
                        {
                            {"HostUrl", new PropertyMetaData {Type = typeof (string), Value = _securityOptions.WebsiteHostUrl}},
                            {"DisplayName", new PropertyMetaData {Type = typeof (string), Value = user.DisplayName}},
                        };

                        await _emailService.SendEmailAsync(new List<string>() { user.EmailAddress }, "Password Changed", PublicEnums.EmailTemplateList.NTF_PASSWORD_CHANGED, variables, _user);
                    }

                    #endregion

                    returnValue = true;
                }
                else
                {
                    _errorResult = "Current password and account password does not match";
                }
            }
            else
            {
                _errorResult = "Unable to find user account";
            }

            return returnValue;
        }

        internal async Task<bool> SetNewPassword(string newPassword)
        {
            bool returnValue = false;
            _errorResult = "";

            Guid loggedInUserID = Guid.Parse(_user.Claims.Where(x => x.Type == "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/UserID").First().Value);

            //Save user details
            var user = _context.Users.FirstOrDefault(x => x.UserID == loggedInUserID);
            if (user != null)
            {
                user.Password = HashProvider.ComputeHash(newPassword, HashProvider.HashAlgorithmList.SHA256, _securityOptions.PasswordSalt);
                user.EditUserID = user.UserID;
                user.EditUserID = loggedInUserID;
                user.EditDateTime = DateTime.UtcNow;

                _context.Update(user);

                await _context.SaveChangesAsync();

                #region  Send change notification email

                if (!string.IsNullOrEmpty(user.EmailAddress))
                {
                    var variables = new Dictionary<string, PropertyMetaData>
                        {
                            {"HostUrl", new PropertyMetaData {Type = typeof (string), Value = _securityOptions.WebsiteHostUrl}},
                            {"DisplayName", new PropertyMetaData {Type = typeof (string), Value = user.DisplayName}},
                        };

                    await _emailService.SendEmailAsync(new List<string>() { user.EmailAddress }, "Password Changed", PublicEnums.EmailTemplateList.NTF_PASSWORD_CHANGED, variables, _user);
                }

                #endregion

                returnValue = true;
            }
            else
            {
                _errorResult = "Unable to find user account";
            }

            return returnValue;
        }

        internal async Task<bool> SetNewPassword(string newPassword, Guid userID)
        {
            bool returnValue = false;
            _errorResult = "";

            Guid loggedInUserID = Guid.Parse(_user.Claims.Where(x => x.Type == "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/UserID").First().Value);

            //Save user details
            var user = _context.Users.FirstOrDefault(x => x.UserID == userID);
            if (user != null)
            {
                user.Password = HashProvider.ComputeHash(newPassword, HashProvider.HashAlgorithmList.SHA256, _securityOptions.PasswordSalt);
                user.EditUserID = user.UserID;
                user.EditUserID = loggedInUserID;
                user.EditDateTime = DateTime.UtcNow;

                _context.Update(user);

                await _context.SaveChangesAsync();

                #region  Send change notification email

                if (!string.IsNullOrEmpty(user.EmailAddress))
                {
                    var variables = new Dictionary<string, PropertyMetaData>
                        {
                            {"HostUrl", new PropertyMetaData {Type = typeof (string), Value = _securityOptions.WebsiteHostUrl}},
                            {"DisplayName", new PropertyMetaData {Type = typeof (string), Value = user.DisplayName}},
                        };

                    await _emailService.SendEmailAsync(new List<string>() { user.EmailAddress }, "Password Changed", PublicEnums.EmailTemplateList.NTF_PASSWORD_CHANGED, variables, _user);
                }

                #endregion

                returnValue = true;
            }
            else
            {
                _errorResult = "Unable to find user account";
            }

            return returnValue;
        }

        internal async Task<bool> SendForgotPasswordLink(string emailAddress)
        {
            bool returnValue = false;
            _errorResult = "";

            var user = _context.Users.FirstOrDefault(x => (x.EmailAddress == emailAddress));

            if (user != null)
            {
                //Create reset link tmp code
                var temporaryTokensTypeID = _context.TemporaryTokensType.First(x => x.EventCode == PublicEnums.TemporaryTokensTypeList.TYPE_FORGOT_PASSWORD.ToString()).TemporaryTokensTypeID;

                int linkValidFor = int.Parse(_context.SystemConfiguration.Where(x => x.EventCode == PublicEnums.SystemConfigurationList.KEY_PASSEORD_RESETLINK_VALIDFOR_MIN.ToString()).First().ConfigValue);
                var expiryDate = DateTime.Now.AddMinutes(linkValidFor);

                UserTemporaryToken token = new UserTemporaryToken()
                {
                    TemporaryTokensTypeID = temporaryTokensTypeID,
                    TokenExpiryDate = expiryDate,
                    UserID = user.UserID,
                    UserTemporaryTokenID = Guid.NewGuid(),
                    CreatedDateTime = DateTime.UtcNow,
                    CreatedUserID = loggedInUserID,
                    EditDateTime = DateTime.UtcNow,
                    EditUserID = loggedInUserID
                };
                _context.UserTemporaryToken.Add(token);

                _context.SaveChanges();

                //Send link email
                string link = _securityOptions.WebsiteHostUrl + "/Account/ResetPassword?T=" + token.UserTemporaryTokenID.ToString();

                var variables = new Dictionary<string, PropertyMetaData>
                {
                    {"HostUrl", new PropertyMetaData {Type = typeof (string), Value = _securityOptions.WebsiteHostUrl}},
                    {"DisplayName", new PropertyMetaData {Type = typeof (string), Value = user.DisplayName}},
                    {"Link", new PropertyMetaData {Type = typeof (string), Value = link}}
                };

                await _emailService.SendEmailAsync(new List<string>() { user.EmailAddress }, "Password Reset", PublicEnums.EmailTemplateList.NTF_PASSWORD_RESET_LINK, variables, _user);

                returnValue = true;
            }
            else
            {
                _errorResult = "Unable to find a user account for " + emailAddress;
            }

            return returnValue;
        }
    }
}
