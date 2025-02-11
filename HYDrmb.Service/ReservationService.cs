using HYDrmb.Abstraction;
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

namespace HYDrmb.Service
{
    public class ReservationService : BaseDbService, IReservationService
    {
        IMemoryCache mmCache;
        public ReservationService(HYDrmbEntities mdb,IMiscLog mlog,IMemoryCache memoryCache) {

            db = mdb;
            mmCache = memoryCache;

        }

        public bool DeleteReservation(int[] ids, string userid)
        {
            throw new NotImplementedException();
        }


        public IRmbReservationEditModel GetReservation(int id, string userid)
        {
            var reservation = db.rmbReservation_view.FirstOrDefault(e => e.Id == id);
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
                
            }
            else
            {
                reservationmodel.SessionDate = reservationmodel.SessionStart.Date;



            }
            reservationmodel.MaxPeriod = 14;
            if(userid!=null)
            {
                userid = userid.StartsWith("u!") ? userid.Substring(2) : userid;
                var useridvalue = TypeExtensions.TryValue<int>(userid, 0);
                var user = db.CoreUsers.FirstOrDefault(e => e.Id == useridvalue || e.UserId == userid);
                reservationmodel.MaxPeriod = (user.level <= 9 || user.IsAdmin) ? 9999 : 14;

                if (reservationmodel.Id == 0)
                {
                    //only set on creation , edit could not change the following fields
                    reservationmodel.ContactName = user.UserName;
                    reservationmodel.ContactPost = user.post;
                    
                    return reservationmodel;
                }

            }
            return reservationmodel;

        }

        public IEnumerable<IEventModel> GetEvents(bool selfonly, string userid, string fromdate, string todate, Dictionary<string, string> colors)
        {
            var dateFrom = UtilExtensions.ParseDateOrDefault(fromdate, "yyyyMMdd", new DateTime(1900, 01, 01));
            var dateTo = UtilExtensions.ParseDateOrDefault(todate, "yyyyMMdd", DateTime.MaxValue).AddDays(1);
            var AMcolor = colors["backcolor" + nameof(SessionType.AM)];
            var PMcolor = colors["backcolor" + nameof(SessionType.PM)];
            var FULLcolor = colors["backcolor" + nameof(SessionType.FULL)];
            var CMcolor = colors["backcolor" + nameof(SessionType.CUSTOM)];
            userid = userid.StartsWith("u!") && selfonly ? userid.Substring(2) : userid;
            var founduser = db.CoreUsers.FirstOrDefault(e => e.UserId == userid || e.Id.ToString() == userid);

            var query = db.rmbReservation_view.Where(e => e.ReservedStartAt >= dateFrom && e.ReservedEndAt <= dateTo);

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
            var founduser = db.CoreUsers.FirstOrDefault(e => e.UserId == userid || e.Id.ToString() == userid);
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

        public bool SaveReservation(IRmbReservationEditModel model, string userid)
        {
            try
            {
                var realmodel = model as RmbReservationEditModel;
                if (realmodel.SessionType == nameof(SessionType.AM))
                {
                    realmodel.SessionStart = DateTime.Today.AddHours(9);
                    realmodel.SessionEnd = DateTime.Today.AddHours(12).AddMinutes(30);

                }
                else if (realmodel.SessionType == nameof(SessionType.PM))
                {
                    realmodel.SessionStart = DateTime.Today.AddHours(13).AddMinutes(30);
                    realmodel.SessionEnd = DateTime.Today.AddHours(18);
                }
                else if (realmodel.SessionType == nameof(SessionType.FULL))
                {
                    realmodel.SessionStart = DateTime.Today.AddHours(9);
                    realmodel.SessionEnd = DateTime.Today.AddHours(18);
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
                    foundmodel.RmbReservedItems.Clear();                    
                }
                foundmodel.RmbReservedItems.Add(realimodel);
                db.Entry(realimodel).State = System.Data.Entity.EntityState.Added;

                db.SaveChanges();
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
