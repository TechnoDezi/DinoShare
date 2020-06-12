using DinoShare.Helpers;
using DinoShare.Helpers.EmailServiceFactory;
using DinoShare.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DinoShare.ViewModels.EmailTemplateViewModelFactory
{
    public class EmailTemplateViewModel
    {
        internal AppDBContext _context;
        internal SecurityOptions _securityOptions;
        internal IEmailService _emailService;
        internal ClaimsPrincipal _user;

        public Guid EmailTemplateID { get; set; }
        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }
        [Required]
        [Display(Name = "Event Code")]
        public string EventCode { get; set; }
        [Required]
        [Display(Name = "Template Body")]
        public string TemplateBody { get; set; }
        [Display(Name = "Template Body Tags")]
        public List<string> TemplateBodyTags { get; set; }

        internal async Task PopulateDetails()
        {
            var item = _context.EmailTemplates.FirstOrDefault(x => x.EmailTemplateID == EmailTemplateID);
            if (item != null)
            {
                Description = item.Description;
                EventCode = item.EventCode;
                TemplateBody = item.TemplateBody;
                TemplateBodyTags = item.TemplateBodyTags.Split(',').ToList();
            }
        }

        internal async Task Save()
        {
            UserHelperFunctions userHelper = new UserHelperFunctions()
            {
                _context = _context,
                _securityOptions = _securityOptions,
                _user = _user
            };
            userHelper.Populate();

            var emailTemplate = _context.EmailTemplates.FirstOrDefault(x => x.EmailTemplateID == EmailTemplateID);
            if (emailTemplate != null)
            {
                emailTemplate.Description = Description;
                emailTemplate.TemplateBody = TemplateBody;
                emailTemplate.EditDateTime = DateTime.UtcNow;
                emailTemplate.EditUserID = userHelper.loggedInUserID;

                _context.Update(emailTemplate);

                await _context.SaveChangesAsync();
            }
        }
    }

    public class EmailTemplateListViewModel
    {
        internal AppDBContext _context;
        internal SecurityOptions _securityOptions;
        internal IEmailService _emailService;

        public string SearchValue { get; set; }
        public PaginationViewModel Pagination { get; set; }

        public List<EmailTemplateViewModel> Templates { get; set; }

        internal async Task PopulateLists()
        {
            if (Pagination == null)
            {
                Pagination = new PaginationViewModel();
                Pagination.Top = 10;
            }

            var list = (from t in _context.EmailTemplates
                        where (!string.IsNullOrEmpty(SearchValue) && (t.Description.Contains(SearchValue) || t.EventCode.Contains(SearchValue)) || string.IsNullOrEmpty(SearchValue))
                        select new EmailTemplateViewModel
                        {
                            Description = t.Description,
                            EmailTemplateID = t.EmailTemplateID,
                            EventCode = t.EventCode,
                            TemplateBody = t.TemplateBody
                        });

            Pagination.TotalRecords = list.Count();
            if (!string.IsNullOrEmpty(Pagination.SortBy))
            {
                list = list.OrderByName(Pagination.SortBy, Pagination.Descending);
            }
            Templates = list.Skip(Pagination.Skip).Take(Pagination.Top).ToList();
        }
    }
}
