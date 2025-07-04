﻿using HYDrmb.Abstraction;
using HYDrmb.Framework;
using HYDrmb.Framework.AppModel;
using HYDrmb.Framework.Tools;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;
using DT=HYDrmb.Abstraction.Constants.DT;
using Ux = HYDrmb.Abstraction.UtilExtensions;

using static HYDrmb.Abstraction.Constants;

namespace HYDrmb.Service
{
    public class ReservationService : BaseDbService, IReservationService
    {
        IMemoryCache mmCache;
        ISettingService sttService;
        public ReservationService(HYDrmbEntities mdb,IMiscLog mlog,IMemoryCache memoryCache,ISettingService settingService) {

            db = mdb;
            mmCache = memoryCache;
            sttService = settingService;
            log = mlog;
        }

        public bool DeleteReservation(int[] ids, string userid)
        {
            try
            {
                foreach (var bk in db.RmbReservations.Where(e => ids.Contains(e.Id) && !e.Invalid))
                {
                    bk.updatedAt = DateTime.Now;
                    bk.updatedBy = userid;
                    bk.Invalid = true;
                    db.Entry(bk).State = EntityState.Modified;

                }
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                log.LogMisc(ex.Message, ex);
                return false;
            }

        }

        public bool IsOccupied(int id, DateTime start, DateTime end, string roomtype)
        {
            var dateOfReserve = start.Date;


            var occupiedlist = db.rmbReservation_view.Where(e => e.Id != id && e.ReservedDate == dateOfReserve && e.RoomType == roomtype).Select(e => new { e.Id, e.ReservedStartAt, e.ReservedEndAt }).ToList().
                Select(e => new FromTillModel { ReservationId = e.Id, TimeFrom = int.Parse(e.ReservedStartAt.ToString("HHmm")), TimeTill = int.Parse(e.ReservedEndAt.ToString("HHmm")) }).ToList();
            occupiedlist.Add(new FromTillModel { ReservationId = id, TimeFrom = int.Parse(start.ToString("HHmm")), TimeTill = int.Parse(end.ToString("HHmm")) });
            var occupied= UtilExtensions.IsOverlaps(occupiedlist.ToArray());


            return occupied;
        }
        public IRmbReservationEditModel GetReservation(int id, string userid)
        {
            var reservation = db.rmbReservation_view.FirstOrDefault(e => e.Id == id);
            var resourcetype = db.RmbResources.FirstOrDefault(e => e.ResourceType == "Meet.Room");
            var hasreserve = reservation != null;
            var reservationmodel = hasreserve ? reservation.MapTo(new RmbReservationEditModel()) : new RmbReservationEditModel();

            if(!hasreserve)
            {
                reservationmodel.SessionDate = DateTime.Today;
                var nearestStartEnd = DateTime.Now.GetNearestTimeFrame();
                reservationmodel.SessionStart = nearestStartEnd.Key;
                reservationmodel.SessionEnd = nearestStartEnd.Value;
                reservationmodel.LocationType = DT.LOC_TI;
                reservationmodel.SessionType = TypeExtensions.GetSessionType(reservationmodel.SessionStart, reservationmodel.SessionEnd);
                reservationmodel.RoomType = resourcetype.ResourceName;
            }
            else
            {
                reservationmodel.SessionDate = reservationmodel.SessionStart.Date;



            }
            reservationmodel.MaxPeriod = 9999;
            if(userid!=null)
            {
                userid = userid.StartsWith("u!") ? userid.Substring(2) : userid;
                var useridvalue = TypeExtensions.TryValue<int>(userid, 0);
                var user = db.CoreUsers.FirstOrDefault(e => (e.Id == useridvalue || e.UserId == userid) && !e.Disabled);
                //reservationmodel.MaxPeriod = (user.level <= 9 || user.IsAdmin) ? 9999 : 14;

                if (reservationmodel.Id == 0)
                {
                    //only set on creation , edit could not change the following fields
                    reservationmodel.ContactName = user.UserName;
                    reservationmodel.ContactPost = user.post;
                    reservationmodel.ContactNumber = user.tel;
                    return reservationmodel;
                }

            }
            return reservationmodel;

        }

