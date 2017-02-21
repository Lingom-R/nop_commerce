using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;

namespace Nop.Core.Domain.Orders
{
    /// <summary>
    /// Represents an order
    /// </summary>
    public partial class Order : BaseEntity
    {

        private ICollection<DiscountUsageHistory> _discountUsageHistory;
        private ICollection<GiftCardUsageHistory> _giftCardUsageHistory;
        private ICollection<OrderNote> _orderNotes;
        private ICollection<OrderItem> _orderItems;
        private ICollection<Shipment> _shipments;

        #region Utilities

        /// <summary>
        /// Parses order.TaxRates string
        /// </summary>
        /// <param name="taxRatesStr"></param>
        /// <returns>Returns the sorted dictionary of taxrateEntries</returns>
        protected virtual SortedDictionary<decimal, TaxRateRec> ParseTaxRates(string taxRatesStr)
        {
            var taxRatesDictionary = new SortedDictionary<decimal, TaxRateRec>();
            if (String.IsNullOrEmpty(taxRatesStr))
                return taxRatesDictionary;

            string[] lines = taxRatesStr.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (String.IsNullOrEmpty(line.Trim()))
                    continue;

                string[] taxes = line.Split(new[] { ':' });
                if (taxes.Length == 6)
                {
                    try
                    {
                        decimal rate = decimal.Parse(taxes[0].Trim(), CultureInfo.InvariantCulture);
                        taxRatesDictionary.Add(rate, new TaxRateRec()
                        {
                            TaxRate = rate,
                            Amount = decimal.Parse(taxes[1].Trim(), CultureInfo.InvariantCulture),
                            DiscountAmount = decimal.Parse(taxes[2].Trim(), CultureInfo.InvariantCulture),
                            BaseAmount = decimal.Parse(taxes[3].Trim(), CultureInfo.InvariantCulture),
                            TaxAmount = decimal.Parse(taxes[4].Trim(), CultureInfo.InvariantCulture),
                            AmountIncludingTax = decimal.Parse(taxes[5].Trim(), CultureInfo.InvariantCulture)
                        });
                    }
                    catch (Exception exc)
                    {
                        Debug.WriteLine(exc.ToString());
                    }
                }
            }

            //add at least one tax rate (0%)
            if (!taxRatesDictionary.Any())
                taxRatesDictionary.Add(decimal.Zero, new TaxRateRec()
                {
                    TaxRate = decimal.Zero,
                    Amount = decimal.Zero,
                    DiscountAmount = decimal.Zero,
                    BaseAmount = decimal.Zero,
                    TaxAmount = decimal.Zero,
                    AmountIncludingTax = decimal.Zero
                });

            return taxRatesDictionary;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the order identifier
        /// </summary>
        public Guid OrderGuid { get; set; }

        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the billing address identifier
        /// </summary>
        public int BillingAddressId { get; set; }

        /// <summary>
        /// Gets or sets the shipping address identifier
        /// </summary>
        public int? ShippingAddressId { get; set; }

        /// <summary>
        /// Gets or sets the pickup address identifier
        /// </summary>
        public int? PickupAddressId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a customer chose "pick up in store" shipping option
        /// </summary>
        public bool PickUpInStore { get; set; }

        /// <summary>
        /// Gets or sets an order status identifier
        /// </summary>
        public int OrderStatusId { get; set; }

        /// <summary>
        /// Gets or sets the shipping status identifier
        /// </summary>
        public int ShippingStatusId { get; set; }

        /// <summary>
        /// Gets or sets the payment status identifier
        /// </summary>
        public int PaymentStatusId { get; set; }

        /// <summary>
        /// Gets or sets the payment method system name
        /// </summary>
        public string PaymentMethodSystemName { get; set; }

        /// <summary>
        /// Gets or sets the customer currency code (at the moment of order placing)
        /// </summary>
        public string CustomerCurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the currency rate
        /// </summary>
        public decimal CurrencyRate { get; set; }

        /// <summary>
        /// Gets or sets the customer tax display type identifier
        /// </summary>
        public int CustomerTaxDisplayTypeId { get; set; }

        /// <summary>
        /// Gets or sets the VAT number (the European Union Value Added Tax)
        /// </summary>
        public string VatNumber { get; set; }

        /// <summary>
        /// Gets or sets the order subtotal (incl tax)
        /// </summary>
        public decimal OrderSubtotalInclTax { get; set; }

        /// <summary>
        /// Gets or sets the order subtotal (excl tax)
        /// </summary>
        public decimal OrderSubtotalExclTax { get; set; }

        /// <summary>
        /// Gets or sets the order subtotal discount (incl tax)
        /// </summary>
        public decimal OrderSubTotalDiscountInclTax { get; set; }

