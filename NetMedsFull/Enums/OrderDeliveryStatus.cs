﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetMedsFull.Enums
{
    public enum OrderDeliveryStatus
    {

        OnProcessing = 1,
        OnDepot = 2,
        OnCourier = 3,
        Delivered = 4,
        OnWaiting = 5,
    }
}
