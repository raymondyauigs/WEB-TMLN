using Autofac;
using HYDrmb.Abstraction;
using HYDrmb.Service;
using HYDrmb.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ionic.Zip;
using System.Data.SqlClient;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using HYDrmb.Framework.AppModel;

namespace HYDrmb.jobweb.Controllers
{
    
    public class BaseController : Controller
    {
        protected IMiscLog miscLog;
        protected IErrorLog errorLog;
        protected IStdbLog stdbLog;
        protected HYDrmbEntities _db;

        protected IUserService userService;
        protected IBookingService bkdService;
        protected ISettingService sttService;
        protected IDriverService dvrService;

        protected IMemoryCache mmCache;
        

        protected const int pagesize = 13;
        protected EditModels _mapping;

        protected override IActionInvoker CreateActionInvoker()
        {

            miscLog = DependencyResolver.Current.GetService<IMiscLog>();
            errorLog = DependencyResolver.Current.GetService<IErrorLog>();
            stdbLog = DependencyResolver.Current.GetService<IStdbLog>();
            _db = DependencyResolver.Current.GetService<HYDrmbEntities>();
            mmCache = DependencyResolver.Current.GetService<IMemoryCache>();
            _db.Database.Log = (msg) => { stdbLog.LogStdb(msg.TrimEnd(Environment.NewLine.ToCharArray())); };
            sttService = DependencyResolver.Current.GetService<ISettingService>();
            userService = DependencyResolver.Current.GetService<IUserService>();
            dvrService = DependencyResolver.Current.GetService<IDriverService>();
            bkdService = DependencyResolver.Current.GetService<IBookingService>();
            _mapping =DependencyResolver.Current.GetService<EditModels>();

            SqlConnection sqlConn = (SqlConnection)_db.Database.Connection;
            sqlConn.InfoMessage -= SqlConn_InfoMessage;
            sqlConn.InfoMessage += SqlConn_InfoMessage;

            return base.CreateActionInvoker();
        }

        private void SqlConn_InfoMessage(object sender, SqlInfoMessageEventArgs args)
        {
            foreach (SqlError err in args.Errors)
            {
                string strOutput = string.Format("Procedure [{0}] Line [{1}]: {2}", err.Procedure, err.LineNumber, err.Message.TrimEnd(Environment.NewLine.ToCharArray()));
                errorLog.LogError(strOutput);
            }
        }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.ContentWidth = "full-width";
            var returnUri = Request.UrlReferrer?.AbsoluteUri;
            if (returnUri != null)
            {
                ViewBag.AdHocReturnUrl = new Uri(returnUri).PathAndQuery;
            }
            

            var tagid = (int)(Session[Constants.Session.TagId] ?? 0);
            ViewBag.UserTag = tagid > 0 ? $"u!{tagid}" : $"!restricted";
            

            base.OnActionExecuting(filterContext);
        }

        [HttpGet, ActionName("DownloadAsync")]
        public async Task<ActionResult> DownloadAsync(string filename)
        {
            
            var filebytes = new byte[] { };
            var contenttype = filename.GetFileType();
            var file = new FileInfo(filename);
            switch (filename)
            {


                default:
                    if (!string.IsNullOrEmpty(filename))
                    {
                        
                        if (!string.IsNullOrEmpty(contenttype) && file.Exists)
                        {
                            using (var ms = new MemoryStream())
                            using (var fs = System.IO.File.OpenRead(filename))
                            {
                                await fs.CopyToAsync(ms);
                                ms.Seek(0, SeekOrigin.Begin);
                                filebytes = ms.ToArray();
                            }

                            return File(filebytes, contenttype,file.Name);
                        }

                    }

                    break;

            }
            
            throw new IOException($"File {file?.Name ?? filename} does not exist!");

        }

        [HttpGet, ActionName("Download")]
        public ActionResult Download(string fileName, string errorMessage, string filetype)
        {


            var tmppath = Server.MapPath("~/App_Data");
            var exportpath = Constants.Setting.exportoutputpath.GetAppKeyValue();
            
            //get the temp folder and file path in server
            string fullPath = string.Empty;

            if (!string.IsNullOrEmpty(filetype) && filetype.Contains("-"))
            {
                var partid = filetype.Substring(filetype.IndexOf("-") + 1);
                filetype = filetype.Replace($"-{partid}", "");
            }


            switch (filetype)
            {


                case "tempfile":
                    fullPath = Path.Combine(tmppath, fileName.TrimStart('\\', ' '));
                    break;
                case "exportfile":
                    fullPath = Path.Combine(exportpath, fileName.TrimStart('\\', ' '));
                    break;

                    //System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition
                    //{
                    //    FileName = fileName,
                    //    Inline = ispdf  // false = prompt the user for downloading;  true = browser to try to show the file inline
                    //};
                    //Response.Headers.Add("Content-Disposition", cd.ToString());
                    //Response.Headers.Add("X-Content-Type-Options", "nosniff");

                    
                    //return File(filedata,ispdf ?  "application/pdf": "applicationvnd.ms-excel");



                default:
                    break;

            }
            if (fileName.EndsWith(".xlsx") || fileName.EndsWith(".xls") || fileName.EndsWith(".xlsm"))
                filetype = "application/vnd.ms-excel";
            else if (fileName.EndsWith(".pdf"))
                filetype = "application/pdf";
            else if (fileName.EndsWith(".doc") || fileName.EndsWith(".docx"))
                filetype = "application/vnd.ms-word";
            else if (fileName.EndsWith(".zip"))
                filetype = "application/zip";

            if (!System.IO.File.Exists(fullPath))
            {
                throw new IOException($"File {fileName} does not exist!");
            }

            //return the file for download, this is an Excel 
            //so I set the file content type to "application/vnd.ms-excel"
            return File(fullPath, filetype, fileName);
        }

    }

}