using HYDrmb.Abstraction;
using HYDrmb.Framework;
using Microsoft.Ajax.Utilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using UI = HYDrmb.Abstraction.Constants.UI;
using DT = HYDrmb.Abstraction.Constants.DT;
using System.Threading;
using System.Web.Http.Controllers;
using HYDrmb.Framework.AppModel;
using System.Data.Entity;

namespace HYDrmb.jobweb.Controllers
{
    [RoutePrefix("api/autocomplete")]
    public class AutoCompleteController : ApiController
    {
        private EditModels _mapping;
        public AutoCompleteController(HYDrmbEntities mdb, IMiscLog mlog, IStdbLog mdlog, IErrorLog mrlog, IMemoryCache mmCache, ISettingService settingService,IReservationService reservationService, EditModels mapping)
        {

            db = mdb;
            errorLog = mrlog;
            miscLog = mlog;
            stdbLog = mdlog;
            mCache = mmCache;
            
            sttService = settingService;
            rsvService = reservationService;
            db.Database.Log = (msg) => { stdbLog.LogStdb(msg.TrimEnd(Environment.NewLine.ToCharArray())); };

                

            SqlConnection sqlConn = (SqlConnection)db.Database.Connection;
            sqlConn.InfoMessage -= SqlConn_InfoMessage;
            sqlConn.InfoMessage += SqlConn_InfoMessage;
            
            _mapping=mapping;

            var colorkeys = _mapping.EventColors.Keys;
            foreach (var csetting in db.CoreSettings.Where(e => colorkeys.Contains(e.SettingId)))
            {
                _mapping.EventColors[csetting.SettingId] = csetting.SettingValue;
            }
        }


        protected IMiscLog miscLog;
        protected IErrorLog errorLog;
        protected IStdbLog stdbLog;
        private HYDrmbEntities db;

        private ISettingService sttService;
        private IReservationService rsvService;
        
        protected IMemoryCache mCache;

        private T GetService<T>()
        {
            return (T)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(T));
        }
        private void SqlConn_InfoMessage(object sender, SqlInfoMessageEventArgs args)
        {
            foreach (SqlError err in args.Errors)
            {
                string strOutput = string.Format("Procedure [{0}] Line [{1}]: {2}", err.Procedure, err.LineNumber, err.Message.TrimEnd(Environment.NewLine.ToCharArray()));
                errorLog.LogError(strOutput);
            }
        }

        public override Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
        {
            return base.ExecuteAsync(controllerContext, cancellationToken);
        }



        [ResponseType(typeof(IEnumerable<KeyValuePair<string, string>>))]
        [HttpGet]
        [Route("levelname")]
        public async Task<IHttpActionResult> Getlevelname(string subtext)
        {
            subtext = (subtext ?? "").Trim();
            var defaultvalue = new KeyValuePair<string, string>("", "");
            var data = new[]{
                new KeyValuePair<int, string>(0, Constants.UI.NAME_ADMINISTRATOR),                
                new KeyValuePair<int, string>(8, Constants.UI.NAME_POWERUSER),
                new KeyValuePair<int, string>(18, Constants.UI.NAME_NORMALUSER)
                    };

            var includes = new[] { subtext };
            var listofoptions = await data.Where(x => string.IsNullOrEmpty(subtext) || includes.Contains(x.Value)).ToAsyncEnumerable().ToArrayAsync();

            return Ok(listofoptions);


        }

        [ResponseType(typeof(rmbReservation_view[]))]
        [HttpGet]
        [Route("reservationrecords/{userid}/{datefrom}/{dateto}/{search?}/{type?}/{colid?}/{sort?}")]
        [Route("reservationrecords/{userid}/{datefrom}/{dateto}/sortonly/{colid?}/{sort?}")]
        public async Task<IHttpActionResult> GetReservation(string userid, string datefrom, string dateto, string search = null, string type = null, string colid = nameof(IviewReservation.ReservedStartAt), string sort = "desc")
        {


            try
            {
                if (search == "*")
                {
                    search = null;
                    type = null;
                }
                object context;
                var selfonly = false;
                string excelexportkey = string.Empty;

                if (Request.Properties.TryGetValue("MS_HttpContext", out context))
                {
                    var httpcontext = context as HttpContextBase;
                    if (httpcontext != null && httpcontext.Session != null)
                    {
                        miscLog.LogMisc("Web Api can read the session now");
                        excelexportkey = httpcontext.Session[Constants.Session.SESSION_EXCELEXPORT]?.ToString();

                        selfonly = TypeExtensions.TryValue(httpcontext.Session[Constants.Session.SESSION_SELFONLY]?.ToString(), selfonly);


                    }
                }

                //escape the '&' sign in search string.
                if (search != null)
                    search = search.Replace("`3", "&").Replace("`8", "/");

                var data = rsvService.GetReservation(selfonly, userid, datefrom, dateto, search, type, colid, sort);

                var result = await data.OfType<rmbReservation_view>().ToAsyncEnumerable().ToListAsync();

                return Ok(result);

            }
            catch (Exception e)
            {
                miscLog.LogMisc(e.Message, e);
            }

            var dummy = new rmbReservation_view[] { }.ToAsyncEnumerable().ToArrayAsync();

            return Ok(dummy);


        }

        [ResponseType(typeof(IEnumerable<KeyValuePair<string, string>>))]
        [HttpGet]
        [Route("wantedtype")]
        public async Task<IHttpActionResult> WantedType(string subtext, string propname = "", string exclude = "*")
        {
            var defaultvalue = new KeyValuePair<string, string>("", "");
            KeyValuePair<int, string>[] tagitems = null;
            var includes = new[] { defaultvalue };

            if(propname == nameof(AutoWantedType.IsVIP) || propname == nameof(AutoWantedType.InQuestion))
            {

                var settings =await sttService.GetSettingFor(UI.SETT_YESNO).Select((e, i) => new KeyValuePair<int, string>(i, e.Key)).ToAsyncEnumerable().ToArrayAsync();
                return Ok(settings);
                
            }
            else if(propname == nameof(AutoWantedType.RoomType))
            {
                var settings = await sttService.GetSettingFor(UI.SETT_ROOMTYPE).Select((e, i) => new KeyValuePair<int, string>(i, e.Key)).ToAsyncEnumerable().ToArrayAsync();
                return Ok(settings);
            }
            else if(propname == nameof(AutoWantedType.UserPost))
            {
                var settings = await sttService.GetSettingFor(UI.SETT_UPOSTTYPE).Select((e, i) => new KeyValuePair<int, string>(i, e.Key)).ToAsyncEnumerable().ToArrayAsync();
                return Ok(settings);

            }
            else if(propname == nameof(AutoWantedType.SessionType))
            {
                var settings = await sttService.GetSettingFor(UI.SETT_SESSNTYPE).Select((e, i) => new KeyValuePair<int, string>(i, e.Key)).ToAsyncEnumerable().ToArrayAsync();
                return Ok(settings);
            }


            return Ok(includes);
        }
    }
}
