using Microsoft.AspNetCore.Mvc.Rendering;
using DinoShare.Helpers;
using DinoShare.Helpers.EmailServiceFactory;
using DinoShare.Models;
using DinoShare.Models.AccountDataModelFactory;
using DinoShare.TemplateParser;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DinoShare.ViewModels.UsersViewModelFactory
{
    public class UsersListViewModel
    {
        internal AppDBContext _context;
        internal SecurityOptions _securityOptions;
        internal IEmailService _emailService;
        internal ClaimsPrincipal _user;
        internal string errorMessage = "";

        public string SearchValue { get; set; }

        public PaginationViewModel Pagination { get; set; }

        public List<UsersListViewModelData> UserList { get; set; }

        internal async Task PopulateList()
        {
            UserHelperFunctions userHelper = new UserHelperFunctions()
            {
                _context = _context,
                _emailService = _emailService,
                _securityOptions = _securityOptions,
                _user = _user
            };
            userHelper.Populate();

            if (Pagination == null)
            {
                Pagination = new PaginationViewModel();
                Pagination.Top = 10;
            }

            var list = (from u in _context.Users
                        where u.IsRemoved == false && (!string.IsNullOrEmpty(SearchValue) && (u.DisplayName.Contains(SearchValue) || u.EmailAddress.Contains(SearchValue)) || string.IsNullOrEmpty(SearchValue))
                        select new UsersListViewModelData
                        {
                            AllowRemove = (userHelper.loggedInUserID == u.UserID) ? false : true,
                            DisplayName = u.DisplayName,
                            EmailAddress = u.EmailAddress,
                            IsSuspended = u.IsSuspended,
                            LoginTries = u.LoginTries,
                            UserID = u.UserID
                        });

            Pagination.TotalRecords = list.Count();
            if (!string.IsNullOrEmpty(Pagination.SortBy))
            {
                list = list.OrderByName(Pagination.SortBy, Pagination.Descending);
            }
            UserList = list.Skip(Pagination.Skip).Take(Pagination.Top).ToList();
        }
    }

    public class UsersListViewModelData
    {
        public Guid UserID { get; set; }
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }
        public int LoginTries { get; set; }
        public bool IsSuspended { get; set; }
        public bool AllowRemove { get; set; }
    }

    public class UserDetailsViewModel
    {
        internal AppDBContext _context;
        internal SecurityOptions _securityOptions;
        internal IEmailService _emailService;
        internal ClaimsPrincipal _user;
        internal string errorMessage = "";
        internal string _tmpPassword;

        #region Properties

        public Guid UserID { get; set; }
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
        [Display(Name = "Account Suspended")]
        public bool IsSuspended { get; set; }
        [Display(Name = "Timezone")]
        public string SelectedTimezone { get; set; }

        public List<UserDetailsViewModelRoles> UserRoles { get; set; }
        public List<SelectListItem> Timezones { get; set; }

        #endregion

        public async Task PopulateLists()
        {
            UserHelperFunctions userHelper = new UserHelperFunctions()
            {
                _context = _context,
                _emailService = _emailService,
                _securityOptions = _securityOptions,
                _user = _user
            };
            userHelper.Populate();

            //Populate user roles
            var allRoles = _context.UserRoles.ToList();
            UserRoles = (from r in allRoles
                         orderby r.Description
                         select new UserDetailsViewModelRoles
                         {
                             Description = r.Description,
                             EventCode = r.EventCode,
                             UserRoleID = r.UserRoleID,
                             Selected = CheckUserHasRole(r.UserRoleID)
                         }).ToList();

            //Populate timezone List
            Timezones = (from t in TimeZoneInfo.GetSystemTimeZones()
                         select new SelectListItem
                         {
                             Value = t.Id,
                             Text = t.DisplayName
                         }).ToList();
        }

        private bool CheckUserHasRole(Guid UserRoleID)
        {
            return _context.LinkUserRole.Any(l => l.UserRoleID == UserRoleID && l.UserID == UserID);
        }

        public async Task<bool> RemoveUser()
        {
            bool returnValue = false;

            var user = _context.Users.Where(x => x.UserID == UserID).FirstOrDefault();
            Guid loggedInUserID = Guid.Parse(_user.Claims.Where(x => x.Type == "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/UserID").First().Value);

            if (user != null)
            {
                //Remove user
                user.EditUserID = loggedInUserID;
                user.EditDateTime = DateTime.UtcNow;
                user.IsRemoved = true;
                _context.Users.Update(user);

                await _context.SaveChangesAsync();

                returnValue = true;
            }

            return returnValue;
        }

        private async Task ClearUserRoles(Guid userID)
        {
            Guid loggedInUserID = Guid.Parse(_user.Claims.Where(x => x.Type == "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/UserID").First().Value);
            var userRoles = _context.LinkUserRole.Where(x => x.UserID == userID);
            if (userRoles != null && userRoles.Count() > 0)
            {
                foreach (var linkRole in userRoles)
                {
                    linkRole.EditUserID = loggedInUserID;
                    _context.LinkUserRole.Remove(linkRole);
                }
            }
        }

        private async Task AddSelectedUserRoles(Guid userID)
        {
            Guid loggedInUserID = Guid.Parse(_user.Claims.Where(x => x.Type == "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/UserID").First().Value);

            foreach (var selectedRole in UserRoles.Where(x => x.Selected == true))
            {
                LinkUserRole link = new LinkUserRole();
                link.LinkUserRoleID = Guid.NewGuid();
                link.UserID = userID;
                link.UserRoleID = selectedRole.UserRoleID;
                link.CreatedUserID = loggedInUserID;
                link.EditUserID = loggedInUserID;
                _context.Add(link);
            }
        }

        public async Task PopulateUserDetails()
        {
            if (UserID != Guid.Empty)
            {
                var user = _context.Users.Where(x => x.UserID == UserID).FirstOrDefault();

                if (user != null)
                {
                    DisplayName = user.DisplayName;
                    EmailAddress = user.EmailAddress;
                    IsSuspended = user.IsSuspended;
                    FirstName = user.FirstName;
                    Surname = user.Surname;
                    SelectedTimezone = user.Timezone;
                }
            }
            else
            {
                SelectedTimezone = _context.SystemConfiguration.First(x => x.EventCode == PublicEnums.SystemConfigurationList.KEY_DEFAULT_TIME_ZONE.ToString()).ConfigValue;
            }
        }

        public async Task<Guid> Save()
        {
            UserHelperFunctions userHelper = new UserHelperFunctions()
            {
                _context = _context,
                _emailService = _emailService,
                _securityOptions = _securityOptions,
                _user = _user
            };
            userHelper.Populate();

            bool isNew = false;
            bool isWasRemoved = false;
            string tempPassword = "";
            PublicEnums.EmailTemplateList emailTemplate = PublicEnums.EmailTemplateList.NTF_REGISTRATION_WELCOME_CUSTOM;

            //Save user details
            var user = _context.Users.FirstOrDefault(x => x.UserID == UserID);
            if (user == null)
            {
                //Check user not deleted before
                user = _context.Users.FirstOrDefault(x => ((x.EmailAddress == EmailAddress && x.EmailAddress != null)) && x.IsRemoved == true);
                if (user == null)
                {
                    //Perform dup-check
                    user = _context.Users.FirstOrDefault(x => ((x.EmailAddress == EmailAddress && x.EmailAddress != null)) && x.IsRemoved == false);
                    if (user == null)
                    {
                        user = new User();
                        isNew = true;
                        user.UserID = Guid.NewGuid();
                        user.IsSuspended = false;
                        user.LoginTries = 0;
                        user.CreatedUserID = userHelper.loggedInUserID;
                        user.CreatedDateTime = DateTime.UtcNow;
                        user.IsRemoved = false;

                        tempPassword = HelperFunctions.GeneratePassword(8);
                        if (!string.IsNullOrEmpty(_tmpPassword))
                        {
                            tempPassword = _tmpPassword;
                        }

                        user.Password = HashProvider.ComputeHash(tempPassword, HashProvider.HashAlgorithmList.SHA256, _securityOptions.PasswordSalt);
                        _tmpPassword = tempPassword;
                    }
                    else
                    {
                        errorMessage = "The user email address already exists. Find the existing user first and edit their details";
                        return Guid.Empty;
                    }
                }
                else
                {
                    tempPassword = HelperFunctions.GeneratePassword(8);
                    if (!string.IsNullOrEmpty(_tmpPassword))
                    {
                        tempPassword = _tmpPassword;
                    }

                    user.Password = HashProvider.ComputeHash(tempPassword, HashProvider.HashAlgorithmList.SHA256, _securityOptions.PasswordSalt);
                    _tmpPassword = tempPassword;

                    user.IsRemoved = false;
                    isWasRemoved = true;
                }
            }

            user.DisplayName = DisplayName;
            user.EmailAddress = EmailAddress;
            user.IsSuspended = IsSuspended;
            user.LoginTries = (IsSuspended == false) ? 0 : user.LoginTries;
            user.EditUserID = userHelper.loggedInUserID;
            user.EditDateTime = DateTime.UtcNow;
            user.FirstName = FirstName;
            user.Surname = Surname;
            user.Timezone = SelectedTimezone;

            if (isNew)
            {
                _context.Add(user);
            }
            else
            {
                _context.Update(user);
            }

            if (isNew || isWasRemoved)
            {
                #region  Send new user registration email

                if (!string.IsNullOrEmpty(EmailAddress))
                {
                    emailTemplate = PublicEnums.EmailTemplateList.NTF_REGISTRATION_WELCOME_CUSTOM;

                    var variables = new Dictionary<string, PropertyMetaData>
                    {
                        {"HostUrl", new PropertyMetaData {Type = typeof (string), Value = _securityOptions.WebsiteHostUrl}},
                        {"DisplayName", new PropertyMetaData {Type = typeof (string), Value = DisplayName}},
                        {"Password", new PropertyMetaData {Type = typeof (string), Value = tempPassword}},
                        {"Username", new PropertyMetaData {Type = typeof (string), Value = EmailAddress}}
                    };

                    await _emailService.SendEmailAsync(new List<string>() { EmailAddress }, "Welcome", emailTemplate, variables, _user);
                }
                #endregion
            }

            await ClearUserRoles(user.UserID);
            await AddSelectedUserRoles(user.UserID);

            await _context.SaveChangesAsync();
            UserID = user.UserID;

            return UserID;
        }

        internal async Task<Guid> UpdateUserProfile()
        {
            UserHelperFunctions userHelper = new UserHelperFunctions()
            {
                _context = _context,
                _emailService = _emailService,
                _securityOptions = _securityOptions,
                _user = _user
            };
            userHelper.Populate();

            //Save user details
            var user = _context.Users.FirstOrDefault(x => x.UserID == UserID);
            if (user != null)
            {
                //Perform Dup Check
                if (EmailAddress != user.EmailAddress)
                {
                    var userDup = _context.Users.FirstOrDefault(x => ((x.EmailAddress == EmailAddress && x.EmailAddress != null)) && x.IsRemoved == false);
                    if (userDup != null)
                    {
                        errorMessage = "The user email address already exists. Please use a different email address.";
                        return Guid.Empty;
                    }
                }

                user.DisplayName = DisplayName;
                user.EmailAddress = EmailAddress;
                user.EditUserID = userHelper.loggedInUserID;
                user.EditDateTime = DateTime.UtcNow;
                user.FirstName = FirstName;
                user.Surname = Surname;
                user.Timezone = SelectedTimezone;

                _context.Update(user);

                await _context.SaveChangesAsync();
                UserID = user.UserID;

                return UserID;
            }
            else
            {
                errorMessage = "Profile details not found";
                return Guid.Empty;
            }
        }
    }

    public class UserDetailsViewModelRoles
    {
        public Guid UserRoleID { get; set; }
        public string Description { get; set; }
        public string EventCode { get; set; }
        public bool Selected { get; set; }
    }
}
