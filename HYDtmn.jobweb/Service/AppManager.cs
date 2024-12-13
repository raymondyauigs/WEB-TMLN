using HYDtmn.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HYDtmn.jobweb.Service
{
    public class AppManager
    {
        public static string CurrentUser
        {
            get
            {
                if (UserState != null)
                    return UserState.UserID;
                return HttpContext.Current.User.Identity.Name;
            }
        }

        public static string WindowUser
        {
            get
            {
                return HttpContext.Current.User.Identity.Name;
            }
        }

        public static UserState UserState
        {
            get
            {
                HttpContext httpContext = HttpContext.Current;
                var cookie = httpContext.Request.Cookies[Constants.Setting.AuthorizeCookieKey];

                var tokenInfo = cookie?.Value ?? "";

                var log = DependencyResolver.Current.GetService<IMiscLog>();
                //log.LogMisc($"cookie received: {tokenInfo}");


                var encodeTokenInfo = JWTHelper.GetDecodingToken(tokenInfo);

                UserState userState = JsonHelper<UserState>.JsonDeserializeObject(encodeTokenInfo);

                return userState;
            }
        }

    }
}