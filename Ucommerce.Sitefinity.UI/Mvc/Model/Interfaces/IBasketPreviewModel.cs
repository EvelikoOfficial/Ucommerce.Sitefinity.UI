﻿using Ucommerce.Sitefinity.UI.Mvc.ViewModels;
using UCommerce.EntitiesV2;

namespace Ucommerce.Sitefinity.UI.Mvc.Model.Interfaces
{
    public interface IBasketPreviewModel
    {
        BasketPreviewViewModel GetViewModelr(PurchaseOrder purchaseOrder);
    }
}
