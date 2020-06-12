using DinoShare.Models.AccountDataModelFactory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DinoShare.Models.SystemModelFactory
{
    public class ApplicationLog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid ApplicationLogID { get; set; }
        public DateTime LogDate { get; set; }
        public string Level { get; set; }
        public string LogOriginator { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public Guid? UserID { get; set; }

        public User User { get; set; }
    }

    public class EmailTemplate
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid EmailTemplateID { get; set; }
        public string Description { get; set; }
        public string EventCode { get; set; }
        public string TemplateBody { get; set; }
        public string TemplateBodyTags { get; set; }

        public Guid? CreatedUserID { get; set; }
        public Guid? EditUserID { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public DateTime? EditDateTime { get; set; }
    }

    public class SystemConfiguration
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid SystemConfigurationID { get; set; }
        public string Description { get; set; }
        public string EventCode { get; set; }
        public string ConfigValue { get; set; }

        public Guid? CreatedUserID { get; set; }
        public Guid? EditUserID { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public DateTime? EditDateTime { get; set; }
    }
}
