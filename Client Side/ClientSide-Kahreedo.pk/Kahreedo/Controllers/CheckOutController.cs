using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Khareedo.Models;
using System.Data;
using System.Net.Mail;
using Khareedo.helper;

namespace Khareedo.Controllers
{
    public class CheckOutController : Controller
    {
        private PayPal.Api.Payment payment;
        KhareedoEntities db = new KhareedoEntities();
        // GET: CheckOut
        public ActionResult Index()
        {
            ViewBag.PayMethod = new SelectList(db.PaymentTypes, "PayTypeID", "TypeName");


            var data = this.GetDefaultData();

            return View(data);
        }


        //PLACE ORDER--LAST STEP
        public ActionResult PlaceOrder(FormCollection getCheckoutDetails)
        {


            if (Convert.ToInt32(getCheckoutDetails["PayMethod"]) == 1)
            {
                int shpID = 1;
                if (db.ShippingDetails.Count() > 0)
                {
                    shpID = db.ShippingDetails.Max(x => x.ShippingID) + 1;
                }
                int payID = 1;
                if (db.Payments.Count() > 0)
                {
                    payID = db.Payments.Max(x => x.PaymentID) + 1;
                }
                int orderID = 1;
                if (db.Orders.Count() > 0)
                {
                    orderID = db.Orders.Max(x => x.OrderID) + 1;
                }



                ShippingDetail shpDetails = new ShippingDetail();
                shpDetails.ShippingID = shpID;
                shpDetails.FirstName = getCheckoutDetails["FirstName"];
                shpDetails.LastName = getCheckoutDetails["LastName"];
                shpDetails.Email = getCheckoutDetails["Email"];
                shpDetails.Mobile = getCheckoutDetails["Mobile"];
                shpDetails.Address = getCheckoutDetails["Address"];
                shpDetails.Province = getCheckoutDetails["Province"];
                shpDetails.City = getCheckoutDetails["City"];
                shpDetails.PostCode = getCheckoutDetails["PostCode"];
                db.ShippingDetails.Add(shpDetails);
                db.SaveChanges();

                Payment pay = new Payment();
                pay.PaymentID = payID;
                pay.Type = Convert.ToInt32(getCheckoutDetails["PayMethod"]);
                db.Payments.Add(pay);
                db.SaveChanges();

                Order o = new Order();
                o.OrderID = orderID;
                o.CustomerID = TempShpData.UserID;
                o.PaymentID = payID;
                o.ShippingID = shpID;
                o.Discount = Convert.ToInt32(getCheckoutDetails["discount"]);
                o.TotalAmount = Convert.ToInt32(getCheckoutDetails["totalAmount"]);
                o.isCompleted = true;
                o.OrderDate = DateTime.Now;
                db.Orders.Add(o);
                db.SaveChanges();

                foreach (var OD in TempShpData.items)
                {
                    OD.OrderID = orderID;
                    OD.Order = db.Orders.Find(orderID);
                    OD.Product = db.Products.Find(OD.ProductID);
                    db.OrderDetails.Add(OD);
                    db.SaveChanges();
                }
                SendMail.GuiEmail("Xác nhận đặt hàng thành công", getCheckoutDetails["Email"], "Cảm ơn bạn đã đặt hàng, mã đơn hàng của bạn là: " + o.OrderID);

                return RedirectToAction("Index", "ThankYou");
            }
            //Paypal
            else
            {
                PayPal.Api.APIContext apiContext = Configuration.GetAPIContext();
                try
                {
                    string payerId = Request.Params["PayerID"];
                    if (string.IsNullOrEmpty(payerId))
                    {

                        int shpID = 1;
                        if (db.ShippingDetails.Count() > 0)
                        {
                            shpID = db.ShippingDetails.Max(x => x.ShippingID) + 1;
                        }
                        int payID = 1;
                        if (db.Payments.Count() > 0)
                        {
                            payID = db.Payments.Max(x => x.PaymentID) + 1;
                        }
                        int orderID = 1;
                        if (db.Orders.Count() > 0)
                        {
                            orderID = db.Orders.Max(x => x.OrderID) + 1;
                        }



                        ShippingDetail shpDetails = new ShippingDetail();
                        shpDetails.ShippingID = shpID;
                        shpDetails.FirstName = getCheckoutDetails["FirstName"];
                        shpDetails.LastName = getCheckoutDetails["LastName"];
                        shpDetails.Email = getCheckoutDetails["Email"];
                        shpDetails.Mobile = getCheckoutDetails["Mobile"];
                        shpDetails.Address = getCheckoutDetails["Address"];
                        shpDetails.Province = getCheckoutDetails["Province"];
                        shpDetails.City = getCheckoutDetails["City"];
                        shpDetails.PostCode = getCheckoutDetails["PostCode"];
                        db.ShippingDetails.Add(shpDetails);
                        db.SaveChanges();

                        Payment pay = new Payment();
                        pay.PaymentID = payID;
                        pay.Type = Convert.ToInt32(getCheckoutDetails["PayMethod"]);
                        db.Payments.Add(pay);
                        db.SaveChanges();

                        Order o = new Order();
                        o.OrderID = orderID;
                        o.CustomerID = TempShpData.UserID;
                        o.PaymentID = payID;
                        o.ShippingID = shpID;
                        o.Discount = Convert.ToInt32(getCheckoutDetails["discount"]);
                        o.TotalAmount = Convert.ToInt32(getCheckoutDetails["totalAmount"]);
                        o.isCompleted = true;
                        o.OrderDate = DateTime.Now;
                        db.Orders.Add(o);
                        db.SaveChanges();
                        
                        string tenSp = "";
                        foreach (var OD in TempShpData.items)
                        {
                            OD.OrderID = orderID;
                            OD.Order = db.Orders.Find(orderID);
                            OD.Product = db.Products.Find(OD.ProductID);
                            tenSp += db.Products.Find(OD.ProductID).Name +", ";
                            db.OrderDetails.Add(OD);
                            db.SaveChanges();
                        }
                        SendMail.GuiEmail("Xác nhận đặt hàng thành công", getCheckoutDetails["Email"], "Cảm ơn bạn đã đặt hàng, mã đơn hàng của bạn là: " + o.OrderID);

                        //this section will be executed first because PayerID doesn't exist
                        //it is returned by the create function call of the payment class
                        // Creating a payment
                        // baseURL is the url on which paypal sendsback the data.
                        // So we have provided URL of this controller only
                        string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/CheckOut/PlaceOrder?";
                        //guid we are generating for storing the paymentID received in  session
                        //after calling the create function and it is used in the payment execution
                        var guid = Convert.ToString((new Random()).Next(100000));
                        //CreatePayment function gives us the payment approval url
                        //on which payer is redirected for paypal account payment
                        var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid,tenSp,o.TotalAmount);
                        //get links returned from paypal in response to Create function  call
                        var links = createdPayment.links.GetEnumerator();
                        string paypalRedirectUrl = null;
                        while (links.MoveNext())
                        {
                            PayPal.Api.Links lnk = links.Current;
                            if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                            {
                                //saving the payapalredirect URL to which user will be redirected for payment
                                paypalRedirectUrl = lnk.href;
                            }
                        }
                        // saving the paymentID in the key guid
                        Session.Add(guid, createdPayment.id);
                        return Redirect(paypalRedirectUrl);
                    }
                    else
                    {
                        // This section is executed when we have received all the payments parameters
                        // from the previous call to the function Create
                        // Executing a payment
                        var guid = Request.Params["guid"];
                        var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                        if (executedPayment.state.ToLower() != "approved")
                        {
                            return View("FailureView");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Error" + ex.Message);
                    return View("FailureView");
                }
               
                return RedirectToAction("Index", "ThankYou");
            }
        }
   
            
        
        /// <summary>
        /// Create payment
        /// </summary>
        /// <returns></returns>
        /// 
        private PayPal.Api.Payment CreatePayment(PayPal.Api.APIContext apiContext, string redirectUrl, string tenSp, int? price)
        {
            var itemList = new PayPal.Api.ItemList() { items = new List<PayPal.Api.Item>() };
            //Các giá trị bao gồm danh sách sản phẩm, thông tin đơn hàng
            //Sẽ được thay đổi bằng hành vi thao tác mua hàng trên website
            var items = new PayPal.Api.Item()
            {
                //Thông tin đơn hàng
                name = tenSp,
                currency = "USD",
                price = String.Format("{0:0.00}", price / 23000),
                quantity = "1",
                sku = "sku"
            };
            itemList.items.Add(items);
            var subtotal = float.Parse(items.price) * float.Parse(items.quantity);
            //Hình thức thanh toán qua paypal
            var payer = new PayPal.Api.Payer() { payment_method = "paypal" };
            // Configure Redirect Urls here with RedirectUrls object
            var redirUrls = new PayPal.Api.RedirectUrls()
            {
                cancel_url = redirectUrl,
                return_url = redirectUrl
            };
            //các thông tin trong đơn hàng
            var details = new PayPal.Api.Details()
            {
                tax = "1",
                shipping = "1",
                subtotal = subtotal.ToString()
            };
            var total = subtotal + float.Parse(details.tax) + float.Parse(details.shipping);
            //Đơn vị tiền tệ và tổng đơn hàng cần thanh toán
            var amount = new PayPal.Api.Amount()
            {
                currency = "USD",
                total = total.ToString(), // Total must be equal to sum of shipping, tax and subtotal.
                details = details
            };
            var transactionList = new List<PayPal.Api.Transaction>();
            //Tất cả thông tin thanh toán cần đưa vào transaction
            transactionList.Add(new PayPal.Api.Transaction()
            {
                description = "Transaction description.",
                invoice_number = Guid.NewGuid().ToString(),
                amount = amount,
                item_list = itemList
            });
            this.payment = new PayPal.Api.Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };
            return this.payment.Create(apiContext);
        }
        /// <summary>
        /// excute payment
        /// </summary>
        /// <returns></returns>
        /// 
       
        private PayPal.Api.Payment ExecutePayment(PayPal.Api.APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PayPal.Api.PaymentExecution() { payer_id = payerId };
            this.payment = new PayPal.Api.Payment() { id = paymentId };
            return this.payment.Execute(apiContext, paymentExecution);
        }
        
       
        // Theo dõi đơn hàng
        public ActionResult FollowOrder()
        {
            return View();
        }
       
        [HttpPost]
        public ActionResult OrderSearchResult(int? id)
        {
            if(id == null)
            {
                return Content("");
            }
            var list = db.OrderDetails.Where(c => c.Order.OrderID == id).ToList();
            return PartialView(list);
        }

    }
}