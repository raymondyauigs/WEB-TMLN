using HYDtmn.jobweb.Models;
using System.Web;
using System.Web.Optimization;
using HYDtmn.jobweb.Tools;

namespace HYDtmn.jobweb
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            new ScriptBundle("~/bundles/jquery").IncludeInOrder(bundles,
                                    "~/Scripts/jquery-{version}.js",                                    
                                    "~/Scripts/jquery.mloading.js",
                                    "~/Scripts/jquery.ajax.js",
                                    "~/Scripts/jquery.auto-complete.js",
                                    "~/Scripts/util.imask.min.js",                                    
                                    "~/Scripts/jquery.utils.js",
                                    "~/Scripts/moment.min.js",
                                    "~/Scripts/big.js",
                                    "~/Scripts/keypress.min.js",
                                    "~/Scripts/popper.min.js",
                                    "~/Scripts/tippy.min.js",
                                    "~/Scripts/util.underscore.js"
                                    );

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            new ScriptBundle("~/bundles/bootstrap").IncludeInOrder(bundles, 
                
                "~/Scripts/alertify.js",
                "~/Scripts/bootstrap.datepicker.js",
                "~/Scripts/bootstrap-noconflict.js",                
                "~/Scripts/alertify.utils.js");

            new ScriptBundle("~/bundles/theme").IncludeInOrder(bundles,
                "~/Scripts/theme.latest.min.js",
                "~/Scripts/theme.brand.min.js",
                "~/Scripts/theme.solid.min.js",
                "~/Scripts/theme.font.min.js");




            bundles.Add(new BetterStyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap-sandstone.css",
                      "~/Content/alertify.css",
                      "~/Content/alertify.bootstrap.css",
                      //"~/Content/bootstrap-theme.css",
                      "~/Content/font-awesome.css",
                      "~/Content/bootstrap-dialog.css",
                      //"~/Content/bootstrap.datepicker.css",
                      "~/Content/jquery.auto-complete.css",
                      "~/Content/jquery.mloading.css",
                      
                      "~/Content/tippy.min.css",
                      "~/Content/tippy.material.min.css",
                      "~/Content/my.newbutton.css",
                      "~/Content/my.button.css",                      
                      "~/Content/util.ag-grid.css",
                      "~/Content/util.ag-grid.theme.css",
                      "~/Content/thumbs.css",
                      "~/Content/Site.css"));
        }
    }
}
