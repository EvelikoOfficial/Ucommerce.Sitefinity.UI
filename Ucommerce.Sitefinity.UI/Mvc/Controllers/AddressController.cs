﻿using System;
using System.Web.Mvc;
using Telerik.Sitefinity.Mvc;
using Telerik.Sitefinity.Personalization;
using Telerik.Sitefinity.Services;
using Ucommerce.Sitefinity.UI.Mvc.Model.Interfaces;
using Ucommerce.Sitefinity.UI.Mvc.ViewModels;

namespace Ucommerce.Sitefinity.UI.Mvc.Controllers
{
    [ControllerToolboxItem(Name = "uAddressInformation_MVC", Title = "Address Information", SectionName = UcommerceUIModule.UCOMMERCE_WIDGET_SECTION, ModuleName = UcommerceUIModule.NAME, CssClass = "ucIcnAddressInformation sfMvcIcn")]
    public class AddressController : Controller, IPersonalizable
    {
        public Guid? NextStepId { get; set; }
        public Guid? PreviousStepId { get; set; }
        public string TemplateName { get; set; } = "Index";

        public ActionResult Index()
        {
            if (SystemManager.IsDesignMode)
            {
                return this.PartialView("_DesignMode");
            }

            var model = ResolveModel();
            var viewModel = model.GetViewModel();

            return View(TemplateName, viewModel);
        }

        [HttpPost]
        public ActionResult Save(AddressSaveViewModel addressRendering)
        {
            var model = ResolveModel();
            var viewModel = model.GetViewModel();
            model.Validate(addressRendering, ModelState);

            if (ModelState.IsValid)
            {
                model.Save(addressRendering);
                if (viewModel.NextStepUrl?.Length == 0)
                {
                    return View(TemplateName, viewModel);
                }
                else
                {
                    return Redirect(viewModel.NextStepUrl);
                }
            }
            return View(TemplateName, viewModel);
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
