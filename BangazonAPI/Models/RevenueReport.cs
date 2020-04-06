using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class RevenueReport
    {
        public int ProductTypeId { get; set; }
        public string ProductType { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
