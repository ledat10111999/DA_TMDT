using Khareedo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Khareedo.Controllers
{
    public class HomeController : Controller
    {
        KhareedoEntities db = new KhareedoEntities();

        // GET: Home
        public ActionResult Index()
        {
            ViewBag.PhoneProduct = db.Products.Where(x => x.Category.CategoryID == 1).ToList();
            ViewBag.tabletProduct = db.Products.Where(x => x.Category.CategoryID ==2).ToList();
            ViewBag.LaptopProduct = db.Products.Where(x => x.Category.CategoryID ==3).ToList();
            
            ViewBag.Slider = db.genMainSliders.ToList();
            ViewBag.PromoRight = db.genPromoRights.ToList();

            this.GetDefaultData();

            return View();
        }      

    }
}