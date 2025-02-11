using HYDrmb.Abstraction;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Ux = HYDrmb.Abstraction.UtilExtensions;
using HYDrmb.Framework.metadata;

namespace HYDrmb.Framework.AppModel
{
    public class EditModels
    {
        public static EditModels _model;
        public static EditModels Default
        {
            get
            {
                if( _model == null )
                    _model = new EditModels();
                return _model;
            }
        }

        public Dictionary<string, string> EventColors = new Dictionary<string, string>()
        {
            {nameof(Constants.Setting.backcolorAM),"#ed7d31"},
            {nameof(Constants.Setting.backcolorPM),"#00b0f0"},
            {nameof(Constants.Setting.backcolorFULL),"#7030a0"},
            {nameof(Constants.Setting.backcolorCUSTOM),"#00b050"},

        };

        public static Dictionary<string, string> _KEYVALS = new Dictionary<string, string>()
        {
            { "Yes / N.A.","YesNA" },
            { "Yes, but exit solely connects to at-grade non-BFA footpath or staircase","YesExit" },
            { "Full Day","FULL" },
            { "Specific Time","CUSTOM" },
            {"AM","AM" },
            {"PM","PM" },
        };
        static EditModels()
        {
         

            var bkdsettingmap = new[] { 
                Ux.AsPairExpr<EditSettingModel,CoreSetting>(e=> e.updatedAt,null), 
                Ux.AsPairExpr<EditSettingModel, CoreSetting>(e => e.updatedBy, null) 
            };
            Ux.GiveBind(bkdsettingmap);
            Ux.GiveBind<CoreSetting, EditSettingModel>();

            var rv_reservationmap = new[] { Ux.AsPairExpr<rmbReservation_view,RmbReservationEditModel>(e=> e.ReservedStartAt,e=> e.SessionStart),
            Ux.AsPairExpr<rmbReservation_view,RmbReservationEditModel>(e=> e.ReservedEndAt,e=> e.SesssionEnd),
            Ux.AsPairExpr<rmbReservation_view,RmbReservationEditModel>(e=> e.ReservedDate,e=> e.SessionDate),
            
            };
            var reservationmap = new[]
            {
                Ux.AsPairExpr<RmbReservationEditModel,RmbReservation>(e=> e.updatedAt,null),
                Ux.AsPairExpr<RmbReservationEditModel,RmbReservation>(e=> e.updatedBy,null),
            };
            Ux.GiveBind(reservationmap);
            var reservermobjmap = new[]
            {
                Ux.AsPairExpr<RmbReservationEditModel,RmbReservedItem>(e=> e.updatedAt,null),
                Ux.AsPairExpr<RmbReservationEditModel,RmbReservedItem>(e=> e.updatedBy,null),
                Ux.AsPairExpr<RmbReservationEditModel,RmbReservedItem>(e=> e.RoomObjectId,e=> e.Id),
                Ux.AsPairExpr<RmbReservationEditModel,RmbReservedItem>(e=> e.Id,e=> e.ReservationId),
                
            };
            Ux.GiveBind(reservermobjmap);


        }
        public static  string RevertValue(string desc)
        {
            var foundpair = new KeyValuePair<string, string>(desc, desc);
            if (_KEYVALS.ContainsKey(desc) || _KEYVALS.ContainsValue(desc))
            {
                foundpair = _KEYVALS.First(e => e.Key == desc || e.Value == desc);
            }
            return foundpair.Value;


        }




    }

    public class EventModel : IEventModel
    {
        public string id { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public string title { get; set; }

        public bool allDay { get; set; }

        public string backgroundColor { get; set; }

        public string SessionType { get; set; }

    }

    [MetadataType(typeof(SettingEdit_MetaData))]
    public class EditSettingModel: IEditSettingModel
    {
        public int Id { get; set; }
        public string SettingId { get; set; }
        public string SettingValue { get; set; }

        public bool CanEdit { get; set; }

        public Nullable<System.DateTime> updatedAt { get; set; }
        public string updatedBy { get; set; }


    }



    public class TabHeaderModel : ITabModel
    {
        public int MaxCount { get; set; }
        public string[] Titles { get; set; }
    }
    [MetadataType(typeof(RmbReservationEdit_MetaData))]
    public class RmbReservationEditModel: IRmbReservationEditModel
    {
        public int Id { get; set; }
        public DateTime SessionDate { get; set; }

        public DateTime SessionStart { get; set; }

        public DateTime SesssionEnd { get; set; }

        public int startTime { get; set; }
        public int endTime { get; set; }

        public int MaxPeriod { get; set; }

        public string SessionType { get; set; }

        public string ContactName { get; set; }

        public string ContactNumber { get; set; }

        public string ContactPost { get; set; }

        public string RoomType { get; set; }

        public int RoomObjectId { get; set; }

        public string LocationType { get; set; }

        public string Remarks { get; set; }

        public DateTime? updatedAt { get; set; }
        public string updatedBy { get; set; }

        public bool Invalid { get; set; }

    }


    public class UrlModel : IUrlModel
    {
        public string UrlTitle { get; set; }

        public int MaxCount { get; set; }

        public string BaseUrl { get; set; }
        public IUrl[] Urls { get; set; }

        public int InitStart { get; set; }

        public IUrlModel ExtractModel(int init, int len)
        {

            return new UrlModel
            {
                Urls = this.Urls,
                BaseUrl = this.BaseUrl,
                InitStart = init,
                MaxCount = len,
                UrlTitle = this.UrlTitle
            };
        }

    }

    public class UrlItem: IUrl
    {
        public string Url { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }

        public int Thumb { get; set; }
    }

}