        public IEnumerable<IEventModel> GetEvents(string resourcetype, bool selfonly, string userid, string fromdate, string todate, Dictionary<string, string> colors)
        {
            var dateFrom = UtilExtensions.ParseDateOrDefault(fromdate, "yyyyMMdd", new DateTime(1900, 01, 01));
            var dateTo = UtilExtensions.ParseDateOrDefault(todate, "yyyyMMdd", DateTime.MaxValue).AddDays(1);
            var AMcolor = colors["backcolor" + nameof(SessionType.AM)];
            var PMcolor = colors["backcolor" + nameof(SessionType.PM)];
            var FULLcolor = colors["backcolor" + nameof(SessionType.FULL)];
            var CMcolor = colors["backcolor" + nameof(SessionType.CUSTOM)];
            userid = userid.StartsWith("u!") && selfonly ? userid.Substring(2) : userid;
            var founduser = db.CoreUsers.FirstOrDefault(e => (e.UserId == userid || e.Id.ToString() == userid) && !e.Disabled);

            var query = db.rmbReservation_view.Where(e => e.ReservedStartAt >= dateFrom && e.ReservedEndAt <= dateTo && (resourcetype==null || e.ResourceType == resourcetype));

            if (founduser != null && !founduser.IsAdmin && selfonly)
            {
                query = query.Where(e => e.ContactPost == founduser.post || e.ContactName == founduser.UserName);
            }

            var events = query.Select(e => new EventModel { start = e.ReservedStartAt, end = e.ReservedEndAt, title = e.ContactName + " (" + e.ContactPost + ")", allDay = false, SessionType = e.SessionType, id = e.Id.ToString() });
            foreach (var d in events)
            {
                var v = d;
                v.backgroundColor = colors["backcolor" + v.SessionType];
                yield return v;
            }

        }

        public IEnumerable<IviewReservation> GetReservation(bool selfonly, string userid, string fromdate, string todate, string search, string type, string colid = nameof(IviewReservation.ReservedStartAt), string sort = "desc")
        {
            var searchvalues = UtilExtensions.ZipStrPair(search, type);
            var dateFrom = UtilExtensions.ParseDateOrDefault(fromdate, "yyyyMMdd", new DateTime(1900, 01, 01));
            var dateTo = UtilExtensions.ParseDateOrDefault(todate, "yyyyMMdd", new DateTime(9999, 12, 30)).AddDays(1);
            var data = Filter<rmbReservation_view>(searchvalues);

            userid = userid.StartsWith("u!") && selfonly ? userid.Substring(2) : userid;
            var founduser = db.CoreUsers.FirstOrDefault(e => (e.UserId == userid || e.Id.ToString() == userid) && !e.Disabled);
            data = data.Where(e => e.ReservedStartAt >= dateFrom && e.ReservedEndAt <= dateTo);
            if (founduser != null && !founduser.IsAdmin && selfonly)
            {
                data = data.Where(e => e.ContactPost == founduser.post || e.ContactName == founduser.UserName);
            }

            var ordering = new List<System.ComponentModel.SortDescription>();
            var reservedStartOrder = new System.ComponentModel.SortDescription { Direction = System.ComponentModel.ListSortDirection.Descending, PropertyName = nameof(rmbReservation_view.ReservedStartAt) };

            if (colid != nameof(IviewReservation.ReservedStartAt))
            {
                ordering.Add(reservedStartOrder);
                ordering.Add(new System.ComponentModel.SortDescription { Direction = sort == "asc" ? System.ComponentModel.ListSortDirection.Ascending : System.ComponentModel.ListSortDirection.Descending, PropertyName = colid });
                data = data.BuildOrder(ordering.ToArray());

            }
            else
            {
                reservedStartOrder.Direction = sort == "asc" ? System.ComponentModel.ListSortDirection.Ascending : System.ComponentModel.ListSortDirection.Descending;
                ordering.Add(reservedStartOrder);
                data = data.BuildOrder(ordering.ToArray());
            }

            if (!selfonly || founduser != null)
            {
                foreach (var d in data)
                    yield return d;
            }
        }

