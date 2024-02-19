﻿using Iyzipay;
using Nop.Core.Domain.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.Iyzico
{
    public class IyzicoHelper
    {
        #region Methods

        /// <summary>
        /// Gets a payment status
        /// </summary>
        /// <param name="Status">Iyzico payment status</param>
        /// <param name="errorMessage">Iyzico pending reason</param>
        /// <returns>Payment status</returns>
        public static PaymentStatus GetPaymentStatus(string paymentStatus)
        {
            var result = PaymentStatus.Pending;

            
                switch (paymentStatus.ToLowerInvariant())
                {
                    case "success":
                        result = PaymentStatus.Paid;
                        break;
                    default:
                        result = PaymentStatus.Refunded;
                        break;
                }
            
            

            return result;
        }

        public static PaymentStatus GetPaymentStatus(int paymentStatus)
        {
            var result = PaymentStatus.Pending;


            if (paymentStatus == 0)
            {

                result = PaymentStatus.Voided;
            }
            else if(paymentStatus == 1)
            {
                result = PaymentStatus.Paid;
            }
            return result;
        }
        public static Options GetOptions(IyzicoPaymentSettings iyzicoPaymentSettings)
        {
            var options = new Options
            {
                ApiKey = iyzicoPaymentSettings.ApiKey,
                SecretKey = iyzicoPaymentSettings.ApiSecret,
                BaseUrl = iyzicoPaymentSettings.ApiUrl
            };

            return options;
        }

        public static decimal ToDecimalInvariant(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value) == false)
                {
                    return Math.Round(decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture), 2);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return 0;
        }

        public static string ToDecimalStringInvariant(decimal value)
        {
            return Convert.ToString(Math.Round(value, 2), System.Globalization.CultureInfo.InvariantCulture);
        }

        public static decimal ToDecimalInvariant(decimal value)
        {
            return Math.Round(value, 2);
        }

        public static string ToIyzicoDateFormat(DateTime? value)
        {
            if (value.HasValue)
            {
                return Convert.ToDateTime(value).ToString("yyyy-MM-dd HH:mm:ss");
            }

            return string.Empty;
        }

        #endregion
    }
}