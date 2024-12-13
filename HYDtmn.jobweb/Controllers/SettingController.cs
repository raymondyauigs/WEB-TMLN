using BootstrapTable.Pager;
using HYDtmn.Abstraction;
using HYDtmn.Framework.AppModel;
using HYDtmn.Framework.Model;
using HYDtmn.Framework.Tools;
using HYDtmn.jobweb.AppModels;
using HYDtmn.jobweb.Service;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HYDtmn.jobweb.Controllers
{
    public class SettingController : BaseController
    {
        [JWTAuth(level =0)]
        [HttpGet]
        public ActionResult Edit(string id)
        {
            ViewBag.ContentWidth = "";
            ViewBag.EditTitle = "Maximum No. of Cars available";
            var editsetting = _db.CoreSettings.FirstOrDefault(e => e.SettingId == id);
            var model = editsetting.MapTo(new EditSettingModel());

            return View(model);
        }

        [JWTAuth(level =0)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditSettingModel model)
        {
            

            return Redirect("/Setting");
        }


        // GET: Setting
        [JWTAuth(level =0)]
        [HttpGet]
        public ActionResult Index()
        {
            return Index(new QySettingModel());
        }
        [HttpPost]
        [JWTAuth(level = 0)]
        [ValidateAntiForgeryToken]
        public ActionResult Index(QySettingModel query)
        {
            ViewBag.ContentWidth = "";
            List<EditSettingModel> settings = new List<EditSettingModel>();
            _db.CoreSettings.Where(e => e.CanEdit).ToList().ForEach(setting =>
            {
                var model = setting.MapTo(new EditSettingModel());
                settings.Add(model);
            });
            if(!string.IsNullOrEmpty(query.AskCommon))
            {
                query.AskCommon = query.AskCommon.ToLower();
                settings = settings.Where(e => e.SettingId.ToLower().Contains(query.AskCommon)).ToList();
            }
            var dict = new List<JSort>();
            if(!string.IsNullOrEmpty(query.sort))
            {
                var sort = new JSort { Order = query.sort.EndsWith("+") ? "asc" : "desc", Column = query.sort.Substring(0, query.sort.Length - 1) };
                dict.Add(sort);

            }
            query.totalRecords = settings.Count;
            if ((query.page - 1) * 10 > query.totalRecords)
            {
                query.page = query.totalRecords / 10;
            }
            query.Records = settings.BuildOrderBys(JSONtoDynamicHelper.SaveDictionaryToJSON(dict)).ToPagerList(query.page);
            return View(query);
        }
    }
}