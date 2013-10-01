﻿using MrCMS.Web.Apps.Amazon.Entities.Orders;
using MrCMS.Web.Apps.Amazon.Models;

namespace MrCMS.Web.Apps.Amazon.Services.Orders.Sync
{
    public interface IShipAmazonOrderService
    {
        void MarkAsShipped(AmazonSyncModel model, AmazonOrder item);
    }
}