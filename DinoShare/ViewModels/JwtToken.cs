using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DinoShare.ViewModels
{
    public class JwtToken
    {
        /// <summary>
        /// Signed access token
        /// </summary>
        public string access_token { get; set; }
        /// <summary>
        /// Total seconds until the token expires
        /// </summary>
        public int expires_in { get; set; }
        /// <summary>
        /// The date and time when the token expires
        /// </summary>
        public DateTime expires_on { get; set; }
    }
}
