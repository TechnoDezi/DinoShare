using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DinoShare.Helpers
{
    public class PublicEnums
    {
        public struct UserRoleList
        {
            public const string ROLE_ADMINISTRATOR = "ROLE_ADMINISTRATOR";
            public const string ROLE_USER = "ROLE_USER";
        }

        public enum TemporaryTokensTypeList
        {
            TYPE_FORGOT_PASSWORD,
        }

        public enum EmailTemplateList
        {
            NTF_REGISTRATION_WELCOME_CUSTOM,
            NTF_PASSWORD_RESET_LINK,
            NTF_PASSWORD_CHANGED,
            NTF_FILE_READY_DOWNLOAD
        }

        public enum LogLevel
        {
            LEVEL_INFORMATION,
            LEVEL_WARNING,
            LEVEL_EXCEPTION,
            LEVEL_CRITICAL
        }

        public enum SystemConfigurationList
        {
            KEY_LOGIN_TOKEN_VALID_MIN,
            KEY_PASSEORD_RESETLINK_VALIDFOR_MIN,
            KEY_LOGIN_RETRYLIMIT,
            KEY_CLEAN_APP_LOG_DAYS,
            KEY_DEFAULT_TIME_ZONE,
        }
    }
}
