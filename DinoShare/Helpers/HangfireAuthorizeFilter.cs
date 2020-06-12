using Hangfire.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DinoShare.Helpers
{
    public class HangfireAuthorizeFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            return HtmlHelperExtensions.UserHasRole(PublicEnums.UserRoleList.ROLE_ADMINISTRATOR.ToString(), httpContext.User);
        }
    }
}
