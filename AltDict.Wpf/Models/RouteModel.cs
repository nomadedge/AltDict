using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AltDict.Wpf.Models
{
    public class RouteModel
    {
        public int Index { get; set; }
        public int StepsCount { get; set; }
        public string RouteName => $"Route {Index + 1}, Steps: {StepsCount}";
    }
}
