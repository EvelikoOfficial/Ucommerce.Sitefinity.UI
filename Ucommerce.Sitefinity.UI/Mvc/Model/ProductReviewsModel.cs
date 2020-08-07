﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ucommerce.EntitiesV2;
using Ucommerce.Api;
using Ucommerce.Catalog.Status;
using UCommerce.Sitefinity.UI.Mvc.Model.Contracts;
using UCommerce.Sitefinity.UI.Mvc.ViewModels;
using Ucommerce.Infrastructure;

namespace UCommerce.Sitefinity.UI.Mvc.Model
{
    public class ProductReviewsModel : IReviewsModel
    {
        public ICatalogContext CatalogContext => ObjectFactory.Instance.Resolve<ICatalogContext>();

        public bool CanProcessRequest(Dictionary<string, object> parameters, out string message)
        {
            if (Telerik.Sitefinity.Services.SystemManager.IsDesignMode)
            {
                message = "The widget is in Page Edit mode.";
                return false;
            }

            message = null;
            return true;
        }

        public virtual ProductReviewsRenderingViewModel GetReviews(int? productId)
        {
            var reviewVm = new ProductReviewsRenderingViewModel();
            var clientIp = System.Web.HttpContext.Current.Request.UserHostName;
            Product currentProduct;

            if (productId.HasValue && productId >= 0)
            {
                currentProduct = Product.Get(productId.Value);
            }
            else
            {
                currentProduct = Product.FirstOrDefault(x => x.Guid == CatalogContext.CurrentProduct.Guid);
            }
            

            reviewVm.Reviews = currentProduct.ProductReviews.Where(pr => pr.ProductReviewStatus.ProductReviewStatusId == (int)Ucommerce.Catalog.Status.ProductReviewStatusCode.Approved
                                                                         && (pr.CultureCode == null || pr.CultureCode == string.Empty || pr.CultureCode == Thread.CurrentThread.CurrentUICulture.Name))
	            .OrderByDescending(pr => pr.CreatedOn)
                .Select(review => new ViewModels.ProductReview
            {
                Name = review.Customer == null ? "(anonymous)" : review.Customer.FirstName + " " + review.Customer.LastName,
                Email = review.Customer?.EmailAddress,
                Title = review.ReviewHeadline,
                CreatedOn = review.CreatedOn,
                Comments = review.ReviewText,
                Ip = review.Ip,
                Rating = review.Rating
            }).ToList();
            reviewVm.Ip = clientIp;

            return reviewVm;
        }
    }
}