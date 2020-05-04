﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Telerik.Sitefinity.Services;
using Telerik.Sitefinity.Web;
using UCommerce.Api;
using UCommerce.Content;
using UCommerce.EntitiesV2;
using UCommerce.Infrastructure;
using UCommerce.Sitefinity.UI.Mvc.ViewModels;
using UCommerce.Transactions;
using UCommerce.Extensions;

namespace UCommerce.Sitefinity.UI.Mvc.Model
{
    /// <summary>
    /// The Model class of the Cart MVC widget.
    /// </summary>
    public class CartModel : ICartModel
    {
        private Guid productDetailsPageId;
        private Guid nextStepId;
        private Guid redirectPageId;
        private readonly TransactionLibraryInternal _transactionLibraryInternal;

        public CartModel(Guid? nextStepId = null, Guid? productDetailsPageId = null, Guid? redirectPageId = null)
        {
            _transactionLibraryInternal = ObjectFactory.Instance.Resolve<TransactionLibraryInternal>();
            this.nextStepId = nextStepId ?? Guid.Empty;
            this.productDetailsPageId = productDetailsPageId ?? Guid.Empty;
            this.redirectPageId = redirectPageId ?? Guid.Empty;
        }

        public virtual CartRenderingViewModel GetViewModel(string refreshUrl, string removeOrderLineUrl)
        {
            var basketVM = new CartRenderingViewModel();

            if (!_transactionLibraryInternal.HasBasket())
            {
                return basketVM;
            }

            PurchaseOrder basket = _transactionLibraryInternal.GetBasket(false).PurchaseOrder;
            basketVM.OrderLines = CartModel.GetOrlerLineList(basket, this.productDetailsPageId);

            this.GetDiscounts(basketVM, basket);
            basketVM.OrderTotal = new Money(basket.OrderTotal.GetValueOrDefault(), basket.BillingCurrency).ToString();
            basketVM.DiscountTotal = basket.DiscountTotal.GetValueOrDefault() > 0
                ? new Money(basket.DiscountTotal.GetValueOrDefault(), basket.BillingCurrency).ToString()
                : "";
            basketVM.TaxTotal = new Money(basket.TaxTotal.GetValueOrDefault(), basket.BillingCurrency).ToString();
            basketVM.SubTotal = new Money(basket.SubTotal.GetValueOrDefault(), basket.BillingCurrency).ToString();
            basketVM.NextStepUrl = GetNextStepUrl(nextStepId);
            basketVM.RedirectUrl = GetRedirectUrl(redirectPageId);
            basketVM.RefreshUrl = refreshUrl;
            basketVM.RemoveOrderlineUrl = removeOrderLineUrl;

            return basketVM;
        }

        internal static IList<OrderlineViewModel> GetOrlerLineList(PurchaseOrder basket, Guid productDetailsPageId)
        {
            var result = new List<OrderlineViewModel>();
            foreach (var orderLine in basket.OrderLines)
            {
                var product = CatalogLibrary.GetProduct(orderLine.Sku);
                var imageService = UCommerce.Infrastructure.ObjectFactory.Instance.Resolve<IImageService>();
                var orderLineViewModel = new OrderlineViewModel
                {
                    Quantity = orderLine.Quantity,
                    ProductName = product.DisplayName(),
                    Sku = orderLine.Sku,
                    VariantSku = orderLine.VariantSku,
                    Total = new Money(orderLine.Total.GetValueOrDefault(), basket.BillingCurrency).ToString(),
                    Discount = orderLine.Discount,
                    Tax = new Money(orderLine.VAT, basket.BillingCurrency).ToString(),
                    Price = new Money(orderLine.Price, basket.BillingCurrency).ToString(),
                    ProductUrl = GetProductUrl(CatalogLibrary.GetProduct(orderLine.Sku), productDetailsPageId),
                    PriceWithDiscount = new Money(orderLine.Price - orderLine.UnitDiscount.GetValueOrDefault(),
                        basket.BillingCurrency).ToString(),
                    OrderLineId = orderLine.OrderLineId,
                    ThumbnailName = imageService.GetImage(product.ThumbnailImageMediaId).Name,
                    ThumbnailUrl = imageService.GetImage(product.ThumbnailImageMediaId).Url
                };
                result.Add(orderLineViewModel);
            }
            return result;
        }

        private void GetDiscounts(CartRenderingViewModel basketVM, PurchaseOrder basket)
        {
            foreach (var item in basket.Discounts)
            {
                if (!string.IsNullOrWhiteSpace(item.Description))
                {
                    if (item.Description.Contains(","))
                    {
                        basketVM.Discounts = item.Description.Split(',').ToList();
                    }
                    else
                    {
                        basketVM.Discounts.Add(item.Description);
                    }
                }
            }
        }

        public virtual bool CanProcessRequest(Dictionary<string, object> parameters, out string message)
        {
            if (Telerik.Sitefinity.Services.SystemManager.IsDesignMode)
            {
                message = "The widget is in Page Edit mode.";
                return false;
            }

            object submitModel = null;

            if (parameters.TryGetValue("submitModel", out submitModel))
            {
                var updateModel = submitModel as CartUpdateBasket;

                if (updateModel != null)
                {
                    foreach (var item in updateModel.RefreshBasket)
                    {
                        if (item.OrderLineQty < 1)
                        {
                            message = string.Format("Quantity of {0} must be greater than 0", item.OrderLineId);
                            return false;
                        }
                    }
                }
            }

            message = null;
            return true;
        }

