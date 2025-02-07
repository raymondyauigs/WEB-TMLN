using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace HYDrmb.jobweb.Models
{
    public class BetterStyleBundle : StyleBundle
    {
        public override IBundleOrderer Orderer
        {
            get
            {
                return new NonOrderingBundleOrderer();
            }
            set
            {
                throw new Exception("Unable to override Non-Ordred bundler");
            }
        }

        public BetterStyleBundle(string virtualPath)
            : base(virtualPath)
        {
        }


        public BetterStyleBundle(string virtualPath, string cdnPath)
            : base(virtualPath, cdnPath)
        {
        }


        public override Bundle Include(params string[] virtualPaths)
        {
            foreach (var virtualPath in virtualPaths)
            {
                //fix transform css image url 
                base.Include(virtualPath, new CssRewriteUrlTransformWrapper());
            }
            return this;
        }
    }

    // This provides files in the same order as they have been added. 
    public class NonOrderingBundleOrderer : IBundleOrderer
    {
        public IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
        {
            return files;
        }
    }
}