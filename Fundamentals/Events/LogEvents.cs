using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fundamentals.Events
{
    public class LogEvents
    {
        public static EventId PreCheckoutError = new EventId(1, "Error in PreCheckout");
        public static EventId CheckoutError = new EventId(2, "Error in Checkout");

        public static EventId ErrorGetPhone = new EventId(10001, "Extract Phone Number");

        //User Event Ids
        public static EventId NewUserCreated = new EventId(20001, "User Created");
        public static EventId PasswordSignInSuccess = new EventId(20001, "Sign In");


        //Payment Event Ids
        public static EventId SaveClientPayment = new EventId(30001, "Payment Save");
        public static EventId VerificationWithGateWay = new EventId(30002, "Payment Verification From GateWay");
        public static EventId PaymentSavedInOrder = new EventId(30003, "Payment Saved In Order");
        public static EventId OrderSavedInPayment = new EventId(30004, "Order Info Saved In Payment");

        //Case Event Ids
        public static EventId CreateCase = new EventId(40001, "Case Generated");
        public static EventId AddCaseToOrder = new EventId(50001, "Case added to an Order");

    }
}
