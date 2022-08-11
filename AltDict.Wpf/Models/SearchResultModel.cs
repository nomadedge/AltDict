using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AltDict.Wpf.Models
{
    public class SearchResultModel
    {
        public string VendorCode1 { get; set; }
        public string Manufacturer1 { get; set; }
        public string VendorCode2 { get; set; }
        public string Manufacturer2 { get; set; }
        public string RouteStep => $"|{VendorCode1} + {Manufacturer1}| -> |{VendorCode2} + {Manufacturer2}|";
    }
}
