using HYDtmn.Abstraction;
using HYDtmn.Framework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HYDtmn.Service
{
    public abstract class BaseDbService
    {
        protected HYDtmnEntities db;
        protected IMiscLog log;

        public virtual bool UpdateProc<T>(string procname,T input ) where T : class
        {
            var paramsqls = typeof(T).GetProperties().ToDictionary(e => "@" + e.Name, e => new SqlParameter("@" + e.Name, e.GetValue(input)));
            var storeproc = procname + " " + string.Join(", ", paramsqls.Keys);
            var result = db.Database.ExecuteSqlCommand(storeproc, paramsqls.Values.ToArray());
            return result == 0;
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
