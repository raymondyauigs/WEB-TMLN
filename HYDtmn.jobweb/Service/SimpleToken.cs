using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HYDtmn.jobweb.Service
{


    public class SimpleToken
    {
        public SimpleToken()
        {
            iss = "HYD";
            iat = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            exp = iat + 30000;
            aud = "";
            sub = "HYD.ENG";
            jti = "HYD." + DateTime.Now.ToString("yyyyMMddhhmmss");
        }

        public string iss { get; set; }
        public double iat { get; set; }
        public double exp { get; set; }
        public string aud { get; set; }
        public double nbf { get; set; }
        public string sub { get; set; }
        public string jti { get; set; }

    }
}