        public bool NotifyBooking(int bookingId, string to, string cc, string from, string url, string title, string templatefile = null)
        {
            try
            {



                if (string.IsNullOrEmpty(templatefile))
                    templatefile = Setting.emailfile.GetAppKeyValue();
                if (string.IsNullOrEmpty(url))
                {
                    log.LogMisc($"NotifyBooking: Booking url not available.");
                    return false;
                }
                var foundbooking = db.rmbReservation_view.FirstOrDefault(e => e.Id == bookingId);
                var notifysetting = sttService.GetSettingFor(UI.SETT_NOTIFY).ToDictionary(e => e.Key, e => e.Value);
                if (foundbooking == null)
                {
                    log.LogMisc($"NotifyBooking: Booking with ID {bookingId} not found.");
                    return false;
                }
                var user = db.CoreUsers.FirstOrDefault(e => e.post == foundbooking.ContactPost && !e.Disabled);
                if (string.IsNullOrEmpty(to) && string.IsNullOrEmpty(notifysetting[UI.NOTIFY_TO]))
                    to = user.email;
                else if(string.IsNullOrEmpty(to))
                    to = notifysetting[UI.NOTIFY_TO];

                if (string.IsNullOrEmpty(cc))
                {
                    cc = notifysetting[UI.NOTIFY_CC];
                }
                var BookingDate = foundbooking.ReservedStartAt.ToString("dd/MM/yyyy");
                var RoomName = foundbooking.RoomName;
                var Remarks = foundbooking.Remarks;
                var specifictime =  foundbooking.ReservedStartAt.ToString("HH:mm")+ " - " + foundbooking.ReservedEndAt.ToString("HH:mm");
                var SessionType = foundbooking.SessionType == nameof(PreferenceType.FULL) ? DT.WHOLE : (
                    foundbooking.SessionType == nameof(PreferenceType.CUSTOM) ? $"{DT.CUSTOM}[{specifictime}]" : foundbooking.SessionType
                    );

                Func<string, string> bodyfiller = (body) =>
                {
                    body = body.Replace("{BookingDate}", BookingDate)
                               .Replace("{SessionType}", SessionType)
                               .Replace("{RoomName}", RoomName)
                               .Replace("{Remarks}", Remarks)
                               .Replace("{Url}", url.Replace("{id}", foundbooking.Id.ToString()))
                               .Replace("{Title}", title);
                    return body;
                };
                log.LogMisc("Room Booking Notification Start!");
                var error = Ux.Notify(from, cc, to, bodyfiller, notifysetting[UI.NOTIFY_SUBJ], templatefile, notifysetting[UI.NOTIFY_SERVER], int.Parse(notifysetting[UI.NOTIFY_PORT]));
                if(!string.IsNullOrEmpty(error))
                {
                    log.LogMisc("Room Booking Notification Error!");
                    log.LogMisc(error);
                    return false;
                }
                return true;

            }
            catch (Exception ex)
            {
                log.LogMisc("Room Booking Notification Error!");
                log.LogMisc(ex.Message, ex);
                return false;
            }
        }
        public bool SaveReservation(IRmbReservationEditModel model, string userid)
        {
            try
            {
                
                var realmodel = model as RmbReservationEditModel;
                var refDate = realmodel.SessionDate;
                if (realmodel.SessionType == nameof(SessionType.AM))
                {
                    realmodel.SessionStart = refDate.AddHours(9);
                    realmodel.SessionEnd = refDate.AddHours(12).AddMinutes(30);

                }
                else if (realmodel.SessionType == nameof(SessionType.PM))
                {
                    realmodel.SessionStart = refDate.AddHours(13).AddMinutes(30);
                    realmodel.SessionEnd = refDate.AddHours(18);
                }
                else if (realmodel.SessionType == nameof(SessionType.FULL))
                {
                    realmodel.SessionStart = refDate.AddHours(9);
                    realmodel.SessionEnd = refDate.AddHours(18);
                }
                else
                {
                    (realmodel.SessionStart, realmodel.SessionEnd) = TypeExtensions.RenewDate(realmodel.SessionStart, realmodel.SessionEnd, refDate);
                }
                

                var foundmodel = db.RmbReservations.Include(v=> v.RmbReservedItems).FirstOrDefault(e => e.Id == model.Id);
                foundmodel = foundmodel ?? new RmbReservation();
                foundmodel = realmodel.MapTo(foundmodel);
                foundmodel.updatedAt = DateTime.Now;
                foundmodel.updatedBy = userid;


                db.Entry(foundmodel).State = model.Id > 0 ? System.Data.Entity.EntityState.Modified : System.Data.Entity.EntityState.Added;
                var realimodel = realmodel.MapTo(new RmbReservedItem());
                var realresource = db.RmbRooms.FirstOrDefault(e => e.RoomName == realmodel.RoomType);
                if(realresource==null)
                {
                    log.LogMisc($"The real real resource for {realmodel.RoomType} could not be found!");
                    return false;
                }
                realimodel.ResourceType = realresource?.ResourceType;
                realimodel.ReservedObjectId = realresource.Id;
                

                if (model.Id == 0)
                {
                    db.RmbReservations.Add(foundmodel);                    
                }
                else
                {
                    
                    db.RmbReservedItems.RemoveRange(foundmodel.RmbReservedItems);
                    db.SaveChanges();
                    realimodel.RmbReservation = foundmodel;
                }
                foundmodel.RmbReservedItems.Add(realimodel);
                

                db.Entry(realimodel).State = System.Data.Entity.EntityState.Added;

                db.SaveChanges();

                model.Id = foundmodel.Id;
                return true;

            }
            catch (Exception ex)
            {
                log.LogMisc("saving error!");
                log.LogMisc(ex.Message, ex);
            }
            return false;
        }
    }
}
