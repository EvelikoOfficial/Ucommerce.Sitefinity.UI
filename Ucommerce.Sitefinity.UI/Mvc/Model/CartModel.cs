﻿using System;
using Ucommerce.Sitefinity.UI.Mvc.Model.Interfaces;
using Ucommerce.Sitefinity.UI.Mvc.ViewModels;
using UCommerce;
using UCommerce.Api;
using UCommerce.EntitiesV2;
using UCommerce.Infrastructure;
using UCommerce.Transactions;

namespace Ucommerce.Sitefinity.UI.Mvc.Model
{
    public class CartModel : ICartModel
    {
        private readonly TransactionLibraryInternal _transactionLibraryInternal;
        private Guid nextStepId;

        public CartModel(Guid? nextStepId = null)
        {
            _transactionLibraryInternal = ObjectFactory.Instance.Resolve<TransactionLibraryInternal>();
            this.nextStepId = nextStepId ?? Guid.Empty;
        }

        public CartRenderingViewModel CreateViewModel(string refreshUrl, string removeOrderLineUrl)
        {
            var basketVM = new CartRenderingViewModel();

            if (!_transactionLibraryInternal.HasBasket())
            {
                return basketVM;
            }

            PurchaseOrder basket = _transactionLibraryInternal.GetBasket(false).PurchaseOrder;

            foreach (var orderLine in basket.OrderLines)
            {
                var orderLineViewModel = new CartRenderingViewModel.OrderlineViewModel();

                orderLineViewModel.Quantity = orderLine.Quantity;
                orderLineViewModel.ProductName = orderLine.ProductName;
                orderLineViewModel.Sku = orderLine.Sku;
                orderLineViewModel.VariantSku = orderLine.VariantSku;
                orderLineViewModel.Total = new Money(orderLine.Total.GetValueOrDefault(), basket.BillingCurrency).ToString();
                orderLineViewModel.Discount = orderLine.Discount;
                orderLineViewModel.Tax = new Money(orderLine.VAT, basket.BillingCurrency).ToString();
                orderLineViewModel.Price = new Money(orderLine.Price, basket.BillingCurrency).ToString();
                orderLineViewModel.ProductUrl = CatalogLibrary.GetNiceUrlForProduct(CatalogLibrary.GetProduct(orderLine.Sku));
                orderLineViewModel.PriceWithDiscount = new Money(orderLine.Price - orderLine.UnitDiscount.GetValueOrDefault(), basket.BillingCurrency).ToString();
                orderLineViewModel.OrderLineId = orderLine.OrderLineId;
                basketVM.OrderLines.Add(orderLineViewModel);
            }

            basketVM.OrderTotal = new Money(basket.OrderTotal.GetValueOrDefault(), basket.BillingCurrency).ToString();
            basketVM.DiscountTotal = new Money(basket.DiscountTotal.GetValueOrDefault(), basket.BillingCurrency).ToString();
            basketVM.TaxTotal = new Money(basket.TaxTotal.GetValueOrDefault(), basket.BillingCurrency).ToString();
            basketVM.SubTotal = new Money(basket.SubTotal.GetValueOrDefault(), basket.BillingCurrency).ToString();
            basketVM.NextStepUrl = GetNextStepAbsoluteUrl(nextStepId);

            basketVM.RefreshUrl = refreshUrl;
            basketVM.RemoveOrderlineUrl = removeOrderLineUrl;

            return basketVM;
        }

        public CartUpdateBasketViewModel UpdateViewModel(CartUpdateBasket model)
        {
            foreach (var updateOrderline in model.RefreshBasket)
            {
                var newQuantity = updateOrderline.OrderLineQty;
                if (newQuantity <= 0)
                {
                    newQuantity = 0;
                }

                _transactionLibraryInternal.UpdateLineItemByOrderLineId(updateOrderline.OrderLineId, newQuantity);
            }

            _transactionLibraryInternal.ExecuteBasketPipeline();

            var basket = _transactionLibraryInternal.GetBasket(false).PurchaseOrder;

            CartUpdateBasketViewModel updatedBasket = new CartUpdateBasketViewModel();

            foreach (var orderLine in basket.OrderLines)
            {
                var orderLineViewModel = new CartUpdateOrderline();
                orderLineViewModel.OrderlineId = orderLine.OrderLineId;
                orderLineViewModel.Quantity = orderLine.Quantity;
                orderLineViewModel.Total = new Money(orderLine.Total.GetValueOrDefault(), basket.BillingCurrency).ToString();
                orderLineViewModel.Discount = orderLine.Discount;
                orderLineViewModel.Tax = new Money(orderLine.VAT, basket.BillingCurrency).ToString();
                orderLineViewModel.Price = new Money(orderLine.Price, basket.BillingCurrency).ToString();
                orderLineViewModel.PriceWithDiscount = new Money(orderLine.Price - orderLine.Discount, basket.BillingCurrency).ToString();

                updatedBasket.Orderlines.Add(orderLineViewModel);
            }

            string orderTotal = new Money(basket.OrderTotal.GetValueOrDefault(), basket.BillingCurrency).ToString();
            string discountTotal = new Money(basket.DiscountTotal.GetValueOrDefault(), basket.BillingCurrency).ToString();
            string taxTotal = new Money(basket.TaxTotal.GetValueOrDefault(), basket.BillingCurrency).ToString();
            string subTotal = new Money(basket.SubTotal.GetValueOrDefault(), basket.BillingCurrency).ToString();

            updatedBasket.OrderTotal = orderTotal;
            updatedBasket.DiscountTotal = discountTotal;
            updatedBasket.TaxTotal = taxTotal;
            updatedBasket.SubTotal = subTotal;

            return updatedBasket;
        }

        private string GetNextStepAbsoluteUrl(Guid nextStepId)
        {
            var nextStepUrl = Pages.UrlResolver.GetPageNodeUrl(nextStepId);

            return Pages.UrlResolver.GetAbsoluteUrl(nextStepUrl);
        }
    }
}
