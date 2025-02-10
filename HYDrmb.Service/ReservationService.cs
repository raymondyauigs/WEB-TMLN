using HYDrmb.Abstraction;
using HYDrmb.Framework;
using HYDrmb.Framework.AppModel;
using HYDrmb.Framework.Tools;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
