using DinoShare.Helpers;
using DinoShare.Models;
using DinoShare.Models.SystemModelFactory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DinoShare.ViewModels.SystemConfigViewModelFactory
{
    public class SystemConfigViewModel
    {
        internal AppDBContext _context;
        internal SecurityOptions _securityOptions;
        internal string errorMessage = "";
        internal ClaimsPrincipal _user;

        public Guid SystemConfigurationID { get; set; }
        [Display(Name = "Description")]
        public string Description { get; set; }
        [Display(Name = "Event Code")]
        public string EventCode { get; set; }
        [Required]
        [Display(Name = "Configuration Value")]
        public string ConfigValue { get; set; }

        internal async Task PopulateModel()
        {
            if (SystemConfigurationID != Guid.Empty)
            {
                var item = _context.SystemConfiguration.Where(x => x.SystemConfigurationID == SystemConfigurationID).FirstOrDefault();
                if (item != null)
                {
                    Description = item.Description;
                    EventCode = item.EventCode;
                    ConfigValue = item.ConfigValue;
                }
            }
        }

        internal async Task<Guid> Save()
        {
            UserHelperFunctions userHelper = new UserHelperFunctions()
            {
                _context = _context,
                _securityOptions = _securityOptions,
                _user = _user
            };
            userHelper.Populate();

            bool isAdd = false;
            var conf = _context.SystemConfiguration.Where(x => x.SystemConfigurationID == SystemConfigurationID).FirstOrDefault();
            if (conf == null)
            {
                conf = new SystemConfiguration();
                conf.SystemConfigurationID = Guid.NewGuid();
                conf.CreatedUserID = userHelper.loggedInUserID;
                conf.CreatedDateTime = DateTime.UtcNow;
                isAdd = true;
            }

            conf.ConfigValue = ConfigValue;
            conf.EditUserID = userHelper.loggedInUserID;
            conf.EditDateTime = DateTime.UtcNow;

            if (isAdd)
            {
                _context.SystemConfiguration.Add(conf);
            }
            else
            {
                _context.SystemConfiguration.Update(conf);
            }

            _context.SaveChanges();

            return conf.SystemConfigurationID;
        }
    }

    public class SystemConfigListViewModel
    {
        internal AppDBContext _context;

        [Display(Name = "Search")]
        public string SearchValue { get; set; }
        public PaginationViewModel Pagination { get; set; }

        public List<SystemConfigViewModel> ConfList { get; set; }

        internal async Task PopulateModel()
        {
            if (Pagination == null)
            {
                Pagination = new PaginationViewModel();
                Pagination.Top = 10;
            }

            var list = (from p in _context.SystemConfiguration
                        where (!string.IsNullOrEmpty(SearchValue) && (p.Description.Contains(SearchValue)) || string.IsNullOrEmpty(SearchValue))
                        orderby p.Description
                        select new SystemConfigViewModel
                        {
                            ConfigValue = p.ConfigValue,
                            Description = p.Description,
                            EventCode = p.EventCode,
                            SystemConfigurationID = p.SystemConfigurationID
                        });

            Pagination.TotalRecords = list.Count();
            if (!string.IsNullOrEmpty(Pagination.SortBy))
            {
                list = list.OrderByName(Pagination.SortBy, Pagination.Descending);
            }
            ConfList = list.Skip(Pagination.Skip).Take(Pagination.Top).ToList();
        }
    }
}
