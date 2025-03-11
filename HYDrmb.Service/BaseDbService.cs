using HYDrmb.Abstraction;
using HYDrmb.Framework;
using HYDrmb.Framework.Tools;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HYDrmb.Service
{
    public abstract class BaseDbService
    {
        protected HYDrmbEntities db;
        protected IMiscLog log;


        public virtual bool UpdateProc<T>(string procname, T input) where T : class
        {
            var paramsqls = typeof(T).GetProperties().ToDictionary(e => "@" + e.Name, e => new SqlParameter("@" + e.Name, e.GetValue(input) ?? (object)DBNull.Value));
            var storeproc = procname + " " + string.Join(", ", paramsqls.Keys);
            var result = db.Database.ExecuteSqlCommand(storeproc, paramsqls.Values.ToArray());
            return result == 0;
        }

        public IQueryable<T> Filter<T>(Dictionary<string, string> searchvalues) where T : class
        {
            var fieldnv = new NameValueCollection();

            var needSearch = searchvalues.Count > 0;
            var hlist = new HashSet<string>();

            if (needSearch)
            {
                foreach (var sr in searchvalues)
                {

                    var propname = typeof(T).GetProperties().Select(e => e.Name).FirstOrDefault(e => e.ToLower() == sr.Key.ToLower());
                    if (propname != null)
                    {
                        var isadded = hlist.Add(propname);
                        if (isadded)
                        {
                            var tolike = typeof(T).GetProperty(propname).PropertyType == typeof(string);
                            EntityFns.FillnvData(fieldnv, tolike ? $"%{sr.Value}%" : sr.Value, propname, tolike ? null : (string.IsNullOrEmpty(sr.Key) ? "==" : sr.Key));
                        }

                    }


                }


            }

            var data = fieldnv.AllKeys.Length == 0 ? db.Set<T>() : EntityFns.GetSearch<T>(db, fieldnv) as IQueryable<T>;

            return data;

        }

        public virtual bool TransactionNow(Func<bool> doIt,string label=null)
        {
            DbContextTransaction scope = null;
            try
            {
                scope= db.Database.BeginTransaction();
                if(!doIt())
                {
                    log.LogMisc($"the intended transaction does not work out for {label}!");
                    scope.Rollback();
                    return false;
                }
                scope.Commit();
                return true;
            }
            catch(DbUpdateException ex) {
                log.LogMisc(ex.Message, ex);
                if(scope != null)
                {
                    scope.Rollback();
                }
                return false;
            }
        }
    }
}
