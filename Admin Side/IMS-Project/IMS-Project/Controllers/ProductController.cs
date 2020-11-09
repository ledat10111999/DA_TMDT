using IMS_Project.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IMS_Project.Controllers
{
    public class ProductController : Controller
    {
        KahreedoEntities db = new KahreedoEntities();

        public ActionResult Index()
        {
            return View(db.Products.ToList());
        }

        public ActionResult Create()
        {
            GetViewBagData();
            return View();
        }
        public void GetViewBagData()
        {
            ViewBag.SupplierID = new SelectList(db.Suppliers, "SupplierID", "CompanyName");
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "Name");
            //ViewBag.SubCategoryID = new SelectList(db.SubCategories, "SubCategoryID", "Name");

        }
        public List<string> uploadImage(HttpPostedFileBase[] files)
        {
            List<string> listImg = new List<string>();
            foreach (HttpPostedFileBase img in files)
            {
                var InputFileName = DateTime.Now.Millisecond.ToString() + Path.GetFileName(img.FileName);
                var serverSavePath = Path.Combine(Server.MapPath("~/UploadedFiles/") + InputFileName);
                img.SaveAs(serverSavePath);
                listImg.Add("/UploadedFiles/"+InputFileName);
            }
            return listImg;
        }

        [HttpPost]
        public ActionResult getSubCategory(int id)
        {
            var list = db.SubCategories.Where(c => c.CategoryID == id).ToList();
            return PartialView(list);
        }


        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Create(IMS_Project.ViewModel.CreateProduct prod)
        {
            if (ModelState.IsValid)
            {
                List<string> listimg = uploadImage(prod.image);
                Product products = new Product()
                {
                    AddBadge = prod.AddBadge,
                    CategoryID = prod.CategoryID,
                    Discount = prod.Discount,
                    ImageURL = listimg[0],
                    LongDescription = prod.LongDescription,
                    Name = prod.Name,
                    Note = prod.Note,
                    OfferBadgeClass = prod.OfferBadgeClass,
                    UnitPrice = prod.UnitPrice,
                    OldPrice = prod.OldPrice,
                    AltText = prod.AltText,
                    SubCategoryID = prod.SubCategoryID,
                    SupplierID = prod.SupplierID,
                    UnitWeight = prod.UnitWeight,
                    OfferTitle = prod.OfferTitle,
                    QuantityPerUnit = prod.QuantityPerUnit,
                    OrderDetails = prod.OrderDetails,
                    Size = prod.Size,
                    ProductAvailable = prod.ProductAvailable,
                    UnitInStock = prod.UnitInStock,
                    ShortDescription = prod.ShortDescription,
                    UnitOnOrder = prod.UnitOnOrder,
                    
                };
                db.Products.Add(products);
                db.SaveChanges();
                return RedirectToAction("Index", "Product");
            }
            GetViewBagData();
            return View();
        }


        //Get Edit
        [HttpGet]
        public ActionResult Edit(int id)
        {
            Product product = db.Products.Single(x => x.ProductID == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            GetViewBagData();
            return View("Edit", product);
        }

        //Post Edit
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Edit(Product prod)
        {
            if (ModelState.IsValid)
            {
                db.Entry(prod).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Product");
            }
            GetViewBagData();
            return View(prod);
        }

        //Get Details
        public ActionResult Details(int id)
        {
            Product  product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        //Get Delete
        public ActionResult Delete(int id)
        {
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);

        }

        //Post Delete Confirmed
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
        
    }
}