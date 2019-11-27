﻿using System.Collections.Generic;
using Ucommerce.Sitefinity.UI.Mvc.ViewModels;

namespace Ucommerce.Sitefinity.UI.Mvc.Model.Interfaces
{
    public interface IConfirmationEmailModel
    {
        bool CanProcessRequest(Dictionary<string, object> parameters, out string message);
        ConfirmationEmailViewModel GetViewModel(string orderGuid);
    }
}
