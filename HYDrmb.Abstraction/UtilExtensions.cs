using Combinatorics.Collections;
using Nelibur.ObjectMapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Xml;
using System.Xml.Serialization;

namespace HYDrmb.Abstraction
{

    public static class IdGenerator
    {
        private static int _counter;

        private static SemaphoreSlim _lock = new SemaphoreSlim(1);

        
        public static async void GetLock()
        {
            await _lock.WaitAsync();
        }

        public static void Release()
        {
            _lock.Release();
        }

        public static uint GetNewId()
        {
            uint newId = unchecked((uint)System.Threading.Interlocked.Increment(ref _counter));
            if (newId == 0)
            {
                _counter = 0;
                return GetNewId();
                //throw new System.Exception("Whoops, ran out of identifiers");
            }
            return newId;
        }
    }

    public static class UtilExtensions
    {
        public static string Email(string from, string[] topeople, string subject, string url, string title, string description, string username, string templatefile, string smtpserver, int port = 25)
        {
            try
            {
                using (var smtp = new System.Net.Mail.SmtpClient(smtpserver, port))
                {
                    smtp.UseDefaultCredentials = true;
                    System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                    message.From = new System.Net.Mail.MailAddress(from);
                    foreach (var to in topeople)
                    {
                        message.To.Add(new System.Net.Mail.MailAddress(to));
                    }

                    string body = string.Empty;
                    using (StreamReader reader = new StreamReader(templatefile))
                    {
                        body = reader.ReadToEnd();
                    }
                    body = body.Replace("{UserName}", username);
                    body = body.Replace("{Title}", title);
                    body = body.Replace("{Url}", url);
                    body = body.Replace("{Description}", description);
                    message.BodyEncoding = Encoding.UTF8;
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;
                    smtp.Send(message);
                }

                return "";
            }
            catch (Exception ex)
            {

                return ex.Message;


            }
        }

