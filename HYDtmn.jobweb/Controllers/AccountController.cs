﻿using BootstrapTable.Pager;
using HYDtmn.Abstraction;
using HYDtmn.Framework.AppModel;
using HYDtmn.jobweb.AppModels;
using HYDtmn.jobweb.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HYDtmn.jobweb.Tools;
using HYDtmn.Framework.Tools;
using DocumentFormat.OpenXml.Bibliography;
using HYDtmn.Framework.Model;
using HYDtmn.Framework;
using System.Net.Http;
using HYDtmn.Service;
using DocumentFormat.OpenXml.Office2010.Excel;


namespace HYDtmn.jobweb.Controllers
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
                var rawquery = _db.CoreUsers.Select(x => new EditUserModel { Id = x.Id, UserName = x.UserName, Person = x.Person, Level = x.level, IsAdmin = x.IsAdmin, IsPowerUser = x.level ==9, CreatedAt = x.createdAt, UpdatedAt = x.updatedAt, Division = x.Division, Disabled = x.Disabled, IsReset = x.IsReset, Email = x.email, Post = x.post, IsVIP= x.level==6 }).ToList();

                

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

                var rawquery = _db.CoreUsers.Where(y => y.UserName.Contains(query.AskUserName) || y.Person.Contains(query.AskUserName) || y.UserId.Contains(query.AskUserName) || y.post.Contains(query.AskUserName)).Select(x => new EditUserModel { Id = x.Id, UserName = x.UserName, Person = x.Person, Level = x.level, IsAdmin = x.IsAdmin, IsPowerUser= x.level ==9 , CreatedAt = x.createdAt, UpdatedAt = x.updatedAt, Email = x.email, Division = x.Division, Disabled = x.Disabled, IsReset = x.IsReset, Post = x.post, IsVIP = x.level==6 || x.IsAdmin }).ToList();

                
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
            
            
            var defaultpwd = _db.CoreSettings.FirstOrDefault(e => e.SettingId == Constants.DataKey.DefaultPwd);

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
            
            
            var defaultpwd = _db.CoreSettings.FirstOrDefault(e => e.SettingId == Constants.DataKey.DefaultPwd);
            if (ModelState.IsValid)
            {
                var msg = userService.EditUser(model.UserId, model.UserName, null, model.Level, model.Post, model.Division, model.IsAdmin,  User.Identity.Name,email: model.Email,password: model.Pwd ?? defaultpwd?.SettingValue ?? "12345678");
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
            var model = new EditUserModel { Id = edituser.Id, UserId = edituser.UserId, UserName = edituser.UserName, Person = edituser.Person, Level = edituser.level, Post = edituser.post, Division = edituser.Division, Email = edituser.email, IsAdmin = edituser.IsAdmin, IsPowerUser= edituser.level==9, IsVIP = edituser.level==6 || edituser.IsAdmin };

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

                userService.EditUser(edituser.UserId, model.UserName, model.Person, model.Level, model.Post, model.Division, model.IsAdmin, User.Identity.Name,adminScope: edituser.AdminScope,email: model.Email, Id: edituser.Id);

            }
            else
            {


                return View(model);
            }
            return Redirect("/Account");
        }


        [JWTAuth(level =99999)]
        [HttpGet]
        public ActionResult ChangePassword(string ReturnUrl = null)
        {
            ViewBag.ContentWidth = "";
            var userid = AppManager.UserState.UserID;
            var changeuser = _db.CoreUsers.FirstOrDefault(y => y.UserId == userid);
            ViewBag.RedirectUrl = changeuser.IsAdmin ? Url.Action("Index", "Account") : Url.Action("Index", "Home");

            var model = new ChangePasswordModel { Id = changeuser.Id, UserName = changeuser.UserName, ReturnUrl = ReturnUrl ?? Request.UrlReferrer.PathAndQuery };
            return View(model);
        }

        [JWTAuth(level = 99999)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            ViewBag.ContentWidth = "";
            if (ModelState.IsValid)
            {
                var changeuser = _db.CoreUsers.FirstOrDefault(y => y.Id == model.Id);



                var passwed = userService.ChangeUserPassword(changeuser.UserId, model.Confirmpwd, User.Identity.Name, model.OldPwd);
                if (passwed)
                    return Redirect("/Home");
                if (changeuser.Disabled)
                {
                    ModelState.AddModelError("OldPwd", "The account is disabled");
                }
                else
                {
                    ModelState.AddModelError("OldPwd", "Old password does not match");
                }

                return View(model);
            }
            else
            {

                return View(model);
            }


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


            var user = _db.CoreUsers.FirstOrDefault(y => y.UserName == account || y.UserId == account || y.post == account);

            if (userService.LoginUser(account, password, AppManager.WindowUser))
            {

                var cookie = new HttpCookie(Constants.Setting.AuthorizeCookieKey, JWTHelper.GenerateToken(user.UserId, user.UserName, user.level, user.post, user.IsAdmin, user.Division, user.email));
                HttpContext.Response.Cookies.Add(cookie);



                if (user.IsReset)
                    return Json(new { returnUrl = Url.Action("ChangePassword", "Account", new { ReturnUrl = returnUrl }) }, JsonRequestBehavior.AllowGet);
                if (!user.IsAdmin || (returnUrl != null && returnUrl.Contains("/Login")))
                {
                    return Json(new { returnUrl = Url.Action("Index", "Home") }, JsonRequestBehavior.AllowGet);
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