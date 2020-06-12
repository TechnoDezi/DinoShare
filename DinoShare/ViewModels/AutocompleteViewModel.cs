using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DinoShare.ViewModels.AutocompleteViewModelFactory
{
    public class AutocompleteViewModel
    {
        public string query { get; set; }
        public List<AutocompleteViewModelData> suggestions { get; set; }
    }

    public class AutocompleteViewModelData
    {
        public string value { get; set; }
        public string data { get; set; }
    }
}
