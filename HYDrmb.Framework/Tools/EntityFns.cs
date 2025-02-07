using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYDrmb.Framework.Tools
{
    public static class EntityFns
    {


        public static void FillnvData(NameValueCollection nv, string value, string name,string op=null)
        {
            var opwithname = GetOpName(name, op ?? "~").First();
            if (opwithname.Key > 0 && !string.IsNullOrEmpty(value))
                nv[opwithname.Value] = $"{value}";
            
                
        }

        public static IEnumerable<KeyValuePair<int, string>> GetOpName(string name, string op)
        {
            int index = 0;
            if (!string.IsNullOrEmpty(op))
            {
                switch (op)
                {
                    case "~":
                        yield return new KeyValuePair<int, string>(++index, $"__{LinqAsset.LikeOp}__{name}");
                        break;
                    case "==":
                        yield return new KeyValuePair<int, string>(++index, $"{name}");
                        break;
                    case "Btw":
                        yield return new KeyValuePair<int, string>(++index, $"__{LinqAsset.GreaterOrEq}__{name}");
                        yield return new KeyValuePair<int, string>(++index, $"__{LinqAsset.LessOrEq}__{name}");
                        break;
                    case ">=":
                        yield return new KeyValuePair<int, string>(++index, $"__{LinqAsset.GreaterOrEq}__{name}");
                        break;
                    case "<":
                        yield return new KeyValuePair<int, string>(++index, $"__{LinqAsset.Less}__{name}");
                        break;
                    case "=<":
                        yield return new KeyValuePair<int, string>(++index, $"__{LinqAsset.LessOrEq}__{name}");
                        break;


                }
            }
            if (index == 0)
                yield return new KeyValuePair<int, string>(-1, "");
        }


        public static IQueryable GetSearch<T>(this DbContext MyContext, NameValueCollection nv) where T : class
        {

            var exprbase = LinqHelper.CreateBaseExpr(typeof(T));

            var query = MyContext.Set<T>().AsQueryable() as IQueryable;
            query = query.Search(exprbase, nv, null);

            return query;

        }
        public static DateTime GetDateTime(this DbContext MyContext)
        {
            return MyContext.Database.SqlQuery<DateTime>("select SYSDATETIME()").First();
        }

        public static IEnumerable<string> GetPrimaryKeyPropertyNames(this DbContext MyContext, Type entityType)
        {
            ObjectContext objectContext = ((IObjectContextAdapter)MyContext).ObjectContext;
            var metavalues = objectContext.MetadataWorkspace
                .GetType(entityType.Name, entityType.Namespace, System.Data.Entity.Core.Metadata.Edm.DataSpace.CSpace)
                .MetadataProperties
                .Where(mp => mp.Name == "KeyMembers").ToList();

            foreach (var m in metavalues)
            {
                var ester = (m.Value as ReadOnlyMetadataCollection<EdmMember>).GetEnumerator();
                while (ester.MoveNext())
                {
                    yield return ester.Current.Name;
                }
            }

        }
    }
}
