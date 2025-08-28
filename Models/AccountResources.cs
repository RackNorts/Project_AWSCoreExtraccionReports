using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSCoreExtraccionReports.Models
{
    public class AccountResources
    {
        public string? Account { get; set; }
        public string? NameAccount { get; set; }
        public string? Resource { get; set; }
        public string? TypeResource { get; set; }
        public string? State { get; set; }
    }
}
