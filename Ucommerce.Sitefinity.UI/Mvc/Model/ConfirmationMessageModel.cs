﻿using Ucommerce.Sitefinity.UI.Mvc.Model.Interfaces;
using Ucommerce.Sitefinity.UI.Mvc.ViewModels;

namespace Ucommerce.Sitefinity.UI.Mvc.Model
{
    public class ConfirmationMessageModel : IConfirmationMessageModel
    {
        public ConfirmationMessageViewModel GetViewModel(string headline, string message)
        {
            var viewModel = new ConfirmationMessageViewModel()
            {
                Headline = headline,
                Message = message
            };

            return viewModel;
        }
    }
}
