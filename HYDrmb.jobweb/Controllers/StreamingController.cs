using HYDrmb.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using HYDrmb.Framework;
using System.Data.SqlClient;
using System.Data.Entity;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Net.Http.Headers;

namespace HYDrmb.jobweb.Controllers
{
    [RoutePrefix("api/streaming")]
    public class StreamingController : ApiController
    {
        //using autofac webapi resolver instead ( need to use same container)
        public StreamingController(HYDrmbEntities itdb, IMiscLog mlog, IStdbLog dlog, IErrorLog elog)
        {
            db = itdb;
            miscLog = mlog;
            errorLog = elog;
            stdbLog = dlog;
            db.Database.Log = (msg) => { stdbLog.LogStdb(msg.TrimEnd(Environment.NewLine.ToCharArray())); };


            SqlConnection sqlConn = (SqlConnection)db.Database.Connection;
            sqlConn.InfoMessage -= SqlConn_InfoMessage;
            sqlConn.InfoMessage += SqlConn_InfoMessage;

        }

        private T GetService<T>()
        {
            return (T)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(T));
        }

        private HYDrmbEntities db;

        protected IMiscLog miscLog;
        protected IErrorLog errorLog;
        protected IStdbLog stdbLog;

        public override Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
        {




            return base.ExecuteAsync(controllerContext, cancellationToken);
        }

        private void SqlConn_InfoMessage(object sender, SqlInfoMessageEventArgs args)
        {
            foreach (SqlError err in args.Errors)
            {
                string strOutput = string.Format("Procedure [{0}] Line [{1}]: {2}", err.Procedure, err.LineNumber, err.Message.TrimEnd(Environment.NewLine.ToCharArray()));
                errorLog.LogError(strOutput);
            }
        }


        [HttpGet]
        [Route("file/{filename}/{thumb:int?}")]
        public async Task<HttpResponseMessage> Get(string filename, int thumb = 0)
        {
            var browsepath = Constants.Setting.browsepath.GetAppKeyValue();

            IdGenerator.GetLock();

            var zippath = string.Empty;
            if (!string.IsNullOrEmpty(filename))
                filename = filename.ItRestoreAmpSign();
            int id = Math.Abs(thumb);
            dynamic item = new object();
            var baspath = Constants.Setting.sharepath.GetAppKeyValue();
            if (item != null)
            {
                
                if (!string.IsNullOrEmpty(item.BackupRelativePath))
                {

                    var relpath = item.BackupRelativePath.ItRestoreAmpSign();
                    zippath = $"{baspath}\\{item.WalkwayBFARecord.StctNo}\\{relpath}";

                }


            }
            else
            {
                
                zippath = $"{baspath}\\{filename}";
            }
            var basename = UtilExtensions.GetBaseName(filename);
            var ext = Path.GetExtension(filename);
            filename = $"{basename}{ext}";

            var tmppath = zippath;
            //Please do not lock the pushstreamcontent
            IdGenerator.Release();
            var contentType = UtilExtensions.GetFileType(filename);
            var isinline = UtilExtensions.CanInline(filename);
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {

                Content = new PushStreamContent(async (stream, context, transportContext) =>
                {
                    //capture the value of zip before lock release
                    try
                    {
                        //var buffer = new byte[65536];

                        using (var fileStream = System.IO.File.OpenRead(tmppath))
                        {
                            await fileStream.CopyToAsync(stream);
                        }
                    }
                    catch (HttpException ex)
                    {
                        if (!File.Exists(tmppath))
                        {
                            errorLog.LogError($"{tmppath} does not exist!");
                        }
                        errorLog.LogError($"{tmppath} has problem!");
                        errorLog.LogError(ex.Message, ex);
                        return;
                    }
                    finally
                    {
                        stream.Close();
                    }

                    //try
                    //{
                    //    using (var fileStream = System.IO.File.OpenRead(zippath))
                    //    {
                    //        await fileStream.CopyToAsync(stream);
                    //    }
                    //}
                    //finally
                    //{
                    //    stream.Close();
                    //}
                }, new MediaTypeHeaderValue(contentType)),
            };



            response.Headers.TransferEncodingChunked = true;
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue(isinline ? "inline" : "attachment")
            {
                FileName = filename
            };

            return await Task<HttpResponseMessage>.Factory.StartNew(() =>
            {
                return response;
            });

            //return response;
        }

        
    }



}