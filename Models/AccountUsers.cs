using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSCoreExtraccionReports.Models
{
    public class AccountUsers
    {
        public string? UserName { get; set; }
        public string? ARN { get; set; }
        public string? UserCreate { get; set; }
        public string? Permissions { get; set; }
    }
}
