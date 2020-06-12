using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using DinoShare.Models;
using DinoShare.Models.AccountDataModelFactory;
using DinoShare.ViewModels;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DinoShare.Helpers
{
    public class AuthenticationService
    {
        internal AppDBContext _context;
        internal SecurityOptions _securityOptions;
        internal HttpContext _httpContext;

        public class AuthenticationResult
        {
            public AuthenticationResult(string errorMessage = null)
            {
                ErrorMessage = errorMessage;
            }

            public String ErrorMessage { get; private set; }
            public Boolean IsSuccess => String.IsNullOrEmpty(ErrorMessage);
        }

        public ClaimsPrincipal SignedInIdentity { get; set; }

        public AuthenticationService()
        {

        }

        public async Task<AuthenticationResult> SignIn(String emailAddressUsername, String password, bool createSignInCookie = true)
        {
            int retryLimit = int.Parse(_context.SystemConfiguration.Where(x => x.EventCode == PublicEnums.SystemConfigurationList.KEY_LOGIN_RETRYLIMIT.ToString()).First().ConfigValue);

            string hashedPassword = HashProvider.ComputeHash(password, HashProvider.HashAlgorithmList.SHA256, _securityOptions.PasswordSalt);

            var user = _context.Users.FirstOrDefault(x => x.IsRemoved == false
            && (x.EmailAddress == emailAddressUsername && x.EmailAddress != null)
            && x.Password == hashedPassword);

            if (user != null)
            {
                if (user.IsSuspended)
                {
                    return new AuthenticationResult("Your account is suspended. Please reset your password or ask your administrator to un-suspend your account.");
                }
                else
                {
                    var identity = CreateIdentity(user);

                    if (user.LoginTries != 0 || user.IsSuspended == true)
                    {
                        user.LoginTries = 0;
                        user.IsSuspended = false;
                        user.EditDateTime = DateTime.UtcNow;
                        user.EditUserID = user.UserID;

                        _context.Update(user);

                        _context.SaveChanges();
                    }

                    if (createSignInCookie)
                    {
                        await _httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                        await _httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, identity, new Microsoft.AspNetCore.Authentication.AuthenticationProperties()
                        {
                            IsPersistent = true
                        });
                    }
                }
            }
            else
            {
                var userObj = _context.Users.FirstOrDefault(x => x.IsRemoved == false
                && (x.EmailAddress == emailAddressUsername && x.EmailAddress != null));

                //Check user onboarded by admin
                if (userObj == null)
                {
                    return new AuthenticationResult("Your email address have not yet been registered as a user on this system.");
                }
                else if (userObj.IsSuspended)
                {
                    return new AuthenticationResult("Your account is suspended. Please reset your password or ask your administrator to un-suspend your account.");
                }
                else
                {
                    if (userObj.LoginTries + 1 >= retryLimit)
                    {
                        userObj.LoginTries = 0;
                        userObj.IsSuspended = true;

                        userObj.EditUserID = userObj.UserID;
                        userObj.EditDateTime = DateTime.UtcNow;
                        _context.Update(userObj);

                        _context.SaveChanges();

                        return new AuthenticationResult("Email or Password is not correct. Your account have been suspended. Please reset your password or contact your administrator.");
                    }
                    else
                    {
                        userObj.LoginTries++;

                        userObj.EditUserID = userObj.UserID;
                        userObj.EditDateTime = DateTime.UtcNow;
                        _context.Update(userObj);

                        _context.SaveChanges();

                        return new AuthenticationResult($"Email or Password is not correct. Login tries {userObj.LoginTries} / {retryLimit}");
                    }
                }
            }

            return new AuthenticationResult();
        }

        internal ClaimsPrincipal CreateIdentity(User user)
        {
            ClaimsPrincipal principal = new ClaimsPrincipal();
            var identity = new ClaimsIdentity("MunchAuthenticationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            identity.AddClaim(new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/UserID", user.UserID.ToString()));

            //Get user roles
            var roles = _context.LinkUserRole.Where(x => x.UserID == user.UserID).ToList();
            foreach (var role in roles)
            {
                var userRole = _context.UserRoles.First(x => x.UserRoleID == role.UserRoleID);

                identity.AddClaim(new Claim(ClaimTypes.Role, userRole.EventCode));
            }

            identity.AddClaim(new Claim(ClaimTypes.Name, user.DisplayName ?? ""));
            identity.AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName ?? ""));
            identity.AddClaim(new Claim(ClaimTypes.Surname, user.Surname ?? ""));
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.EmailAddress));
            identity.AddClaim(new Claim("Timezone", user.Timezone ?? ""));

            if (!String.IsNullOrEmpty(user.EmailAddress))
            {
                identity.AddClaim(new Claim(ClaimTypes.Email, user.EmailAddress));
            }

            // add your own claims if you need to add more information stored on the cookie
            //Look up roles
            principal.AddIdentity(identity);

            SignedInIdentity = principal;
            return principal;
        }
    }
}
