using DinoShare.Helpers;
using DinoShare.Models.AccountDataModelFactory;
using DinoShare.Models.SystemModelFactory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DinoShare.ViewModels;

namespace DinoShare.Models
{
    public static class DbInitializer
    {
        public static void Initialize(AppDBContext context, SecurityOptions securityOptions)
        {
            AddUserRoles(context, securityOptions);
            AddInitialUserAccounts(context, securityOptions);
            AddSystemConfiguration(context, securityOptions);
            AddEmailTemplates(context, securityOptions);
            AddTemporaryTokensType(context, securityOptions);
        }

        private static void AddSystemConfiguration(AppDBContext context, SecurityOptions securityOptions)
        {
            bool itemAdded = false;

            if (context.SystemConfiguration.Any(x => x.EventCode == PublicEnums.SystemConfigurationList.KEY_LOGIN_TOKEN_VALID_MIN.ToString()) == false)
            {
                context.SystemConfiguration.Add(new SystemConfiguration()
                {
                    EventCode = PublicEnums.SystemConfigurationList.KEY_LOGIN_TOKEN_VALID_MIN.ToString(),
                    Description = "Login token valid for days",
                    ConfigValue = "7",
                    CreatedDateTime = DateTime.UtcNow,
                    CreatedUserID = null,
                    EditDateTime = DateTime.UtcNow,
                    EditUserID = null,
                    SystemConfigurationID = Guid.NewGuid()
                });
                itemAdded = true;
            }
            if (context.SystemConfiguration.Any(x => x.EventCode == PublicEnums.SystemConfigurationList.KEY_PASSEORD_RESETLINK_VALIDFOR_MIN.ToString()) == false)
            {
                context.SystemConfiguration.Add(new SystemConfiguration()
                {
                    EventCode = PublicEnums.SystemConfigurationList.KEY_PASSEORD_RESETLINK_VALIDFOR_MIN.ToString(),
                    CreatedDateTime = DateTime.UtcNow,
                    CreatedUserID = null,
                    EditDateTime = DateTime.UtcNow,
                    EditUserID = null,
                    Description = "Password reset link valid for minutes",
                    ConfigValue = "30",
                    SystemConfigurationID = Guid.NewGuid()
                });
                itemAdded = true;
            }
            if (context.SystemConfiguration.Any(x => x.EventCode == PublicEnums.SystemConfigurationList.KEY_LOGIN_RETRYLIMIT.ToString()) == false)
            {
                context.SystemConfiguration.Add(new SystemConfiguration()
                {
                    EventCode = PublicEnums.SystemConfigurationList.KEY_LOGIN_RETRYLIMIT.ToString(),
                    CreatedDateTime = DateTime.UtcNow,
                    CreatedUserID = null,
                    EditDateTime = DateTime.UtcNow,
                    EditUserID = null,
                    Description = "Login retry limit",
                    ConfigValue = "3",
                    SystemConfigurationID = Guid.NewGuid()
                });
                itemAdded = true;
            }
            if (context.SystemConfiguration.Any(x => x.EventCode == PublicEnums.SystemConfigurationList.KEY_CLEAN_APP_LOG_DAYS.ToString()) == false)
            {
                context.SystemConfiguration.Add(new SystemConfiguration()
                {
                    EventCode = PublicEnums.SystemConfigurationList.KEY_CLEAN_APP_LOG_DAYS.ToString(),
                    CreatedDateTime = DateTime.UtcNow,
                    CreatedUserID = null,
                    EditDateTime = DateTime.UtcNow,
                    EditUserID = null,
                    Description = "Clean application log older than days",
                    ConfigValue = "90",
                    SystemConfigurationID = Guid.NewGuid()
                });
                itemAdded = true;
            }
            if (context.SystemConfiguration.Any(x => x.EventCode == PublicEnums.SystemConfigurationList.KEY_DEFAULT_TIME_ZONE.ToString()) == false)
            {
                context.SystemConfiguration.Add(new SystemConfiguration()
                {
                    EventCode = PublicEnums.SystemConfigurationList.KEY_DEFAULT_TIME_ZONE.ToString(),
                    CreatedDateTime = DateTime.UtcNow,
                    CreatedUserID = null,
                    EditDateTime = DateTime.UtcNow,
                    EditUserID = null,
                    Description = "Default Time Zone",
                    ConfigValue = "South Africa Standard Time",
                    SystemConfigurationID = Guid.NewGuid()
                });
                itemAdded = true;
            }

            if (itemAdded)
            {
                context.SaveChanges();
            }
        }

