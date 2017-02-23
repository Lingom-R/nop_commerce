﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Web.Framework.Security.Captcha;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Common;
using Nop.Web.Models.Media;
using Nop.Web.Models.ShoppingCart;

namespace Nop.Web.Factories
{
    public partial class ShoppingCartModelFactory : IShoppingCartModelFactory
    {
        #region Fields

        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICheckoutAttributeFormatter _checkoutAttributeFormatter;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IDiscountService _discountService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IShippingService _shippingService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly IPaymentService _paymentService;
        private readonly IPermissionService _permissionService;
        private readonly IDownloadService _downloadService;
        private readonly ICacheManager _cacheManager;
        private readonly IWebHelper _webHelper;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly HttpContextBase _httpContext;

        private readonly MediaSettings _mediaSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly OrderSettings _orderSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly TaxSettings _taxSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly AddressSettings _addressSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly IRewardPointService _rewardPointService;

        #endregion

		#region Ctor

        public ShoppingCartModelFactory(IAddressModelFactory addressModelFactory,
            IStoreContext storeContext,
            IWorkContext workContext,
            IShoppingCartService shoppingCartService,
            IPictureService pictureService,
            ILocalizationService localizationService,
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            ITaxService taxService, ICurrencyService currencyService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICheckoutAttributeFormatter checkoutAttributeFormatter,
            IOrderProcessingService orderProcessingService,
            IDiscountService discountService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IShippingService shippingService,
            IOrderTotalCalculationService orderTotalCalculationService,
            ICheckoutAttributeService checkoutAttributeService,
            IPaymentService paymentService,
            IPermissionService permissionService,
            IDownloadService downloadService,
            ICacheManager cacheManager,
            IWebHelper webHelper,
            IGenericAttributeService genericAttributeService,
            HttpContextBase httpContext,
            MediaSettings mediaSettings,
            ShoppingCartSettings shoppingCartSettings,
            CatalogSettings catalogSettings,
            OrderSettings orderSettings,
            ShippingSettings shippingSettings,
            TaxSettings taxSettings,
            CaptchaSettings captchaSettings,
            AddressSettings addressSettings,
            RewardPointsSettings rewardPointsSettings,
            CustomerSettings customerSettings,
            IRewardPointService rewardPointService)
        {
            this._addressModelFactory = addressModelFactory;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._shoppingCartService = shoppingCartService;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
            this._productAttributeFormatter = productAttributeFormatter;
            this._productAttributeParser = productAttributeParser;
            this._taxService = taxService;
            this._currencyService = currencyService;
            this._priceCalculationService = priceCalculationService;
            this._priceFormatter = priceFormatter;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._checkoutAttributeFormatter = checkoutAttributeFormatter;
            this._orderProcessingService = orderProcessingService;
            this._discountService = discountService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._shippingService = shippingService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._checkoutAttributeService = checkoutAttributeService;
            this._paymentService = paymentService;
            this._permissionService = permissionService;
            this._downloadService = downloadService;
            this._cacheManager = cacheManager;
            this._webHelper = webHelper;
            this._genericAttributeService = genericAttributeService;
            this._httpContext = httpContext;

            this._mediaSettings = mediaSettings;
            this._shoppingCartSettings = shoppingCartSettings;
            this._catalogSettings = catalogSettings;
            this._orderSettings = orderSettings;
            this._shippingSettings = shippingSettings;
            this._taxSettings = taxSettings;
            this._captchaSettings = captchaSettings;
            this._addressSettings = addressSettings;
            this._rewardPointsSettings = rewardPointsSettings;
            this._customerSettings = customerSettings;
            this._rewardPointService = rewardPointService;
        }

        #endregion

        #region Utilities

        protected virtual IList<ShoppingCartModel.CheckoutAttributeModel> PrepareCheckoutAttributeModels(IList<ShoppingCartItem> cart)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            var model = new List<ShoppingCartModel.CheckoutAttributeModel>();

            var checkoutAttributes = _checkoutAttributeService.GetAllCheckoutAttributes(_storeContext.CurrentStore.Id, !cart.RequiresShipping());
            foreach (var attribute in checkoutAttributes)
            {
                var attributeModel = new ShoppingCartModel.CheckoutAttributeModel
                {
                    Id = attribute.Id,
                    Name = attribute.GetLocalized(x => x.Name),
                    TextPrompt = attribute.GetLocalized(x => x.TextPrompt),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                    DefaultValue = attribute.DefaultValue
                };
                if (!String.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
                {
                    attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = _checkoutAttributeService.GetCheckoutAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new ShoppingCartModel.CheckoutAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.GetLocalized(x => x.Name),
                            ColorSquaresRgb = attributeValue.ColorSquaresRgb,
                            IsPreSelected = attributeValue.IsPreSelected,
                        };
                        attributeModel.Values.Add(attributeValueModel);

                        //display price if allowed
                        if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                        {
                            decimal priceAdjustmentBase = _taxService.GetCheckoutAttributePrice(attributeValue);
                            decimal priceAdjustment = _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, _workContext.WorkingCurrency);
                            if (priceAdjustmentBase > decimal.Zero)
                                attributeValueModel.PriceAdjustment = "+" + _priceFormatter.FormatPrice(priceAdjustment);
                            else if (priceAdjustmentBase < decimal.Zero)
                                attributeValueModel.PriceAdjustment = "-" + _priceFormatter.FormatPrice(-priceAdjustment);
                        }
                    }
                }



                //set already selected attributes
                var selectedCheckoutAttributes = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService, _storeContext.CurrentStore.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.Checkboxes:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            if (!String.IsNullOrEmpty(selectedCheckoutAttributes))
                            {
                                //clear default selection
                                foreach (var item in attributeModel.Values)
                                    item.IsPreSelected = false;

                                //select new values
                                var selectedValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(selectedCheckoutAttributes);
                                foreach (var attributeValue in selectedValues)
                                    foreach (var item in attributeModel.Values)
                                        if (attributeValue.Id == item.Id)
                                            item.IsPreSelected = true;
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //do nothing
                            //values are already pre-set
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            if (!String.IsNullOrEmpty(selectedCheckoutAttributes))
                            {
                                var enteredText = _checkoutAttributeParser.ParseValues(selectedCheckoutAttributes, attribute.Id);
                                if (enteredText.Any())
                                    attributeModel.DefaultValue = enteredText[0];
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            //keep in mind my that the code below works only in the current culture
                            var selectedDateStr = _checkoutAttributeParser.ParseValues(selectedCheckoutAttributes, attribute.Id);
                            if (selectedDateStr.Any())
                            {
                                DateTime selectedDate;
                                if (DateTime.TryParseExact(selectedDateStr[0], "D", CultureInfo.CurrentCulture,
                                                       DateTimeStyles.None, out selectedDate))
                                {
                                    //successfully parsed
                                    attributeModel.SelectedDay = selectedDate.Day;
                                    attributeModel.SelectedMonth = selectedDate.Month;
                                    attributeModel.SelectedYear = selectedDate.Year;
                                }
                            }

                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            if (!String.IsNullOrEmpty(selectedCheckoutAttributes))
                            {
                                var downloadGuidStr = _checkoutAttributeParser.ParseValues(selectedCheckoutAttributes, attribute.Id).FirstOrDefault();
                                Guid downloadGuid;
                                Guid.TryParse(downloadGuidStr, out downloadGuid);
                                var download = _downloadService.GetDownloadByGuid(downloadGuid);
                                if (download != null)
                                    attributeModel.DefaultValue = download.DownloadGuid.ToString();
                            }
                        }
                        break;
                    default:
                        break;
                }

