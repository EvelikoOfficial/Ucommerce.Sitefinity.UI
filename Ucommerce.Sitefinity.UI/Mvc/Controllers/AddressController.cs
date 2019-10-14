﻿using System;
using System.Web.Mvc;
using Telerik.Sitefinity.Mvc;
using Ucommerce.Sitefinity.UI.Mvc.Model.Interfaces;
using Ucommerce.Sitefinity.UI.Mvc.ViewModels;

namespace Ucommerce.Sitefinity.UI.Mvc.Controllers
{
    [ControllerToolboxItem(Name = "uAddressInformation_MVC", Title = "Address Information", SectionName = UcommerceUIModule.UCOMMERCE_WIDGET_SECTION, ModuleName = UcommerceUIModule.NAME, CssClass = "sfMvcIcn")]
    public class AddressController : Controller
    {
        public Guid? NextStepId { get; set; }
        public Guid? PreviousStepId { get; set; }
        public string TemplteName { get; set; } = "Index";

        public ActionResult Index()
        {
            var model = ResolveModel();
            var viewModel = model.GetViewModel();

            return View(TemplteName, viewModel);
        }

        [HttpPost]
        public ActionResult Save(AddressSaveViewModel addressRendering)
        {
            var model = ResolveModel();
            var viewModel = model.GetViewModel();

            if (!addressRendering.IsShippingAddressDifferent)
            {
                ModelState.Remove("ShippingAddress.FirstName");
                ModelState.Remove("ShippingAddress.LastName");
                ModelState.Remove("ShippingAddress.EmailAddress");
                ModelState.Remove("ShippingAddress.Line1");
                ModelState.Remove("ShippingAddress.PostalCode");
                ModelState.Remove("ShippingAddress.City");
            }
            if (ModelState.IsValid)
            {
                model.Save(addressRendering);
                if (viewModel.NextStepUrl?.Length == 0)
                {
                    return View(TemplteName, viewModel);
                }
                else
                {
                    return Redirect(viewModel.NextStepUrl);
                }
            }
            return View(TemplteName, viewModel);
        }

        protected override void HandleUnknownAction(string actionName)
        {
            this.ActionInvoker.InvokeAction(this.ControllerContext, "Index");
        }

        private IAddressModel ResolveModel()
        {
            var container = UcommerceUIModule.Container;
            var model = container.Resolve<IAddressModel>(
                new
                {
                    nextStepId = this.NextStepId,
                    previousStepId = this.PreviousStepId
                });

            return model;
        }
    }
}
