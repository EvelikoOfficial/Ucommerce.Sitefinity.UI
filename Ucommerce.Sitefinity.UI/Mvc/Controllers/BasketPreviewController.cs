﻿using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Telerik.Sitefinity.Mvc;
using Telerik.Sitefinity.Personalization;
using Telerik.Sitefinity.Services;
using Ucommerce.Sitefinity.UI.Mvc.Model.Interfaces;
using Ucommerce.Sitefinity.UI.Mvc.ViewModels;
using UCommerce.Infrastructure;
using UCommerce.Transactions;

namespace Ucommerce.Sitefinity.UI.Mvc.Controllers
{
    [ControllerToolboxItem(Name = "uBasketPreview_MVC", Title = "Basket Preview", SectionName = UcommerceUIModule.UCOMMERCE_WIDGET_SECTION, ModuleName = UcommerceUIModule.NAME, CssClass = "sfMvcIcn")]
    public class BasketPreviewController : Controller, IPersonalizable
    {
        public Guid? NextStepId { get; set; }
        public Guid? PreviousStepId { get; set; }
        public string TemplateName { get; set; } = "Index";

        private readonly TransactionLibraryInternal _transactionLibraryInternal;

        public BasketPreviewController()
        {
            _transactionLibraryInternal = ObjectFactory.Instance.Resolve<TransactionLibraryInternal>();
        }

        public ActionResult Index()
        {
            if (SystemManager.IsDesignMode)
            {
                return new EmptyResult();
            }

            var model = ResolveModel();
            var purchaseOrder = _transactionLibraryInternal.GetBasket(false).PurchaseOrder;

            var basketPreviewViewModel = model.GetViewModelr(purchaseOrder);

            ViewBag.RowSpan = 4;
            if (purchaseOrder.DiscountTotal > 0)
            {
                ViewBag.RowSpan++;
            }
            if (purchaseOrder.ShippingTotal > 0)
            {
                ViewBag.RowSpan++;
            }
            if (purchaseOrder.PaymentTotal > 0)
            {
                ViewBag.RowSpan++;
            }

            return View(TemplateName, basketPreviewViewModel);
        }

        [HttpPost]
        public ActionResult RequestPayment()
        {
            var payment = _transactionLibraryInternal.GetBasket().PurchaseOrder.Payments.First();
            if (payment.PaymentMethod.PaymentMethodServiceName == null)
            {
                return Redirect("/confirmation");
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string paymentUrl = _transactionLibraryInternal.GetPaymentPageUrl(payment);
            return Redirect(paymentUrl);
        }

        protected override void HandleUnknownAction(string actionName)
        {
            this.ActionInvoker.InvokeAction(this.ControllerContext, "Index");
        }

        private IBasketPreviewModel ResolveModel()
        {
            var container = UcommerceUIModule.Container;
            var model = container.Resolve<IBasketPreviewModel>(new
            {
                nextStepId = this.NextStepId,
                previousStepId = this.PreviousStepId
            });

            return model;
        }
    }
}
