using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using M2.Util;
using System.Web;
using System.Globalization;
using Kuyam.Utility;
using Kuyam.Repository;

namespace Kuyam.Domain.Payments
{
    /// <summary>
    /// Payment service
    /// </summary>
    public partial class PaymentService 
    {
        #region Fields

        private readonly HttpContextBase _httpContext;
        private readonly IWebHelper _webHelper;
        #endregion

        #region Ctor

        public PaymentService()
        {
            //this._httpContext = httpContext;
            //this._webHelper = webHelper;
        }

        #endregion

        /// <summary>
        /// Gets Paypal URL
        /// </summary>
        /// <returns></returns>
        private string GetPaypalUrl()
        {
            return ConfigurationManager.AppSettings["UseSandbox"].ToBool() ? "https://www.sandbox.paypal.com/us/cgi-bin/webscr" :
                "https://www.paypal.com/us/cgi-bin/webscr";
        }

        private string BusinessEmail()
        {
            return ConfigurationManager.AppSettings["BusinessEmail"].ToString();
        }


        #region Methods

        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var builder = new StringBuilder();
            builder.Append(GetPaypalUrl());
            string cmd = string.Empty;
            bool PassProductNamesAndTotals = false;
            if (PassProductNamesAndTotals)
            {
                cmd = "_cart";
            }
            else
            {
                cmd = "_xclick";
            }
            builder.AppendFormat("?cmd={0}&business={1}", cmd, HttpUtility.UrlEncode(this.BusinessEmail()));
            if (PassProductNamesAndTotals)
            {
                builder.AppendFormat("&upload=1");

                //get the items in the cart
                decimal cartTotal = decimal.Zero;
                var cartItems = postProcessPaymentRequest.Order.OrderGettyImageDetails;
                int x = 1;
                if (cartItems != null)
                {
                    foreach (var item in cartItems)
                    {
                        var unitPriceExclTax = item.UnitPrice;
                        var priceExclTax = item.Price;
                        //round
                        var unitPriceExclTaxRounded = Math.Round(unitPriceExclTax.Value, 2);
                        //get the product variant so we can get the name
                        builder.AppendFormat("&item_name_" + x + "={0}", HttpUtility.UrlEncode(item.Title));
                        builder.AppendFormat("&amount_" + x + "={0}", unitPriceExclTaxRounded.ToString("0.00", CultureInfo.InvariantCulture));
                        builder.AppendFormat("&quantity_" + x + "={0}", item.Quantity);
                        x++;
                        cartTotal += priceExclTax.Value;
                    }
                }

                //the checkout attributes that have a dollar value and send them to Paypal as items to be paid for
                //var caValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(postProcessPaymentRequest.Order.CheckoutAttributesXml);
                //foreach (var val in caValues)
                //{
                //    var attPrice = _taxService.GetCheckoutAttributePrice(val, false, postProcessPaymentRequest.Order.Customer);
                //    //round
                //    var attPriceRounded = Math.Round(attPrice, 2);
                //    if (attPrice > decimal.Zero) //if it has a price
                //    {
                //        var ca = val.CheckoutAttribute;
                //        if (ca != null)
                //        {
                //            var attName = ca.Name; //set the name
                //            builder.AppendFormat("&item_name_" + x + "={0}", HttpUtility.UrlEncode(attName)); //name
                //            builder.AppendFormat("&amount_" + x + "={0}", attPriceRounded.ToString("0.00", CultureInfo.InvariantCulture)); //amount
                //            builder.AppendFormat("&quantity_" + x + "={0}", 1); //quantity
                //            x++;
                //            cartTotal += attPrice;
                //        }
                //    }
                //}

                //order totals
               
                //payment method additional fee
                //var paymentMethodAdditionalFeeExclTax = postProcessPaymentRequest.Order.PaymentMethodAdditionalFeeExclTax;
                //var paymentMethodAdditionalFeeExclTaxRounded = Math.Round(paymentMethodAdditionalFeeExclTax, 2);
                //if (paymentMethodAdditionalFeeExclTax > decimal.Zero)
                //{
                //    builder.AppendFormat("&item_name_" + x + "={0}", "Payment method fee");
                //    builder.AppendFormat("&amount_" + x + "={0}", paymentMethodAdditionalFeeExclTaxRounded.ToString("0.00", CultureInfo.InvariantCulture));
                //    builder.AppendFormat("&quantity_" + x + "={0}", 1);
                //    x++;
                //    cartTotal += paymentMethodAdditionalFeeExclTax;
                //}

               

                //if (cartTotal > postProcessPaymentRequest.Order.OrderTotal)
                //{
                //    /* Take the difference between what the order total is and what it should be and use that as the "discount".
                //     * The difference equals the amount of the gift card and/or reward points used. 
                //     */
                //    decimal discountTotal = cartTotal - postProcessPaymentRequest.Order.OrderTotal.Value;
                //    discountTotal = Math.Round(discountTotal, 2);
                //    //gift card or rewared point amount applied to cart in nopCommerce - shows in Paypal as "discount"
                //    builder.AppendFormat("&discount_amount_cart={0}", discountTotal.ToString("0.00", CultureInfo.InvariantCulture));
                //}
            }
            else
            {
                //pass order total
                builder.AppendFormat("&item_name=Order Number {0}", postProcessPaymentRequest.Order.OrderID);
                var orderTotal = Math.Round(postProcessPaymentRequest.Order.OrderTotal.Value, 2);
                builder.AppendFormat("&amount={0}", orderTotal.ToString("0.00", CultureInfo.InvariantCulture));
            }

