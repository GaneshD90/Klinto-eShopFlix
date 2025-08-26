using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.DTOs
{
    public class CartDTO
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool IsActive { get; set; }
        public decimal Total { get; set; }
        public decimal Tax { get; set; }
        public decimal GrandTotal { get; set; }
        public List<CartItemDTO> CartItems { get; set; }
    }
}
