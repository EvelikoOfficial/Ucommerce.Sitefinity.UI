﻿using System.Collections.Generic;
using UCommerce.Sitefinity.UI.Mvc.Model.Interfaces;
using UCommerce.Sitefinity.UI.Mvc.ViewModels;

namespace UCommerce.Sitefinity.UI.Mvc.Model
{
    public class ConfirmationMessageModel : IConfirmationMessageModel
    {
        public virtual ConfirmationMessageViewModel GetViewModel(string headline, string message)
        {
            var viewModel = new ConfirmationMessageViewModel()
            {
                Headline = headline,
                Message = message
            };

            return viewModel;
        }

        public virtual bool CanProcessRequest(Dictionary<string, object> parameters, out string message)
        {
            if (Telerik.Sitefinity.Services.SystemManager.IsDesignMode)
            {
                message = "The widget is in Page Edit mode.";
                return false;
            }

            message = null;
            return true;
        }
    }
}
