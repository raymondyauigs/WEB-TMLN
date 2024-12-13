using HYDtmn.Abstraction;
using HYDtmn.Framework.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml.Linq;

namespace HYDtmn.Framework.Tools
{
    public static class JSONtoDynamicHelper
    {
        public const int DEF_INDEX = 0;
        public const int VAL_INDEX = 1;
        public const string COMBO_WILDCARD = "*";
        public static dynamic GetDynamicObjFromJSON(string jsonvalues)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            var d = jss.Deserialize<dynamic>(jsonvalues);
            return d;
        }

        public static List<T> CreateJSONObjList<T>(string strJSON) where T : class
        {
            if (string.IsNullOrEmpty(strJSON) || strJSON =="{}")
            {  return new List<T>(); }

            var formattedValues = strJSON.Replace(Environment.NewLine, string.Empty);
            if(!formattedValues.TrimStart().StartsWith("["))
            {
                formattedValues = $"[{formattedValues}]";
            }
            var arrJSON = JArray.Parse(formattedValues);

            var data = arrJSON
                       .Select(json => json.ToObject<T>())
                       // should be no difference if the following is commented
                       //.OrderBy(i => i.tablename)
                       .ToList();
            return data;
        }

        public static string SaveXmlLeafToJSON(string xmlData, out IDictionary<string, object> output, params string[] parents)
        {
            output = null;
            parents = parents ?? new string[] { };
            try
            {
                var xdoc = XElement.Parse(xmlData);

                var leaves = from d in xdoc.Descendants()
                             where !d.Elements().Any() && !parents.Any(x => x.Equals(d.Parent.Name.LocalName, StringComparison.InvariantCultureIgnoreCase))
                             select d;
                var leavesDict = leaves.ToDictionary(x => x.Name.LocalName, x => (object)x.Value);
                output = leavesDict;
                return SaveDictionaryToJSON(leavesDict);
            }
            catch (Exception ex)
            {
                var err = UtilExtensions.ToErrorList(ex).ToList();
            }
            return null;
        }



