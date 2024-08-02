﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Common.Interfaces
{
    public interface IBookingRepository: IRepository<Booking>
    {
        void Update(Booking entity);

        void UpdateStatus(int bookingId, string bookingStatus, int villanumber);
        void UpdateStripePaymentID(int bookingId, string sessionId, string paymentIntentId);

    }
}
