using PosudaShop.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PosudaShop.Models.ViewModels.Shop
{
    public class OrderVM
    {
        public OrderVM()
        {
        }

        public OrderVM(OrderDTO row)
        {
            OrderId = row.OrderId;
            UserId = row.UserId;
            CreatedAt = row.CreatedAt;
        }

        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public IEnumerable<SelectListItem> Status { get; set; }
    }
}