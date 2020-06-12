using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DinoShare.ViewModels
{
    public class AuthenticateRequest
    {
        [Required]
        [Display(Name = "Email Address / Username")]
        public string EmailAddressUsername { get; set; }

        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