        public static string Notify(string from, string cc, string to, Func<string, string> bodyfiller, string subject, string templatefile, string smtpserver, int port = 25)
        {
            try
            {
                using (var smtp = new System.Net.Mail.SmtpClient(smtpserver, port))
                {
                    smtp.UseDefaultCredentials = true;
                    System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                    message.From = new System.Net.Mail.MailAddress(from);
                    if (!string.IsNullOrEmpty(cc))
                    {
                        foreach (var ccitem in cc.ItSplit(";"))
                        {
                            message.CC.Add(new System.Net.Mail.MailAddress(ccitem));
                        }

                    }
                    if(!string.IsNullOrEmpty(to))
                    {
                        foreach(var toitem in to.ItSplit(";"))
                        {
                            message.To.Add(new System.Net.Mail.MailAddress(toitem));
                        }
                    }
                    
                    string body = string.Empty;
                    using (StreamReader reader = new StreamReader(templatefile))
                    {
                        body = reader.ReadToEnd();
                    }
                    body = bodyfiller(body);
                    message.BodyEncoding = Encoding.UTF8;
                    message.Subject = $"AUTO:{subject}";
                    message.Body = body;
                    message.IsBodyHtml = true;
                    smtp.Send(message);
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;

            }
        }
        public static bool IsOverlaps(params IFromTillModel[] models)
        {
            if (models.Length <= 1)
                return false;

            var modelkeylist = Enumerable.Range(0, models.Length).ToArray();
            var comb = new Combinations<int>(modelkeylist, 2, GenerateOption.WithoutRepetition);
            var overlap = false;
            foreach(var vertex in comb)
            {
                overlap = overlap|| IsOverlap(models[vertex[0]], models[vertex[1]]);
                if (overlap) break;

            }
            return overlap;
        }



        public static bool IsOverlap(IFromTillModel model1,IFromTillModel model2)
        {
            if(Math.Max(model1.TimeTill,model2.TimeTill)-Math.Min(model1.TimeFrom,model2.TimeFrom) < (model1.TimeTill-model1.TimeFrom) + (model2.TimeTill-model2.TimeFrom))
                return true;

            return false;

        }

        public static bool CanInline(string fileName)
        {
            var fileNamelwr = fileName.ToLower();
            if (fileNamelwr.EndsWith(".jpg"))
                return true;
            else if (fileNamelwr.EndsWith(".jpg"))
                return true;
            else if (fileNamelwr.EndsWith(".png"))
                return true;
            else if (fileNamelwr.EndsWith(".tif"))
                return true;
            else if (fileNamelwr.EndsWith(".gif"))
                return true;
            else if (fileNamelwr.EndsWith("pdf"))
                return true;
            return false;

        }

        public static string GetFileType(string fileNamen)
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

        public static string AsFfix(this int index)
        {
            switch (index + 1)
            {
                case 1:
                    return "st";
                case 2:
                    return "nd";
                case 3:
                    return "rd";
                default:
                    return "th";
            }
        }

        public static string ConcatMany(this string start, string sep, params string[] source)
        {
            var head = start ?? string.Empty;

            return string.Join(sep, head, string.Join(sep, source)).Trim();
        
        }
        public static string[] SplitMany(this IEnumerable<string> source, string sep=",")
        {
            var firstsep = new[] { sep[0] };
            return source.Where(e => !string.IsNullOrEmpty(e)).ToArray().SelectMany(e => e.Split(firstsep, StringSplitOptions.RemoveEmptyEntries)).ToArray();
        }

        public static KeyValuePair<int,string>[] GetEmpty(int key=-1,string value="(None)")
        {
            return new[] { new KeyValuePair<int, string>(key,value) };
        }
        public static string GetBaseName(string file)
        {
            var basename = Path.GetFileNameWithoutExtension(file);

            if (basename.LastIndexOf(".") > 0 && (basename.Length - basename.LastIndexOf(".")) > 5)
            {
                basename = basename.Substring(0, basename.LastIndexOf("."));
                return basename;
            }
            return basename;
        }

        /// <summary>
        /// The WriteFile.
        /// </summary>
        /// <param name="filestring">The filestring<see cref="string"/>.</param>
        /// <param name="fullpath">The fullpath<see cref="string"/>.</param>
        /// <param name="maphandler">The maphandler<see cref="Func{string, string}"/>.</param>
        /// <param name="withPath">The withPath<see cref="bool"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string WriteFile(string filestring, string fullpath, Func<string, string> maphandler, bool withPath = false)
        {
            return WriteFile(ToByteArray(filestring), fullpath, maphandler, withPath);
        }

        /// <summary>
        /// The WriteFile.
        /// </summary>
        /// <param name="filedata">The filedata<see cref="byte[]"/>.</param>
        /// <param name="fullpath">The fullpath<see cref="string"/>.</param>
        /// <param name="maphandler">The maphandler<see cref="Func{string, string}"/>.</param>
        /// <param name="withPath">The withPath<see cref="bool"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string WriteFile(byte[] filedata, string fullpath, Func<string, string> maphandler, bool withPath = false)
        {
            var filename = Path.GetFileName(fullpath);
            var path = Path.GetDirectoryName(fullpath);


            path = maphandler(path);

            var filepath = Path.Combine(path, filename);
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                using (var ms = new MemoryStream(filedata))
                using (var fs = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                {

                    ms.CopyTo(fs);
                    ms.Flush();
                    fs.Flush();
                    fs.Close();

                }

            }
            catch (Exception ex)
            {
                throw new IOException($"{filename} cannot be saved to {path}!", ex);
            }
            if (withPath)
                return filepath;
            return filename;
        }

        /// <summary>
        /// The unlock is to unlock the file specified by filepath.
        /// </summary>
        /// <param name="filepath">The filepath<see cref="string"/>.</param>
        /// <param name="logging">The logging<see cref="Action{string}"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool unlock(string filepath, Action<string> logging)
        {
            try
            {
                using (FileStream fileStream = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))

                {
                    logging("try unlock....");
                    fileStream.Unlock(0, fileStream.Length);

                }

            }
            catch (Exception ex)
            {
                logging($"{string.Join("\n", ex.ToErrorMessageList())}");
                return false;
            }


            return true;
        }

