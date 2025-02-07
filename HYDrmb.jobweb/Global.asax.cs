using Autofac;
using HYDrmb.Abstraction;
using HYDrmb.Framework;
using HYDrmb.jobweb.Service;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace HYDrmb.jobweb
{
    /// <summary>
    /// Defines the <see cref="MvcApplication" />.
    /// </summary>
    public class MvcApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// Defines the GetScope.
        /// </summary>
        public static Func<ILifetimeScope> GetScope;


        protected void Application_BeginRequest()
        {
            var db = DependencyResolver.Current.GetService<HYDrmbEntities>();

            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept, Special-Request-Header");
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Credentials", "true");
            HttpContext.Current.Response.Headers.Remove("Access-Control-Allow-Methods");
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            HttpContext.Current.SetSessionStateBehavior(System.Web.SessionState.SessionStateBehavior.Required);

            CultureInfo newCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            newCulture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
            newCulture.DateTimeFormat.DateSeparator = "/";
            
            Thread.CurrentThread.CurrentCulture = newCulture;

            if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
            {
                HttpContext.Current.Response.Flush();
            }
        }


        /// <summary>
        /// The Application_Start.
        /// </summary>
        protected void Application_Start()
        {

            var basepath = Server.MapPath("~");
            var container = AutofacConfig.Configure(basepath, Constants.log4netForAdmin);

            GetScope = () => container.BeginLifetimeScope();

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            using (var scope = GetScope())
            {
                var trylog = scope.Resolve<IMiscLog>();
                trylog.LogMisc("test logger....");
            }
        }

        /// <summary>
        /// The Application_Error.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="EventArgs"/>.</param>
        protected void Application_Error(object sender, EventArgs e)
        {
            HttpContext ctx = HttpContext.Current;
            var lasterror = ctx.Server.GetLastError();
            var lastexception = lasterror as HttpException;
            var errorLog = DependencyResolver.Current.GetService<IErrorLog>();
            errorLog.LogError($"Having error messages: {string.Join("\n", lasterror.ToErrorMessageList())}");
            errorLog.LogError($"{lasterror.Message}", lasterror);
        }
    }
}
