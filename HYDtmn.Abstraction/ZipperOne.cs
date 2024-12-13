using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYDtmn.Abstraction
{
    public static class ZipperOne
    {
        public static string Create(string outfilepath, string subpath, params string[] filepaths)
        {

            SharpCompress.Common.ArchiveEncoding encoding = new SharpCompress.Common.ArchiveEncoding();
            encoding.Default = Encoding.GetEncoding("utf-8");
            SharpCompress.Writers.WriterOptions options = new SharpCompress.Writers.WriterOptions(SharpCompress.Common.CompressionType.Deflate);
            options.ArchiveEncoding = encoding;
            using (var archive = ZipArchive.Create())
            {
                int count = 0;
                foreach(var file in filepaths)
                {
                    
                    if(System.IO.File.Exists(file))
                    {
                        var filename = Path.GetFileName(file);
                        if(!string.IsNullOrEmpty(subpath))
                            filename = Path.Combine(subpath, filename);
                        archive.AddEntry(filename, file);
                        count++;
                    }
                }
                using(var zip = File.OpenWrite(outfilepath))
                {
                    archive.SaveTo(zip, options);
                }

            }
            if(System.IO.File.Exists(outfilepath))
                return outfilepath;
            return null;
        }

        public static string CreateFromPath(string frompath,string outfilepath )
        {
            SharpCompress.Common.ArchiveEncoding encoding = new SharpCompress.Common.ArchiveEncoding();
            encoding.Default = Encoding.GetEncoding("utf-8");
            SharpCompress.Writers.WriterOptions options = new SharpCompress.Writers.WriterOptions(SharpCompress.Common.CompressionType.Deflate);
            options.ArchiveEncoding = encoding;
            using(var archive = ZipArchive.Create())
            {
                archive.AddAllFromDirectory(frompath);
                using (var zip = File.OpenWrite(outfilepath))
                {
                    archive.SaveTo(zip, options);
                }
            }

            if (System.IO.File.Exists(outfilepath))
                return outfilepath;
            return null;

        }
    }
}
