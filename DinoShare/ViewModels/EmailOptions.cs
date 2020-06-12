using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DinoShare.ViewModels
{
    public class EmailOptions
    {
        public bool EmailEnabled { get; set; }
        public string FromName { get; set; }
        public string FromAddress { get; set; }
        public string LocalDomain { get; set; }
        public string MailServerAddress { get; set; }
        public int MailServerPort { get; set; }
        public bool RequireLogin { get; set; }
        public string Username { get; set; }
        public string UserPassword { get; set; }
        public bool EnableSsl { get; set; }
    }
}