        /// <summary>
        /// Gets or sets the order subtotal discount (excl tax)
        /// </summary>
        public decimal OrderSubTotalDiscountExclTax { get; set; }

        /// <summary>
        /// Gets or sets the order shipping (incl tax)
        /// </summary>
        /// 
        public decimal OrderShippingInclTax { get; set; }
        /// <summary>
        /// Gets or sets the order shipping (excl tax)
        /// </summary>
        /// 
        public decimal OrderShippingExclTax { get; set; }

        /// Gets or sets the order shipping (non taxable)
        /// </summary>
        public decimal OrderShippingNonTaxable { get; set; }
        
        /// <summary>
        /// Gets or sets the payment method additional fee (incl tax)
        /// </summary>
        public decimal PaymentMethodAdditionalFeeInclTax { get; set; }

        /// <summary>
        /// Gets or sets the payment method additional fee (excl tax)
        /// </summary>
        public decimal PaymentMethodAdditionalFeeExclTax { get; set; }

        /// <summary>
        /// Gets or sets the payment method additional fee (non taxable)
        /// </summary>
        public decimal PaymentMethodAdditionalFeeNonTaxable { get; set; }
        /// <summary>
        /// Gets or sets the tax rates
        /// </summary>
        public string TaxRates { get; set; }

        /// <summary>
        /// Gets or sets the order tax
        /// </summary>
        public decimal OrderTax { get; set; }

        /// <summary>
        /// Gets or sets the order discount (applied to order total)
        /// </summary>
        public decimal OrderDiscount { get; set; }

        /// <summary>
        /// Gets or sets the order total to pay
        /// </summary>
        public decimal OrderTotal { get; set; }

        /// <summary>
        /// Gets or sets the refunded amount
        /// </summary>
        public decimal RefundedAmount { get; set; }

        /// <summary>
        /// Gets or sets the reward points history entry identifier when reward points were earned (gained) for placing this order
        /// </summary>
        public int? RewardPointsHistoryEntryId { get; set; }

        /// <summary>
        /// Gets or sets the checkout attribute description
        /// </summary>
        public string CheckoutAttributeDescription { get; set; }

        /// <summary>
        /// Gets or sets the checkout attributes in XML format
        /// </summary>
        public string CheckoutAttributesXml { get; set; }

        /// <summary>
        /// Gets or sets the customer language identifier
        /// </summary>
        public int CustomerLanguageId { get; set; }

        /// <summary>
        /// Gets or sets the affiliate identifier
        /// </summary>
        public int AffiliateId { get; set; }

        /// <summary>
        /// Gets or sets the customer IP address
        /// </summary>
        public string CustomerIp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether storing of credit card number is allowed
        /// </summary>
        public bool AllowStoringCreditCardNumber { get; set; }

        /// <summary>
        /// Gets or sets the card type
        /// </summary>
        public string CardType { get; set; }

        /// <summary>
        /// Gets or sets the card name
        /// </summary>
        public string CardName { get; set; }

        /// <summary>
        /// Gets or sets the card number
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// Gets or sets the masked credit card number
        /// </summary>
        public string MaskedCreditCardNumber { get; set; }

        /// <summary>
        /// Gets or sets the card CVV2
        /// </summary>
        public string CardCvv2 { get; set; }

        /// <summary>
        /// Gets or sets the card expiration month
        /// </summary>
        public string CardExpirationMonth { get; set; }

        /// <summary>
        /// Gets or sets the card expiration year
        /// </summary>
        public string CardExpirationYear { get; set; }

        /// <summary>
        /// Gets or sets the authorization transaction identifier
        /// </summary>
        public string AuthorizationTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the authorization transaction code
        /// </summary>
        public string AuthorizationTransactionCode { get; set; }

        /// <summary>
        /// Gets or sets the authorization transaction result
        /// </summary>
        public string AuthorizationTransactionResult { get; set; }

        /// <summary>
        /// Gets or sets the capture transaction identifier
        /// </summary>
        public string CaptureTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the capture transaction result
        /// </summary>
        public string CaptureTransactionResult { get; set; }

        /// <summary>
        /// Gets or sets the subscription transaction identifier
        /// </summary>
        public string SubscriptionTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the paid date and time
        /// </summary>
        public DateTime? PaidDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the shipping method
        /// </summary>
        public string ShippingMethod { get; set; }

        /// <summary>
        /// Gets or sets the shipping rate computation method identifier or the pickup point provider identifier (if PickUpInStore is true)
        /// </summary>
        public string ShippingRateComputationMethodSystemName { get; set; }

