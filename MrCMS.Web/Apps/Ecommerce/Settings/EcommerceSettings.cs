﻿using System.Web.Mvc;
using MrCMS.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MrCMS.Helpers;
using MrCMS.Website;
using MrCMS.Website.Caching;
using NHibernate;
using MrCMS.Web.Apps.Ecommerce.Entities.Currencies;

namespace MrCMS.Web.Apps.Ecommerce.Settings
{
    public class EcommerceSettings : SiteSettingsBase
    {
        [DisplayName("Search Products per Page")]
        public string SearchProductsPerPage { get; set; }
        [DisplayName("Previous Price text")]
        public string PreviousPriceText { get; set; }

        [DisplayName("Product Image not specified replacement image")]
        public string DefaultNoProductImage { get; set; }

        [DisplayName("PDF Invoice Logo Image")]
        public string ReportLogoImage { get; set; }

        [DisplayName("PDF Invoice Footer Text")]
        public string ReportFooterText { get; set; }

        [DisplayName("Enable Wish Lists")]
        public bool EnableWishlists { get; set; }

        public IEnumerable<int> ProductPerPageOptions
        {
            get
            {
                return SearchProductsPerPage.Split(',').Where(s =>
                                                                  {
                                                                      int result;
                                                                      return int.TryParse(s, out result);
                                                                  }).Select(s => Convert.ToInt32(s));
            }
        }
        public IEnumerable<SelectListItem> ProductPerPageOptionItems
        {
            get
            {
                return ProductPerPageOptions.BuildSelectItemList(i => string.Format("{0} products per page", i), i => i.ToString(),
                                                   emptyItem: null);

            }
        }

        public override bool RenderInSettings
        {
            get { return false; }
        }

        [DisplayName("Site Currency")]
        public int CurrencyId { get; set; }

        public Currency Currency
        {
            get
            {
                var session = MrCMSApplication.Get<ISession>();
                return CurrencyId > 0 ? session.Get<Currency>(CurrencyId) : session.QueryOver<Currency>().Take(1).SingleOrDefault();
            }
        }

        public string CurrencyCode
        {
            get { return Currency != null ? Currency.Code : null; }
        }
    }
}