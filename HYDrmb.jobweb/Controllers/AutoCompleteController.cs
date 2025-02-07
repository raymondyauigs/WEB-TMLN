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
        public AutoCompleteController(HYDrmbEntities mdb, IMiscLog mlog, IStdbLog mdlog, IErrorLog mrlog, IMemoryCache mmCache, ISettingService settingService, EditModels mapping)
        {

            db = mdb;
            errorLog = mrlog;
            miscLog = mlog;
            stdbLog = mdlog;
            mCache = mmCache;
            
            sttService = settingService;

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
