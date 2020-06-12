using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DinoShare.Helpers
{
    public static class HtmlHelperExtensions
    {
        public static bool UserHasRole(string roleEventCode, ClaimsPrincipal user)
        {
            bool returnValue = false;

            var check = user.Identity.IsAuthenticated;
            if (check)
            {
                if (user.Claims.Count() > 0)
                {
                    if (user.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == roleEventCode))
                    {
                        returnValue = true;
                    }
                }
            }

            return returnValue;
        }
    }

    public static class TypeExtentions
    {
        public static DateTime ToUTCTimezone(this DateTime date, ClaimsPrincipal user)
        {
            string timezone = "";

            if (user.Claims.Any(x => x.Type == "Timezone") && !string.IsNullOrEmpty(user.Claims.Where(x => x.Type == "Timezone").First().Value))
            {
                timezone = user.Claims.Where(x => x.Type == "Timezone").First().Value.ToString();
            }
            else
            {
                timezone = "South Africa Standard Time";
            }

            TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            return TimeZoneInfo.ConvertTimeToUtc(date, zone);
        }

        public static DateTime ToTimezoneFromUtc(this DateTime date, ClaimsPrincipal user)
        {
            string timezone = "";

            if (user != null)
            {
                if (user.Claims.Any(x => x.Type == "Timezone") && !string.IsNullOrEmpty(user.Claims.Where(x => x.Type == "Timezone").First().Value))
                {
                    timezone = user.Claims.Where(x => x.Type == "Timezone").First().Value.ToString();
                }
                else
                {
                    timezone = "South Africa Standard Time";
                }
            }
            else
            {
                timezone = "South Africa Standard Time";
            }

            TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            return TimeZoneInfo.ConvertTimeFromUtc(date, zone);
        }

        public static DateTime? ToUTCTimezone(this DateTime? date, ClaimsPrincipal user)
        {
            if (date.HasValue)
            {
                string timezone = "";

                if (user.Claims.Any(x => x.Type == "Timezone") && !string.IsNullOrEmpty(user.Claims.Where(x => x.Type == "Timezone").First().Value))
                {
                    timezone = user.Claims.Where(x => x.Type == "Timezone").First().Value.ToString();
                }
                else
                {
                    timezone = "South Africa Standard Time";
                }

                TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
                DateTime currentDate = DateTime.SpecifyKind(date.Value, DateTimeKind.Unspecified);
                return TimeZoneInfo.ConvertTimeToUtc(currentDate, zone);
            }
            else
            {
                return date;
            }
        }

        public static DateTime? ToTimezoneFromUtc(this DateTime? date, ClaimsPrincipal user)
        {
            if (date.HasValue)
            {
                string timezone = "";

                if (user.Claims.Any(x => x.Type == "Timezone") && !string.IsNullOrEmpty(user.Claims.Where(x => x.Type == "Timezone").First().Value))
                {
                    timezone = user.Claims.Where(x => x.Type == "Timezone").First().Value.ToString();
                }
                else
                {
                    timezone = "South Africa Standard Time";
                }

                TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
                return TimeZoneInfo.ConvertTimeFromUtc(date.Value, zone);
            }
            else
            {
                return date;
            }
        }

        public static IQueryable<T> OrderByName<T>(this IQueryable<T> source, string propertyName, Boolean isDescending)
        {

            if (source == null) throw new ArgumentNullException("source");
            if (propertyName == null) throw new ArgumentNullException("propertyName");

            Type type = typeof(T);
            ParameterExpression arg = Expression.Parameter(type, "x");

            PropertyInfo pi = type.GetProperty(propertyName);
            Expression expr = Expression.Property(arg, pi);
            type = pi.PropertyType;

            Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);

            String methodName = isDescending ? "OrderByDescending" : "OrderBy";
            object result = typeof(Queryable).GetMethods().Single(
                method => method.Name == methodName
                        && method.IsGenericMethodDefinition
                        && method.GetGenericArguments().Length == 2
                        && method.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), type)
                .Invoke(null, new object[] { source, lambda });
            return (IQueryable<T>)result;
        }

        public static byte[] ToExcel(this DataTable dt)
        {
            byte[] returnValue = null;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Sheet1");
                ws.Cells["A1"].LoadFromDataTable(dt, true);
                ws.Cells.AutoFitColumns();

                // Add some styling
                using (ExcelRange rng = ws.Cells[1, 1, 1, dt.Columns.Count])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Font.Color.SetColor(System.Drawing.Color.Black);
                }

                MemoryStream output = new MemoryStream();
                pck.SaveAs(output);

                returnValue = output.ToArray();
            }

            return returnValue;
        }
    }
}