                model.Add(attributeModel);
            }

            return model;
        }

        protected virtual EstimateShippingModel PrepareEstimateShippingModel(IList<ShoppingCartItem> cart, bool setEstimateShippingDefaultAddress = true)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            var model = new EstimateShippingModel();

            model.Enabled = cart.Any() && cart.RequiresShipping() && _shippingSettings.EstimateShippingEnabled;
            if (model.Enabled)
            {
                //countries
                int? defaultEstimateCountryId = (setEstimateShippingDefaultAddress && _workContext.CurrentCustomer.ShippingAddress != null)
                    ? _workContext.CurrentCustomer.ShippingAddress.CountryId
                    : model.CountryId;
                model.AvailableCountries.Add(new SelectListItem
                {
                    Text = _localizationService.GetResource("Address.SelectCountry"),
                    Value = "0"
                });
                foreach (var c in _countryService.GetAllCountriesForShipping(_workContext.WorkingLanguage.Id))
                    model.AvailableCountries.Add(new SelectListItem
                    {
                        Text = c.GetLocalized(x => x.Name),
                        Value = c.Id.ToString(),
                        Selected = c.Id == defaultEstimateCountryId
                    });

                //states
                int? defaultEstimateStateId = (setEstimateShippingDefaultAddress && _workContext.CurrentCustomer.ShippingAddress != null)
                    ? _workContext.CurrentCustomer.ShippingAddress.StateProvinceId
                    : model.StateProvinceId;
                var states = defaultEstimateCountryId.HasValue
                    ? _stateProvinceService.GetStateProvincesByCountryId(defaultEstimateCountryId.Value,_workContext.WorkingLanguage.Id).ToList()
                    : new List<StateProvince>();
                if (states.Any())
                {
                    foreach (var s in states)
                    {
                        model.AvailableStates.Add(new SelectListItem
                        {
                            Text = s.GetLocalized(x => x.Name),
                            Value = s.Id.ToString(),
                            Selected = s.Id == defaultEstimateStateId
                        });
                    }
                }
                else
                {
                    model.AvailableStates.Add(new SelectListItem
                    {
                        Text = _localizationService.GetResource("Address.OtherNonUS"),
                        Value = "0"
                    });
                }

                if (setEstimateShippingDefaultAddress && _workContext.CurrentCustomer.ShippingAddress != null)
                    model.ZipPostalCode = _workContext.CurrentCustomer.ShippingAddress.ZipPostalCode;
            }

            return model;
        }

        protected virtual ShoppingCartModel.ShoppingCartItemModel PrepareShoppingCartItemModel(IList<ShoppingCartItem> cart, ShoppingCartItem sci)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            if (sci == null)
                throw new ArgumentNullException("sci");


            var cartItemModel = new ShoppingCartModel.ShoppingCartItemModel
            {
                Id = sci.Id,
                Sku = sci.Product.FormatSku(sci.AttributesXml, _productAttributeParser),
                ProductId = sci.Product.Id,
                ProductName = sci.Product.GetLocalized(x => x.Name),
                ProductSeName = sci.Product.GetSeName(),
                Quantity = sci.Quantity,
                AttributeInfo = _productAttributeFormatter.FormatAttributes(sci.Product, sci.AttributesXml),
                TaxRate = sci.TaxRate == null ? "" : sci.TaxRate.Value.ToString("G29")
            };

            //allow editing?
            //1. setting enabled?
            //2. simple product?
            //3. has attribute or gift card?
            //4. visible individually?
            cartItemModel.AllowItemEditing = _shoppingCartSettings.AllowCartItemEditing &&
                                             sci.Product.ProductType == ProductType.SimpleProduct &&
                                             (!String.IsNullOrEmpty(cartItemModel.AttributeInfo) ||
                                              sci.Product.IsGiftCard) &&
                                             sci.Product.VisibleIndividually;

            //disable removal?
            //1. do other items require this one?
            cartItemModel.DisableRemoval = cart.Any( item => item.Product.RequireOtherProducts && item.Product.ParseRequiredProductIds().Contains(sci.ProductId));

            //allowed quantities
            var allowedQuantities = sci.Product.ParseAllowedQuantities();
            foreach (var qty in allowedQuantities)
            {
                cartItemModel.AllowedQuantities.Add(new SelectListItem
                {
                    Text = qty.ToString(),
                    Value = qty.ToString(),
                    Selected = sci.Quantity == qty
                });
            }

            //recurring info
            if (sci.Product.IsRecurring)
                cartItemModel.RecurringInfo = string.Format(_localizationService.GetResource("ShoppingCart.RecurringPeriod"),
                        sci.Product.RecurringCycleLength,
                        sci.Product.RecurringCyclePeriod.GetLocalizedEnum(_localizationService, _workContext));

            //rental info
            if (sci.Product.IsRental)
            {
                var rentalStartDate = sci.RentalStartDateUtc.HasValue
                    ? sci.Product.FormatRentalDate(sci.RentalStartDateUtc.Value)
                    : "";
                var rentalEndDate = sci.RentalEndDateUtc.HasValue
                    ? sci.Product.FormatRentalDate(sci.RentalEndDateUtc.Value)
                    : "";
                cartItemModel.RentalInfo =
                    string.Format(_localizationService.GetResource("ShoppingCart.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);
            }

            //reward points program
            cartItemModel.ExcludeFromRewardPoints = sci.Product.ExcludeFromRewardPoints;

            //unit prices
            decimal itemSubtotal;

            decimal taxRate;
            decimal unitPrice = _priceCalculationService.GetUnitPrice(sci);
            string unitAttributesXml = sci.AttributesXml;
            string subAttributesXml = "";
            decimal shoppingCartUnitPriceWithDiscountBase = _taxService.GetProductPrice(sci.Product, unitPrice, out taxRate, ref unitAttributesXml);
            cartItemModel.TaxRate = taxRate.ToString("G29");

            if (sci.Product.CallForPrice)
            {
                cartItemModel.UnitPrice = _localizationService.GetResource("Products.CallForPrice");
            }
            else
            {
                //moved up for taxRate
                //decimal taxRate;
                //decimal shoppingCartUnitPriceWithDiscountBase = _taxService.GetProductPrice(sci.Product, _priceCalculationService.GetUnitPrice(sci), out taxRate);
                decimal shoppingCartUnitPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, _workContext.WorkingCurrency);
                cartItemModel.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);
            }
            //subtotal, discount
            if (sci.Product.CallForPrice)
            {
                cartItemModel.SubTotal = _localizationService.GetResource("Products.CallForPrice");
                itemSubtotal = decimal.Zero;
            }
            else
            {
                //sub total
                List<DiscountForCaching> scDiscounts;
                int? maximumDiscountQty;
                decimal shoppingCartItemDiscountBase;
                subAttributesXml = sci.AttributesXml;
                decimal subTotal = _priceCalculationService.GetSubTotal(sci, true, out shoppingCartItemDiscountBase, out scDiscounts, out maximumDiscountQty);
                decimal shoppingCartItemSubTotalWithDiscountBase = _taxService.GetProductPrice(sci.Product, subTotal, out taxRate, ref subAttributesXml);
                decimal shoppingCartItemSubTotalWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemSubTotalWithDiscountBase, _workContext.WorkingCurrency);

                itemSubtotal = shoppingCartItemSubTotalWithDiscount;
                cartItemModel.SubTotal = _priceFormatter.FormatPrice(shoppingCartItemSubTotalWithDiscount);
                cartItemModel.MaximumDiscountedQty = maximumDiscountQty;

                //display an applied discount amount
                if (shoppingCartItemDiscountBase > decimal.Zero)
                {
                    shoppingCartItemDiscountBase = _taxService.GetProductPrice(sci.Product, shoppingCartItemDiscountBase, out taxRate);
                    if (shoppingCartItemDiscountBase > decimal.Zero)
                    {
                        decimal shoppingCartItemDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemDiscountBase, _workContext.WorkingCurrency);
                        cartItemModel.Discount = _priceFormatter.FormatPrice(shoppingCartItemDiscount);
                    }
                }
            }
            //attribute tax info
            if (!String.IsNullOrEmpty(sci.AttributesXml))
            {
                //pass new tax AttributesXml here only after all price calculations have been completed
                sci.AttributesXml = string.IsNullOrEmpty(subAttributesXml) ? unitAttributesXml: subAttributesXml;
                cartItemModel.AttributeInfo = _productAttributeFormatter.FormatAttributes(sci.Product, sci.AttributesXml, _workContext.CurrentCustomer, subTotal: itemSubtotal);
                cartItemModel.hasTaxInfoInAttributeXML = _productAttributeParser.hasTaxInfoInAttributeXML(sci.AttributesXml);
            }

            //picture
            if (_shoppingCartSettings.ShowProductImagesOnShoppingCart)
            {
                cartItemModel.Picture = PrepareCartItemPictureModel(sci,
                    _mediaSettings.CartThumbPictureSize, true, cartItemModel.ProductName);
            }

            //item warnings
            var itemWarnings = _shoppingCartService.GetShoppingCartItemWarnings(
                _workContext.CurrentCustomer,
                sci.ShoppingCartType,
                sci.Product,
                sci.StoreId,
                sci.AttributesXml,
                sci.CustomerEnteredPrice,
                sci.RentalStartDateUtc,
                sci.RentalEndDateUtc,
                sci.Quantity,
                false);
            foreach (var warning in itemWarnings)
                cartItemModel.Warnings.Add(warning);

            return cartItemModel;
        }

        protected virtual WishlistModel.ShoppingCartItemModel PrepareWishlistItemModel(IList<ShoppingCartItem> cart, ShoppingCartItem sci)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            if (sci == null)
                throw new ArgumentNullException("sci");

            var cartItemModel = new WishlistModel.ShoppingCartItemModel
            {
                Id = sci.Id,
                Sku = sci.Product.FormatSku(sci.AttributesXml, _productAttributeParser),
                ProductId = sci.Product.Id,
                ProductName = sci.Product.GetLocalized(x => x.Name),
                ProductSeName = sci.Product.GetSeName(),
                Quantity = sci.Quantity,
                AttributeInfo = _productAttributeFormatter.FormatAttributes(sci.Product, sci.AttributesXml),
                //VatRate = sci.VatRate
            };

            //allow editing?
            //1. setting enabled?
            //2. simple product?
            //3. has attribute or gift card?
            //4. visible individually?
            cartItemModel.AllowItemEditing = _shoppingCartSettings.AllowCartItemEditing &&
                                             sci.Product.ProductType == ProductType.SimpleProduct &&
                                             (!String.IsNullOrEmpty(cartItemModel.AttributeInfo) ||
                                              sci.Product.IsGiftCard) &&
                                             sci.Product.VisibleIndividually;

            //allowed quantities
            var allowedQuantities = sci.Product.ParseAllowedQuantities();
            foreach (var qty in allowedQuantities)
            {
                cartItemModel.AllowedQuantities.Add(new SelectListItem
                {
                    Text = qty.ToString(),
                    Value = qty.ToString(),
                    Selected = sci.Quantity == qty
                });
            }


            //recurring info
            if (sci.Product.IsRecurring)
                cartItemModel.RecurringInfo = string.Format(_localizationService.GetResource("ShoppingCart.RecurringPeriod"),
                        sci.Product.RecurringCycleLength,
                        sci.Product.RecurringCyclePeriod.GetLocalizedEnum(_localizationService, _workContext));

            //rental info
            if (sci.Product.IsRental)
            {
                var rentalStartDate = sci.RentalStartDateUtc.HasValue
                    ? sci.Product.FormatRentalDate(sci.RentalStartDateUtc.Value)
                    : "";
                var rentalEndDate = sci.RentalEndDateUtc.HasValue
                    ? sci.Product.FormatRentalDate(sci.RentalEndDateUtc.Value)
                    : "";
                cartItemModel.RentalInfo =
                    string.Format(_localizationService.GetResource("ShoppingCart.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);
            }

            //unit prices
            if (sci.Product.CallForPrice)
            {
                cartItemModel.UnitPrice = _localizationService.GetResource("Products.CallForPrice");
            }
            else
            {
                decimal taxRate;
                decimal shoppingCartUnitPriceWithDiscountBase = _taxService.GetProductPrice(sci.Product,
                    _priceCalculationService.GetUnitPrice(sci), out taxRate);
                decimal shoppingCartUnitPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, _workContext.WorkingCurrency);
                cartItemModel.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);
            }
            //subtotal, discount
            if (sci.Product.CallForPrice)
            {
                cartItemModel.SubTotal = _localizationService.GetResource("Products.CallForPrice");
            }
            else
            {
                //sub total
                List<DiscountForCaching> scDiscounts;
                int? maximumDiscountQty;
                decimal shoppingCartItemDiscountBase;
                decimal taxRate;
                decimal shoppingCartItemSubTotalWithDiscountBase = _taxService.GetProductPrice(sci.Product, _priceCalculationService.GetSubTotal(sci, true, out shoppingCartItemDiscountBase, out scDiscounts,
                        out maximumDiscountQty), out taxRate);
                decimal shoppingCartItemSubTotalWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemSubTotalWithDiscountBase, _workContext.WorkingCurrency);
                cartItemModel.SubTotal = _priceFormatter.FormatPrice(shoppingCartItemSubTotalWithDiscount);
                cartItemModel.MaximumDiscountedQty = maximumDiscountQty;

                //display an applied discount amount
                if (shoppingCartItemDiscountBase > decimal.Zero)
                {
                    shoppingCartItemDiscountBase = _taxService.GetProductPrice(sci.Product, shoppingCartItemDiscountBase, out taxRate);
                    if (shoppingCartItemDiscountBase > decimal.Zero)
                    {
                        decimal shoppingCartItemDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemDiscountBase, _workContext.WorkingCurrency);
                        cartItemModel.Discount = _priceFormatter.FormatPrice(shoppingCartItemDiscount);
                    }
                }
            }

            //picture
            if (_shoppingCartSettings.ShowProductImagesOnWishList)
            {
                cartItemModel.Picture = PrepareCartItemPictureModel(sci,
                    _mediaSettings.CartThumbPictureSize, true, cartItemModel.ProductName);
            }

            //item warnings
            var itemWarnings = _shoppingCartService.GetShoppingCartItemWarnings(
                _workContext.CurrentCustomer,
                sci.ShoppingCartType,
                sci.Product,
                sci.StoreId,
                sci.AttributesXml,
                sci.CustomerEnteredPrice,
                sci.RentalStartDateUtc,
                sci.RentalEndDateUtc,
                sci.Quantity,
                false);
            foreach (var warning in itemWarnings)
                cartItemModel.Warnings.Add(warning);

            return cartItemModel;
        }

        protected virtual ShoppingCartModel.OrderReviewDataModel PrepareOrderReviewDataModel(IList<ShoppingCartItem> cart)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            var model = new ShoppingCartModel.OrderReviewDataModel();
            model.Display = true;

            //billing info
            var billingAddress = _workContext.CurrentCustomer.BillingAddress;
            if (billingAddress != null)
            {
                _addressModelFactory.PrepareAddressModel(model.BillingAddress,
                        address: billingAddress,
                        excludeProperties: false,
                        addressSettings: _addressSettings);
            }

            //shipping info
            if (cart.RequiresShipping())
            {
                model.IsShippable = true;

                var pickupPoint = _workContext.CurrentCustomer.GetAttribute<PickupPoint>(SystemCustomerAttributeNames.SelectedPickupPoint,_storeContext.CurrentStore.Id);
                model.SelectedPickUpInStore = _shippingSettings.AllowPickUpInStore && pickupPoint != null;
                if (!model.SelectedPickUpInStore)
                {
                    if (_workContext.CurrentCustomer.ShippingAddress != null)
                    {
                        _addressModelFactory.PrepareAddressModel(model.ShippingAddress,
                            address: _workContext.CurrentCustomer.ShippingAddress,
                            excludeProperties: false,
                            addressSettings: _addressSettings);
                    }
                }
                else
                {
                    var country = _countryService.GetCountryByTwoLetterIsoCode(pickupPoint.CountryCode);
                    var state = _stateProvinceService.GetStateProvinceByAbbreviation(pickupPoint.StateAbbreviation);
                    model.PickupAddress = new AddressModel
                    {
                        Address1 = pickupPoint.Address,
                        City = pickupPoint.City,
                        CountryName = country != null ? country.Name : string.Empty,
                        StateProvinceName = state != null ? state.Name : string.Empty,
                        ZipPostalCode = pickupPoint.ZipPostalCode
                    };
                }

                //selected shipping method
                var shippingOption = _workContext.CurrentCustomer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id);
                if (shippingOption != null)
                    model.ShippingMethod = shippingOption.Name;
            }

            //payment info
            var selectedPaymentMethodSystemName = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.SelectedPaymentMethod, _storeContext.CurrentStore.Id);
            var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(selectedPaymentMethodSystemName);
            model.PaymentMethod = paymentMethod != null
                ? paymentMethod.GetLocalizedFriendlyName(_localizationService, _workContext.WorkingLanguage.Id)
                : "";

            //custom values
            var processPaymentRequest = _httpContext.Session["OrderPaymentInfo"] as ProcessPaymentRequest;
            if (processPaymentRequest != null)
            {
                model.CustomValues = processPaymentRequest.CustomValues;
            }

            return model;
        }

        #endregion

        #region Methods

        public virtual PictureModel PrepareCartItemPictureModel(ShoppingCartItem sci, int pictureSize, bool showDefaultPicture, string productName)
        {
            var pictureCacheKey = string.Format(ModelCacheEventConsumer.CART_PICTURE_MODEL_KEY, sci.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
            var model = _cacheManager.Get(pictureCacheKey,
                //as we cache per user (shopping cart item identifier)
                //let's cache just for 3 minutes
                3, () =>
            {
                //shopping cart item picture
                var sciPicture = sci.Product.GetProductPicture(sci.AttributesXml, _pictureService, _productAttributeParser);
                return new PictureModel
                {
                    ImageUrl = _pictureService.GetPictureUrl(sciPicture, pictureSize, showDefaultPicture),
                    Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), productName),
                    AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), productName),
                };
            });
            return model;
        }

        public virtual ShoppingCartModel PrepareShoppingCartModel(ShoppingCartModel model,
            IList<ShoppingCartItem> cart, bool isEditable = true,
            bool validateCheckoutAttributes = false,
            bool prepareEstimateShippingIfEnabled = true, bool setEstimateShippingDefaultAddress = true,
            bool prepareAndDisplayOrderReviewData = false)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            if (model == null)
                throw new ArgumentNullException("model");

            //simple properties
            model.OnePageCheckoutEnabled = _orderSettings.OnePageCheckoutEnabled;
            if (!cart.Any())
                return model;
            model.IsEditable = isEditable;
            model.ShowProductImages = _shoppingCartSettings.ShowProductImagesOnShoppingCart;
            model.ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage;
            var checkoutAttributesXml = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService, _storeContext.CurrentStore.Id);
            model.CheckoutAttributeInfo = _checkoutAttributeFormatter.FormatAttributes(checkoutAttributesXml, _workContext.CurrentCustomer);
            bool minOrderSubtotalAmountOk = _orderProcessingService.ValidateMinOrderSubtotalAmount(cart);
            if (!minOrderSubtotalAmountOk)
            {
                decimal minOrderSubtotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderSubtotalAmount, _workContext.WorkingCurrency);
                model.MinOrderSubtotalWarning = string.Format(_localizationService.GetResource("Checkout.MinOrderSubtotalAmount"), _priceFormatter.FormatPrice(minOrderSubtotalAmount, true, false));
            }
            model.TermsOfServiceOnShoppingCartPage = _orderSettings.TermsOfServiceOnShoppingCartPage;
            model.TermsOfServiceOnOrderConfirmPage = _orderSettings.TermsOfServiceOnOrderConfirmPage;
            model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoShoppingCart;

            //discount and gift card boxes
            model.DiscountBox.Display = _shoppingCartSettings.ShowDiscountBox;
            var discountCouponCodes = _workContext.CurrentCustomer.ParseAppliedDiscountCouponCodes();
            foreach (var couponCode in discountCouponCodes)
            {
                var discount = _discountService.GetAllDiscountsForCaching(couponCode: couponCode)
                    .FirstOrDefault(d => d.RequiresCouponCode && _discountService.ValidateDiscount(d, _workContext.CurrentCustomer).IsValid);

                //MF 22.11.16 was this a bug? if coupon is already present but a non compatible item get's added it will throw an error
                if (discount == null)
                    _workContext.CurrentCustomer.RemoveDiscountCouponCode(couponCode);
                else
                    //MF 22.11.16
                    model.DiscountBox.AppliedDiscountsWithCodes.Add(new ShoppingCartModel.DiscountBoxModel.DiscountInfoModel()
                    {
                        Id = discount.Id,
                        CouponCode = discount.CouponCode
                    });
            }
            model.GiftCardBox.Display = _shoppingCartSettings.ShowGiftCardBox;

            //cart warnings
            var cartWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributesXml, validateCheckoutAttributes);
            foreach (var warning in cartWarnings)
                model.Warnings.Add(warning);

            //checkout attributes
            model.CheckoutAttributes = PrepareCheckoutAttributeModels(cart);

            //estimate shipping
            if (prepareEstimateShippingIfEnabled)
            {
                model.EstimateShipping = PrepareEstimateShippingModel(cart, setEstimateShippingDefaultAddress);
            }

            //cart items
            foreach (var sci in cart)
            {
                var cartItemModel = PrepareShoppingCartItemModel(cart, sci);
                model.Items.Add(cartItemModel);
            }

            #region Payment methods

            //all payment methods (do not filter by country here as it could be not specified yet)
            var paymentMethods = _paymentService
                .LoadActivePaymentMethods(_workContext.CurrentCustomer, _storeContext.CurrentStore.Id)
                .Where(pm => !pm.HidePaymentMethod(cart))
                .ToList();
            //payment methods displayed during checkout (not with "Button" type)
            var nonButtonPaymentMethods = paymentMethods
                .Where(pm => pm.PaymentMethodType != PaymentMethodType.Button)
                .ToList();
            //"button" payment methods(*displayed on the shopping cart page)
            var buttonPaymentMethods = paymentMethods
                .Where(pm => pm.PaymentMethodType == PaymentMethodType.Button)
                .ToList();
            foreach (var pm in buttonPaymentMethods)
            {
                if (cart.IsRecurring() && pm.RecurringPaymentType == RecurringPaymentType.NotSupported)
                    continue;

                string actionName;
                string controllerName;
                RouteValueDictionary routeValues;
                pm.GetPaymentInfoRoute(out actionName, out controllerName, out routeValues);

                model.ButtonPaymentMethodActionNames.Add(actionName);
                model.ButtonPaymentMethodControllerNames.Add(controllerName);
                model.ButtonPaymentMethodRouteValues.Add(routeValues);
            }
            //hide "Checkout" button if we have only "Button" payment methods
            model.HideCheckoutButton = !nonButtonPaymentMethods.Any() && model.ButtonPaymentMethodRouteValues.Any();

            #endregion

            //order review data
            if (prepareAndDisplayOrderReviewData)
            {
                model.OrderReviewData = PrepareOrderReviewDataModel(cart);
            }

            return model;
        }

        public virtual WishlistModel PrepareWishlistModel(WishlistModel model, IList<ShoppingCartItem> cart, bool isEditable = true)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            if (model == null)
                throw new ArgumentNullException("model");

            model.EmailWishlistEnabled = _shoppingCartSettings.EmailWishlistEnabled;
            model.IsEditable = isEditable;
            model.DisplayAddToCart = _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart);
            model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoWishlist;

            if (!cart.Any())
                return model;

            //simple properties
            var customer = cart.GetCustomer();
            model.CustomerGuid = customer.CustomerGuid;
            model.CustomerFullname = customer.GetFullName();
            model.ShowProductImages = _shoppingCartSettings.ShowProductImagesOnWishList;
            model.ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage;

            //cart warnings
            var cartWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, "", false);
            foreach (var warning in cartWarnings)
                model.Warnings.Add(warning);

            //cart items
            foreach (var sci in cart)
            {
                var cartItemModel = PrepareWishlistItemModel(cart, sci);
                model.Items.Add(cartItemModel);
            }

            return model;
        }

        public virtual MiniShoppingCartModel PrepareMiniShoppingCartModel()
        {
            var model = new MiniShoppingCartModel
            {
                ShowProductImages = _shoppingCartSettings.ShowProductImagesInMiniShoppingCart,
                //let's always display it
                DisplayShoppingCartButton = true,
                CurrentCustomerIsGuest = _workContext.CurrentCustomer.IsGuest(),
                AnonymousCheckoutAllowed = _orderSettings.AnonymousCheckoutAllowed,
            };


            //performance optimization (use "HasShoppingCartItems" property)
            if (_workContext.CurrentCustomer.HasShoppingCartItems)
            {
                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
                model.TotalProducts = cart.GetTotalProducts();
                if (cart.Any())
                {
                    //subtotal
                    decimal orderSubTotalDiscountAmountBase;
                    List<DiscountForCaching> orderSubTotalAppliedDiscounts;
                    decimal subTotalWithoutDiscountBase;
                    decimal subTotalWithDiscountBase;
                    var subTotalIncludingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
                    _orderTotalCalculationService.GetShoppingCartSubTotal(cart, subTotalIncludingTax,
                        out orderSubTotalDiscountAmountBase, out orderSubTotalAppliedDiscounts,
                        out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);
                    decimal subtotalBase = subTotalWithoutDiscountBase;
                    decimal subtotal = _currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, _workContext.WorkingCurrency);
                    model.SubTotal = _priceFormatter.FormatPrice(subtotal, false, _workContext.WorkingCurrency, _workContext.WorkingLanguage, subTotalIncludingTax);

                    var requiresShipping = cart.RequiresShipping();
                    //a customer should visit the shopping cart page (hide checkout button) before going to checkout if:
                    //1. "terms of service" are enabled
                    //2. min order sub-total is OK
                    //3. we have at least one checkout attribute
                    var checkoutAttributesExistCacheKey = string.Format(ModelCacheEventConsumer.CHECKOUTATTRIBUTES_EXIST_KEY,
                        _storeContext.CurrentStore.Id, requiresShipping);
                    var checkoutAttributesExist = _cacheManager.Get(checkoutAttributesExistCacheKey,
                        () =>
                        {
                            var checkoutAttributes = _checkoutAttributeService.GetAllCheckoutAttributes(_storeContext.CurrentStore.Id, !requiresShipping);
                            return checkoutAttributes.Any();
                        });

                    bool minOrderSubtotalAmountOk = _orderProcessingService.ValidateMinOrderSubtotalAmount(cart);
                    bool downloadableProductsRequireRegistration =
                        _customerSettings.RequireRegistrationForDownloadableProducts && cart.Any(sci => sci.Product.IsDownload);

                    model.DisplayCheckoutButton = !_orderSettings.TermsOfServiceOnShoppingCartPage &&
                        minOrderSubtotalAmountOk &&
                        !checkoutAttributesExist &&
                        !(downloadableProductsRequireRegistration
                            && _workContext.CurrentCustomer.IsGuest());

                    //products. sort descending (recently added products)
                    foreach (var sci in cart
                        .OrderByDescending(x => x.Id)
                        .Take(_shoppingCartSettings.MiniShoppingCartProductNumber)
                        .ToList())
                    {
                        var cartItemModel = new MiniShoppingCartModel.ShoppingCartItemModel
                        {
                            Id = sci.Id,
                            ProductId = sci.Product.Id,
                            ProductName = sci.Product.GetLocalized(x => x.Name),
                            ProductSeName = sci.Product.GetSeName(),
                            Quantity = sci.Quantity,
                            AttributeInfo = _productAttributeFormatter.FormatAttributes(sci.Product, sci.AttributesXml)
                        };

                        //unit prices
                        if (sci.Product.CallForPrice)
                        {
                            cartItemModel.UnitPrice = _localizationService.GetResource("Products.CallForPrice");
                            decimal taxRate;
                            decimal shoppingCartUnitPriceWithDiscountBase = _taxService.GetProductPrice(sci.Product, 0, out taxRate);
                            cartItemModel.TaxRate = taxRate.ToString("G29");
                        }
                        else
                        {
                            decimal taxRate;
                            decimal unitPrice = _priceCalculationService.GetUnitPrice(sci);
                            string attributesXml = sci.AttributesXml;
                            decimal shoppingCartUnitPriceWithDiscountBase = _taxService.GetProductPrice(sci.Product, unitPrice, out taxRate, ref attributesXml);
                            decimal shoppingCartUnitPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, _workContext.WorkingCurrency);
                            cartItemModel.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);
                            cartItemModel.TaxRate = taxRate.ToString("G29");
                        }

                        //picture
                        if (_shoppingCartSettings.ShowProductImagesInMiniShoppingCart)
                        {
                            cartItemModel.Picture = PrepareCartItemPictureModel(sci,
                                _mediaSettings.MiniCartThumbPictureSize, true, cartItemModel.ProductName);
                        }

                        model.Items.Add(cartItemModel);
                    }
                }
            }

            return model;
        }

        public virtual OrderTotalsModel PrepareOrderTotalsModel(IList<ShoppingCartItem> cart, bool isEditable)
        {
            var model = new OrderTotalsModel();
            model.IsEditable = isEditable;

            if (cart.Any())
            {
                bool includingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax;
                bool subTotalIncludingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;

                model.includingTax = includingTax; //MF 22.11.16
                //total
                decimal orderTotalDiscountAmountBase;
                List<DiscountForCaching> orderTotalAppliedDiscounts;
                List<DiscountForCaching> subTotalAppliedDiscounts;
                List<DiscountForCaching> shippingAppliedDiscounts;
                List<AppliedGiftCard> appliedGiftCards;
                RewardPoints redeemableRewardPoints;
                TaxSummary taxSummary;
                decimal earnedRewardPointsBaseAmount;
                
                decimal? shoppingCartTotalBase = _orderTotalCalculationService.GetShoppingCartTotal(cart,
                    out orderTotalDiscountAmountBase, out orderTotalAppliedDiscounts, out subTotalAppliedDiscounts, out shippingAppliedDiscounts,
                    out appliedGiftCards, out redeemableRewardPoints, out taxSummary, out earnedRewardPointsBaseAmount, includingTax);

                decimal shoppingCartTaxBase = taxSummary.TotalAmountTax;

                if (shoppingCartTotalBase.HasValue)
                {
                    decimal shoppingCartTotal = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartTotalBase.Value, _workContext.WorkingCurrency);
                    model.OrderTotal = _priceFormatter.FormatPrice(shoppingCartTotal, true, false);
                    decimal orderAmount = _currencyService.ConvertFromPrimaryStoreCurrency(taxSummary.TotalAmount, _workContext.WorkingCurrency);
                    model.OrderAmount = _priceFormatter.FormatPrice(orderAmount, true, true);
                    decimal orderAmountIncl = _currencyService.ConvertFromPrimaryStoreCurrency(taxSummary.TotalAmountIncludingTax, _workContext.WorkingCurrency);
                    model.OrderAmountIncl = _priceFormatter.FormatPrice(orderAmountIncl, true, includingTax);

                    decimal orderTotalAmountInCustomerCurrency = _currencyService.ConvertFromPrimaryStoreCurrency(taxSummary.TotalAmountIncludingTax + (taxSummary.TotalPaymentFeeAmountNonTaxable ?? decimal.Zero) + (taxSummary.TotalShippingAmountNonTaxable ?? decimal.Zero), _workContext.WorkingCurrency);
                    model.OrderTotalAmountIncl = _priceFormatter.FormatPrice(orderTotalAmountInCustomerCurrency, true, includingTax);

                }

                //subtotal
                decimal subtotalBase = decimal.Zero;
                decimal subtotalDiscount = decimal.Zero;
                if (includingTax != subTotalIncludingTax)
                {
                    decimal orderSubTotalDiscountAmount;
                    decimal subTotalWithoutDiscountBase;
                    decimal subTotalWithDiscountBase;
                    decimal subTotalRewardPointsBaseAmount;
                    TaxSummary taxSummaryNeg;
                    _orderTotalCalculationService.GetShoppingCartSubTotal(cart, subTotalIncludingTax,
                        out orderSubTotalDiscountAmount, out subTotalAppliedDiscounts,
                        out subTotalWithoutDiscountBase, out subTotalWithDiscountBase,
                        out taxSummaryNeg,
                        out subTotalRewardPointsBaseAmount);
                    subtotalBase = subTotalWithoutDiscountBase;
                    subtotalDiscount = orderSubTotalDiscountAmount;
                }
                else
                {
                    subtotalBase = taxSummary.TotalSubTotalAmount;
                    subtotalDiscount = taxSummary.TotalSubTotalDiscAmount;
                }
                model.HasRewardPointsProduct = cart.HasRewardPointsProduct();

                decimal subtotal = _currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, _workContext.WorkingCurrency);
                model.SubTotal = _priceFormatter.FormatPrice(subtotal, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, subTotalIncludingTax);

                if (subtotalDiscount > decimal.Zero)
                {
                    decimal orderSubTotalDiscountAmount = _currencyService.ConvertFromPrimaryStoreCurrency(subtotalDiscount, _workContext.WorkingCurrency);
                    model.SubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountAmount, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, subTotalIncludingTax);
                }


                //shipping info
                model.RequiresShipping = cart.RequiresShipping();
                if (model.RequiresShipping)
                {
                    decimal? shoppingCartShippingBase = taxSummary.TotalShippingAmountTaxable; //_orderTotalCalculationService.GetShoppingCartShippingTotal(cart);
                    if (shoppingCartShippingBase.HasValue)
                    {
                        decimal shoppingCartShipping = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartShippingBase.Value, _workContext.WorkingCurrency);
                        model.Shipping = _priceFormatter.FormatShippingPrice(shoppingCartShipping, true);

                        //selected shipping method
                        var shippingOption = _workContext.CurrentCustomer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id);
                        if (shippingOption != null)
                            model.SelectedShippingMethod = shippingOption.Name;
                    }

                    //non taxable
                    decimal? shoppingCartShippingNonTaxableBase = taxSummary.TotalShippingAmountNonTaxable; 
                    if (shoppingCartShippingNonTaxableBase.HasValue)
                    {
                        decimal shoppingCartShippingNonTaxable = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartShippingNonTaxableBase.Value, _workContext.WorkingCurrency);
                        model.ShippingNonTaxable = _priceFormatter.FormatShippingPrice(shoppingCartShippingNonTaxable, true);

                        //selected shipping method
                        var shippingOption = _workContext.CurrentCustomer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id);
                        if (shippingOption != null)
                            model.SelectedShippingMethod = shippingOption.Name;
                    }
                }
                else
                {
                    model.HideShippingTotal = _shippingSettings.HideShippingTotal;
                }

                //payment method fee
                decimal paymentFee = taxSummary.TotalPaymentFeeAmountTaxable;
                if (paymentFee != decimal.Zero) //tfc allow negative fee
                {
                    decimal paymentMethodAdditionalFeeWithTax = _currencyService.ConvertFromPrimaryStoreCurrency(paymentFee, _workContext.WorkingCurrency);
                    model.PaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeWithTax, true);
                }
                //payment method fee non taxable
                decimal paymentFeeNonTaxable = taxSummary.TotalPaymentFeeAmountNonTaxable ?? decimal.Zero;
                if (paymentFeeNonTaxable != decimal.Zero) //tfc allow negative fee
                {
                    decimal paymentMethodAdditionalFeeNonTaxable = _currencyService.ConvertFromPrimaryStoreCurrency(paymentFeeNonTaxable, _workContext.WorkingCurrency);
                    model.PaymentMethodAdditionalFeeNonTaxable = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeNonTaxable, true);
                }

                //tax
                bool displayTax = true;
                bool displayTaxRates = true;
                if (_taxSettings.HideTaxInOrderSummary && includingTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    decimal shoppingCartTax = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartTaxBase, _workContext.WorkingCurrency);

                    if (shoppingCartTaxBase == 0 && _taxSettings.HideZeroTax)
                    {
                        displayTax = false;
                        displayTaxRates = false;
                    }
                    else
                    {
                        var taxRates = taxSummary.TaxRates;
                        displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                        displayTax = !displayTaxRates;

                        model.Tax = _priceFormatter.FormatPrice(shoppingCartTax, true, false);
                        foreach (var tr in taxRates)
                        {
                            model.TaxRates.Add(new OrderTotalsModel.TaxRate
                            {
                                Rate = _priceFormatter.FormatTaxRate(tr.Value.TaxRate),
                                Amount = _priceFormatter.FormatPrice(_currencyService.ConvertFromPrimaryStoreCurrency(tr.Value.EntryAmount, _workContext.WorkingCurrency), true, false),
                                DiscountAmount = _priceFormatter.FormatPrice(_currencyService.ConvertFromPrimaryStoreCurrency(tr.Value.DiscountAmount, _workContext.WorkingCurrency), true, false),
                                BaseAmount = _priceFormatter.FormatPrice(_currencyService.ConvertFromPrimaryStoreCurrency(tr.Value.BaseAmount, _workContext.WorkingCurrency), true, false),
                                TaxAmount = _priceFormatter.FormatPrice(_currencyService.ConvertFromPrimaryStoreCurrency(tr.Value.TaxAmount, _workContext.WorkingCurrency), true, false)
                            });
                        }
                    }
                }
                model.DisplayTaxRates = displayTaxRates;
                model.DisplayTax = displayTax;

                //discount
                if (orderTotalDiscountAmountBase > decimal.Zero)
                {
                    decimal orderTotalDiscountAmount = _currencyService.ConvertFromPrimaryStoreCurrency(orderTotalDiscountAmountBase, _workContext.WorkingCurrency);
                    model.OrderTotalDiscount = _priceFormatter.FormatPrice(-orderTotalDiscountAmount, true, true);
                }

                //gift cards
                if (appliedGiftCards != null && appliedGiftCards.Any())
                {
                    foreach (var appliedGiftCard in appliedGiftCards)
                    {
                        var gcModel = new OrderTotalsModel.GiftCard
                        {
                            Id = appliedGiftCard.GiftCard.Id,
                            CouponCode = appliedGiftCard.GiftCard.GiftCardCouponCode,
                        };
                        decimal amountCanBeUsed = _currencyService.ConvertFromPrimaryStoreCurrency(appliedGiftCard.AmountCanBeUsed, _workContext.WorkingCurrency);
                        gcModel.Amount = _priceFormatter.FormatPrice(-amountCanBeUsed, true, false);

                        decimal remainingAmountBase = appliedGiftCard.GiftCard.GetGiftCardRemainingAmount() - appliedGiftCard.AmountCanBeUsed;
                        decimal remainingAmount = _currencyService.ConvertFromPrimaryStoreCurrency(remainingAmountBase, _workContext.WorkingCurrency);
                        gcModel.Remaining = _priceFormatter.FormatPrice(remainingAmount, true, false);

                        model.GiftCards.Add(gcModel);
                    }
                }

                //reward points to be spent (redeemed)
                model.EarnedRewardPointsAreTaxable = _rewardPointsSettings.EarnedRewardPointsAreTaxable;
                if (redeemableRewardPoints.Amount > decimal.Zero)
                {
                    model.RedeemedRewardPoints = redeemableRewardPoints.Points;
                    decimal redeemedRewardPointsAmountInCustomerCurrency = _currencyService.ConvertFromPrimaryStoreCurrency(redeemableRewardPoints.Amount, _workContext.WorkingCurrency);
                    model.RedeemedRewardPointsAmount = _priceFormatter.FormatPrice(-redeemedRewardPointsAmountInCustomerCurrency, true, false);
                    //correct total discount for possibly included reward poins
                    if (model.EarnedRewardPointsAreTaxable && orderTotalDiscountAmountBase > decimal.Zero)
                    {
                        decimal orderTotalDiscountAmount = _currencyService.ConvertFromPrimaryStoreCurrency(Math.Max(orderTotalDiscountAmountBase - redeemableRewardPoints.Amount, 0), _workContext.WorkingCurrency);
                        if (orderTotalDiscountAmount == decimal.Zero)
                            model.OrderTotalDiscount = "";
                        else
                            model.OrderTotalDiscount = _priceFormatter.FormatPrice(-orderTotalDiscountAmount, true, includingTax);
                    }
                }

                //purchased reward points to be spent (redeemed)
                if (redeemableRewardPoints.AmountPurchased > decimal.Zero)
                {
                    model.RedeemedRewardPointsPurchased = redeemableRewardPoints.PointsPurchased;
                    decimal redeemedRewardPointsAmountInCustomerCurrency = _currencyService.ConvertFromPrimaryStoreCurrency(redeemableRewardPoints.AmountPurchased, _workContext.WorkingCurrency);
                    model.RedeemedRewardPointsAmountPurchased = _priceFormatter.FormatPrice(-redeemedRewardPointsAmountInCustomerCurrency, true, false);
                }

                //reward points to be earned
                if (_rewardPointsSettings.Enabled &&
                    _rewardPointsSettings.DisplayHowMuchWillBeEarned &&
                    shoppingCartTotalBase.HasValue)
                {
                    model.WillEarn = false;
                    decimal redeemedAmountPurchased = decimal.Zero;
                    if (_rewardPointsSettings.EarnRewardPointsOnlyWhenUsingPurchasedRewardPoints && redeemableRewardPoints != null)
                    {
                        //points were calculated
                        bool hasRewardPointsProduct = model.HasRewardPointsProduct;
                        if (hasRewardPointsProduct || redeemableRewardPoints.AmountPurchased == decimal.Zero)
                        {
                            redeemedAmountPurchased = earnedRewardPointsBaseAmount;
                            if (hasRewardPointsProduct)
                                model.WillEarn = true;
                        }
                        else
                        {
                            redeemedAmountPurchased = redeemableRewardPoints.AmountPurchased;
                            model.WillEarn = true;
                        }
                    }
                    else
                    {
                        //use_rewardpoints was not yet set. We show points that could be earned.
                        redeemedAmountPurchased = earnedRewardPointsBaseAmount;
                        model.WillEarn = !_rewardPointsSettings.EarnRewardPointsOnlyWhenUsingPurchasedRewardPoints;
                    }


                    decimal amount = _rewardPointService.GetRewardPointsBaseAmount(earnedRewardPointsBaseAmount, redeemedAmountPurchased);
                    model.WillEarnRewardPoints = _rewardPointService.CalculateRewardPoints(_workContext.CurrentCustomer, amount);
                    model.WillEarnRewardPointsBasedOnAmount = amount;

                }

            }

            return model;
        }

        public virtual EstimateShippingResultModel PrepareEstimateShippingResultModel(IList<ShoppingCartItem> cart, int? countryId, int? stateProvinceId, string zipPostalCode)
        {
            var model = new EstimateShippingResultModel();

            if (cart.RequiresShipping())
            {
                var address = new Address
                {
                    CountryId = countryId,
                    Country = countryId.HasValue ? _countryService.GetCountryById(countryId.Value) : null,
                    StateProvinceId = stateProvinceId,
                    StateProvince = stateProvinceId.HasValue ? _stateProvinceService.GetStateProvinceById(stateProvinceId.Value) : null,
                    ZipPostalCode = zipPostalCode,
                };
                GetShippingOptionResponse getShippingOptionResponse = _shippingService
                    .GetShippingOptions(cart, address, _workContext.CurrentCustomer, storeId: _storeContext.CurrentStore.Id);
                if (getShippingOptionResponse.Success)
                {
                    if (getShippingOptionResponse.ShippingOptions.Any())
                    {
                        foreach (var shippingOption in getShippingOptionResponse.ShippingOptions)
                        {
                            var soModel = new EstimateShippingResultModel.ShippingOptionModel
                            {
                                Name = shippingOption.Name,
                                Description = shippingOption.Description,

                            };
                            //calculate discounted and taxed rate
                            List<DiscountForCaching> appliedDiscounts = null;
                            decimal shippingTotal = _orderTotalCalculationService.AdjustShippingRate(shippingOption.Rate,
                                cart, out appliedDiscounts);

                            decimal rateBase = _taxService.GetShippingPrice(shippingTotal, _workContext.CurrentCustomer);
                            decimal rate = _currencyService.ConvertFromPrimaryStoreCurrency(rateBase, _workContext.WorkingCurrency);
                            soModel.Price = _priceFormatter.FormatShippingPrice(rate, true);
                            model.ShippingOptions.Add(soModel);
                        }
                    }
                }
                else
                    foreach (var error in getShippingOptionResponse.Errors)
                        model.Warnings.Add(error);

                if (_shippingSettings.AllowPickUpInStore)
                {
                    var pickupPointsResponse = _shippingService.GetPickupPoints(address, _workContext.CurrentCustomer, storeId: _storeContext.CurrentStore.Id);
                    if (pickupPointsResponse.Success)
                    {
                        if (pickupPointsResponse.PickupPoints.Any())
                        {
                            var soModel = new EstimateShippingResultModel.ShippingOptionModel
                            {
                                Name = _localizationService.GetResource("Checkout.PickupPoints"),
                                Description = _localizationService.GetResource("Checkout.PickupPoints.Description"),
                            };
                            var pickupFee = pickupPointsResponse.PickupPoints.Min(x => x.PickupFee);
                            if (pickupFee > 0)
                            {
                                pickupFee = _taxService.GetShippingPrice(pickupFee, _workContext.CurrentCustomer);
                                pickupFee = _currencyService.ConvertFromPrimaryStoreCurrency(pickupFee, _workContext.WorkingCurrency);
                            }
                            soModel.Price = _priceFormatter.FormatShippingPrice(pickupFee, true);
                            model.ShippingOptions.Add(soModel);
                        }
                    }
                    else
                        foreach (var error in pickupPointsResponse.Errors)
                            model.Warnings.Add(error);
                }

            }

            return model;
        }

        public virtual WishlistEmailAFriendModel PrepareWishlistEmailAFriendModel(WishlistEmailAFriendModel model, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnEmailWishlistToFriendPage;
            if (!excludeProperties)
            {
                model.YourEmailAddress = _workContext.CurrentCustomer.Email;
            }
            return model;
        }

        #endregion
    }
}
