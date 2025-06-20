using BootstrapTable.Pager;
using HYDrmb.Abstraction;
using HYDrmb.Framework.AppModel;
using HYDrmb.jobweb.AppModels;
using HYDrmb.jobweb.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HYDrmb.jobweb.Tools;
using HYDrmb.Framework.Tools;
using DocumentFormat.OpenXml.Bibliography;
using HYDrmb.Framework.Model;
using HYDrmb.Framework;
using System.Net.Http;
using HYDrmb.Service;
using DocumentFormat.OpenXml.Office2010.Excel;
using UI = HYDrmb.Abstraction.Constants.UI;

namespace HYDrmb.jobweb.Controllers
{
    
    public class AccountController : BaseController
    {
        [JWTAuth(level = 0)]
        [HttpGet]
        public ActionResult Index()
        {
            return Index(new QyUserModel());
        }

        //[Authorize]
        [HttpPost]
        [JWTAuth(level = 0)]
        [ValidateAntiForgeryToken]
        public ActionResult Index(QyUserModel query)
        {
            ViewBag.LevelNames = userService.GetLevels().ToList();

            if (string.IsNullOrEmpty(query.AskUserName))
            {
                var rawquery = _db.CoreUsers.Select(x => new EditUserModel { Id = x.Id, UserName = x.UserName, Person = x.Person, Level = x.level, IsAdmin = x.IsAdmin, IsPowerUser = x.level ==9, CreatedAt = x.createdAt, UpdatedAt = x.updatedAt, Division = x.Division, Disabled = x.Disabled, IsReset = x.IsReset, Email = x.email, Post = x.post, Telephone = x.tel, IsVIP= x.level==6 }).ToList();

                

                if (!query.ShowAll)
                    rawquery = rawquery.Where(e => !e.Disabled).ToList();
                var dict = new List<JSort>();
                if (!string.IsNullOrEmpty(query.sort))
                {

                    var sort = new JSort { Order = query.sort.EndsWith("+") ? "asc" : "desc", Column = query.sort.Substring(0, query.sort.Length - 1) };
                    dict.Add(sort);
                    dict.Add(new JSort { Column = nameof(EditUserModel.UserName), Order = "asc" });

                }
                else
                {
                    dict.Add(new JSort { Column = nameof(EditUserModel.Division), Order = "asc" });
                    dict.Add(new JSort { Column = nameof(EditUserModel.Post), Order = "asc" });
                }
                query.totalRecords = rawquery.Count;
                if ((query.page - 1) * 10 > query.totalRecords)
                {
                    query.page = query.totalRecords / 10;
                }
                query.Records = rawquery.BuildOrderBys(JSONtoDynamicHelper.SaveDictionaryToJSON(dict)).ToPagerList(query.page);
            }
            else
            {

                var rawquery = _db.CoreUsers.Where(y => y.UserName.Contains(query.AskUserName) || y.Person.Contains(query.AskUserName) || y.UserId.Contains(query.AskUserName) || y.post.Contains(query.AskUserName)).Select(x => new EditUserModel { Id = x.Id, UserName = x.UserName, Person = x.Person, Level = x.level, IsAdmin = x.IsAdmin, IsPowerUser= x.level ==9 , CreatedAt = x.createdAt, UpdatedAt = x.updatedAt, Email = x.email, Division = x.Division, Disabled = x.Disabled, IsReset = x.IsReset, Post = x.post, Telephone=x.tel, IsVIP = x.level==6 || x.IsAdmin }).ToList();

                
                if (!query.ShowAll)
                    rawquery = rawquery.Where(e => !e.Disabled).ToList();

                var dict = new List<JSort>();
                if (!string.IsNullOrEmpty(query.sort))
                {

                    var sort = new JSort { Order = query.sort.EndsWith("+") ? "asc" : "desc", Column = query.sort.Substring(0, query.sort.Length - 1) };
                    dict.Add(sort);
                    dict.Add(new JSort { Column = nameof(EditUserModel.UserName), Order = "asc" });

                }
                else
                {
                    dict.Add(new JSort { Column = nameof(EditUserModel.Division), Order = "asc" });
                    dict.Add(new JSort { Column = nameof(EditUserModel.Post), Order = "asc" });
                }

                query.totalRecords = rawquery.Count;

                if ((query.page - 1) * 10 > query.totalRecords)
                {
                    query.page = query.totalRecords / 10;
                }

                query.Records = rawquery.BuildOrderBys(JSONtoDynamicHelper.SaveDictionaryToJSON(dict)).ToPagerList(query.page);



            }
            return View(query);

            
        }