        public virtual CartUpdateBasketViewModel Update(CartUpdateBasket model)
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

            var updatedBasket = MapCartUpdate(model);

            return updatedBasket;
        }

        public virtual CartUpdateBasketViewModel RemoveVoucher(CartUpdateBasket model)
        {
            var basket = _transactionLibraryInternal.GetBasket(false).PurchaseOrder;
            var prop = basket.OrderProperties.FirstOrDefault(v => v.Key == "voucherCodes");
            var vouchers = model.Vouchers;

            if (vouchers.Any())
            {
                foreach (var voucher in vouchers)
                {
                    if (prop != null)
                    {
                        prop.Value = prop.Value.Replace(voucher + ",", string.Empty);
                        prop.Save();
                    }
                }
            }

            basket.Save();
            _transactionLibraryInternal.ExecuteBasketPipeline();

            var updatedBasket = MapCartUpdate(model);
            updatedBasket.Vouchers.Except(vouchers).ToList();

            return updatedBasket;
        }

        public virtual CartUpdateBasketViewModel AddVoucher(CartUpdateBasket model)
        {
            if (model.Vouchers.Any())
            {
                foreach (var modelVoucher in model.Vouchers)
                {
                    MarketingLibrary.AddVoucher(modelVoucher);
                }
            }

            _transactionLibraryInternal.ExecuteBasketPipeline();
            var updatedBasket = MapCartUpdate(model);
            updatedBasket.Vouchers = model.Vouchers;

            return updatedBasket;
        }

        private static CartUpdateBasketViewModel MapOrderline(PurchaseOrder basket)
        {
            var updatedBasket = new CartUpdateBasketViewModel();

            foreach (var orderLine in basket.OrderLines)
            {
                var orderLineViewModel = new CartUpdateOrderline();
                orderLineViewModel.OrderlineId = orderLine.OrderLineId;
                orderLineViewModel.Quantity = orderLine.Quantity;
                orderLineViewModel.Total =
                    new Money(orderLine.Total.GetValueOrDefault(), basket.BillingCurrency).ToString();
                orderLineViewModel.Discount = orderLine.Discount;
                orderLineViewModel.Tax = new Money(orderLine.VAT, basket.BillingCurrency).ToString();
                orderLineViewModel.Price = new Money(orderLine.Price, basket.BillingCurrency).ToString();
                orderLineViewModel.PriceWithDiscount =
                    new Money(orderLine.Price - orderLine.Discount, basket.BillingCurrency).ToString();

                updatedBasket.OrderLines.Add(orderLineViewModel);
            }

            return updatedBasket;
        }

        private CartUpdateBasketViewModel MapCartUpdate(CartUpdateBasket model)
        {
            var basket = _transactionLibraryInternal.GetBasket(false).PurchaseOrder;
            var updatedBasket = MapOrderline(basket);

            string orderTotal = new Money(basket.OrderTotal.GetValueOrDefault(), basket.BillingCurrency).ToString();
            string discountTotal = basket.DiscountTotal.GetValueOrDefault() > 0
                ? new Money(basket.DiscountTotal.GetValueOrDefault(), basket.BillingCurrency).ToString()
                : "";
            string taxTotal = new Money(basket.TaxTotal.GetValueOrDefault(), basket.BillingCurrency).ToString();
            string subTotal = new Money(basket.SubTotal.GetValueOrDefault(), basket.BillingCurrency).ToString();

            updatedBasket.OrderTotal = orderTotal;
            updatedBasket.DiscountTotal = discountTotal;
            updatedBasket.TaxTotal = taxTotal;
            updatedBasket.SubTotal = subTotal;
            updatedBasket.Vouchers.AddRange(model.Vouchers);

            return updatedBasket;
        }

        private string GetNextStepUrl(Guid nextStepId)
        {
            var nextStepUrl = Pages.UrlResolver.GetPageNodeUrl(nextStepId);

            return Pages.UrlResolver.GetAbsoluteUrl(nextStepUrl);
        }

        private string GetRedirectUrl(Guid redirectPageId)
        {
            var redirectUrl = Pages.UrlResolver.GetPageNodeUrl(redirectPageId);

            return Pages.UrlResolver.GetAbsoluteUrl(redirectUrl);
        }

        private static string GetProductUrl(Product product, Guid detailPageId)
        {
            if (detailPageId == Guid.Empty)
            {
                return CatalogLibrary.GetNiceUrlForProduct(product);
            }

            var baseUrl = UCommerce.Sitefinity.UI.Pages.UrlResolver.GetPageNodeUrl(detailPageId);

            string catUrl;
            var productCategory = product.GetCategories().FirstOrDefault();
            if (productCategory == null)
            {
                catUrl = CategoryModel.DefaultCategoryName;
            }
            else
            {
                catUrl = CategoryModel.GetCategoryPath(productCategory);
            }

            var rawtUrl = string.Format("{0}/{1}", catUrl, product.ProductId);
            string relativeUrl = string.Concat(VirtualPathUtility.RemoveTrailingSlash(baseUrl), "/", rawtUrl);

            string url;

            if (SystemManager.CurrentHttpContext.Request.Url != null)
            {
                url = UrlPath.ResolveUrl(relativeUrl, true);
            }
            else
            {
                url = UCommerce.Sitefinity.UI.Pages.UrlResolver.GetAbsoluteUrl(relativeUrl);
            }

            return url;
        }
    }
}