            builder.AppendFormat("&custom={0}", postProcessPaymentRequest.Order.OrderGuID);
            builder.AppendFormat("&charset={0}", "utf-8");
            builder.Append(string.Format("&no_note=1&currency_code={0}", HttpUtility.UrlEncode(ConfigManager.currencyCode)));
            builder.AppendFormat("&invoice={0}", postProcessPaymentRequest.Order.OrderID);
            builder.AppendFormat("&rm=2", new object[0]);
            //if (postProcessPaymentRequest.Order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            //    builder.AppendFormat("&no_shipping=2", new object[0]);
            //else
            //    builder.AppendFormat("&no_shipping=1", new object[0]);

            string returnUrl = _webHelper.GetStoreLocation(false) + "Plugins/PaymentPayPalStandard/PDTHandler";
            string cancelReturnUrl = _webHelper.GetStoreLocation(false) + "Plugins/PaymentPayPalStandard/CancelOrder";
            builder.AppendFormat("&return={0}&cancel_return={1}", HttpUtility.UrlEncode(returnUrl), HttpUtility.UrlEncode(cancelReturnUrl));
                       

            //address
            //builder.AppendFormat("&address_override=1");
            //builder.AppendFormat("&first_name={0}",  HttpUtility.UrlEncode(postProcessPaymentRequest.Order.BillingAddress.FirstName));
            //builder.AppendFormat("&last_name={0}", HttpUtility.UrlEncode(postProcessPaymentRequest.Order.BillingAddress.LastName));
            //builder.AppendFormat("&address1={0}", HttpUtility.UrlEncode(postProcessPaymentRequest.Order.BillingAddress.Address1));
            //builder.AppendFormat("&address2={0}", HttpUtility.UrlEncode(postProcessPaymentRequest.Order.BillingAddress.Address2));
            //builder.AppendFormat("&city={0}", HttpUtility.UrlEncode(postProcessPaymentRequest.Order.BillingAddress.City));
            //if (!String.IsNullOrEmpty(postProcessPaymentRequest.Order.BillingAddress.PhoneNumber))
            //{
            //    //strip out all non-digit characters from phone number;
            //    string billingPhoneNumber = System.Text.RegularExpressions.Regex.Replace(postProcessPaymentRequest.Order.BillingAddress.PhoneNumber, @"\D", string.Empty);
            //    if (billingPhoneNumber.Length >= 10)
            //    {
            //        builder.AppendFormat("&night_phone_a={0}", HttpUtility.UrlEncode(billingPhoneNumber.Substring(0, 3)));
            //        builder.AppendFormat("&night_phone_b={0}", HttpUtility.UrlEncode(billingPhoneNumber.Substring(3, 3)));
            //        builder.AppendFormat("&night_phone_c={0}", HttpUtility.UrlEncode(billingPhoneNumber.Substring(6, 4)));
            //    }
            //}
            //if (postProcessPaymentRequest.Order.BillingAddress.StateProvince != null)
            //    builder.AppendFormat("&state={0}", HttpUtility.UrlEncode(postProcessPaymentRequest.Order.BillingAddress.StateProvince.Abbreviation));
            //else
            //    builder.AppendFormat("&state={0}", "");
            //if (postProcessPaymentRequest.Order.BillingAddress.Country != null)
            //    builder.AppendFormat("&country={0}", HttpUtility.UrlEncode(postProcessPaymentRequest.Order.BillingAddress.Country.TwoLetterIsoCode));
            //else
            //    builder.AppendFormat("&country={0}", "");
            //builder.AppendFormat("&zip={0}", HttpUtility.UrlEncode(postProcessPaymentRequest.Order.BillingAddress.ZipPostalCode));
            //builder.AppendFormat("&email={0}", HttpUtility.UrlEncode(postProcessPaymentRequest.Order.BillingAddress.Email));

            _httpContext.Response.Redirect(builder.ToString());
        }

        
        #endregion
    }
}
