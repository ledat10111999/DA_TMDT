using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Khareedo.Models;
using System.Data;
using Khareedo.helper;

namespace Khareedo.Controllers
{
    public class AccountController : Controller
    {
        KhareedoEntities db = new KhareedoEntities();

        // GET: Account
        public ActionResult Index()
        {
            this.GetDefaultData();

            var usr = db.Customers.Find(TempShpData.UserID);
            return View(usr);

        }


        //REGISTER CUSTOMER
        [HttpPost]
        public ActionResult Register(Customer cust)
        {
            if (ModelState.IsValid)
            {
                var check = db.Customers.FirstOrDefault(c => c.UserName == cust.UserName);
                if (check == null)
                {
                    cust.ConfirmEmail = false;
                    db.Customers.Add(cust);
                    db.SaveChanges();
                    TempShpData.UserID = GetUser(cust.UserName).CustomerID;
                    SendMail.GuiEmail("Đăng ký tài khoản", cust.Email, string.Format("Dear {0} <br/> Thank you for your registration, please click on the  below link to complete your registration: <a href =\"{1}\"  title =\"User Email Confirm\">{1}</a>", cust.First_Name + cust.Last_Name, Url.Action("ConfirmEmail", "Account", new { Token = cust.CustomerID, Email = cust.Email }, Request.Url.Scheme)));
                    return RedirectToAction("Confirm", "Account", new { Email = cust.Email });
                }
                ModelState.AddModelError("Register", "Tài khoản đã tồn tại");
                return View("Login",cust);
              
            }
            return View("Login",cust);
        }

        // confirm email
        public ActionResult ConfirmEmail(int Token, string Email)
        {
            var user = db.Customers.FirstOrDefault(c => c.CustomerID == Token);
            if (user != null)
            {
                if (user.Email == Email)
                {
                 

                    Session["username"] = user.UserName;
                    user.ConfirmEmail = true;
                    db.SaveChanges();
                    //await SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home", new { ConfirmedEmail = user.Email });
                }
                else
                {
                    return RedirectToAction("Confirm", "Account", new { Email = user.Email });
                }
            }
            else
            {
                return RedirectToAction("Confirm", "Account", new { Email = "" });
        }
    }
        public ActionResult Confirm(string Email)
        {
            ViewBag.Email = Email;
            return View();
        }
        //LOG IN
        public ActionResult Login()
        {
            return View();
        }

         [HttpPost]
        public ActionResult Login(FormCollection formColl)
        {
            string usrName = formColl["UserName"];
            string Pass = formColl["Password"];

            
            if (ModelState.IsValid)
            {
              
              
                    var cust = (from m in db.Customers
                                where (m.UserName == usrName && m.Password == Pass)
                                select m).SingleOrDefault();

                    if (cust != null)
                    {
                        if(cust.ConfirmEmail == false)
                    {
                        ModelState.AddModelError("", "Email Chưa được xác thực");
                        return View(formColl);
                    }
                        TempShpData.UserID = cust.CustomerID;
                        Session["username"] = cust.UserName;
                        return RedirectToAction("Index", "Home");
                    }
                
                
                      
            }
            ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu");
            return View(formColl);
        }

        //LOG OUT
         public ActionResult Logout()
         {
             Session["username"] = null;
             TempShpData.UserID = 0;
             TempShpData.items = null;
             return RedirectToAction("Index", "Home");
         }

       

        public Customer GetUser(string _usrName)
        {
            var cust = (from c in db.Customers
                        where c.UserName == _usrName
                        select c).FirstOrDefault();
            return cust;
        }

        //UPDATE CUSTOMER DATA
        [HttpPost]
        public ActionResult Update(Customer cust)
        {
            if (ModelState.IsValid)
            {
                db.Entry(cust).State = EntityState.Modified;
                db.SaveChanges();
                Session["username"] = cust.UserName;
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
    }
}