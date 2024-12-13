using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using HYDtmn.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;


namespace HYDtmn.jobweb.Service
{
    public class AutofacConfig
    {



        public static IContainer Configure(string basepath, string log4netconstant)
        {
            log4net.GlobalContext.Properties["LogSite"] = UtilExtensions.GetAppKeyValue(Constants.WebKey.logpath);
            var log4netfile = UtilExtensions.GetAppKeyValue(log4netconstant);
            
            LogService.SetupLog4net(basepath, log4netfile);

            ContainerBuilder builder = new ContainerBuilder();
            var assemblies = new Assembly[] { typeof(AutofacConfig).Assembly };
            builder.RegisterControllers(assemblies.ToArray());
            builder.RegisterAssemblyTypes(typeof(AutofacConfig).Assembly);

            var moduleType = typeof(Autofac.Module);
            var foundtypes = (from assembly in assemblies
                              from type in assembly.GetTypes()
                              where moduleType.IsAssignableFrom(type) && type.GetConstructor(Type.EmptyTypes) != null
                              select type).Distinct();


            var modules = (from type in foundtypes
                           select Activator.CreateInstance(type) as Autofac.Module).ToArray();
            // Initialize AutoMapper with each instance of the profiles found.
            foreach (var module in modules.Distinct())
            {
                builder.RegisterModule(module);
            }

            var container = builder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            return container;
        }
    }
}