        [HttpGet]
        public ActionResult SendTest()
        {
            try
            {

                var people = new[] { "sys_programmer.bstr@hyd.gov.hk" };
                var templatefile = Constants.Setting.emailfile.GetAppKeyValue();
                if (System.IO.File.Exists(templatefile))
                {
                    miscLog.LogMisc($"The template file exists {templatefile}!");
                }
                miscLog.LogMisc($"Try to use the template file exists {templatefile} to send email!");
                var error = UtilExtensions.Email("sys_programmer.bstr@hyd.gov.hk", people, "Room Booking", GetBaseUrl(), "New Booking", "Booking Base", "See Ya", Constants.Setting.emailfile.GetAppKeyValue(), "smtp.hyd.gov.hk");

                if (!string.IsNullOrEmpty(error))
                {
                    miscLog.LogMisc("SendTest has error!");
                    miscLog.LogMisc(error);
                }

            }
            catch (Exception ex)
            {
                return Json(new { failure = true, result = "failure", msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true, result = "success" }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        //[Authorize]
        [JWTAuth(level = 0)]
        public ActionResult Delete(int Id)
        {
            var accItem = _db.CoreUsers.FirstOrDefault(y => y.Id == Id);
            if (accItem == null)
            {
                return Json(new { status = "failure", msg = $"The user id {Id} does not exist!" }, JsonRequestBehavior.AllowGet);
            }

            accItem.updatedAt = DateTime.Now;
            accItem.updatedBy = AppManager.UserState.UserID;

            _db.Entry(accItem).State = System.Data.Entity.EntityState.Modified;
            accItem.Disabled = !accItem.Disabled;
            if(accItem.Disabled)
            {
                userService.UpdateProc<LGNDiableInput>("upLGNUserDisable", new LGNDiableInput {  UserId = accItem.UserId });
            }
            //_db.DFAUsers.Remove(accItem);
            _db.SaveChanges();

            return Json(new { status = "success", msg = "", returnUrl = Url.Action("Index", "Account") }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        //[Authorize]
        [JWTAuth( level = 0)]
        public ActionResult Register()
        {
            ViewBag.ContentWidth = "";
            ViewBag.LevelNames = UtilityUI.GetSelectList<int>(userService.GetLevels(), -1);
            ViewBag.LevelTypes = userService.GetLevels(0).Select(e => new KeyValuePair<string, string>(e.Value, $"{e.Key}")).ToList();
            
            
            var defaultpwd = _db.CoreSettings.FirstOrDefault(e => e.SettingId == Constants.Setting.DefaultPwd);

            return View(new CreateUserModel { Level = 18, IsAdmin = false, Pwd = defaultpwd?.SettingValue ?? "12345678", Confirmpwd = defaultpwd?.SettingValue ?? "12345678"});
        }


        //[Authorize]
        [JWTAuth( level = 0)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(CreateUserModel model)
        {
            ViewBag.ContentWidth = "";
            ViewBag.LevelNames = UtilityUI.GetSelectList<int>(userService.GetLevels(), -1);
            ViewBag.LevelTypes = userService.GetLevels(0).Select(e => new KeyValuePair<string, string>(e.Value, $"{e.Key}")).ToList();
            
            
            var defaultpwd = _db.CoreSettings.FirstOrDefault(e => e.SettingId == Constants.Setting.DefaultPwd);
            if (ModelState.IsValid)
            {
                var msg = userService.EditUser(model.UserId, model.UserName, null, model.Level, model.Post,model.Telephone, model.Division, model.IsAdmin,  User.Identity.Name,email: model.Email,password: model.Pwd ?? defaultpwd?.SettingValue ?? "12345678");
                if (!string.IsNullOrEmpty(msg?.ToString()))
                {
                    ModelState.AddModelError("UserId", msg?.ToString());
                    return View(model);

                }

            }
            else
            {


                return View(model);
            }
            return Redirect("/Account");
        }

        //[Authorize]
        [JWTAuth(level = 0)]
        [HttpGet]
        public ActionResult EditUser(int Id)
        {
            ViewBag.ContentWidth = "";
            ViewBag.LevelNames = UtilityUI.GetSelectList<int>(userService.GetLevels(), -1);

            ViewBag.LevelTypes = userService.GetLevels(0).Select(e => new KeyValuePair<string, string>(e.Value, $"{e.Key}")).ToList();
            
            

            var edituser = _db.CoreUsers.FirstOrDefault(y => y.Id == Id);
            var model = new EditUserModel { Id = edituser.Id, UserId = edituser.UserId, UserName = edituser.UserName, Person = edituser.Person, Level = edituser.level, Post = edituser.post, Division = edituser.Division, Email = edituser.email, Telephone=edituser.tel, IsAdmin = edituser.IsAdmin, IsPowerUser= edituser.level==9, IsVIP = edituser.level==6 || edituser.IsAdmin };

            return View(model);
        }

        //[Authorize]
        [JWTAuth( level = 0)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(EditUserModel model)
        {
            ViewBag.ContentWidth = "";
            ViewBag.LevelNames = UtilityUI.GetSelectList<int>(userService.GetLevels(0), -1);

            ViewBag.LevelTypes = userService.GetLevels(0).Select(e => new KeyValuePair<string, string>($"{e.Value}", $"{e.Key}")).ToList();

            if (ModelState.IsValid)
            {
                var edituser = _db.CoreUsers.FirstOrDefault(y => y.Id == model.Id);

                userService.EditUser(edituser.UserId, model.UserName, model.Person, model.Level, model.Post, model.Telephone, model.Division, model.IsAdmin, User.Identity.Name,adminScope: edituser.AdminScope,email: model.Email, Id: edituser.Id);

            }
            else
            {


                return View(model);
            }
            return Redirect("/Account");
        }





        //[Authorize]
        [JWTAuth( level = 0)]
        [HttpGet]
        public ActionResult ChangePwd(int Id)
        {
            ViewBag.ContentWidth = "";
            var changeuser = _db.CoreUsers.FirstOrDefault(y => y.Id == Id);

            var model = new ChangePwdModel { Id = changeuser.Id, UserName = changeuser.UserName };
            return View(model);
        }

        //[Authorize]
        [JWTAuth( level = 0)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePwd(ChangePwdModel model)
        {
            ViewBag.ContentWidth = "";
            var changeuser = _db.CoreUsers.FirstOrDefault(y => y.Id == model.Id);

            if (ModelState.IsValid)
            {
                if (model.Pwd != model.Confirmpwd)
                {
                    ModelState.AddModelError("Confirmpwd", "The password is not matched!");

                    return View(model);
                }


                userService.ChangeUserPassword(changeuser.UserId, model.Confirmpwd, User.Identity.Name);



            }
            else
            {
                return View(model);
            }

            return Redirect("/Account");
            
        }


        [HttpGet]
        [JWTAuth(false)]
        public ActionResult Login(string returnUrl=null,bool needClear=false)
        {
            ViewBag.ContentWidth = "";

            if (needClear)
            {
                if(AppManager.UserState!=null)
                {
                    //messageService.SeenFor(AppManager.UserState.UserID, AppManager.UserState.UserName);

                    //bool found = false;
                    //foreach(var foundmsg in _db.DFAMessages.Where(e => e.UserId == AppManager.UserState.UserID))
                    //{
                    //    if (foundmsg != null)
                    //    {
                    //        found = true;
                    //        foundmsg.Seen = true;
                    //        foundmsg.UpdatedBy = AppManager.UserState.UserID;
                    //        foundmsg.UpdatedAt = DateTime.Now;
                    //        _db.Entry(foundmsg).State = System.Data.Entity.EntityState.Modified;

                            
                    //    }
                    //}
                    //if (found)
                    //    _db.SaveChanges();

                }

                var cookie = new HttpCookie(Constants.Setting.AuthorizeCookieKey, "");
                HttpContext.Response.Cookies.Add(cookie);
                Session[Constants.Session.UserName] = null;
                Session[Constants.Session.UserId] = null;
                Session[Constants.Session.UserLevel] = null;
                Session[Constants.Session.Message] = null;
                Session[Constants.Session.MessageCount] = null;
                Session[Constants.Session.IsBSGuy] = null;
                Session[Constants.Session.UserEmail] = null;
                

            }


            return View(new LoginModel { returnUrl=returnUrl });
        }

        [HttpPost]
        [JWTAuth(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string account, string password,string returnUrl)
        {
            //var user = userService.EditUser("001", account,AppManager.WindowUser, password);


            var user = _db.CoreUsers.FirstOrDefault(y => (y.UserName == account || y.UserId == account || y.post == account) && !y.Disabled );

            if (userService.LoginUser(account, password, AppManager.WindowUser))
            {

                var cookie = new HttpCookie(Constants.Setting.AuthorizeCookieKey, JWTHelper.GenerateToken(user.UserId, user.UserName, user.level, user.post, user.IsAdmin, user.Division, user.email));
                HttpContext.Response.Cookies.Add(cookie);



                if (user.IsReset)
                    return Json(new { returnUrl = Url.Action("ChangePassword", "Account", new { ReturnUrl = returnUrl }) }, JsonRequestBehavior.AllowGet);
                if (!user.IsAdmin || (returnUrl != null && returnUrl.Contains("/Login")))
                {
                    return Json(new { returnUrl = Url.Action("Index", "Reservation") }, JsonRequestBehavior.AllowGet);
                }



                return Json(new { returnUrl = returnUrl }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var error = user == null ? "User not found!" : (user.Disabled ? "The account is disabled. Please contact your Division Administrator!" : "Password is not correct!");
                var cookie = new HttpCookie(Constants.Setting.AuthorizeCookieKey, "");
                HttpContext.Response.Cookies.Add(cookie);
                return Json(error, JsonRequestBehavior.AllowGet);
            }
        }
    }
}