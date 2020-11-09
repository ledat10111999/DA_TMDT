using IMS_Project.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace IMS_Project.ViewModel
{
    public class CreateProduct: Product
    {
        [Required(ErrorMessage = "Nhập hình ảnh")]
        public HttpPostedFileBase[] image { get; set; }
    }
}