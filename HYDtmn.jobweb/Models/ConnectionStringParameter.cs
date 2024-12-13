using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HYDtmn.jobweb.Models
{
    public class ConnectionStringParameter : NamedParameter
    {
        public ConnectionStringParameter(string connectionString) : base("connectionString", connectionString)
        {

        }

        public static ConnectionStringParameter Create()
        {
            return new ConnectionStringParameter(HYDtmn.Framework.Model.Builder.CreateString());
        }
    }
}