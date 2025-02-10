using HYDrmb.Abstraction;
using HYDrmb.Framework.AppModel;
using HYDrmb.jobweb.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static HYDrmb.Abstraction.Constants;

namespace HYDrmb.jobweb.Controllers
{
    public class ReservationController : BaseController
    {
        // GET: Reservation
        public ActionResult Index(bool selfonly=false)
        {
            Session[Constants.Session.SESSION_EXCELEXPORT] = "".RandomString(8).RandomString(8);


            ViewBag.YesNoBag = sttService.GetSettingFor(UI.SETT_YESNO).Select(e => e.Key).ToArray();
            ViewBag.UserPostBag = sttService.GetSettingFor(UI.SETT_UPOSTTYPE).Select(e => e.Key).ToArray();
            ViewBag.SessionTypeBag = sttService.GetSettingFor(UI.SETT_SESSNTYPE).Select(e => e.Key).ToArray();
            ViewBag.RoomTypeBag = sttService.GetSettingFor(UI.SETT_ROOMTYPE).Select(e=> e.Key).ToArray();

            var model = new QueryReservationModel { SelfOnly = AppManager.UserState == null ? false : selfonly };
            Session[Constants.Session.SESSION_SELFONLY] = model.SelfOnly;
            ViewBag.UserTag = model.SelfOnly ? ViewBag.UserTag : "!restricted";
            return View(model);
        }

        public ActionResult Calendar(bool selfonly = false)
        {
            Session[Constants.Session.SESSION_EXCELEXPORT] = "".RandomString(8).RandomString(8);

            var model = new QueryReservationCalendarViewModel { SelfOnly = AppManager.UserState == null ? false : selfonly };
            Session[Constants.Session.SESSION_SELFONLY] = model.SelfOnly;
            ViewBag.UserTag = model.SelfOnly ? ViewBag.UserTag : "!restricted";
            return View(model);
        }
    }
}