﻿using System.Collections.Generic;

namespace Ucommerce.Sitefinity.UI.Mvc.ViewModels
{
    public class CartUpdateBasketViewModel
    {
        public CartUpdateBasketViewModel()
        {
            OrderLines = new List<CartUpdateOrderline>();
        }

        public IList<CartUpdateOrderline> OrderLines { get; set; }
        public string OrderTotal { get; set; }
        public string DiscountTotal { get; set; }
        public string TaxTotal { get; set; }
        public string SubTotal { get; set; }
    }
}