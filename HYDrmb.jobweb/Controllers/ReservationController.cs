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


        [HttpPost]
        [JWTAuth(true, level = 18)]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int[] ids)

        {
            var userid = AppManager.UserState.UserID;

            var matchedcount = _db.rmbReservation_view.Count(e => ids.Contains(e.Id));
            var matched = matchedcount == ids.Length;
            if (!matched)
            {
                return Json(new { status = "failure", msg = $"some reservations do not exist!" }, JsonRequestBehavior.AllowGet);
            }
            var success = rvsService.DeleteReservation(ids, AppManager.UserState.UserID);
            if (!success)
            {
                return Json(new { status = "failure", msg = $"Please check log for error detail!" }, JsonRequestBehavior.AllowGet);
            }


            string adhocreturnUrl = ViewBag.AdHocReturnUrl;

            return Json(new { status = "success", msg = "", returnUrl = adhocreturnUrl ?? Url.Action("Index", "Reservation") }, JsonRequestBehavior.AllowGet);
        }

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

        [HttpGet]
        public ActionResult Display(int id, bool fromCal = false)
        {
            ViewBag.ContentWidth = "";
            ViewBag.YesNoType = sttService.GetSettingFor(UI.SETT_YESNO).ToList();
            ViewBag.SESTypeOptions = sttService.GetSettingFor(UI.SETT_SESSNTYPE).ToList();
            var model = rvsService.GetReservation(id, AppManager.UserState?.UserID);
            ViewBag.TimeIntervalBag = TypeExtensions.GetTimeIntervals(DateTime.Today.AddHours(9), DateTime.Today.AddHours(18), 15);
            ViewBag.LocationType = sttService.GetSettingFor(UI.SETT_LOCATION).ToList();
            ViewBag.RoomType = sttService.GetSettingFor(UI.SETT_ROOMTYPE).ToList();

            ViewBag.EditEnabled = AppManager.UserState != null && AppManager.UserState.UserName == model.ContactName && AppManager.UserState.Post == model.ContactPost;

            var selfonly = (bool?)Session[Constants.Session.SESSION_SELFONLY] ?? false;
            if (fromCal)
            {
                TempData[Constants.Setting.ReturnUrl] = Url.Action("Calendar", "Reservation", new { selfonly = selfonly });
                TempData[Constants.Setting.fromCal] = true;
            }
            else
            {
                TempData[Constants.Setting.ReturnUrl] = Url.Action("Index", "Reservation", new { selfonly = selfonly });
            }
            return View(model);
        }

        [JWTAuth(true, level = 18)]
        public ActionResult Edit(int id = 0)
        {

            //remove the full-width if required
            ViewBag.ContentWidth = "";
            ViewBag.SESTypeOptions = sttService.GetSettingFor(UI.SETT_SESSNTYPE).ToList();
            ViewBag.YesNoType = sttService.GetSettingFor(UI.SETT_YESNO).ToList();
            var model = rvsService.GetReservation(id, AppManager.UserState.UserID);

            ViewBag.TimeIntervalBag = TypeExtensions.GetTimeIntervals(DateTime.Today.AddHours(9), DateTime.Today.AddHours(18), 15);
            ViewBag.LocationType = sttService.GetSettingFor(UI.SETT_LOCATION).ToList();
            ViewBag.RoomType = sttService.GetSettingFor(UI.SETT_ROOMTYPE).ToList();
            var fromCal = (bool?)TempData[Constants.Setting.fromCal] ?? false;
            var selfonly = (bool?)Session[Constants.Session.SESSION_SELFONLY] ?? false;

            if (fromCal)
            {
                TempData[Constants.Setting.ReturnUrl] = Url.Action("Calendar", "Reservation", new { selfonly = selfonly });

            }
            else
            {
                TempData[Constants.Setting.ReturnUrl] = Url.Action("Index", "Reservation", new { selfonly = selfonly });
            }

            return View(model as RmbReservationEditModel);
        }

        [JWTAuth(true, level = 18)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(RmbReservationEditModel model)
        {
            ViewBag.ContentWidth = "";
            if (ModelState.IsValid)
            {
                if (AppManager.UserState != null)
                {
                    var (start, end) =model.SessionType!=nameof(SessionType.CUSTOM) ? model.SessionType.GetSessionTimeFrame(): (model.SessionStart,model.SessionEnd);

                    var occupied = rvsService.IsOccupied(model.Id, start, end,model.RoomType);

                    if(!occupied)
                    {
                        var savevalid = rvsService.TransactionNow(() => rvsService.SaveReservation(model, AppManager.UserState.UserID), "Save Reservation");
                        if (savevalid)
                        {
                            //only book redirect
                            string returnUrl = TempData[Constants.Setting.ReturnUrl]?.ToString() ?? Url.Action("Index");

                            return Redirect(returnUrl);

                        }
                        else
                        {
                            ModelState.AddModelError(nameof(model.ContactName), "Please check log for internal error of saving reservation!");

                        }
                    }
                    else
                    {
                        ModelState.AddModelError(nameof(model.SessionDate), $"The {model.RoomType} is occupied on specified time frame!");
                    }


                }
                else
                {
                    ModelState.AddModelError(nameof(model.ContactName), "Login user must not be null!");
                }
            }

            //Please prepare the keyvalue mappings for model when error occur for resubmit
            ViewBag.YesNoType = sttService.GetSettingFor(UI.SETT_YESNO).ToList();
            ViewBag.SESTypeOptions = sttService.GetSettingFor(UI.SETT_SESSNTYPE).ToList();
            ViewBag.LocationType = sttService.GetSettingFor(UI.SETT_LOCATION).ToList();
            ViewBag.RoomType = sttService.GetSettingFor(UI.SETT_ROOMTYPE).ToList();

            ViewBag.TimeIntervalBag = TypeExtensions.GetTimeIntervals(DateTime.Today.AddHours(9), DateTime.Today.AddHours(18), 15);
            return View(model);
        }

    }
}