        public static string SaveDictionaryToJSON(object data)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string jsonString = serializer.Serialize((object)data);
            return jsonString;
        }

        public static KeyValuePair<string, object[]> FilterNullOrEmptyValues<T>(string defs, string searchvalues, IDictionary<string, string> remap = null) where T : class
        {
            var propertynames = typeof(T).GetProperties().Select(y => y.Name);

            if (string.IsNullOrEmpty(searchvalues))
            {
                return new KeyValuePair<string, object[]>("", new object[] { });
            }


            var defvalpair = new[] { (IDictionary<string, object>)JSONtoDynamicHelper.GetDynamicObjFromJSON(defs), (IDictionary<string, object>)JSONtoDynamicHelper.GetDynamicObjFromJSON(searchvalues) };



            var removedkeys = new List<string>();
            if (remap == null)
            {
                remap = new Dictionary<string, string>();
                foreach (var k in defvalpair[0].Keys)
                {

                    var keylast = (k.ToLower().LastIndexOf("_begin") > 0 || k.ToLower().LastIndexOf("_end") > 0) ? Math.Max(k.ToLower().LastIndexOf("_begin"), k.ToLower().LastIndexOf("_end")) : k.Length;
                    var key = k.Substring(0, keylast);

                    if (typeof(Array).IsAssignableFrom(defvalpair[1][k]?.GetType()) && (defvalpair[1][k] as IEnumerable<object>).Any(y => y?.ToString() == "*"))
                    {
                        continue;
                    }
                    else if (string.IsNullOrEmpty(defvalpair[1][k]?.ToString()) || !propertynames.Any(y => key.Equals(y, StringComparison.InvariantCultureIgnoreCase)))
                        continue;


                    if (k.EndsWith("_begin", StringComparison.InvariantCultureIgnoreCase))
                    {

                        remap.Add(k, k.Substring(0, k.ToLower().LastIndexOf("_begin")));

                    }
                    else if (k.EndsWith("_end", StringComparison.InvariantCultureIgnoreCase))
                    {
                        remap.Add(k, k.Substring(0, k.ToLower().LastIndexOf("_end")));
                    }
                    else
                    {
                        remap.Add(k, k);
                    }
                }
            }



            var needmap = remap != null && remap.Keys.Count > 0;

            // remove the empty value key
            foreach (var v in defvalpair[1])
            {


                if (needmap)
                {

                    if (!remap.Any(x => x.Key.Equals(v.Key, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        defvalpair[0].Remove(v.Key);
                        removedkeys.Add(v.Key);
                    }


                }

                if (string.IsNullOrEmpty(v.Value?.ToString()) || v.Value?.ToString() == COMBO_WILDCARD)
                {
                    defvalpair[0].Remove(v.Key);
                    removedkeys.Add(v.Key);
                }
            }
            foreach (var k in removedkeys)
            {
                defvalpair[1].Remove(k);
            }





            var defops = TypeExtensions.SymbToOps();
            //Please note that the filter definitions have keys matching the IQueryable column name (not the projected model's column name)

            var conds = string.Join(" and ", TypeExtensions.DefaultConds(defops,
                defvalpair[0].Select(x =>
                {
                    var mappedkey = needmap ? remap[x.Key] : x.Key;


                    return new KeyValuePair<string, string>(mappedkey, x.Value?.ToString());

                }
                ).ToArray()));

            foreach (var p in defvalpair[1].ToArray())
            {
                var curvalue = defvalpair[1][p.Key];
                var curtype = curvalue?.GetType();
                if (curtype != null && typeof(Array).IsAssignableFrom(curtype))
                {
                    if (defvalpair[0][p.Key].ToString().EndsWith("+s"))
                    {
                        var svaluelist = new List<string>();
                        foreach (var v in curvalue as IEnumerable<object>)
                        {
                            svaluelist.Add(v?.ToString());
                        }
                        defvalpair[1][p.Key] = svaluelist;
                    }
                    else if (defvalpair[0][p.Key].ToString().EndsWith("+d"))
                    {
                        var ivaluelist = new List<int>();
                        foreach (var v in curvalue as IEnumerable<object>)
                        {
                            int vint = 0;
                            if (int.TryParse(v?.ToString(), out vint))
                            {
                                ivaluelist.Add(vint);
                            }

                        }
                        defvalpair[1][p.Key] = ivaluelist;
                    }
                }
                else
                {
                    defvalpair[1][p.Key] = TypeExtensions.GetDateValue(p.Value.ToString(), defvalpair[0][p.Key].ToString()) ?? defvalpair[1][p.Key];
                }



            }

            var values = defvalpair[1].Values.ToArray();

            return new KeyValuePair<string, object[]>(conds, values);

        }



        public static KeyValuePair<string, NameValueCollection> PrepareTablesNVSet(IEnumerable<JsonSearchClass> records)
        {
            string tablename = null;
            NameValueCollection nvSet = new NameValueCollection();
            if (records.FirstOrDefault() != null)
                tablename = records.First().tablename;

            var checklist = new Dictionary<string, List<string>>();

            foreach (var item in records)
            {
                string prefix = null;
                if (!string.IsNullOrEmpty(item.group))
                {
                    if (!checklist.ContainsKey(item.group))
                    {
                        checklist[item.group] = new List<string>();
                        checklist[item.group].Add(item.value);
                    }
                    else
                        checklist[item.group].Add(item.value);
                }
                else
                {
                    switch (item.op)
                    {
                        case "=":
                            prefix = string.Empty;
                            break;
                        case "!=": //Add By Jonathan LO
                            prefix = string.Format("__{0}__", LinqAsset.NotEq);
                            if (item.value == "null") //For frontend hidden value, Jonathan Lo "if column not null"
                            {
                                item.value = null;
                            }
                            break;
                        case ">=":
                            prefix = string.Format("__{0}__", LinqAsset.GreaterOrEq);
                            break;
                        case "<=":
                            prefix = string.Format("__{0}__", LinqAsset.LessOrEq);
                            break;
                        //20170807 Barbara Add Less for DateTime To Search
                        case "<":
                            prefix = string.Format("__{0}__", LinqAsset.Less);
                            break;
                        case "g":
                            prefix = string.Format("__{0}__", LinqAsset.CheckListOp);
                            break;
                        default:
                            prefix = string.Format("__{0}__", LinqAsset.LikeOp);
                            break;
                    }


                    nvSet.Add(prefix + item.key, item.value);
                }
            }
            foreach (var subkey in checklist.Keys)
            {
                nvSet.Add(string.Format("__{0}__", LinqAsset.CheckListOp) + subkey, string.Join(",", checklist[subkey]));
            }
            return new KeyValuePair<string, NameValueCollection>(tablename, nvSet);
        }

        public static IEnumerable<List<T>> GroupListwithTableNames<T>(List<T> lstJSON) where T : class, IBelongtoTable
        {
            var tblParms = lstJSON
                           .GroupBy(l => l.tablename)
                           .Select(group => new { tablename = group.Key, lstItems = group.ToList() });

            foreach (var p in tblParms)
                yield return p.lstItems;
        }
    }

}