        /// <summary>
        /// Gets or sets the serialized CustomValues (values from ProcessPaymentRequest)
        /// </summary>
        public string CustomValuesXml { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity has been deleted
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the date and time of order creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the invoice ID
        /// </summary>
        public string InvoiceId { get; set; }
        /// <summary>
        /// Gets or sets the invoice date UTC
        /// </summary>
        public DateTime? InvoiceDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the order total base amount excl. tax
        /// </summary>
        public decimal OrderAmount { get; set; } //MF 09.12.16

        /// <summary>
        /// Gets or sets the order total amount incl. tax
        /// </summary>
        public decimal OrderAmountIncl { get; set; } //MF 09.12.16
        /// <summary>
        /// Gets or sets the order total discount amount incl. tax
        /// </summary>
        public decimal OrderDiscountIncl { get; set; }
        /// <summary>
        /// Gets or sets the order total earned reward points base amount incl. tax
        /// </summary>
        public decimal EarnedRewardPointsBaseAmountIncl { get; set; }
        /// <summary>
        /// Gets or sets the order total earned reward points base amount excl. tax
        /// </summary>
        public decimal EarnedRewardPointsBaseAmountExcl { get; set; }
        /// <summary>
        /// Gets or sets the custom order number without prefix
        /// </summary>
        public string CustomOrderNumber { get; set; }
        /// <summary>


        #endregion

        #region Navigation properties

        /// <summary>
        /// Gets or sets the customer
        /// </summary>
        public virtual Customer Customer { get; set; }

        /// <summary>
        /// Gets or sets the billing address
        /// </summary>
        public virtual Address BillingAddress { get; set; }

        /// <summary>
        /// Gets or sets the shipping address
        /// </summary>
        public virtual Address ShippingAddress { get; set; }

        /// <summary>
        /// Gets or sets the pickup address
        /// </summary>
        public virtual Address PickupAddress { get; set; }

        /// <summary>
        /// Gets or sets the reward points history record (spent by a customer when placing this order)
        /// </summary>
        public virtual RewardPointsHistory RedeemedRewardPointsEntry { get; set; }

        /// <summary>
        /// Gets or sets discount usage history
        /// </summary>
        public virtual ICollection<DiscountUsageHistory> DiscountUsageHistory
        {
            get { return _discountUsageHistory ?? (_discountUsageHistory = new List<DiscountUsageHistory>()); }
            protected set { _discountUsageHistory = value; }
        }

        /// <summary>
        /// Gets or sets gift card usage history (gift card that were used with this order)
        /// </summary>
        public virtual ICollection<GiftCardUsageHistory> GiftCardUsageHistory
        {
            get { return _giftCardUsageHistory ?? (_giftCardUsageHistory = new List<GiftCardUsageHistory>()); }
            protected set { _giftCardUsageHistory = value; }
        }

        /// <summary>
        /// Gets or sets order notes
        /// </summary>
        public virtual ICollection<OrderNote> OrderNotes
        {
            get { return _orderNotes ?? (_orderNotes = new List<OrderNote>()); }
            protected set { _orderNotes = value; }
        }

        /// <summary>
        /// Gets or sets order items
        /// </summary>
        public virtual ICollection<OrderItem> OrderItems
        {
            get { return _orderItems ?? (_orderItems = new List<OrderItem>()); }
            protected set { _orderItems = value; }
        }

        /// <summary>
        /// Gets or sets shipments
        /// </summary>
        public virtual ICollection<Shipment> Shipments
        {
            get { return _shipments ?? (_shipments = new List<Shipment>()); }
            protected set { _shipments = value; }
        }

        #endregion

        #region Custom properties

        /// <summary>
        /// Gets or sets the order status
        /// </summary>
        public OrderStatus OrderStatus
        {
            get
            {
                return (OrderStatus)this.OrderStatusId;
            }
            set
            {
                this.OrderStatusId = (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the payment status
        /// </summary>
        public PaymentStatus PaymentStatus
        {
            get
            {
                return (PaymentStatus)this.PaymentStatusId;
            }
            set
            {
                this.PaymentStatusId = (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the shipping status
        /// </summary>
        public ShippingStatus ShippingStatus
        {
            get
            {
                return (ShippingStatus)this.ShippingStatusId;
            }
            set
            {
                this.ShippingStatusId = (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the customer tax display type
        /// </summary>
        public TaxDisplayType CustomerTaxDisplayType
        {
            get
            {
                return (TaxDisplayType)this.CustomerTaxDisplayTypeId;
            }
            set
            {
                this.CustomerTaxDisplayTypeId = (int)value;
            }
        }

        /// <summary>
        /// Gets the applied tax rates
        /// </summary>
        public SortedDictionary<decimal, TaxRateRec> TaxRatesDictionary
        {
            get
            {
                return ParseTaxRates(this.TaxRates);
            }
        }

        #endregion
    }

    #region Nested classes
    public partial class TaxRateRec
    {
        public decimal TaxRate { get; set; }
        public decimal Amount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal AmountIncludingTax { get; set; }
    }
    #endregion
}
