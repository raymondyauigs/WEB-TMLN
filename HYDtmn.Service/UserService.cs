using HYDtmn.Abstraction;
using HYDtmn.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WK = HYDtmn.Abstraction.Constants.WebKey;
using DK = HYDtmn.Abstraction.Constants.DataKey;
using DT = HYDtmn.Abstraction.Constants.DT;
using HYDtmn.Framework.AppModel;

namespace HYDtmn.Service
{
    public class UserService:BaseDbService, IUserService
    {

        public UserService(HYDtmnEntities mdb,IMiscLog mlog) { 

            db = mdb;
            log = mlog;
        }

        private int getlevel(bool ispower,bool isadmin)
        {
            if (ispower)
                return 8;
            else if (isadmin)
                return 0;

            return 18;



        }

        public IEnumerable<KeyValuePair<int, string>> GetLevels(params int[] exclude)
        {
            return new[] {

                new KeyValuePair<int,string>(18, DT.NAME_NORMALUSER),
                new KeyValuePair<int,string>(9, DT.NAME_POWERUSER),
                new KeyValuePair<int,string>(6, DT.NAME_VIPUSER),
                new KeyValuePair<int,string>(0, DT.NAME_ADMINISTRATOR),
            }.Where(e=> !exclude.Contains(e.Key));

        }

        public string ImportDataTable(DataTable dt, string updateby, out int count)
        {
            count = 0;
            try
            {
                var ph = new PasswordHasher();
                var settingpwd = db.CoreSettings.FirstOrDefault(e => e.SettingId == DK.DefaultPwd);
                var hashph = ph.HashPassword(settingpwd?.SettingValue ?? "12345678");

                var newuserlist = new List<CoreUser>();

                foreach (var newu in dt.AsEnumerable().Select(y => new
                {
                    username = y.Field<string>("User Name"),
                    userid = y.Field<string>("User ID"),
                    level = getlevel(y.Field<string>("PowerUser") == "T", y.Field<string>("Admin") == "T"),
                    post = y.Field<string>("Post"),
                    division = y.Field<string>("Division"),
                    isadmin = y.Field<string>("Admin") == "T",
                    adminscope = string.Empty,
                    email = y.Field<string>("Email"),
                }))

                {
                    if (newu.level > 0)
                    {
                        var founduser = db.CoreUsers.FirstOrDefault(e => e.UserId == newu.userid);
                        if (founduser != null)
                        {
                            db.CoreUsers.Remove(founduser);
                            db.Entry(founduser).State = System.Data.Entity.EntityState.Deleted;
                        }


                        newuserlist.Add(new CoreUser { UserId = newu.userid, UserName = newu.username, IsAdmin = newu.isadmin, level = newu.level, EncPassword = hashph, IsReset = true, Division = newu.division, post = newu.post, email = newu.email, AdminScope = newu.adminscope, createdAt = DateTime.Now, updatedAt = DateTime.Now, updatedBy = updateby });

                    }
                }
                if (newuserlist.Count > 0)
                    db.CoreUsers.AddRange(newuserlist);
                count = newuserlist.Count;
                db.SaveChanges();


            }
            catch (Exception ex)
            {
                log.LogMisc(ex.Message, ex);
                return ex.Message;
            }
            return "";
        }
        public bool ResetPassword()
        {
            try
            {
                var code = DK.SETT_USRSET;
                var reset = db.CoreSettings.FirstOrDefault(e => e.SettingId == code);
                var defaultpwd = db.CoreSettings.FirstOrDefault(e => e.SettingId == DK.DefaultPwd);
                var isreset = false;
                if (reset != null && bool.TryParse(reset.SettingValue, out isreset) && isreset)
                {
                    foreach (var u in db.CoreUsers)
                    {
                        if (string.IsNullOrEmpty(u.EncPassword))
                        {
                            var ph = new PasswordHasher();
                            u.EncPassword = ph.HashPassword(defaultpwd?.SettingValue ?? "12345678");
                            db.Entry(u).State = System.Data.Entity.EntityState.Modified;

                        }
                    }

                    reset.SettingValue = "false";
                    db.Entry(reset).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                log.LogMisc(ex.Message, ex);
            }
            return false;
        }


        public bool ChangeUserPassword(string userid, string password, string editedby, string oldpassword = null)
        {
            try
            {
                var ph = new PasswordHasher();
                var enpassword = ph.HashPassword(password);


                var u = db.CoreUsers.FirstOrDefault(y => y.UserId == userid);
                if (u != null)
                {

                    if (!string.IsNullOrEmpty(oldpassword))
                    {
                        u.IsReset = false;

                        if (u.Disabled)
                            return false;

                        if (ph.VerifyHashedPassword(u.EncPassword, oldpassword) != PasswordVerificationResult.Success)
                            return false;

                    }
                    else
                    {
                        u.IsReset = true;
                        u.Disabled = false;
                    }
                    UpdateProc<LGNPasswordInput>("upLGNPasswordUpdate", new LGNPasswordInput { EncPassword = enpassword, UserId = u.UserId });
                    u.EncPassword = enpassword;
                    
                    u.updatedAt = DateTime.Now;
                    u.updatedBy = editedby;


                    db.Entry(u).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                log.LogMisc(ex.Message, ex);
            }




            return false;
        }



        public object EditUser(string userid, string userName, string person, int level, string post, string division, bool isAdmin,string editedby, string adminScope=null, string email = null, string password = null, int Id = 0)
        {
            try
            {
                var ph = new PasswordHasher();

                if (Id == 0)
                {
                    var enpassword = ph.HashPassword(password);

                    if (string.IsNullOrEmpty(userid))
                    {
                        userid = "".RandomString().RandomString(); //UtilExtensions.RandomString();
                    }

                    if (db.CoreUsers.Any(e => e.UserId == userid))
                    {
                        return "User Id is duplicated!";
                    }

                    var u = new CoreUser { UserId = userid, UserName = userName, Person = person, level = level, post = post, EncPassword = enpassword, Division = division, IsAdmin = isAdmin, IsReset = true, AdminScope = adminScope, email = email, updatedBy = editedby, updatedAt = DateTime.Now, createdAt = DateTime.Now };


                    db.CoreUsers.Add(u);
                    db.SaveChanges();
                }
                else
                {
                    var u = db.CoreUsers.FirstOrDefault(y => y.Id == Id);
                    if (u != null)
                    {
                        u.level = level;
                        u.IsAdmin = isAdmin;
                        u.Division = division;
                        u.Disabled = false;
                        u.UserName = userName;
                        u.updatedAt = DateTime.Now;
                        u.updatedBy = editedby;
                        u.post = post;
                        u.email = email;
                        u.AdminScope = adminScope;
                        db.Entry(u).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }


            }
            catch (Exception ex)
            {
                log.LogMisc(ex.Message, ex);
            }
            return null;
        }

        public bool LoginUser(string userid, string password, string editedby)
        {
            var ph = new PasswordHasher();
            var user = db.CoreUsers.FirstOrDefault(y => userid == y.UserName || userid == y.UserId || userid== y.post);
            if (user == null)
                return false;

            if (!user.Disabled && ph.VerifyHashedPassword(user.EncPassword, password) == PasswordVerificationResult.Success)
            {
                user.loginedAt = DateTime.Now;
                user.updatedBy = editedby;

                db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return true;
            }
            return false;

        }


    }
}
