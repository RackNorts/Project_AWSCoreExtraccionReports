using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSCoreExtraccionReports.Models
{
     public class AccountPermissionSetAndPolicy
    {
        public string? Account { get; set; }
        public string? NamePolicy { get; set; }
        public string? JSonConfigurationPolicy { get; set; }
    }
}
