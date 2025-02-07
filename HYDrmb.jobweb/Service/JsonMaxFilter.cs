using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HYDrmb.jobweb.Service
{
    public class JsonMaxFilter : IResultFilter
    {
        public int? MaxJsonLength { get; set; }

        public int? RecursionLimit { get; set; }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext.Result is JsonResult jsonResult)
            {
                // override properties only if they're not set
                jsonResult.MaxJsonLength = jsonResult.MaxJsonLength ?? MaxJsonLength;
                jsonResult.RecursionLimit = jsonResult.RecursionLimit ?? RecursionLimit;
            }
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }
    }
}