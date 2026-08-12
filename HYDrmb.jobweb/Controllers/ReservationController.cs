using DocumentFormat.OpenXml.Office2010.Excel;
using HYDrmb.Abstraction;
using HYDrmb.Framework.AppModel;
using HYDrmb.jobweb.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
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
            ViewBag.RoomNameBag = sttService.GetSettingFor(UI.SETT_ROOMNAME).Select(e=> e.Key).ToArray();

            var model = new QueryReservationModel { SelfOnly = AppManager.UserState == null ? false : selfonly };
            Session[Constants.Session.SESSION_SELFONLY] = model.SelfOnly;
            Session[Constants.Session.SESSION_FROMCAL] = false;
            ViewBag.UserTag = model.SelfOnly ? ViewBag.UserTag : "!restricted";
            var notices = rvsService.IsBlockedOrErrors(DT.ConfRoomType, DT.MeetRoomType,DT.NookRoomType).ToArray();
            ViewBag.Notice =notices.Length >0 ? string.Join("\n",notices ): "";

            return View(model);
        }

        public ActionResult Calendar(string resourcetype,bool selfonly = false)
        {
            resourcetype = resourcetype ?? Session[Constants.Session.SESSION_RESRCTYPE]?.ToString();
            //var resource = _db.RmbResources.FirstOrDefault(e => e.ResourceType == resourcetype);
            var room = _db.RmbRooms.FirstOrDefault(e=> e.ResourceType == resourcetype);
            var resourcesuffix = resourcetype.Substring(resourcetype.IndexOf(".") + 1);
            //var alternateresource = _db.RmbResources.FirstOrDefault(e => e.ResourceType != resourcetype && e.ResourceType.EndsWith(resourcesuffix));
            var alternateresources = _db.RmbRooms.Where(e=> e.ResourceType != resourcetype).ToArray();
            var alternateRoom = alternateresources[0];
            var nextRoom = alternateresources[1];
            ViewBag.ResourceBtnColors = new Dictionary<string, string> { { DT.ConfRoomType, "btn-amber" }, { DT.MeetRoomType, "btn-pink" }, { DT.NookRoomType, "btn-teal" } };
            Session[Constants.Session.SESSION_EXCELEXPORT] = "".RandomString(8).RandomString(8);

            var model = new QueryReservationCalendarViewModel { SelfOnly = AppManager.UserState == null ? false : selfonly, ResourceType = room?.ResourceType, ResourceName = room?.RoomName, AlternateResourceName = alternateRoom.RoomName, AlternateResourceType = alternateRoom.ResourceType, NextResourceName = nextRoom.RoomName, NextResourceType= nextRoom.ResourceType };
            Session[Constants.Session.SESSION_SELFONLY] = model.SelfOnly;
            Session[Constants.Session.SESSION_RESRCTYPE] = model.ResourceType;
            Session[Constants.Session.SESSION_FROMCAL] = true;
            ViewBag.UserTag = model.SelfOnly ? ViewBag.UserTag : "!restricted";
            return View(model);
        }

        [HttpGet]
        public ActionResult Display(int id, bool fromCal = false)
        {
            ViewBag.ContentWidth = "";
            ViewBag.YesNoType = sttService.GetSettingFor(UI.SETT_YESNO).ToList();
            ViewBag.SESTypeOptions = sttService.GetSettingFor(UI.SETT_SESSNTYPE).ToList();
            var model = rvsService.GetReservation(id, AppManager.UserState?.UserID,true);
            if (model.Id == 0)
            {
                model = new RmbReservationEditModel();
            }
            ViewBag.TimeIntervalBag = TypeExtensions.GetTimeIntervals(DateTime.Today.AddHours(9), DateTime.Today.AddHours(18), 15);
            ViewBag.LocationType = sttService.GetSettingFor(UI.SETT_LOCATION).ToList();
            ViewBag.RoomName = sttService.GetSettingFor(UI.SETT_ROOMNAME).ToList();

            ViewBag.EditEnabled = AppManager.UserState != null && AppManager.UserState.UserName == model.ContactName && AppManager.UserState.Post == model.ContactPost;

            var selfonly = (bool?)Session[Constants.Session.SESSION_SELFONLY] ?? false;
            if (fromCal)
            {
                TempData[Constants.Setting.ReturnUrl] = Url.Action("Calendar", "Reservation", new { selfonly = selfonly });
                Session[Constants.Session.SESSION_FROMCAL] = true;
            }
            else
            {
                TempData[Constants.Setting.ReturnUrl] = Url.Action("Index", "Reservation", new { selfonly = selfonly });
            }

            return View(model);
        }



        [JWTAuth(true, level = 18)]
        public ActionResult Edit(int id = 0,string datestr=null)
        {
            var date = TypeExtensions.TryValue<DateTime>(datestr, DateTime.Today);
            //remove the full-width if required
            ViewBag.ContentWidth = "";
            ViewBag.SESTypeOptions = sttService.GetSettingFor(UI.SETT_SESSNTYPE).ToList();
            ViewBag.YesNoType = sttService.GetSettingFor(UI.SETT_YESNO).ToList();
            var model = rvsService.GetReservation(id, AppManager.UserState.UserID);

            ViewBag.TimeIntervalBag = TypeExtensions.GetTimeIntervals(DateTime.Today.AddHours(9), DateTime.Today.AddHours(18), 15);
            ViewBag.LocationType = sttService.GetSettingFor(UI.SETT_LOCATION).ToList();
            ViewBag.RoomName = sttService.GetSettingFor(UI.SETT_ROOMNAMELOC).ToList();
            var fromCal = (bool?)Session[Constants.Session.SESSION_FROMCAL] ?? false;
            var selfonly = (bool?)Session[Constants.Session.SESSION_SELFONLY] ?? false;

            var resourcetype = Session[Constants.Session.SESSION_RESRCTYPE]?.ToString() ?? DT.ConfRoomType;

            if (date != null && model.Id == 0)
            {
                //var resourcename = _db.RmbRooms.FirstOrDefault(e => e.ResourceType == resourcetype)?.RoomName;
                model.SessionDate = date;
                model.SessionType = nameof(SessionType.FULL);
                //model.RoomName = resourcename;
                //model.RoomType = resourcetype;
            }


            if (fromCal)
            {


                TempData[Constants.Setting.ReturnUrl] = Url.Action("Calendar", "Reservation", new { selfonly = selfonly, resourcetype });

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
                    var (start, end) =model.SessionType!=nameof(SessionType.CUSTOM) ? model.SessionType.GetSessionTimeFrame(model.SessionDate): TypeExtensions.RenewDate(model.SessionStart,model.SessionEnd, model.SessionDate);
                    var roomType = rvsService.ToRoomType(model.RoomName);
                    var occupied = rvsService.IsOccupied(model.Id, start, end,roomType);

                    if(!occupied)
                    {
                        string error = "";
                        var savevalid = rvsService.TransactionNow(() => rvsService.SaveReservation(model, AppManager.UserState.UserID,out error), "Save Reservation");
                        if (savevalid)
                        {
                            var baseurl = Session[Constants.Session.SESSION_BASEURL];
                            rvsService.NotifyBooking(model.Id, null,null, "sys_programmer.bstr@hyd.gov.hk", baseurl + Url.Action("Display", "Reservation", new { id = model.Id }), "Please click here for booking details.");


                            //only book redirect
                            string returnUrl = TempData[Constants.Setting.ReturnUrl]?.ToString() ?? Url.Action("Index");

                            return Redirect(returnUrl);

                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(error))
                            {
                                var errorparts = error.ItSplit("|").ToArray();
                                ModelState.AddModelError(errorparts[0], errorparts[1]);
                            }
                                
                            else
                                ModelState.AddModelError(nameof(model.ContactName), "Please check log for internal error of saving reservation!");

                        }
                    }
                    else
                    {
                        ModelState.AddModelError(nameof(model.SessionDate), $"The {model.RoomName} is occupied on specified time frame!");
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
            ViewBag.RoomName = sttService.GetSettingFor(UI.SETT_ROOMNAMELOC).ToList();

            ViewBag.TimeIntervalBag = TypeExtensions.GetTimeIntervals(DateTime.Today.AddHours(9), DateTime.Today.AddHours(18), 15);
            return View(model);
        }

    }
}