        private static void AddInitialUserAccounts(AppDBContext context, SecurityOptions securityOptions)
        {
            if (context.Users.Any() == false)
            {
                var password = "password";
                string hashedPassword = HashProvider.ComputeHash(password, HashProvider.HashAlgorithmList.SHA256, securityOptions.PasswordSalt);

                var users = new User[]
                {
                    new User
                    {
                        DisplayName = "Dezi Van Vuuren",
                        FirstName = "Dezi",
                        Surname = "Van Vuuren",
                        EmailAddress = "technodezi@hotmail.com",
                        Password = hashedPassword,
                        UserID = Guid.NewGuid(),
                        CreatedDateTime = DateTime.UtcNow,
                        CreatedUserID = Guid.Empty,
                        EditDateTime = DateTime.UtcNow,
                        EditUserID = Guid.Empty,

                    }
                };

                foreach (User s in users)
                {
                    context.Users.Add(s);

                    var adminRole = context.UserRoles.Where(x => x.EventCode == PublicEnums.UserRoleList.ROLE_ADMINISTRATOR).First();
                    LinkUserRole roleLink = new LinkUserRole()
                    {
                        LinkUserRoleID = Guid.NewGuid(),
                        UserID = s.UserID,
                        UserRoleID = adminRole.UserRoleID
                    };
                    context.LinkUserRole.Add(roleLink);
                }
                context.SaveChanges();
            }
        }

        private static void AddUserRoles(AppDBContext context, SecurityOptions securityOptions)
        {
            bool itemAdded = false;

            if (context.UserRoles.Any(x => x.EventCode == PublicEnums.UserRoleList.ROLE_ADMINISTRATOR) == false)
            {
                context.UserRoles.Add(new UserRole { Description = "Administrator", EventCode = PublicEnums.UserRoleList.ROLE_ADMINISTRATOR.ToString(), UserRoleID = Guid.NewGuid() });
                itemAdded = true;
            }
            if (context.UserRoles.Any(x => x.EventCode == PublicEnums.UserRoleList.ROLE_USER) == false)
            {
                context.UserRoles.Add(new UserRole { Description = "Standard User", EventCode = PublicEnums.UserRoleList.ROLE_USER.ToString(), UserRoleID = Guid.NewGuid() });
                itemAdded = true;
            }

            if (itemAdded)
            {
                context.SaveChanges();
            }
        }

        private static void AddTemporaryTokensType(AppDBContext context, SecurityOptions securityOptions)
        {
            bool itemAdded = false;

            if (context.TemporaryTokensType.Any(x => x.EventCode == PublicEnums.TemporaryTokensTypeList.TYPE_FORGOT_PASSWORD.ToString()) == false)
            {
                context.TemporaryTokensType.Add(new TemporaryTokensType { Description = "Forgot Password", EventCode = PublicEnums.TemporaryTokensTypeList.TYPE_FORGOT_PASSWORD.ToString(), TemporaryTokensTypeID = Guid.NewGuid() });
                itemAdded = true;
            }

            if (itemAdded)
            {
                context.SaveChanges();
            }
        }

