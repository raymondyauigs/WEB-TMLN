using HYDtmn.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using HYDtmn.jobweb.Service;
using System.Reflection;
using System.Web.Http.WebHost;

namespace HYDtmn.jobweb
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            //if you want specific sessionstate handling in web api (discomment following)

            //var httpControllerRouteHandler = typeof(HttpControllerRouteHandler).GetField("_instance",
            //                         BindingFlags.Static |
            //                         BindingFlags.NonPublic);

            //if (httpControllerRouteHandler != null)
            //{
            //    httpControllerRouteHandler.SetValue(null,
            //        new Lazy<HttpControllerRouteHandler>(() => new SessionHttpControllerRouteHandler(), true));
            //}


            // Web API routes
            config.MapHttpAttributeRoutes();

            var log = (IErrorLog)config.DependencyResolver.GetService(typeof(IErrorLog));
            config.Filters.Add(new CustomApiFilter(log));


            //if you want specific sessionstate handling in web api (comment following)
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
