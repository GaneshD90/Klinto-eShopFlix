using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.DTOs
{
    public class RazorPayOrderDTO
    {
        public int Amount { get; set; }
        public string Currency { get; set; }
        public string Receipt { get; set; }
    }
}