        private static void AddEmailTemplates(AppDBContext context, SecurityOptions securityOptions)
        {
            if (context.EmailTemplates.Any(t => t.EventCode == PublicEnums.EmailTemplateList.NTF_REGISTRATION_WELCOME_CUSTOM.ToString()) == false)
            {
                EmailTemplate template = new EmailTemplate()
                {
                    Description = "Registration welcome email",
                    CreatedDateTime = DateTime.UtcNow,
                    CreatedUserID = null,
                    EditDateTime = DateTime.UtcNow,
                    EditUserID = null,
                    EventCode = PublicEnums.EmailTemplateList.NTF_REGISTRATION_WELCOME_CUSTOM.ToString(),
                    EmailTemplateID = Guid.NewGuid(),
                    TemplateBodyTags = "[DisplayName],[Username],[Password],[HostUrl]",
                    TemplateBody = @"
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta name=""viewport"" content=""width=device-width"" />
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
    <title>Welcome to DinoShare</title>
    <link href=""[HostUrl]/plugins/transactional-email-templates/templates/styles.css"" media=""all"" rel=""stylesheet"" type=""text/css"" />
</head>

<body itemscope=itemscope itemtype=""http://schema.org/EmailMessage"">

    <table class=""body-wrap"">
        <tr>
            <td></td>
            <td class=""container"" width=""600"">
                <div class=""content"">
                    <table class=""main"" width=""100%"" cellpadding=""0"" cellspacing=""0"" itemprop=""action"" itemscope=itemscope itemtype=""http://schema.org/ConfirmAction"">
                        <tr>
                            <td class=""content-wrap"">
                                <meta itemprop=""name"" content=""Confirm Email"" />
                                <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                    <tr>
                                        <td class=""content-block"">
                                            <h3>Welcom to DinoShare [DisplayName]</h3>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class=""content-block"">
                                            This email serves as an indication that you have been granted access to / registered on the DinoShare system.
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class=""content-block"">
                                            You can now log on to the DinoShare system using your username <b>[Username]</b> and your temporary password as indicated below.
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class=""content-block"">
                                            Your temporary password is: <b>[Password]</b>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class=""content-block"">
                                            Please log in then navigate to Profile > Change Password, to change your temporary password.
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class=""content-block aligncenter"" itemprop=""handler"" itemscope=itemscope itemtype=""http://schema.org/HttpActionHandler"">
                                            <a href=""[HostUrl]"" class=""btn-primary"" itemprop=""url"">Open DinoShare system</a>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                    <div class=""footer"">
                        <table width=""100%"">
                            <tr>
                                <td class=""aligncenter content-block"">DinoShare</td>
                            </tr>
                        </table>
                    </div>
                </div>
            </td>
            <td></td>
        </tr>
    </table>

</body>
</html>
                    "
                };

                context.EmailTemplates.Add(template);
                context.SaveChanges();
            }

            if (context.EmailTemplates.Any(t => t.EventCode == PublicEnums.EmailTemplateList.NTF_PASSWORD_RESET_LINK.ToString()) == false)
            {
                EmailTemplate template = new EmailTemplate()
                {
                    Description = "Password reset link email",
                    CreatedDateTime = DateTime.UtcNow,
                    CreatedUserID = null,
                    EditDateTime = DateTime.UtcNow,
                    EditUserID = null,
                    EventCode = PublicEnums.EmailTemplateList.NTF_PASSWORD_RESET_LINK.ToString(),
                    EmailTemplateID = Guid.NewGuid(),
                    TemplateBodyTags = "[DisplayName],[HostUrl],[Link]",
                    TemplateBody = @"
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta name=""viewport"" content=""width=device-width"" />
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
    <title>Password reset link</title>
    <link href=""[HostUrl]/plugins/transactional-email-templates/templates/styles.css"" media=""all"" rel=""stylesheet"" type=""text/css"" />
</head>

<body itemscope=itemscope itemtype=""http://schema.org/EmailMessage"">

    <table class=""body-wrap"">
        <tr>
            <td></td>
            <td class=""container"" width=""600"">
                <div class=""content"">
                    <table class=""main"" width=""100%"" cellpadding=""0"" cellspacing=""0"" itemprop=""action"" itemscope=itemscope itemtype=""http://schema.org/ConfirmAction"">
                        <tr>
                            <td class=""content-wrap"">
                                <meta itemprop=""name"" content=""Password reset"" />
                                <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                    <tr>
                                        <td class=""content-block"">
                                            <h3>Dear [DisplayName]</h3>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class=""content-block"">
                                            You have requested a password reset for DinoShare. Please click the Reset Password button to reset your password.<br />
                                            If you did not request a password reset, please ignore this email.
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class=""content-block aligncenter"" itemprop=""handler"" itemscope=itemscope itemtype=""http://schema.org/HttpActionHandler"">
                                            <a href=""[Link]"" class=""btn-primary"" itemprop=""url"">Reset Password</a>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                    <div class=""footer"">
                        <table width=""100%"">
                            <tr>
                                <td class=""aligncenter content-block"">DinoShare</td>
                            </tr>
                        </table>
                    </div>
                </div>
            </td>
            <td></td>
        </tr>
    </table>

</body>
</html>
                    "
                };

                context.EmailTemplates.Add(template);
                context.SaveChanges();
            }

            if (context.EmailTemplates.Any(t => t.EventCode == PublicEnums.EmailTemplateList.NTF_PASSWORD_CHANGED.ToString()) == false)
            {
                EmailTemplate template = new EmailTemplate()
                {
                    Description = "Password Changed email",
                    CreatedDateTime = DateTime.UtcNow,
                    CreatedUserID = null,
                    EditDateTime = DateTime.UtcNow,
                    EditUserID = null,
                    EventCode = PublicEnums.EmailTemplateList.NTF_PASSWORD_CHANGED.ToString(),
                    EmailTemplateID = Guid.NewGuid(),
                    TemplateBodyTags = "[DisplayName],[HostUrl]",
                    TemplateBody = @"
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta name=""viewport"" content=""width=device-width"" />
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
    <title>Password Changed</title>
    <link href=""[HostUrl]/plugins/transactional-email-templates/templates/styles.css"" media=""all"" rel=""stylesheet"" type=""text/css"" />
</head>

<body itemscope=itemscope itemtype=""http://schema.org/EmailMessage"">

    <table class=""body-wrap"">
        <tr>
            <td></td>
            <td class=""container"" width=""600"">
                <div class=""content"">
                    <table class=""main"" width=""100%"" cellpadding=""0"" cellspacing=""0"" itemprop=""action"" itemscope=itemscope itemtype=""http://schema.org/ConfirmAction"">
                        <tr>
                            <td class=""content-wrap"">
                                <meta itemprop=""name"" content=""Password reset"" />
                                <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                    <tr>
                                        <td class=""content-block"">
                                            <h3>Dear [DisplayName]</h3>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class=""content-block"">
                                            This emails is to notify you that your password on the DinoShare system have been changed. Should this be in error please contact your Administrator as soon as possible.
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                    <div class=""footer"">
                        <table width=""100%"">
                            <tr>
                                <td class=""aligncenter content-block"">DinoShare</td>
                            </tr>
                        </table>
                    </div>
                </div>
            </td>
            <td></td>
        </tr>
    </table>

</body>
</html>
                    "
                };

                context.EmailTemplates.Add(template);
                context.SaveChanges();
            }

            if (context.EmailTemplates.Any(t => t.EventCode == PublicEnums.EmailTemplateList.NTF_FILE_READY_DOWNLOAD.ToString()) == false)
            {
                EmailTemplate template = new EmailTemplate()
                {
                    Description = "File ready for download",
                    CreatedDateTime = DateTime.UtcNow,
                    CreatedUserID = null,
                    EditDateTime = DateTime.UtcNow,
                    EditUserID = null,
                    EventCode = PublicEnums.EmailTemplateList.NTF_FILE_READY_DOWNLOAD.ToString(),
                    EmailTemplateID = Guid.NewGuid(),
                    TemplateBodyTags = "[DisplayName],[ZipDownloadLink]",
                    TemplateBody = @"
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta name=""viewport"" content=""width=device-width"" />
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
    <title>File ready for download</title>
    <link href=""[HostUrl]/plugins/transactional-email-templates/templates/styles.css"" media=""all"" rel=""stylesheet"" type=""text/css"" />
</head>

<body itemscope=itemscope itemtype=""http://schema.org/EmailMessage"">

    <table class=""body-wrap"">
        <tr>
            <td></td>
            <td class=""container"" width=""600"">
                <div class=""content"">
                    <table class=""main"" width=""100%"" cellpadding=""0"" cellspacing=""0"" itemprop=""action"" itemscope=itemscope itemtype=""http://schema.org/ConfirmAction"">
                        <tr>
                            <td class=""content-wrap"">
                                <meta itemprop=""name"" content=""Password reset"" />
                                <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                    <tr>
                                        <td class=""content-block"">
                                            <h3>Dear [DisplayName]</h3>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class=""content-block"">
                                            Your file is ready for download<br><br>
                                            <a href=""[ZipDownloadLink]"">Please click here to download the file</a>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                    <div class=""footer"">
                        <table width=""100%"">
                            <tr>
                                <td class=""aligncenter content-block"">DinoShare</td>
                            </tr>
                        </table>
                    </div>
                </div>
            </td>
            <td></td>
        </tr>
    </table>

</body>
</html>
                    "
                };

                context.EmailTemplates.Add(template);
                context.SaveChanges();
            }
        }
    }
}
