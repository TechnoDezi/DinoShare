using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DinoShare.ViewModels
{
    public class SecurityOptions
    {
        public string SecretKey { get; set; }
        public string PasswordSalt { get; set; }
        public string WebsiteHostUrl { get; set; }
        public string BlobStorageConnectionString { get; set; }
        public string BlobContainerReference { get; set; }
        public bool UseFileStorage { get; set; }
        public string FolderLocation { get; set; }
    }
}
