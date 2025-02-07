using HYDrmb.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HYDrmb.jobweb.Service
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

                
                var encodeTokenInfo = JWTHelper.GetDecodingToken(tokenInfo);

                UserState userState = JsonHelper<UserState>.JsonDeserializeObject(encodeTokenInfo);

                return userState;
            }
        }

    }
}