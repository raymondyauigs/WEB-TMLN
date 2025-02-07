using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace HYDrmb.Abstraction
{
    public static class TypeExtensions
    {
        static Regex numAlpha = new Regex("(?<Alpha>[a-zA-Z]*)(?<Numeric>[0-9]*)");
        public static T ToObject<T>(this DataRow row) where T : class, new()
        {
            T obj = new T();

            foreach (var prop in obj.GetType().GetProperties())
            {
                try
                {
                    if (prop.PropertyType.IsGenericType && prop.PropertyType.Name.Contains("Nullable"))
                    {
                        if (!string.IsNullOrEmpty(row[prop.Name].ToString()))
                            prop.SetValue(obj, Convert.ChangeType(row[prop.Name],
                            Nullable.GetUnderlyingType(prop.PropertyType), null));
                        //else do nothing
                    }
                    else
                        prop.SetValue(obj, Convert.ChangeType(row[prop.Name], prop.PropertyType), null);
                }
                catch
                {
                    continue;
                }
            }
            return obj;
        }
        /// <summary>
        /// Converts a DataTable to a list with generic objects
        /// </summary>
        /// <typeparam name="T">Generic object</typeparam>
        /// <param name="table">DataTable</param>
        /// <returns>List with generic objects</returns>
        public static List<T> DataTableToList<T>(this DataTable table) where T : class, new()
        {
            try
            {
                List<T> list = new List<T>();

                foreach (var row in table.AsEnumerable())
                {
                    var obj = row.ToObject<T>();

                    list.Add(obj);
                }

                return list;
            }
            catch
            {
                return null;
            }
        }

        public static T GetEnum<T>(this string val) where T: Enum
        {
            try
            {
                var enumT= (T)Enum.Parse(typeof(T), val); ;
                return enumT;
            }
            catch(Exception ex)
            {
                return default(T);
            }
            
            
        }

        public static string GetFileType(this string fileNamen)
        {
            var fileName = fileNamen?.ToLower();
            if (fileName.EndsWith(".xlsx") || fileName.EndsWith(".xls"))
                return "application/vnd.ms-excel";
            else if (fileName.EndsWith(".pdf"))
                return "application/pdf";
            else if (fileName.EndsWith(".doc") || fileName.EndsWith(".docx"))
                return "application/vnd.ms-word";
            else if (fileName.EndsWith(".zip"))
                return "application/zip";
            else if (fileName.EndsWith(".jpg"))
                return "image/jpeg";
            else if (fileName.EndsWith(".png"))
                return "image/png";
            else if (fileName.EndsWith(".tif"))
                return "image/tiff";
            else if (fileName.EndsWith(".gif"))
                return "image/gif";
            else if (fileName.EndsWith(".mp4"))
                return "video/mp4";



            return "application/octet-stream";
        }

        public static int MonthDiff(this DateTime startmonth,DateTime endmonth)
        {
            return ((endmonth.Year - startmonth.Year) * 12) + endmonth.Month - startmonth.Month;
        }

        public static int ItAddPlus(this object total,int value)
        {
            var totalval = 0;
            if(total!=null)
            {
                totalval = (int)total;
            }
            return totalval += value;
        }

        public static void AddInline(this HttpResponseBase Response, string filename)
        {
            System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition
            {
                FileName = filename,
                Inline = true   // false = prompt the user for downloading;  true = browser to try to show the file inline
            };
            Response.Headers.Remove("Content-Disposition");
            Response.Headers.Add("Content-Disposition", cd.ToString());
            Response.Headers.Remove("X-Content-Type-Options");
            Response.Headers.Add("X-Content-Type-Options", "nosniff");
        }

        public static DateTime Trim(this DateTime date, long ticks)
        {
            return new DateTime(date.Ticks - (date.Ticks % ticks), date.Kind);
        }

        public static string[] GetTimeIntervals(DateTime start,DateTime end,int stepinMins)
        {
            var nofIntervals = (int)((end - start).Ticks / (stepinMins * TimeSpan.TicksPerMinute));
            DateTime[] intervals = new DateTime[nofIntervals+1];
            for(int i = 0;i<nofIntervals+1;i++)
            {
                
                intervals[i] = start.AddMinutes(i*stepinMins); 
            }
            return intervals.Select(e => e.ToString("HH:mm")).ToArray();
        }
        public static string GetSessionType(DateTime startsession, DateTime endsession)
        {
            if(endsession <= DateTime.Today.AddHours(12).AddMinutes(30))
            {
                return nameof( SessionType.AM);
            }
            else if(startsession >= DateTime.Today.AddHours(13) )
            {
                return nameof(SessionType.PM);
            }
            return nameof( SessionType.CUSTOM);
        }
        public static KeyValuePair<DateTime,DateTime> GetNearestTimeFrame(this DateTime date)
        {
            var nearest = date.Trim(TimeSpan.TicksPerMinute);
            var nearestminute = (nearest.Minute % 15) > 0 ? (nearest.Minute / 15) * 15 + 15 : nearest.Minute;
            var nearestStart = nearest.AddMinutes(-nearest.Minute + nearestminute);
            var toswitchsession = (nearestStart > DateTime.Today.AddHours(12).AddMinutes(30)) && (nearestStart < DateTime.Today.AddHours(13));
            nearestStart = toswitchsession ? DateTime.Today.AddHours(13) : nearestStart;

            var sessionEnd = nearestStart.AddMinutes(30);
            if(sessionEnd > DateTime.Today.AddHours(18))
            {
                nearestStart = DateTime.Today.AddDays(1).AddHours(9);
                sessionEnd = nearestStart.AddMinutes(30);
            }
            return new KeyValuePair<DateTime, DateTime>(nearestStart, sessionEnd);
            
        }

        public static string TrimStartAt(this string input, string find,int count=1)
        {
            var tmpinput = input;
            if (string.IsNullOrEmpty(input))
                return input;
            while (tmpinput.IndexOf(find) > -1 && tmpinput.StartsWith(find))
            {
                tmpinput = tmpinput.Substring(tmpinput.IndexOf(find)+find.Length);
                count--;
                if (count <= 0)
                    break;
            }

            return tmpinput;
        }

        public static string TrimEndAt(this string input, string find)
        {
            var tmpinput = input;
            if (string.IsNullOrEmpty(input))
                return input;
            while(tmpinput.LastIndexOf(find) >-1 && tmpinput.EndsWith(find))
            {
                tmpinput = tmpinput.Substring(0, tmpinput.LastIndexOf(find) -find.Length);
            }

            return tmpinput;
        }

        public static bool Contains(this string input, string find, StringComparison comparisonType)
        {
            return String.IsNullOrWhiteSpace(input) ? false : input.IndexOf(find, comparisonType) > -1;
        }

        public static string SplitCamelCase(this string str)
        {
            return Regex.Replace(Regex.Replace(str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
        }

        public static string BeAndSentence(this string str,string sep=",")
        {
            
            if (!string.IsNullOrEmpty(str) && str.Contains(sep))
            {
                var indx = str.LastIndexOf(sep);
                return str.Substring(0, indx) + " and " + str.Substring(indx + 1);
            }
            return str;
        }

        public static IEnumerable<string> NoEmpty(this IEnumerable<string> input)
        {
            foreach(var i in input.Where(e=> !string.IsNullOrEmpty(e)))
                yield return i;
        }

        public static IEnumerable<string> ItSplit(this string str,string sep=",")
        {
            
            if (string.IsNullOrEmpty(str))
                yield return "";
            else if (string.IsNullOrEmpty(sep))
                yield return str;
            else
            {
                var sb = new StringBuilder(str);
                while (sb.ToString().IndexOf(sep) >= 0)
                {
                    var sbstr = sb.ToString();
                    var sepindex = sbstr.IndexOf(sep);
                    sbstr = sbstr.Substring(0, sepindex);
                    yield return sbstr.Trim();

                    sb = sb.Remove(0, sepindex + sep.Length);
                }
                if(sb.Length >0)
                {
                    yield return sb.ToString().Trim();
                }

            }


        }

        private static string escapeSql(string source)
        {
            string rslt = source.Replace("'", "''");
            return rslt;
        }

        public static string nullable(object data, bool nullable = true, bool unicode = false, bool trim = false)
        {
            string rslt = "'" + escapeSql(trim ? data.ToString().Trim() : data.ToString());
            if (nullable && (data == null || data.ToString().Length <= 0))
                return "null";
            if (unicode)
                rslt = "N" + rslt;
            rslt += "'";
            return rslt;
        }

        public static string pad(string source, char padSource, int length, bool right = true)
        {
            string rslt = null;
            if (source == null || source.Length <= 0)
                return rslt;

            string padtarget = new string(padSource, length);
            string padsource = padtarget + source;
            rslt = padsource.Substring(padsource.Length - length, length);

            if (!right)
                rslt = padsource.Substring(0, length);
            return rslt;
        }


        public static string RemoveSuffix(this string oldvalue, params string[] suffixes)
        {
            string value = oldvalue;

            foreach (var suffix in suffixes)
            {
                if (string.IsNullOrEmpty(suffix))
                    continue;
                if (value.EndsWith(suffix, StringComparison.InvariantCultureIgnoreCase))
                {
                    value = value.Substring(0, value.LastIndexOf(suffix, StringComparison.InvariantCultureIgnoreCase));
                }
            }

            return value;
        }

        public static IDictionary<string, string> SymbToOps()
        {
            var dictOfSymbs = new Dictionary<string, string>();
            dictOfSymbs.Add("gte", " >= {@} ");
            dictOfSymbs.Add("gt", " > {@} ");
            dictOfSymbs.Add("lt", " < {@} ");
            dictOfSymbs.Add("lte", " <= {@} ");
            dictOfSymbs.Add("eq", " = {@} ");
            dictOfSymbs.Add("ct", ".Contains({@}) ");
            dictOfSymbs.Add("ot", "{@}.Contains(outerIt.{K}) ");
            return dictOfSymbs;

        }

        public static IDictionary<string, string> SymbToTypes()
        {
            var dictOfSymbs = new Dictionary<string, string>();
            dictOfSymbs.Add("+d", "Int32");
            dictOfSymbs.Add("+s", "String");
            dictOfSymbs.Add("+b", "Boolean");
            dictOfSymbs.Add("+f", "Double");
            dictOfSymbs.Add("+c", "Date");
            dictOfSymbs.Add("+t", "DateTime");
            return dictOfSymbs;

        }

        public static IEnumerable<object> DefaultConds(IDictionary<string, string> symbsdict = null, params KeyValuePair<string, string>[] symbs)
        {
            int i = -1;
            if (symbsdict == null)
                symbsdict = SymbToOps();
            foreach (var s in symbs.Select(x => new KeyValuePair<string, string>(x.Key, x.Value.Contains("+") ? x.Value.Substring(0, x.Value.LastIndexOf("+")) : x.Value)))
            {
                if (s.Value == "ot")
                {
                    yield return symbsdict[s.Value].Replace("{@}", $"@{++i}").Replace("{K}", s.Key);
                }
                else
                    yield return s.Key + symbsdict[s.Value].Replace("{@}", $"@{++i}");

            }
        }

        public static object GetDateValue(string value, string symb, IDictionary<string, string> symbsdict = null)
        {

            if (symbsdict == null)
                symbsdict = SymbToTypes();
            var type = symb.Contains("+") ? symb.Substring(symb.LastIndexOf("+")) : symb;
            if (symbsdict.ContainsKey(type) && symbsdict[type].StartsWith("Date"))
            {
                var resultvalue = symbsdict[type] == "Date" ? DateTime.Today : DateTime.Now;
                if (DateTime.TryParseExact(value, "dd/MM/yyyy", null, DateTimeStyles.None, out resultvalue))
                    return resultvalue;
            }
            return null;
        }


        public static IEnumerable<object> DefaultValues(IDictionary<string, string> symbsdict = null, params string[] symbs)
        {
            if (symbsdict == null)
                symbsdict = SymbToTypes();
            foreach (var s in symbs.Select(x => x.Contains("+") ? x.Substring(x.LastIndexOf("+")) : x))
            {
                if (!s.Contains("+") || !symbsdict.ContainsKey(s))
                    yield return s;
                else
                {
                    switch (symbsdict[s])
                    {
                        case "Date":
                            yield return DateTime.Today.ToString("dd/MM/yyyy");

                            break;
                        case "DateTime":
                            yield return DateTime.Now.ToString("H:mm");
                            break;
                        case "Int32":
                            yield return 0;
                            break;
                        case "Double":
                            yield return 0.0;
                            break;
                        case "Boolean":
                            yield return false;
                            break;
                        case "String":
                            yield return "";
                            break;
                        default:
                            yield return s;
                            break;


                    }
                }

            }
        }


        public static string SerializeToXmlString(object targetInstance)
        {
            string retVal = string.Empty;
            TextWriter writer = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(targetInstance.GetType());
            serializer.Serialize(writer, targetInstance);
            retVal = writer.ToString();
            return retVal;
        }
        public static object DeserializeFromXmlString(string objectXml, Type targetType)
        {
            object retVal = null;
            XmlSerializer serializer = new XmlSerializer(targetType);
            StringReader stringReader = new StringReader(objectXml);
            XmlTextReader xmlReader = new XmlTextReader(stringReader);
            retVal = serializer.Deserialize(xmlReader);
            return retVal;
        }


        public static Type GetInnerType(object enums)
        {
            Type[] interfaces = enums.GetType().GetInterfaces();
            foreach (var i in interfaces)
            {
                if (i.IsGenericType && i.GetGenericTypeDefinition().Equals(typeof(IEnumerable<>)))
                    return i.GetGenericArguments()[0];
            }
            return null;
        }

        public static string IncPadding(this string value,char pad='0',int len=4,int inc=1)
        {
            var bricks = numAlpha.Match(value);
            var strpart = bricks.Groups["Alpha"].Value;
            var numpart =TryValue( bricks.Groups["Numeric"].Value,0);
            var word = strpart+ $"{(inc+numpart)}".PadLeft(len,pad);
            return word;
        }

        public static T TryValue<T>(string value,T defval) where T: struct
        {
            if(value !=null)
            {
                value = value.Trim();
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));

                }
                catch {

                    return defval;
                }

            }
               

            return defval;
        }

        public static Type GetTypeForDefaultValue(object value)
        {
            DateTime resultvalue = DateTime.Now;
            if (DateTime.TryParse(value.ToString(), out resultvalue))
                return typeof(DateTime);
            else
                return BaseType(value.GetType());
        }

        public static Type BaseType(Type objType)
        {
            // ensure the passed objType 1) is valid, 2) .IsValueType, 3) and is logicially nullable
            if (objType != null && objType.IsValueType && objType.IsGenericType && objType.GetGenericTypeDefinition() == typeof(Nullable<>))
                return Nullable.GetUnderlyingType(objType);
            else
                return objType;
        }

        public static bool IsSimpleType(
            this Type type)
        {
            return
                type.IsValueType ||
                type.IsPrimitive ||
                new Type[] {
                typeof(String),
                typeof(Decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(Guid)
            }.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object;
        }

        public static string ItFormatAs(this string valueformat, int pos)
        {
            return "{" + pos.ToString() + ":" + valueformat + "}";
        }

        public static bool ItContains(this string value, string target)
        {
            if (string.IsNullOrEmpty(value))
                return false;
            if (string.IsNullOrEmpty(target))
                return true;
            return value.ToLower().Contains(target.ToLower());
        }

        public static bool IsInteger(this string s)
        {
            try
            {
                if (s == null)
                    return false;
                int t = int.Parse(s);
                return true;
            }
            catch
            {
                return false;
            }
        }


        public static bool IsBase64String(this string s)
        {
            s = s.Trim();
            return (s.Length % 4 == 0) && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);

        }


    }
}
