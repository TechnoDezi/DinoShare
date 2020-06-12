using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DinoShare.ViewModels
{
    public class PaginationViewModel
    {
        public int TotalRecords { get; set; }
        public int Skip { get; set; }
        public int Top { get; set; }
        public bool Descending { get; set; }
        public string SortBy { get; set; }
    }
}
