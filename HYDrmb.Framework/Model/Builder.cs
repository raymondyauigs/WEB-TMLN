using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HYDrmb.Abstraction;

namespace HYDrmb.Framework.Model
{
    public static class Builder
    {
        public static string CreateString()
        {
            var encTool = new StringEncrypService();
            var sqlconn = new SqlConnectionStringBuilder
            {
                DataSource = Constants.DBkey.dbsource.GetAppKeyValue(),
                InitialCatalog = Constants.DBkey.dbcatalog.GetAppKeyValue(),
                UserID = Constants.DBkey.dbuser.GetAppKeyValue(),
                Password = encTool.DecryptString(Constants.DBkey.dbpwd.GetAppKeyValue()),
                MultipleActiveResultSets = true,
                PersistSecurityInfo = true,
                ApplicationName = "EntityFramework",
            };
            var entityconn = new EntityConnectionStringBuilder
            {
                Provider = Constants.DBkey.dbprovider.GetAppKeyValue(),
                Metadata = Constants.DBkey.dbmeta.GetAppKeyValue(),
                ProviderConnectionString = sqlconn.ConnectionString,

            };
            return entityconn.ConnectionString;
        }


    }
}
