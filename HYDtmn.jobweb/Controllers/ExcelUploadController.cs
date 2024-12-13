using DocumentFormat.OpenXml.EMMA;
using HYDtmn.Abstraction;
using HYDtmn.jobweb.Service;
using HYDtmn.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HYDtmn.jobweb.Controllers
{
    public class ExcelUploadController : BaseController
    {
        // GET: ExcelUpload
        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file, string filetype)
        {

            int ret = 0;
            ViewBag.isObsolete = TempData["isObsolete"];
            if (file != null && file.ContentLength > 0)
                try
                {

                    var basename = Path.GetFileNameWithoutExtension(file.FileName);
                    var ran = basename.RandomString();
                    var ext = Path.GetExtension(file.FileName);
                    var filename = $"{basename}.{ran}{ext}";

                    
                    DataTable tb = tb = ExcelHelper.ReadToDataTableByNPOI(file.InputStream, ext);
                    
                    //userService.ImportDataTable(tb, AppManager.UserState.UserID,out ret);
                    //var obsolete = isobsolete.HasValue ? isobsolete.Value : false;
                    //var obsoletetb = tb.AsEnumerable().Where(e => e.Field<string>("obsolete") == "Y").CopyToDataTable();

                    ViewBag.Message = "File uploaded successfully";
                    ViewBag.Status = "success";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                    ViewBag.Status = "error";

                    return Json(new { status = "failure", failure = true, message = $"Could not import file {file.FileName} having error: {ex.Message}" }, JsonRequestBehavior.AllowGet);
                }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }
            return Json(new { status = "success", success = true, count=ret },  JsonRequestBehavior.AllowGet);
        }
    }
}