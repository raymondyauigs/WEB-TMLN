using HYDtmn.jobweb.Service;
using System.Web;
using System.Web.Mvc;

namespace HYDtmn.jobweb
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new AllowCrossSiteAttribute());
            filters.Add(new JWTAuthAttribute(false));
            filters.Add(new JsonMaxFilter { MaxJsonLength = int.MaxValue });
            filters.Add(new HandleErrorAttribute());
        }
    }
}
