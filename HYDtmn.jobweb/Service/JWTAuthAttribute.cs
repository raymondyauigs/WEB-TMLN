using Autofac.Features.ResolveAnything;
using HYDtmn.Abstraction;
using HYDtmn.Framework;
using HYDtmn.jobweb.Controllers;
using HYDtmn.jobweb.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI.WebControls;

namespace HYDtmn.jobweb.Service
{
    public class JWTAuthAttribute : FilterAttribute, IAuthorizationFilter
    {
        public JWTAuthAttribute(bool _isCheck = true,int defaultlevel=18,bool restricted=false)
        {
            this.isCheck = _isCheck;
            this.level = defaultlevel;
            this.restricted = restricted;
            
        }
        public int level { get; set; }
        private bool isCheck { get; }

        private bool restricted { get; }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var httpContext = filterContext.HttpContext;
            var actionDescription = filterContext.ActionDescriptor;

            var currentPath = filterContext.RequestContext.HttpContext.Request.Url.PathAndQuery;

            var baseurl = UrlHelper.GenerateContentUrl("~/", filterContext.HttpContext).TrimEnd('/');

            var db = DependencyResolver.Current.GetService<HYDtmnEntities>();
            var lg = DependencyResolver.Current.GetService<IMiscLog>();

            

            if (actionDescription.IsDefined(typeof(AllowAnonymousAttribute), false) ||
                actionDescription.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), false)) { return; }

            bool isDisabled = false;

            bool isReset = false;

            var founduser = AppManager.UserState == null ? null : db.CoreUsers.FirstOrDefault(y => y.UserId == AppManager.UserState.UserID);

            //lg.LogMisc($"your url info: {baseurl} + {currentPath} + {httpContext.Request.Url}");

            if (AppManager.UserState!=null)
            {
                
                //var firstmsg = db.DFAMessages.FirstOrDefault(e => e.UserId == AppManager.UserState.UserID && !e.Seen);


                if (founduser!=null && !founduser.Disabled )
                {
                    isDisabled = founduser.Disabled;
                    isReset = founduser.IsReset;

                    var cookie = new HttpCookie(Constants.Setting.AuthorizeCookieKey, JWTHelper.GenerateToken(AppManager.UserState.UserID, AppManager.UserState.UserName, founduser.level,founduser.post,founduser.IsAdmin, founduser.Division,founduser.email));
                    filterContext.HttpContext.Response.Cookies.Add(cookie);
                    httpContext.Session[Constants.Session.TagId] = founduser.Id;
                    httpContext.Session[Constants.Session.UserId] = AppManager.UserState.UserID;
                    httpContext.Session[Constants.Session.UserName] = AppManager.UserState.UserName;
                    httpContext.Session[Constants.Session.UserLevel] = AppManager.UserState.Level;
                    httpContext.Session[Constants.Session.IsAdmin] = AppManager.UserState.IsAdmin;
                    httpContext.Session[Constants.Session.Division] = AppManager.UserState.Division;
                    httpContext.Session[Constants.Session.UserEmail] = AppManager.UserState.Email;
                    httpContext.Session[Constants.Session.IsBSGuy] = founduser.Division == "B&S";
                    httpContext.Session[Constants.Session.FullMenu] = founduser.level < 1;
                    List<string> msglist = new List<string>();




                    //}



                }
                else
                {
                    var cookie = new HttpCookie(Constants.Setting.AuthorizeCookieKey, "");
                    filterContext.HttpContext.Response.Cookies.Add(cookie);
                    
                }

            }
            else
            {
                httpContext.Session[Constants.Session.TagId] = 0;
                httpContext.Session[Constants.Session.UserLevel] = "";
                httpContext.Session[Constants.Session.UserName] = "";
                httpContext.Session[Constants.Session.UserId] = "";
                httpContext.Session[Constants.Session.IsAdmin] = false;
                httpContext.Session[Constants.Session.Division] = "";
                httpContext.Session[Constants.Session.UserEmail] = "";
                httpContext.Session[Constants.Session.Message] = "";

                var user = filterContext.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(user))
                    httpContext.Session[Constants.Session.UserId] = user;
            }

            if (!isCheck && !isReset) return;


            

            if (isReset )
            {

                if(!(currentPath.Contains("/Login") || currentPath.Contains("/ChangePassword") ))
                {
                    
                    var r = new UriBuilder();
                    r.Path = baseurl+"/Account/Login";
                    var returnurl = filterContext.RequestContext.HttpContext.Request.Url.PathAndQuery;

                    var uri = r.Uri.AddQuery("needClear",true.ToString());
                    filterContext.Result = new RedirectResult(uri.PathAndQuery);
                }
            }            
            else if ( AppManager.UserState == null || isDisabled || (AppManager.UserState.Level> this.level && !AppManager.UserState.IsAdmin) 
                || (this.level==0 && !AppManager.UserState.IsAdmin) || (restricted && !string.IsNullOrEmpty(AppManager.UserState.Division)))
            {
                if (httpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new JsonResult()
                    {
                        Data = new { Status = "Fail", Message = "403 Forbin", StatusCode = "403" },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    var query = filterContext.RequestContext.HttpContext.Request.QueryString;
                    
                    var returnurl = filterContext.RequestContext.HttpContext.Request.Url.PathAndQuery;



                    var r = new UriBuilder();
                    r.Path = baseurl+"/Account/Login";
                    var uri = r.Uri.AddQuery("returnUrl", returnurl);
                    
                    //var uri = new Uri("http:localhost//Account/Login").AddQuery("returnUrl", returnurl);
                    
                    filterContext.Result = new RedirectResult(uri.PathAndQuery);
                }
            }


        }
    }
}