        /// <summary>
        /// Creates a byte array from the string, using the 
        /// System.Text.Encoding.Default encoding unless another is specified.
        /// </summary>
        /// <param name="str">The str<see cref="string"/>.</param>
        /// <param name="encoding">The encoding<see cref="Encoding"/>.</param>
        /// <returns>The <see cref="byte[]"/>.</returns>
        public static byte[] ToByteArray(string str, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.Default;
            return encoding.GetBytes(str);
        }

        public static int RandomTime(int maxms = 1500)
        {
            StringBuilder sb = new StringBuilder();
            int myIntValue = unchecked((int)DateTime.Now.Ticks);
            var rnd = new Random(myIntValue);
            var rndvalue = rnd.Next(500, 1000000) % maxms;
            return rndvalue;

        }

        public static string CopyToUniqueFile(string filename,string fragment,string nextpath=null)
        {
            if (!File.Exists(filename))
                return "";
            var filepath = Path.GetDirectoryName(filename);
            var baseName = Path.GetFileNameWithoutExtension(filename);
            if(baseName.LastIndexOf(".")> 0)
            {
                baseName = baseName.Substring(0, baseName.LastIndexOf("."));
            }
            var ext = Path.GetExtension(filename);
            var newfilename = $"{baseName}.{fragment}{ext}";
            if (string.IsNullOrEmpty(nextpath))
            {
                newfilename = Path.Combine(filepath, newfilename);
            }
            else
            {
                newfilename = Path.Combine(nextpath, newfilename);
            }
            
            File.Copy(filename, newfilename);

            return newfilename;

        }

        public static (string,string) CopyToUniqueFile(string filename, string nextpath = null, int ranlen = 5,bool timed=true)
        {
            int count = 20;
            if (!File.Exists(filename))
                return ("","");
            var timeStr = DateTime.Now.ToString("HHmmff");
            if (!timed)
                timeStr = "";

            while (true && count > 0)
            {

                var filepath = Path.GetDirectoryName(filename);
                var baseName = Path.GetFileNameWithoutExtension(filename);
                var ext = Path.GetExtension(filename);
                var ranvalue = baseName.RandomString(ranlen);
                var fragment = $"{timeStr}{ranvalue}";
                var newfilename = $"{baseName}.{fragment}{ext}";
                if (string.IsNullOrEmpty(nextpath))
                {
                    newfilename = Path.Combine(filepath, newfilename);
                }
                else
                {
                    newfilename = Path.Combine(nextpath, newfilename);
                }
                if (File.Exists(newfilename))
                    Thread.Sleep(RandomTime());
                else
                {
                    File.Copy(filename, newfilename);
                    return (fragment,newfilename);
                }


                count--;

            }

            return ("","");

        }



        public static bool RenameFile(string filename)
        {
            var curDir = Path.GetDirectoryName(filename);
            var oldnamepart = Path.GetFileNameWithoutExtension(filename);
            var exten = Path.GetExtension(filename);
            var rnme = oldnamepart.RandomString();
            var newname = Path.Combine(curDir, $"{oldnamepart}.{rnme}{exten}");
            try
            {
                File.Move(filename, newname);
                return true;
            }
            catch
            {
                return false;
            }


            
        }

        public static T2 MapTo<T1,T2>(this T1 source,T2 target) 
            where T1: class 
            where T2 : class
        {
            return TinyMapper.Map<T1, T2>(source, target);
        }

        private static Expression<Func<T, object>> AsExpr<T>(Func<T, object> f)
        {
            return x => f(x);
        }

        public static KeyValuePair<Expression<Func<T1, object>>,Expression< Func<T2, object>>> AsPairExpr<T1, T2>(Expression<Func<T1, object>> f1,Expression<Func<T2, object>> f2)
            where T1 : class
            where T2 : class
        {
            return new KeyValuePair<Expression<Func<T1, object>>, Expression<Func<T2, object>>>(f1, f2);
        }


