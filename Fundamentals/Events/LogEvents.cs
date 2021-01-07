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
        public static EventId CustomOrderError = new EventId(3, "Error in CustomOrder");
        public static EventId CustomOrderDetailError = new EventId(4, "Error in CustomOrderDetails");
        public static EventId PreCheckoutCallBack = new EventId(5, "PreCheckout With Callback");

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

        //SMS Send
        public static EventId SendSMSSuccess = new EventId(60001, "SMS Sent");
        public static EventId SendSMSFailure = new EventId(60002, "SMS Sending Failed");

        public static EventId ConsultantListError = new EventId(60001, "MyCases Error");

        public static EventId ChangeCaseManager = new EventId(70001, "CM changed Success");
        public static EventId ChangeCaseManagerFail = new EventId(70002, "CM changed Fail");

    }
}
