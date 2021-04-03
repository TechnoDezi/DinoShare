using ImageMagick;
using DinoShare.Models;
using DinoShare.Models.SystemModelFactory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DinoShare.Helpers
{
    public static class HelperFunctions
    {
        public static string GeneratePassword(int length)
        {
            int maxSize = length;
            char[] chars = new char[30];
            string a;
            a = "1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ*%$#@";

            chars = a.ToCharArray();
            int size = maxSize;
            byte[] data = new byte[1];

            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            size = maxSize;
            data = new byte[size];
            crypto.GetNonZeroBytes(data);

            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data) { result.Append(chars[b % (chars.Length)]); }

            return result.ToString();
        }

        public static MemoryStream ResizeImagePreportional(byte[] blobData, int maxWidth, int maxHeight, int quality)
        {
            if (blobData != null)
            {
                MemoryStream outputStream = new MemoryStream();

                using (var image = new MagickImage(blobData))
                {
                    image.Resize(maxWidth, maxHeight);
                    image.Strip();
                    image.Quality = quality;
                    image.Write(outputStream);
                }

                return outputStream;
            }
            else
            {
                return null;
            }
        }

        public static void Log(AppDBContext _context, PublicEnums.LogLevel logLevel, string originator, string message, ClaimsPrincipal user = null, Exception ex = null)
        {
            try
            {
                Guid? userID = null;

                if (user != null && user.Identity != null && user.Identities.First().IsAuthenticated == true)
                {
                    userID = Guid.Parse(user.Claims.Where(x => x.Type == "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/UserID").First().Value);
                }

                ApplicationLog logEntry = new ApplicationLog()
                {
                    ApplicationLogID = Guid.NewGuid(),
                    Exception = (ex != null) ? ex.ToString() : "",
                    Level = logLevel.ToString(),
                    LogDate = DateTime.Now,
                    LogOriginator = originator,
                    Message = message,
                    UserID = userID
                };

                _context.Add(logEntry);
                _context.SaveChanges();
            }
            catch
            {
                //Log to some other location
            }
        }
    }
}