        public static KeyValuePair<Func<T1, object>, Func<T2, object>> AsPair<T1, T2>(Func<T1,object> f1,Func<T2,object> f2)
            where T1: class
            where T2: class
        {
            return new KeyValuePair<Func<T1, object>, Func<T2, object>>(f1, f2);
        }

        public static KeyValuePair<T1?,string> AsNullable<T1>(KeyValuePair<T1,string> pair) where T1: struct
        {
            return new KeyValuePair<T1?, string>(pair.Key, pair.Value);

        }

        public static IEnumerable<KeyValuePair<T1?, string>> AsNullables<T1>(this IEnumerable<KeyValuePair<T1, string>> pairs) where T1 : struct
        {
            foreach(var pair in pairs)
            yield return new KeyValuePair<T1?, string>(pair.Key, pair.Value);

        }

        public static void GiveBind<T1,T2>(params KeyValuePair<Expression<Func<T1,object>>,Expression<Func<T2, object>>>[] pairbindings )
            where T1: class
            where T2: class
        {
            if(pairbindings.Length>0)
            {
                TinyMapper.Bind<T1,T2>(cfg => {
                    foreach (var b in pairbindings)
                    {
                        if(b.Value!=null)
                        {

                            cfg.Bind(b.Key,b.Value);
                        }
                        else
                        {
                            //If value is null, that means ignore required
                            
                            cfg.Ignore(b.Key);
                        }
                        
                    }

                });
            }
            else
            {
                TinyMapper.Bind<T1, T2>();
            }


        }

        public static T ToObjectAs<T>(this string xml) where T : class
        {
            var x = new XmlSerializer(typeof(T));
            using (StringReader textreader = new StringReader(xml))
            {
                return (T)x.Deserialize(textreader);
            }

        }
        public static string AsXmlString<T>(this T item) where T: class
        {
            var x = new XmlSerializer(typeof(T));

            using (StringWriter textWriter = new StringWriter())
            {
                x.Serialize(textWriter, item);

                return textWriter.ToString();
            }

        }


        /// <summary>
        /// The RandomString.
        /// </summary>
        /// <param name="size">The size<see cref="int"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string RandomString(this string me, int size = 5)
        {
            StringBuilder sb = new StringBuilder();

            int myIntValue = unchecked((int)DateTime.Now.Ticks + me.GetHashCode());
            myIntValue = unchecked(myIntValue + (int)IdGenerator.GetNewId());
            var rnd = new Random(myIntValue);
            for (int i = 0; i < size; i++)
            {


                sb.Append(Convert.ToChar(rnd.Next(65, 90)));
            }

            return sb.ToString().ToLowerInvariant();
        }

        /// <summary>
        /// The RandomString.
        /// </summary>
        /// <param name="size">The size<see cref="int"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string RandomString(int size = 5)
        {
            StringBuilder sb = new StringBuilder();
            int myIntValue = unchecked((int)DateTime.Now.Ticks);
            var rnd = new Random(myIntValue);
            for (int i = 0; i < size; i++)
            {
                

                sb.Append(Convert.ToChar(rnd.Next(65, 90)));
            }

            return sb.ToString();
        }

        // all error checking left out for brevity

        // a.k.a., linked list style enumerator
        public static IEnumerable<TSource> FromHierarchy<TSource>(
            this TSource source,
            Func<TSource, TSource> nextItem,
            Func<TSource, bool> canContinue)
        {
            for (var current = source; canContinue(current); current = nextItem(current))
            {
                yield return current;
            }
        }

        public static IEnumerable<TSource> FromHierarchy<TSource>(
            this TSource source,
            Func<TSource, TSource> nextItem)
            where TSource : class
        {
            return FromHierarchy(source, nextItem, s => s != null);
        }

        public static IEnumerable<string> ToErrorMessageList(this Exception ex, bool allerror = false)
        {
            if (!allerror && (ex is ThreadAbortException || ex is OperationCanceledException))
            {
                return new[] { ex.Message };
            }
            else
                return ex.FromHierarchy(x => x.InnerException)
                    .Select(x => x.Message);
        }

