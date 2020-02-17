﻿using System;
using System.Web.Mvc;
using Telerik.Sitefinity.Mvc;
using Telerik.Sitefinity.Personalization;
using UCommerce.Infrastructure;
using UCommerce.Sitefinity.UI.Api.Model;
using UCommerce.Sitefinity.UI.Mvc.Model;
using UCommerce.Transactions;

namespace UCommerce.Sitefinity.UI.Mvc.Controllers
{
    /// <summary>
    /// The controller class for the Mini Basket MVC widget.
    /// </summary>
    [ControllerToolboxItem(Name = "uMiniBasket_MVC", Title = "Mini Basket", SectionName = UCommerceUIModule.UCOMMERCE_WIDGET_SECTION, ModuleName = UCommerceUIModule.NAME, CssClass = "ucIcnMiniBasket sfMvcIcn")]
    public class MiniBasketController : Controller, IPersonalizable
    {
        public Guid? CartPageId { get; set; }
        public string TemplateName { get; set; } = "Index";
        private readonly TransactionLibraryInternal _transactionLibraryInternal;

        public MiniBasketController()
        {
            _transactionLibraryInternal = ObjectFactory.Instance.Resolve<TransactionLibraryInternal>();
        }

        public ActionResult Index()
        {
            var miniBasketModel = ResolveModel();
            string message;

            var parameters = new System.Collections.Generic.Dictionary<string, object>();

            if (!miniBasketModel.CanProcessRequest(parameters, out message))
            {
                return this.PartialView("_Warning", message);
            }

            var miniBasketRenderingViewModel = miniBasketModel.CreateViewModel(Url.Action("Refresh"));
            var detailTemplateName = this.detailTemplateNamePrefix + this.TemplateName;

            if (!_transactionLibraryInternal.HasBasket())
            {
                return View(detailTemplateName, miniBasketRenderingViewModel);
            }

            return View(detailTemplateName, miniBasketRenderingViewModel);
        }

        [HttpGet]
        [Route("uc/checkout/mini-basket")]
        public ActionResult Refresh()
        {
            var model = ResolveModel();
            var viewModel = model.Refresh();
            var parameters = new System.Collections.Generic.Dictionary<string, object>();

            if (!model.CanProcessRequest(parameters, out var message))
            {
                return this.Json(new OperationStatusDTO { Status = "failed", Message = message }, JsonRequestBehavior.AllowGet);
            }

            var responseDTO = new OperationStatusDTO();
            responseDTO.Status = "success";
            responseDTO.Data.Add("data", viewModel);

            return Json(responseDTO, JsonRequestBehavior.AllowGet);
        }

        protected override void HandleUnknownAction(string actionName)
        {
            this.ActionInvoker.InvokeAction(this.ControllerContext, "Index");
        }

        private IMiniBasketModel ResolveModel()
        {
            var container = UCommerceUIModule.Container;
            var model = container.Resolve<IMiniBasketModel>(
                new
                {
                    cartPageId = this.CartPageId
                });

            return model;
        }

        private string detailTemplateNamePrefix = "Detail.";
    }
}