        public static IEnumerable<KeyValuePair<string, string>> ToErrorList(this Exception ex)
        {
            if (ex is ThreadAbortException || ex is OperationCanceledException)
            {
                return new[] { new KeyValuePair<string, string>("Operation Cancelled", ex.Message) };
            }
            else
                return ex.FromHierarchy(x => x.InnerException)
                    .Select(x => new KeyValuePair<string, string>(x.Source, x.Message))
                    .Union(new[] { new KeyValuePair<string, string>("stacktrace", ex.StackTrace) });
        }


        public static string GetAppKeyValue(this string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            var value = WebConfigurationManager.AppSettings.Get(key);
            if (value != null)
                return value;

            return ConfigurationManager.AppSettings.Get(key);
        }



        public static object GetRecursiveExpo(IDictionary<string, object> rootObj,ref string statusCode, params string[] keys)
        {
            var currentOj = rootObj;
            var cnt = keys.Length;
            statusCode = "found";

            if (currentOj == null)
            {
                statusCode = "error";
                return null;
            }
                

            foreach (var k in keys)
            {
                cnt--;
                if (currentOj.ContainsKey(k))
                {
                    if (currentOj[k] is ExpandoObject)
                    {
                        currentOj = (IDictionary<string, object>)currentOj[k];
                    }
                    else if (currentOj[k] is object)
                    {
                        if (cnt == 0)
                            return (object)currentOj[k];
                        statusCode = "error";
                        return null;
                    }
                    else if (currentOj[k] is IList)
                    {
                        if (cnt == 0)
                            return (IList)currentOj[k];
                        statusCode = "error";
                        return null;

                    }
                }
                else
                {
                    statusCode = "error";
                }


            }
            statusCode = "error";
            return null;
        }
        public static T GetAppKeyValue<T>(this string key)
        {

            if (string.IsNullOrEmpty(key))
                return default(T);

            var value = WebConfigurationManager.AppSettings.Get(key) ?? ConfigurationManager.AppSettings.Get(key);
            T valueT = default(T);
            try
            {
                valueT = (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {


            }

            return valueT;

        }

        public static string ItRevertAmpSign(this string str,string sub="--")
        {
            return (str ?? "").Replace("&", "--");
        }
        public static string ItRestoreAmpSign(this string str,string sub="--")
        {
            return (str ?? "").Replace("--", "&");
        }
        public static string ItSubStr(this string strval,string sep="_")
        {
            if (string.IsNullOrEmpty(strval))
                return "";
            return strval.Substring(strval.IndexOf(sep)+1);
        }

        public static Dictionary<string,string> ZipStrPair(string values,string fields,string sep= "!")
        {
            if (string.IsNullOrEmpty(values) || string.IsNullOrEmpty(fields))
                return new Dictionary<string, string>();

            var valuelist = values.Split(sep.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var fieldlist = fields.Split(sep.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (fieldlist.Length > 0)

                return fieldlist.Select((v, i) => new KeyValuePair<string, string>(v, valuelist[i])).ToDictionary(e => e.Key, e => e.Value);

            return new Dictionary<string, string>();
        }

        public static DateTime ParseDateOrDefault(string datestr,string fmt,DateTime defaultval)
        {
            var newvalue = defaultval;
            var success = DateTime.TryParseExact(datestr, fmt, null, System.Globalization.DateTimeStyles.None, out newvalue);
            if (success)
                return newvalue;
            return defaultval;

        }

        public static IEnumerable<string> ItSorts(string sep,string partsep,int keyindex=0,params string[] items)
        {
            foreach(var item in items)
            {
                yield return string.Join(sep, item.ItSplit(sep).Select(e =>
                {
                    var parts = e.Split(partsep.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    return new KeyValuePair<string, string>(parts[keyindex], e);
                }).OrderBy(e=> e.Key.Length).ThenBy(e => e.Key).Select(e => e.Value));
            }

        }
    }
}
