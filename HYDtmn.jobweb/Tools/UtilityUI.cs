using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.WebPages;
using DocumentFormat.OpenXml.Office2013.PowerPoint.Roaming;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using HYDtmn.Abstraction;
using HYDtmn.jobweb.Models;
using log4net.Util.TypeConverters;
using Newtonsoft.Json.Linq;
using OpenXmlPowerTools;
using WebGrease.Css.Ast.Selectors;
using WebGrease.Css.Extensions;
using static HYDtmn.jobweb.Tools.MaterialHelper;

namespace HYDtmn.jobweb.Tools
{



    public class FakeView : IView
    {
        public void Render(ViewContext viewContext, TextWriter writer)
        {
            throw new InvalidOperationException();
        }
    }
    public static class BundleExtensions
    {
        public static HtmlHelper GetHtmlHelper(this Controller controller)
        {
            var viewContext = new ViewContext(controller.ControllerContext, new FakeView(), controller.ViewData, controller.TempData, TextWriter.Null);
            return new HtmlHelper(viewContext, new ViewPage());
        }
        public static void IncludeInOrder(this ScriptBundle bundle,BundleCollection collection,params string[] scripts)
        {
            var mybundle = bundle.Include(scripts);
            mybundle.Orderer = new AsIsBundleOrderer();
            collection.Add(mybundle);

        }
    }

    public static class WidgetHelper
    {


        public static TabHeaderWidget BeginTabHeaderWidget(this HtmlHelper htmlHelper, params string[] titles)
        {
            return new TabHeaderWidget(htmlHelper.ViewContext, titles);
        }

        //Please use the following in the div-container has class = "accordion arrows"
        public static AccordionItemWidget BeginAccItemWidget(this HtmlHelper htmlHelper, int itemNumber, string itemLabel, bool toCheck)
        {
            return new AccordionItemWidget(htmlHelper.ViewContext, itemNumber, itemLabel, toCheck);
        }
        //Please use the following in the div-container has class = "accordion arrows"
        public static AccordionHeaderWidget BeginAccHeaderWidget(this HtmlHelper htmlHelper, string title, bool withButton, bool hideButton)
        {
            return new AccordionHeaderWidget(htmlHelper.ViewContext, title, withButton, hideButton);
        }


        public class TabHeaderWidget : EmptyWidget
        {
            private readonly string[] tabTitles;

            public TabHeaderWidget(ViewContext viewContext, params string[] titles) : base(viewContext, 2)
            {
                this.tabTitles = titles;

                this.BeginWidget(this.tabTitles);
            }
            protected void BeginWidget(string[] titles)
            {
                var tablist = new TagBuilder("div");
                tablist.Attributes["role"] = "tablist";
                tablist.AddCssClass("mdc-tab-bar");
                var scroller = new TagBuilder("div");
                scroller.AddCssClass("mdc-tab-scroller");
                var area = new TagBuilder("div");
                area.AddCssClass("mdc-tab-scroller__scroll-area");
                var scrollcontent = new TagBuilder("div");


                foreach (var item in titles.Select((x, i) => new { index = i, title = x }))
                {

                    var tabclass = $"tabexit{item.index}";
                    var btn = new TagBuilder("button");
                    btn.AddCssClass("mdc-tab");
                    btn.Attributes["tabindex"] = $"{item.index}";
                    btn.Attributes["role"] = "tab";

                    btn.AddCssClass(tabclass);
                    if (item.index == 0)
                    {
                        btn.Attributes["aria-selected"] = "true";
                        btn.AddCssClass("mdc-tab--active");
                    }
                    var contentspan = new TagBuilder("span");
                    contentspan.AddCssClass("mdc-tab__content");
                    var labelspan = new TagBuilder("span");
                    labelspan.AddCssClass("mdc-tab__text-label");
                    labelspan.SetInnerText(item.title);
                    var indicator = new TagBuilder("span");
                    indicator.AddCssClass("mdc-tab-indicator");
                    if (item.index == 0)
                    {
                        indicator.AddCssClass("mdc-tab-indicator--active");
                    }
                    var underline = new TagBuilder("span");
                    underline.AddCssClass("mdc-tab-indicator__content");
                    underline.AddCssClass("mdc-tab-indicator__content--underline");
                    var ripper = new TagBuilder("span");
                    ripper.AddCssClass("mdc-tab__ripple");
                    contentspan.InnerHtml = labelspan.ToString();
                    indicator.InnerHtml = underline.ToString();
                    btn.InnerHtml = contentspan.ToString() + indicator.ToString() + ripper.ToString();
                    scrollcontent.InnerHtml += btn.ToString();





                }
                area.InnerHtml = scrollcontent.ToString();
                scroller.InnerHtml = area.ToString();
                tablist.InnerHtml = scroller.ToString();
                var container = new TagBuilder("div");
                container.AddCssClass("card-container");
                var card = new TagBuilder("div");
                card.AddCssClass("mdc-card");
                this.textWriter.WriteLine(container.ToString(TagRenderMode.StartTag));
                this.textWriter.WriteLine(card.ToString(TagRenderMode.StartTag));
                this.textWriter.WriteLine(tablist.ToString());
                //the inner expected to be array of "div with class = "content-grid" " and exactly one with "content-grid-active"

            }
        }

        public class AccordionHeaderWidget : EmptyWidget
        {
            private readonly string headerLabel;
            private readonly bool hasButton;
            private readonly bool hideButton;
            public AccordionHeaderWidget(ViewContext viewContext, string title, bool withbutton, bool tohide) : base(viewContext, withbutton ? 2 : 1)
            {
                this.headerLabel = title;
                this.hasButton = withbutton;
                this.hideButton = tohide;
                this.BeginWidget(this.headerLabel, this.hasButton, this.hideButton);
            }
            protected void BeginWidget(string hlab, bool wbtn, bool hidebtn)
            {
                var headerwrap = new TagBuilder("div");
                headerwrap.AddCssClass("acheader");
                headerwrap.AddCssClass("box");
                headerwrap.AddCssClass("header");
                var label = new TagBuilder("label");
                label.AddCssClass("box-title");
                label.Attributes["for"] = "acc-close";
                label.SetInnerText(hlab);
                this.textWriter.WriteLine(headerwrap.ToString(TagRenderMode.StartTag));
                this.textWriter.WriteLine(label.ToString());
                if (wbtn)
                {
                    var btnwrap = new TagBuilder("div");
                    btnwrap.AddCssClass("header");
                    btnwrap.AddCssClass("end");
                    btnwrap.AddCssClass("withgap");
                    btnwrap.AddCssClass("btn-wrapper");
                    if (hidebtn)
                    {
                        btnwrap.AddCssClass("hidden");
                    }


                    this.textWriter.WriteLine(btnwrap.ToString(TagRenderMode.StartTag));

                }



            }
        }
        public class AccordionItemWidget : EmptyWidget
        {
            private readonly int itemNumber;
            private readonly string itemLabel;
            private readonly bool itemChecked;
            public AccordionItemWidget(ViewContext viewContext, int itemnumber, string itemlabel, bool tocheck) : base(viewContext, 2)
            {
                itemNumber = itemnumber;
                itemLabel = itemlabel;
                itemChecked = tocheck;
                this.BeginWidget(this.itemNumber, this.itemLabel, this.itemChecked);
            }
            protected void BeginWidget(int inum, string ilab, bool tocheck)
            {
                var checkinput = new TagBuilder("input");
                checkinput.AddCssClass("ac");


                checkinput.Attributes["type"] = "radio";
                checkinput.Attributes["name"] = $"accordion{inum}";
                checkinput.Attributes["id"] = $"cb{inum}";
                if (tocheck)
                {
                    checkinput.AddCssClass("ac-check");
                    checkinput.Attributes["checked"] = "checked";
                }

                var sectionwrap = new TagBuilder("div");
                sectionwrap.AddCssClass("acsection");
                sectionwrap.AddCssClass("box");
                var labeltitle = new TagBuilder("label");
                labeltitle.AddCssClass("box-title");
                labeltitle.Attributes["for"] = $"cb{inum}";
                labeltitle.SetInnerText(ilab);
                var labelclose = new TagBuilder("label");
                labelclose.AddCssClass("box-close");
                labelclose.Attributes["for"] = "acc-close";
                var contentwrap = new TagBuilder("div");
                contentwrap.AddCssClass("box-content");

                this.textWriter.WriteLine(checkinput.ToString(TagRenderMode.SelfClosing));
                this.textWriter.WriteLine(sectionwrap.ToString(TagRenderMode.StartTag));
                this.textWriter.WriteLine(labeltitle.ToString());
                this.textWriter.WriteLine(labelclose.ToString());
                this.textWriter.WriteLine(contentwrap.ToString(TagRenderMode.StartTag));

            }
        }

        public class EmptyWidget : IDisposable
        {
            protected readonly ViewContext viewContext;
            protected readonly System.IO.TextWriter textWriter;
            protected readonly int closeCount;



            public EmptyWidget(ViewContext viewContext, int closecount = 0)
            {
                this.viewContext = viewContext;
                this.textWriter = viewContext.Writer;
                this.closeCount = closecount;
            }

            protected virtual void EndWidget()
            {

                var closetag = Enumerable.Range(0, closeCount).Select(e => "</div>").ToArray();

                if (closetag.Length > 0)
                {
                    foreach (var c in closetag)
                    {
                        this.textWriter.WriteLine(c);
                    }

                }

            }

            #region IDisposable

            private Boolean isDisposed;

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            public virtual void Dispose(Boolean disposing)
            {
                if (!this.isDisposed)
                {
                    this.isDisposed = true;
                    this.EndWidget();
                    this.textWriter.Flush();
                }
            }

            #endregion
        }


    }
    public static class MaterialHelper
    {
        public enum SvgType
        {
            Home,
            About,
            Contact,
            Note
        }
        public enum WrapType
        {
            Panel,
            Header,
            Input,
            TextArea,
            NumberSpinner,
            SelectList,
            RadioGroup,
            CheckBoxOnly,
            CheckBox,
            Password,

        }

        

        public static MvcHtmlString GetOption<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, int consume, bool isrequired, List<KeyValuePair<string, string>> optionpair,bool isSelect, string defaultvalue = null,string inputcss=null,bool isdisabled=false, object htmlAttributes = null)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            var value = (TValue)metadata.Model;

            var holder = ModelMetadata.FromLambdaExpression(expression, html.ViewData).Watermark;

            var name = ModelMetadata.FromLambdaExpression(expression, html.ViewData).DisplayName;

            if (string.IsNullOrEmpty(name))
                name = metadata.PropertyName;


            if (string.IsNullOrEmpty(holder))
                holder = name;
            var stringvalue = value?.ToString() ?? defaultvalue;

            if(TypeExtensions.BaseType(typeof(TValue)) == typeof(bool))
            {
                if(optionpair==null)
                {
                    optionpair = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("False", "False"), new KeyValuePair<string, string>("True", "True") };
                }
                if(stringvalue?.ToLower() == "true")
                {
                    stringvalue = "True";
                }
                else
                {
                    stringvalue = "False";
                }

            }
            var hasError = html.ViewData.ModelState[metadata.PropertyName]?.Errors != null;
            var error = hasError ? string.Join("\n", html.ViewData.ModelState[metadata.PropertyName].Errors.Select(e => e.ErrorMessage)) : "";


            inputcss = inputcss ?? string.Empty;

            return GetTemplate(isSelect ? WrapType.SelectList: WrapType.RadioGroup, consume, holder: holder, propname: metadata.PropertyName, label: name, isrequired: isrequired,  selectedvalue: stringvalue,optionpair:optionpair,inputcss:inputcss,isdisabled:isdisabled,error:error,htmlAttributes:htmlAttributes);

        }

        public static MvcHtmlString GetArea<TModel,TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, int consume, bool isrequired, string defaultvalue = null,string inputcss=null,bool isdisabled=false)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            var value = (TValue)metadata.Model;

            var holder = ModelMetadata.FromLambdaExpression(expression, html.ViewData).Watermark;

            var name = ModelMetadata.FromLambdaExpression(expression, html.ViewData).DisplayName;

            if (string.IsNullOrEmpty(name))
                name = metadata.PropertyName;

            if (string.IsNullOrEmpty(holder))
                holder = name;
            var stringvalue = value?.ToString() ?? defaultvalue;

            var hasError = html.ViewData.ModelState[metadata.PropertyName]?.Errors != null;
            var error = hasError ? string.Join("\n", html.ViewData.ModelState[metadata.PropertyName].Errors.Select(e => e.ErrorMessage)) : "";

            return GetTemplate(WrapType.TextArea, consume, holder: holder, propname: metadata.PropertyName, label: name, isrequired: isrequired, selectedvalue: stringvalue,inputcss:inputcss,error:error,isdisabled:isdisabled);
        }

        public static MvcHtmlString GetNumber<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, int consume, bool isrequired,int min,int max, string defaultvalue = null,string inputcss=null,bool isdisabled=false,string hardlabel=null)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            var value = (TValue)metadata.Model;

            var holder = ModelMetadata.FromLambdaExpression(expression, html.ViewData).Watermark;

            var name = ModelMetadata.FromLambdaExpression(expression, html.ViewData).DisplayName;
            if (string.IsNullOrEmpty(name))
                name = metadata.PropertyName;

            if (string.IsNullOrEmpty(holder))
                holder = name;
            var stringvalue = GetDisplayValue<TModel, TValue>(value, metadata.PropertyName) ?? defaultvalue;
            if (stringvalue==null || int.Parse( stringvalue) <min)
            {
                stringvalue = min.ToString();
            }
            name = hardlabel ?? name;

            var hasError = html.ViewData.ModelState[metadata.PropertyName]?.Errors != null;
            var error = hasError ? string.Join("\n", html.ViewData.ModelState[metadata.PropertyName].Errors.Select(e => e.ErrorMessage)) : "";

            return GetTemplate(WrapType.NumberSpinner, consume, holder: holder, propname: metadata.PropertyName, label: name, isrequired: isrequired,  selectedvalue: stringvalue,spinmin:min,spinmax:max,inputcss: inputcss,isdisabled:isdisabled,error:error);

        }

        public static string GetDisplayValue<TModel,TValue>(TValue value,string propertyname,bool isdisabled=false,string defaultvalue=null)
        {
            var metadataType = typeof(TModel).GetCustomAttributes(typeof(MetadataTypeAttribute), true)
                .OfType<MetadataTypeAttribute>().FirstOrDefault();

            

            if(metadataType!=null)
            {
                var innermetadata = ModelMetadataProviders.Current.GetMetadataForType(null, metadataType.MetadataClassType);

                var displayattrb = innermetadata.ModelType.GetProperty(propertyname)?.GetCustomAttributes(typeof(DisplayFormatAttribute), false)?.FirstOrDefault();
                if(displayattrb!=null)
                {
                    string defaultval = null;
                    if(TypeExtensions.BaseType(typeof(TValue)) == typeof(DateTime) && value==null && !isdisabled)
                    {
                        if(defaultvalue!=null && defaultvalue == "(today)")
                        {
                            defaultval = string.Format((displayattrb as DisplayFormatAttribute).DataFormatString, DateTime.Today);
                        }
                        

                        return defaultval;
                    }
                    return string.Format((displayattrb as DisplayFormatAttribute).DataFormatString, value );
                }

            }

            return value?.ToString();

        }

        public static MvcHtmlString GetPassword<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, int consume, bool isrequired,    object htmlAttributes = null)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            var value = (TValue)metadata.Model;



            var holder = ModelMetadata.FromLambdaExpression(expression, html.ViewData).Watermark;

            var name = ModelMetadata.FromLambdaExpression(expression, html.ViewData).DisplayName;
            if (string.IsNullOrEmpty(name))
                name = metadata.PropertyName;

            if (string.IsNullOrEmpty(holder))
                holder = name;
            var stringvalue = GetDisplayValue<TModel, TValue>(value, metadata.PropertyName, false) ;
            var hasError = html.ViewData.ModelState[metadata.PropertyName]?.Errors != null;
            var error =hasError ?  string.Join("\n", html.ViewData.ModelState[metadata.PropertyName].Errors.Select(e => e.ErrorMessage)) : "";

            return GetTemplate(WrapType.Password, consume, holder: holder, propname: metadata.PropertyName, label: name, isrequired: isrequired,  selectedvalue: stringvalue,error:error, htmlAttributes: htmlAttributes);

        }
        public static MvcHtmlString GetInput<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression,int consume,bool isrequired,bool isdate,string defaultvalue=null,string inputcss=null,bool isdisabled=false,string urlis=null,string backcolorcss=null, object htmlAttributes = null,string hardlabel=null)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            var value = (TValue)metadata.Model;
            
            
            
            var holder = ModelMetadata.FromLambdaExpression(expression, html.ViewData).Watermark;

            
            var name = ModelMetadata.FromLambdaExpression(expression, html.ViewData).DisplayName;
            if (string.IsNullOrEmpty(name))
                name = metadata.PropertyName;

            if(!string.IsNullOrEmpty(hardlabel))
            {
                name = hardlabel;
            }

            if (string.IsNullOrEmpty(holder))
                holder = name;
            var stringvalue = metadata != null && metadata.DisplayFormatString != null && value != null ? string.Format(metadata.DisplayFormatString, value) : "";
            if (string.IsNullOrEmpty(stringvalue))
                stringvalue = GetDisplayValue<TModel, TValue>(value, metadata.PropertyName, isdisabled,defaultvalue) ?? defaultvalue;

            inputcss = inputcss ?? "";
            var hasError = html.ViewData.ModelState[metadata.PropertyName]?.Errors != null;
            var error = hasError ? string.Join("\n", html.ViewData.ModelState[metadata.PropertyName].Errors.Select(e => e.ErrorMessage)) : "";

            return GetTemplate(WrapType.Input, consume, holder: holder, propname: metadata.PropertyName, label: name,isrequired:isrequired,inputcss: isdate ? $"{inputcss} datepicker": inputcss,selectedvalue:stringvalue,isdisabled:isdisabled,urlis:urlis,backcolorcss:backcolorcss,error:error,htmlAttributes:htmlAttributes);

        }

        public static MvcHtmlString GetMdcGroupDisplayFor<TModel,TKey,TDesc>(this HtmlHelper<TModel> html, Expression<Func<TModel,IEnumerable<TKey>>> expKeys, Expression<Func<TModel,IEnumerable<TDesc>>> expDescs)
        {
            ModelMetadata metadataKeys = ModelMetadata.FromLambdaExpression(expKeys, html.ViewData);
            ModelMetadata metadataDescs = ModelMetadata.FromLambdaExpression(expDescs, html.ViewData);

            var  keys = (IEnumerable<TKey>)metadataKeys.Model;
            var  descs = (IEnumerable<TDesc>)metadataDescs.Model;
            var htmlstring = "";

            foreach(var index in  Enumerable.Range(0, keys.Count()))
            {
                var propname = keys.Skip(index).First();
                var desc = descs.Skip(index).First();

                var wrap = new TagBuilder("div");
                var innerwrap = new TagBuilder("div");
                var rippletag = new TagBuilder("div");
                var labeltag = new TagBuilder("label");
                var hiddentag = new TagBuilder("input");


                wrap.AddCssClass("mdc-form-field");
                var inputtag = new TagBuilder("input");

                

                innerwrap.AddCssClass("mdc-checkbox");



                inputtag.AddCssClass("mdc-checkbox__native-control");
                inputtag.Attributes["type"] = "checkbox";
                inputtag.Attributes["id"] = propname.ToString();
                hiddentag.Attributes["type"] = "hidden";
                hiddentag.Attributes["name"] = propname.ToString();

                var backgroundwrap = new TagBuilder("div");
                backgroundwrap.AddCssClass("mdc-checkbox__background");
                var marktag = new TagBuilder("div");
                marktag.AddCssClass("mdc-checkbox_mixedmark");
                var svgtag = new TagBuilder("svg");
                svgtag.AddCssClass("mdc-checkbox__checkmark");
                svgtag.Attributes["viewBox"] = "0 0 24 24";
                var pathtag = new TagBuilder("path");
                pathtag.AddCssClass("mdc-checkbox__checkmark-path");
                pathtag.Attributes["fill"] = "none";
                pathtag.Attributes["d"] = "M1.73,12.91 8.1,19.28 22.79,4.59";
                svgtag.InnerHtml = pathtag.ToString();
                backgroundwrap.InnerHtml = svgtag.ToString() + marktag.ToString();
                rippletag.AddCssClass("mdc-checkbox__ripple");
                innerwrap.InnerHtml = inputtag.ToString() + hiddentag.ToString() + backgroundwrap.ToString() + rippletag.ToString();
                labeltag.Attributes["for"] = propname.ToString();
                labeltag.Attributes["id"] = propname.ToString() + "-label";

                labeltag.SetInnerText(desc.ToString());
                //labeltag.AddCssClass("text-nowrap");


                wrap.InnerHtml = innerwrap.ToString() + labeltag.ToString();

                htmlstring += wrap.ToString();
            }

            return new MvcHtmlString(htmlstring);

        }

        public static MvcHtmlString GetMdcDisplayFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression,WrapType wraptype,bool nolabel,string inputcss=null,string pseudoname=null)
        {

            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            var placeholdervalue = ModelMetadata.FromLambdaExpression(expression, html.ViewData).Watermark;

            var propname = metadata.PropertyName;

            var namevalue = ModelMetadata.FromLambdaExpression(expression, html.ViewData).DisplayName;
            TagBuilder mdcwrap = new TagBuilder("div");


            TValue value = (TValue)metadata.Model;
            string defaultvalue = value?.ToString();
            var formatattr =string.IsNullOrEmpty(propname) ? null :  metadata.ContainerType.GetProperty(propname)?.GetCustomAttributes(typeof(DisplayFormatAttribute), false)?.FirstOrDefault();

            if (formatattr != null)
            {

                defaultvalue = string.Format((formatattr as DisplayFormatAttribute).DataFormatString, value);
            }

            var wrap = new TagBuilder("div");
            var innerwrap = new TagBuilder("div");
            var rippletag = new TagBuilder("div");
            var labeltag = new TagBuilder("label");
            var hiddentag = new TagBuilder("input");


            wrap.AddCssClass("mdc-form-field");
            var inputtag = new TagBuilder("input");

            propname = propname ?? pseudoname;

            switch (wraptype)
            {
                case WrapType.CheckBoxOnly:
                case WrapType.CheckBox:
                    innerwrap.AddCssClass("mdc-checkbox");
                    
                    if(wraptype== WrapType.CheckBoxOnly)
                    {
                        innerwrap.AddCssClass("mdc-checkbox--disabled");
                        inputtag.Attributes["disabled"] = "disabled";
                    }

                    
                    inputtag.AddCssClass("mdc-checkbox__native-control");
                    inputtag.Attributes["type"] = "checkbox";
                    inputtag.Attributes["id"] = propname ;
                    hiddentag.Attributes["type"] = "hidden";
                    hiddentag.Attributes["name"] = propname;

                    if(!string.IsNullOrEmpty(inputcss))
                    {
                        hiddentag.AddCssClass(inputcss);
                        inputtag.AddCssClass("checkbox-" + inputcss);
                        labeltag.AddCssClass("label-" + inputcss);
                    }

                    
                    
                    bool boolvalue = false;
                    boolvalue = bool.TryParse(defaultvalue, out boolvalue) ? boolvalue : false;
                    if (boolvalue)
                        inputtag.Attributes["checked"] = "checked";

                    hiddentag.Attributes["pseudo"] = value?.ToString();
                    hiddentag.Attributes["value"] =  boolvalue.ToString();
                    var backgroundwrap=new TagBuilder("div");
                    backgroundwrap.AddCssClass("mdc-checkbox__background");
                    var marktag = new TagBuilder("div");
                    marktag.AddCssClass("mdc-checkbox_mixedmark");
                    var svgtag = new TagBuilder("svg");
                    svgtag.AddCssClass("mdc-checkbox__checkmark");
                    svgtag.Attributes["viewBox"] = "0 0 24 24";
                    var pathtag = new TagBuilder("path");
                    pathtag.AddCssClass("mdc-checkbox__checkmark-path");
                    pathtag.Attributes["fill"] = "none";
                    pathtag.Attributes["d"] = "M1.73,12.91 8.1,19.28 22.79,4.59";
                    svgtag.InnerHtml = pathtag.ToString();
                    backgroundwrap.InnerHtml = svgtag.ToString() +marktag.ToString();
                    rippletag.AddCssClass("mdc-checkbox__ripple");
                    innerwrap.InnerHtml = inputtag.ToString()+hiddentag.ToString()+ backgroundwrap.ToString() + rippletag.ToString();
                    labeltag.Attributes["for"] = propname;
                    labeltag.Attributes["id"] = propname + "-label";
                    
                    if (!nolabel)
                    {
                        labeltag.SetInnerText(namevalue ?? value?.ToString());
                        labeltag.AddCssClass("text-nowrap");
                    }
                    wrap.InnerHtml = innerwrap.ToString() + labeltag.ToString();

                    break;
                default:
                    break;
            }

            return new MvcHtmlString(wrap.ToString());
        }

        public static MvcHtmlString GetSimUICheckboxFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, bool nolabel, string inputcss = null, string pseudoname = null, bool rightlabel = false)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            var placeholdervalue = ModelMetadata.FromLambdaExpression(expression, html.ViewData).Watermark;

            var propname = metadata.PropertyName;

            var namevalue = ModelMetadata.FromLambdaExpression(expression, html.ViewData).DisplayName;
            TValue value = (TValue)metadata.Model;
            string defaultvalue = value?.ToString();
            var formatattr = string.IsNullOrEmpty(propname) ? null : metadata.ContainerType.GetProperty(propname)?.GetCustomAttributes(typeof(DisplayFormatAttribute), false)?.FirstOrDefault();

            if (formatattr != null)
            {

                defaultvalue = string.Format((formatattr as DisplayFormatAttribute).DataFormatString, value);
            }
            var wrap = new TagBuilder("div");
            var labeltag = new TagBuilder("label");
            var hiddentag = new TagBuilder("input");
            var inputtag = new TagBuilder("input");
            var svgsketag = new TagBuilder("svg");
            var svgtag = new TagBuilder("svg");
            var symtag = new TagBuilder("symbol");
            var poytag = new TagBuilder("polyline");
            var usetag = new TagBuilder("use");
            wrap.AddCssClass(inputcss);
            wrap.AddCssClass("simUI");
            labeltag.AddCssClass("cbx");
            inputtag.AddCssClass("inp-cbx");
            svgtag.AddCssClass("inline-svg");
            svgsketag.Attributes["width"] = "12px";
            svgsketag.Attributes["height"] = "10px";
            symtag.Attributes["viewbox"] = "0 0 12 10";
            poytag.Attributes["points"] = "1.5 6 4.5 9 10.5 1";

            var spanwrap = new TagBuilder("span");
            var spanlabel = new TagBuilder("span");




            propname = propname ?? pseudoname;



            inputtag.Attributes["type"] = "checkbox";
            inputtag.Attributes["id"] = propname;
            hiddentag.Attributes["type"] = "hidden";
            hiddentag.Attributes["name"] = propname;
            labeltag.Attributes["for"] = propname;
            usetag.Attributes["xlink:href"] = "#svg-" + propname;
            symtag.Attributes["id"] = "svg-" + propname;
            symtag.InnerHtml = poytag.ToString();
            svgtag.InnerHtml = symtag.ToString();
            svgsketag.InnerHtml = usetag.ToString();
            spanwrap.InnerHtml = svgsketag.ToString();

            bool boolvalue = false;
            boolvalue = bool.TryParse(defaultvalue, out boolvalue) ? boolvalue : false;
            if (boolvalue)
                inputtag.Attributes["checked"] = "checked";
            hiddentag.Attributes["pseudo"] = value?.ToString();
            hiddentag.Attributes["value"] = boolvalue.ToString();

            if (!nolabel)
            {
                spanlabel.SetInnerText(namevalue ?? value?.ToString());

            }
            labeltag.InnerHtml = spanwrap.ToString() + spanlabel.ToString();

            if (!string.IsNullOrEmpty(inputcss))
            {
                hiddentag.AddCssClass(inputcss);
                inputtag.AddCssClass("checkbox-" + inputcss);
                labeltag.AddCssClass("label-" + inputcss);
            }
            wrap.InnerHtml = hiddentag.ToString() + inputtag.ToString() + labeltag.ToString() + svgtag.ToString();

            return new MvcHtmlString(wrap.ToString());
        }
        public static MvcHtmlString GetSvg(this HtmlHelper html, SvgType svgtype)
        {
            var svgwrap = new TagBuilder("svg");

            var path1 = new TagBuilder("path");
            var path2 = new TagBuilder("path");
            var path3 = new TagBuilder("path");
            var path4 = new TagBuilder("path");
            var path5 = new TagBuilder("path");
            var path6 = new TagBuilder("path");
            var path7 = new TagBuilder("path");
            var path8 = new TagBuilder("path");
            var path9 = new TagBuilder("path");

            var poyl1 = new TagBuilder("polyline");
            var cir1 = new TagBuilder("circle");
            var lin1 = new TagBuilder("line");
            svgwrap.Attributes["viewBox"] = "0 0 24 24";
            svgwrap.Attributes["height"] = "24px";
            svgwrap.Attributes["width"] = "24px";
            svgwrap.Attributes["stroke"] = "currentColor";
            svgwrap.Attributes["stroke-with"] = "2";
            svgwrap.Attributes["fill"] = "none";
            svgwrap.Attributes["stroke-linecap"] = "round";
            svgwrap.Attributes["stroke-linejoin"] = "round";
            svgwrap.Attributes["class"] = "css-i6dzq1";


            switch (svgtype)
            {
                case SvgType.Note:
                    svgwrap.Attributes["viewBox"] = "0 0 32 32";
                    svgwrap.Attributes["height"] = "32px";
                    svgwrap.Attributes["width"] = "32px";
                    var g = new TagBuilder("g");
                    path1.Attributes["d"] = "M31.414,7.585l-6-6C25.039,1.21,24.529,1,24,1H3   C1.346,1,0,2.345,0,4v24c0,1.654,1.346,3,3,3h26c1.654,0,3-1.346,3-3V9C32,8.469,31.789,7.96,31.414,7.585z M30,28   c0,0.553-0.447,1-1,1H3c-0.553,0-1-0.447-1-1V4c0-0.553,0.447-1,1-1h20v4h-0.002c0,1.657,1.344,3,3,3h1H30V28z M26.998,9h-1   c-1.102,0-2-0.897-2-2H24V3l6,6H26.998z";
                    path1.Attributes["clip-rule"] = "evenodd";
                    path1.Attributes["fill-rule"] = "evenodd";
                    path2.Attributes["d"] = "M15.5,8h5C20.775,8,21,7.776,21,7.5S20.775,7,20.5,7h-5   C15.224,7,15,7.223,15,7.5S15.224,8,15.5,8z";
                    path2.Attributes["clip-rule"] = "evenodd";
                    path2.Attributes["fill-rule"] = "evenodd";

                    path3.Attributes["d"] = "M15.5,11h5c0.275,0,0.5-0.224,0.5-0.5S20.775,10,20.5,10h-5   c-0.276,0-0.5,0.224-0.5,0.5S15.224,11,15.5,11z";
                    path3.Attributes["clip-rule"] = "evenodd";
                    path3.Attributes["fill-rule"] = "evenodd";

                    path4.Attributes["d"] = "M15,13.5c0,0.276,0.224,0.5,0.5,0.5h12c0.275,0,0.5-0.224,0.5-0.5   S27.775,13,27.5,13h-12C15.224,13,15,13.223,15,13.5z";
                    path4.Attributes["clip-rule"] = "evenodd";
                    path4.Attributes["fill-rule"] = "evenodd";

                    path5.Attributes["d"] = "M27.5,19h-23C4.224,19,4,19.223,4,19.5C4,19.775,4.224,20,4.5,20   h23c0.275,0,0.5-0.225,0.5-0.5C28,19.223,27.775,19,27.5,19z";
                    path5.Attributes["clip-rule"] = "evenodd";
                    path5.Attributes["fill-rule"] = "evenodd";

                    path6.Attributes["d"] = "M27.5,22h-23C4.224,22,4,22.223,4,22.5C4,22.775,4.224,23,4.5,23   h23c0.275,0,0.5-0.225,0.5-0.5C28,22.223,27.775,22,27.5,22z";
                    path6.Attributes["clip-rule"] = "evenodd";
                    path6.Attributes["fill-rule"] = "evenodd";

                    path7.Attributes["d"] = "M27.5,25h-23C4.224,25,4,25.223,4,25.5C4,25.775,4.224,26,4.5,26   h23c0.275,0,0.5-0.225,0.5-0.5C28,25.223,27.775,25,27.5,25z";
                    path7.Attributes["clip-rule"] = "evenodd";
                    path7.Attributes["fill-rule"] = "evenodd";

                    path8.Attributes["d"] = "M27.5,16h-23C4.224,16,4,16.223,4,16.5S4.224,17,4.5,17h23   c0.275,0,0.5-0.224,0.5-0.5S27.775,16,27.5,16z";
                    path8.Attributes["clip-rule"] = "evenodd";
                    path8.Attributes["fill-rule"] = "evenodd";

                    path9.Attributes["d"] = "M5,14h7c0.553,0,1-0.447,1-1V7c0-0.553-0.447-1-1-1H5   C4.447,6,4,6.447,4,7v6C4,13.552,4.447,14,5,14z M6,8h5v4H6V8z";
                    path9.Attributes["clip-rule"] = "evenodd";
                    path9.Attributes["fill-rule"] = "evenodd";
                    g.InnerHtml = path1.ToString() + path2.ToString() + path3.ToString() + path4.ToString() + path5.ToString() + path6.ToString() + path7.ToString() + path8.ToString() + path9.ToString();
                    svgwrap.InnerHtml = g.ToString();


                    break;
                case SvgType.Home:

                    path1.Attributes["d"] = "M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z";
                    poyl1.Attributes["points"] = "9 22 9 12 15 12 15 22";
                    svgwrap.InnerHtml = path1.ToString() + poyl1.ToString();

                    break;
                case SvgType.About:

                    cir1.Attributes["cx"] = "12";
                    cir1.Attributes["cy"] = "12";
                    cir1.Attributes["r"] = "10";
                    path1.Attributes["d"] = "M9.09 9a3 3 0 0 1 5.83 1c0 2-3 3-3 3";
                    lin1.Attributes["x1"] = "12";
                    lin1.Attributes["x2"] = "12";
                    lin1.Attributes["y1"] = "17";
                    lin1.Attributes["y2"] = "17";
                    svgwrap.InnerHtml = cir1.ToString() + path1.ToString() + lin1.ToString();


                    break;
                case SvgType.Contact:
                    path1.Attributes["d"] = "M21 11.5a8.38 8.38 0 0 1-.9 3.8 8.5 8.5 0 0 1-7.6 4.7 8.38 8.38 0 0 1-3.8-.9L3 21l1.9-5.7a8.38 8.38 0 0 1-.9-3.8 8.5 8.5 0 0 1 4.7-7.6 8.38 8.38 0 0 1 3.8-.9h.5a8.48 8.48 0 0 1 8 8v.5z";
                    svgwrap.InnerHtml = path1.ToString();
                    break;
            }
            return new MvcHtmlString(svgwrap.ToString());
        }
        public static MvcHtmlString GetminiTab(this HtmlHelper helper, MvcHtmlString field, string label, bool nopadding = true, bool selected = false, MvcHtmlString svg = null)
        {
            var id = label.Replace(" ", "-");
            var inputwrap = new TagBuilder("input");
            inputwrap.Attributes["type"] = "radio";
            inputwrap.Attributes["name"] = "nav";
            inputwrap.Attributes["id"] = id;
            if (selected)
                inputwrap.Attributes["checked"] = "checked";
            inputwrap.AddCssClass("tab-element");
            inputwrap.AddCssClass("nav");
            inputwrap.AddCssClass($"{id}-radio");
            var contentwrap = new TagBuilder("div");
            contentwrap.AddCssClass("page");
            if (nopadding)
                contentwrap.AddCssClass("nopadding");
            contentwrap.AddCssClass($"{id}-page");
            var fieldwrap = new TagBuilder("div");
            fieldwrap.AddCssClass("page-contents");
            fieldwrap.InnerHtml = field.ToString();
            contentwrap.InnerHtml = fieldwrap.ToString();

            var labelwrap = new TagBuilder("label");
            labelwrap.Attributes["for"] = id;
            labelwrap.AddCssClass("nav");
            labelwrap.AddCssClass("tab-element");
            var spanwrap = new TagBuilder("span");
            if (svg == null)
                spanwrap.SetInnerText(label);
            else
                spanwrap.InnerHtml = svg.ToString() + label;
            labelwrap.InnerHtml = spanwrap.ToString();


            return new MvcHtmlString(inputwrap.ToString() + contentwrap.ToString() + labelwrap.ToString());
        }

        public static MvcHtmlString GetEmptyWrap(this HtmlHelper html, int consume, string inputcss = "")
        {
            var wrap = new TagBuilder("div");
            MaterialComponentCssList.GetDivWrap(consume).ToList().ForEach(e => wrap.AddCssClass(e));
            wrap.AddCssClass(inputcss);
            return new MvcHtmlString(wrap.ToString());
        }

        public static MvcHtmlString GetTemplate(WrapType wraptype,int consume=0,string headervalue=null,string propname=null,string holder=null,string label=null,bool isrequired=false,int spinmin=1899,int spinmax=2028,List<KeyValuePair<string,string>> optionpair=null,string selectedvalue=null,string inputcss=null,bool isdisabled=false,string urlis=null,string backcolorcss=null,string error=null, object htmlAttributes = null)
        {
            var wrap = new TagBuilder("div");
            var eyewrap = new TagBuilder("div");
            var innerwrap = new TagBuilder("div");
            var labelwrap =new TagBuilder("label");
            var labelspan = new TagBuilder("span");

            var endspan = new TagBuilder("span");
            var startspan =new TagBuilder("span");
            endspan.AddCssClass("mdc-line-ripple");
            var helperwrapper = new TagBuilder("div");
            helperwrapper.AddCssClass("mdc-text-field-helper-line");
            var helpertag = new TagBuilder("div");
            helpertag.AddCssClass("mdc-text-field-helper-text");            
            helpertag.AddCssClass("mdc-text-field-helper-text--validation-msg");
            helpertag.AddCssClass("error-brick");
            helpertag.Attributes["id"] = $"{propname}-helper-text";
            helpertag.Attributes["aria-hidden"] = "true";
            var bouncerwrapper = new TagBuilder("div");
            bool hasError = !string.IsNullOrEmpty(error);
            if (hasError)
            {
                helpertag.SetInnerText(error);
                helperwrapper.InnerHtml = helpertag.ToString();
                
            }

            bouncerwrapper.Attributes["id"] = $"{propname}-bouncer-message";
            bouncerwrapper.AddCssClass("bouncer-validation");

            switch (wraptype)
            {
                case WrapType.Panel:
                    wrap.AddCssClass("mdc-layout-grid");
                    innerwrap.AddCssClass("mdc-layout-grid__inner");
                    wrap.InnerHtml = innerwrap.ToString();
                    return new MvcHtmlString(wrap.ToString());
                    
                case WrapType.Header:
                    MaterialComponentCssList.GetDivWrapFull().ToList().ForEach(e => wrap.AddCssClass(e));
                    var header = new TagBuilder("h2");
                    header.SetInnerText(headervalue);
                    wrap.InnerHtml = header.ToString();
                    return new MvcHtmlString(wrap.ToString());
                case WrapType.TextArea:
                    MaterialComponentCssList.GetDivWrap(consume).ToList().ForEach(e => wrap.AddCssClass(e));
                    "mdc-text-field mdc-text-field--filled mdc-text-field--textarea w-100".Split(' ').ToList().ForEach(e => labelwrap.AddCssClass(e));
                    labelwrap.Attributes["id"] = $"{propname}-label";
                    startspan.AddCssClass("mdc-text-field__ripple");
                    var resizer=new TagBuilder("span");
                    resizer.AddCssClass("mdc-text-field__resizer");
                    var textwrap = new TagBuilder("textarea");
                    textwrap.Attributes["id"] = $"{propname}";
                    textwrap.Attributes["name"] = $"{propname}";
                    textwrap.Attributes["data-bouncer-target"] = '#'+bouncerwrapper.Attributes["id"];
                    if(hasError)
                    {
                        labelwrap.AddCssClass("mdc-text-field--invalid");
                    }

                    if (isdisabled)
                    {
                        textwrap.MergeAttribute("disabled", string.Empty);
                        labelwrap.AddCssClass("mdc-text-field--disabled");
                    }
                    if(inputcss.ItSplit(" ").Contains("norequired"))
                    {
                        wrap.AddCssClass("norequired");
                    }


                    if (!string.IsNullOrEmpty(inputcss))
                    {
                        textwrap.AddCssClass(inputcss);

                    }
                    if (!string.IsNullOrEmpty(selectedvalue))
                    {
                        textwrap.SetInnerText( selectedvalue);


                    }
                    if (isrequired) textwrap.MergeAttribute("required", string.Empty);
                    textwrap.Attributes["aria-labelledby"] = $"{propname}-floating-label";
                    textwrap.Attributes["rows"] = "4";
                    var rezspan = new TagBuilder("span");
                    rezspan.AddCssClass("mdc-text-field__resizer");
                    textwrap.AddCssClass("mdc-text-field__input");
                    rezspan.InnerHtml = textwrap.ToString();
                    
                    labelspan.AddCssClass("mdc-floating-label");
                    labelspan.Attributes["id"] = $"{propname}-floating-label";
                    labelspan.SetInnerText(label);
                    labelwrap.InnerHtml = startspan.ToString() + rezspan.ToString() + labelspan.ToString() + endspan.ToString();
                    wrap.InnerHtml = labelwrap.ToString() + (hasError ? helperwrapper.ToString() : "") + bouncerwrapper.ToString();
                    if(!string.IsNullOrEmpty(inputcss))
                    {
                        wrap.AddCssClass(inputcss + "-wrapper");
                    }
                    return new MvcHtmlString(wrap.ToString());
                case WrapType.NumberSpinner:
                case WrapType.Input:
                case WrapType.Password:
                    MaterialComponentCssList.GetDivWrap(consume).ToList().ForEach(e => wrap.AddCssClass(e));

                    "mdc-text-field mdc-text-field--filled w-100".Split(' ').ToList().ForEach(e => labelwrap.AddCssClass(e));
                    labelwrap.Attributes["id"] = $"{propname}-label";

                    if (hasError)
                    {
                        labelwrap.AddCssClass("mdc-text-field--invalid");
                    }



                    if (!string.IsNullOrEmpty(backcolorcss))
                    {
                        labelwrap.AddCssClass(backcolorcss);
                    }

                    if (isdisabled)
                    {
                        labelwrap.AddCssClass("mdc-text-field--disabled");
                        if(backcolorcss==null)
                        {
                            labelwrap.AddCssClass("darker-disabled");
                        }
                    }
                        

                    startspan.AddCssClass("mdc-text-field__ripple");
                    var inputwrap = new TagBuilder("input");
                    inputwrap.Attributes["data-bouncer-target"] ='#'+ bouncerwrapper.Attributes["id"];
                    if (htmlAttributes != null)
                    {
                        
                        inputwrap.MergeAttributes(new RouteValueDictionary(htmlAttributes));
                    }
                    inputwrap.Attributes["id"] = $"{propname}";
                    inputwrap.Attributes["name"] = $"{propname}";
                    inputwrap.Attributes["aria-controls"] = $"{propname}-helper-text";
                    inputwrap.Attributes["aria-describedby"] = $"{propname}-helper-text";
                    if (wraptype== WrapType.Input && !string.IsNullOrEmpty(urlis))
                    {
                        inputwrap.Attributes["urlis"] = urlis;
                    }

                    inputwrap.Attributes["type"] =wraptype== WrapType.Input ? "text" : (wraptype== WrapType.Password ? "password": "number");
                    if (isdisabled)
                    {
                        inputwrap.MergeAttribute("disabled", String.Empty);
                        if(backcolorcss == null)
                        {
                            inputwrap.AddCssClass("darker-disabled");
                        }
                    }
                        
                    if (!string.IsNullOrEmpty(inputcss))
                    {
                        inputwrap.AddCssClass(inputcss);

                    }
                    if(!string.IsNullOrEmpty(selectedvalue))
                    {
                        inputwrap.Attributes["value"] = selectedvalue;
                    }
                    var isPassword = false;
                    if(wraptype== WrapType.Password)
                    {
                        isPassword = true;
                        
                        eyewrap.AddCssClass( "mdc-text-field--eye");
                        var eyeopensvg = new TagBuilder("svg");
                        var eyeopenpath = new TagBuilder("path");
                        eyeopensvg.MergeAttribute("fill", "none");
                        eyeopensvg.MergeAttribute("viewBox", "0 0 640 512");
                        eyeopensvg.MergeAttribute("xmlns", "http://www.w3.org/2000/svg");
                        eyeopenpath.MergeAttribute("fill", "currentColor");
                        eyeopenpath.MergeAttribute("d", "M572.52 241.4C518.29 135.59 410.93 64 288 64S57.68 135.64 3.48 241.41a32.35 32.35 0 0 0 0 29.19C57.71 376.41 165.07 448 288 448s230.32-71.64 284.52-177.41a32.35 32.35 0 0 0 0-29.19zM288 400a144 144 0 1 1 144-144 143.93 143.93 0 0 1-144 144zm0-240a95.31 95.31 0 0 0-25.31 3.79 47.85 47.85 0 0 1-66.9 66.9A95.78 95.78 0 1 0 288 160z");
                        eyeopensvg.AddCssClass("mdc-text-field--eyeopen");
                        eyeopensvg.InnerHtml = eyeopenpath.ToString();
                        
                        var eyeclosesvg = new TagBuilder("svg");
                        var eyeclosepath = new TagBuilder("path");
                        eyeclosesvg.MergeAttribute("fill", "none");
                        eyeclosesvg.MergeAttribute("viewBox", "0 0 640 512");
                        eyeclosesvg.MergeAttribute("xmlns", "http://www.w3.org/2000/svg");
                        eyeclosepath.MergeAttribute("fill", "currentColor");
                        eyeclosepath.MergeAttribute("d", "M320 400c-75.85 0-137.25-58.71-142.9-133.11L72.2 185.82c-13.79 17.3-26.48 35.59-36.72 55.59a32.35 32.35 0 0 0 0 29.19C89.71 376.41 197.07 448 320 448c26.91 0 52.87-4 77.89-10.46L346 397.39a144.13 144.13 0 0 1-26 2.61zm313.82 58.1l-110.55-85.44a331.25 331.25 0 0 0 81.25-102.07 32.35 32.35 0 0 0 0-29.19C550.29 135.59 442.93 64 320 64a308.15 308.15 0 0 0-147.32 37.7L45.46 3.37A16 16 0 0 0 23 6.18L3.37 31.45A16 16 0 0 0 6.18 53.9l588.36 454.73a16 16 0 0 0 22.46-2.81l19.64-25.27a16 16 0 0 0-2.82-22.45zm-183.72-142l-39.3-30.38A94.75 94.75 0 0 0 416 256a94.76 94.76 0 0 0-121.31-92.21A47.65 47.65 0 0 1 304 192a46.64 46.64 0 0 1-1.54 10l-73.61-56.89A142.31 142.31 0 0 1 320 112a143.92 143.92 0 0 1 144 144c0 21.63-5.29 41.79-13.9 60.11z");
                        eyeclosesvg.AddCssClass("mdc-text-field--eyeclose");
                        eyeclosesvg.InnerHtml = eyeclosepath.ToString();
                        eyewrap.InnerHtml = eyeopensvg.ToString() + eyeclosesvg.ToString();

                        inputwrap.Attributes["autocomplete"] = "off";


                    }
                        
                    if (wraptype == WrapType.NumberSpinner) {
                        inputwrap.Attributes["min"] = spinmin.ToString(); inputwrap.Attributes["max"]= spinmax.ToString();
                    }

                    inputwrap.Attributes["placeholder"] = holder;
                    if (isrequired) inputwrap.MergeAttribute("required", string.Empty);
                    inputwrap.Attributes["aria-labelledby"] = $"{propname}-floating-label";
                    inputwrap.AddCssClass("mdc-text-field__input");
                    
                    labelspan.AddCssClass("mdc-floating-label");
                    labelspan.Attributes["id"]= $"{propname}-floating-label";
                    labelspan.SetInnerText(label);
                    labelwrap.InnerHtml = startspan.ToString() + inputwrap.ToString() + labelspan.ToString() + endspan.ToString() + (isPassword ? eyewrap.ToString() : "");
                    wrap.InnerHtml = labelwrap.ToString() + (hasError ? helperwrapper.ToString() : "") + bouncerwrapper.ToString();
                    return new MvcHtmlString(wrap.ToString());                    
                case WrapType.SelectList:
                    MaterialComponentCssList.GetDivWrap(consume).ToList().ForEach(e => wrap.AddCssClass(e));

                    if (!string.IsNullOrEmpty(inputcss))
                    {
                        wrap.AddCssClass(inputcss);
                    }
                    var downarrow = inputcss.ItSplit(" ").Contains("down-arrow");
                    innerwrap.Attributes["include"] = downarrow ? "form-input-select(down)" :"form-input-select()";
                    if(hasError)
                    {
                        innerwrap.AddCssClass("mdc-select--invalid");
                    }
                    var selectwrap = new TagBuilder("select");
                    
                    selectwrap.Attributes["data-bouncer-target"] = '#' + bouncerwrapper.Attributes["id"];
                    if (isrequired)
                        selectwrap.MergeAttribute("required", string.Empty);
                    if (isdisabled)
                        selectwrap.MergeAttribute("disabled",string.Empty);
                    selectwrap.Attributes["name"] = propname;
                    var suffixprop = "";
                    if (htmlAttributes != null)
                    {
                        var addattribs = new RouteValueDictionary(htmlAttributes);
                        if (addattribs["suffixprop"] != null)
                        {
                            suffixprop = addattribs["suffixprop"]?.ToString();
                        }
                        selectwrap.MergeAttributes(addattribs);
                    }
                    if (optionpair!=null)
                    {
                        var placeholder = new TagBuilder("option");
                        //placeholder.SetInnerText(holder);
                        placeholder.AddCssClass("hidden");
                        placeholder.MergeAttribute("value", "");
                        selectwrap.InnerHtml += placeholder.ToString();
                        foreach(var opt in optionpair)
                        {
                            var valop = new TagBuilder("option");
                            var firstvalue = opt.Value.ItSplit("!").FirstOrDefault();
                            valop.Attributes["value"] = suffixprop!=null ? firstvalue:   opt.Value;
                            if(!string.IsNullOrEmpty(suffixprop))
                            {

                                valop.Attributes[suffixprop] = string.Join("!", opt.Value.ItSplit("!").Skip(1));
                                if(firstvalue == selectedvalue)
                                {
                                    valop.Attributes["selected"] = "selected";
                                }
                            }
                            if(opt.Value == selectedvalue)
                            {
                                valop.Attributes["selected"] = "selected";
                            }
                            valop.SetInnerText(opt.Key);
                            selectwrap.InnerHtml += valop.ToString();

                        }
                    }


                    labelspan.SetInnerText(holder);
                    //labelspan.AddCssClass("hidden");
                    labelwrap.AddCssClass("theme-select-wrapper");
                    labelwrap.Attributes["for"] = propname;
                    selectwrap.AddCssClass("theme-select");
                    labelwrap.InnerHtml = labelspan.ToString() + selectwrap.ToString();
                    innerwrap.InnerHtml = labelwrap.ToString();
                    bouncerwrapper.AddCssClass("near-top");
                    wrap.InnerHtml = innerwrap.ToString() + (hasError ? helperwrapper.ToString() : "") + bouncerwrapper.ToString();
                    return new MvcHtmlString(wrap.ToString());

                case WrapType.RadioGroup:
                    MaterialComponentCssList.GetDivWrap(consume).ToList().ForEach(e => wrap.AddCssClass(e));
                    if(!string.IsNullOrEmpty(inputcss))
                    {
                        wrap.AddCssClass(inputcss);
                    }
                    var question = new TagBuilder("p");
                    question.SetInnerText(holder);
                    wrap.InnerHtml += question.ToString();
                    
                    foreach(var opt in optionpair)
                    {
                        var formwrap = new TagBuilder("div");
                        formwrap.AddCssClass("mdc-form-field");
                        var radiowrap = new TagBuilder("div");
                        radiowrap.AddCssClass("mdc-radio");
                        if (isdisabled)
                            radiowrap.AddCssClass("mdc-radio--disabled");
                        var valop = new TagBuilder("input");
                        valop.Attributes["name"] = propname;
                        valop.Attributes["type"] = "radio";
                        valop.Attributes["value"] = opt.Value;
                        valop.Attributes["data-bouncer-target"] ='#'+ bouncerwrapper.Attributes["id"];
                        valop.AddCssClass("mdc-radio__native-control");
                        if (isdisabled)
                            valop.MergeAttribute("disabled", String.Empty);
                        if(opt.Value == selectedvalue)
                        {
                            valop.Attributes["checked"] = "checked";
                            
                        }
                        var backop = new TagBuilder("div");
                        backop.AddCssClass("mdc-radio__background");
                        if (htmlAttributes != null)
                        {
                            valop.MergeAttributes(new RouteValueDictionary(htmlAttributes));
                        }
                        var outcircle = new TagBuilder("div");
                        outcircle.AddCssClass("mdc-radio__outer-circle");
                        var inncircle = new TagBuilder("div");
                        inncircle.AddCssClass("mdc-radio__inner-circle");
                        var endradio =new TagBuilder("div");
                        endradio.AddCssClass("mdc-radio__ripple");
                        var radiolabel = new TagBuilder("label");
                        radiolabel.SetInnerText(opt.Key);
                        backop.InnerHtml = outcircle.ToString() + inncircle.ToString();
                        radiowrap.InnerHtml = valop.ToString() + backop.ToString() + endradio.ToString();
                        formwrap.InnerHtml = radiowrap.ToString() + radiolabel.ToString();
                        wrap.InnerHtml += formwrap.ToString();
                    }
                    bouncerwrapper.AddCssClass("no-absolute");
                    wrap.InnerHtml += (hasError ? helperwrapper.ToString() : "") + bouncerwrapper.ToString();
                    return new MvcHtmlString(wrap.ToString());
                    
            }

            return new MvcHtmlString(wrap.ToString());
        }
        
    }
    public  static class UtilityUI
    {



        public static MvcHtmlString GetThumb(this HtmlHelper helper, string type,string title,string text,string href,string src=null )
        {
            var linkwrap = new TagBuilder("a");
            linkwrap.AddCssClass("thumb-view");
            
            linkwrap.MergeAttribute("href", href);
            var labelwrap = new TagBuilder("div");
            var titlespan = new TagBuilder("span");
            var textspan = new TagBuilder("span");
            titlespan.SetInnerText(title);
            textspan.SetInnerText(text);
            labelwrap.InnerHtml = titlespan.ToString() + textspan.ToString();

            if (!string.IsNullOrEmpty(src))
            {
                
                
                linkwrap.AddCssClass("thumb-img");
                linkwrap.MergeAttribute("style", $"background-image:url({src})");
                if(type=="img")
                {
                    var img = new TagBuilder("img");
                    img.MergeAttribute("src", src);
                    img.MergeAttribute("alt", title);
                    linkwrap.InnerHtml = img.ToString() + labelwrap.ToString();
                }
                else
                {
                    var vdo = new TagBuilder("video");
                    var vsrc = new TagBuilder("source");
                    vsrc.MergeAttribute("type", type);
                    vsrc.MergeAttribute("src", src);
                    vdo.InnerHtml = vsrc.ToString();
                    linkwrap.InnerHtml = vdo.ToString() + labelwrap.ToString();
                }

            }
            else
            {
                linkwrap.AddCssClass("thumb-txt");
                linkwrap.InnerHtml = labelwrap.ToString();
            }
            return new MvcHtmlString(linkwrap.ToString());
        }

        public static MvcHtmlString AntiForgeryTokenForAjaxPost(this HtmlHelper helper, string idden)
        {
            var antiForgeryInputTag = helper.AntiForgeryToken().ToString();
            var input = new TagBuilder("input");
            input.MergeAttribute("id", idden);
            input.MergeAttribute("type", "hidden");
            input.MergeAttribute("name", "__RequestVerificationToken");

            // Above gets the following: <input name="__RequestVerificationToken" type="hidden" value="PnQE7R0MIBBAzC7SqtVvwrJpGbRvPgzWHo5dSyoSaZoabRjf9pCyzjujYBU_qKDJmwIOiPRDwBV1TNVdXFVgzAvN9_l2yt9-nf4Owif0qIDz7WRAmydVPIm6_pmJAI--wvvFQO7g0VvoFArFtAR2v6Ch1wmXCZ89v0-lNOGZLZc1" />
            var removedStart = antiForgeryInputTag.Replace(@"<input name=""__RequestVerificationToken"" type=""hidden"" value=""", "");

            var tokenValue = removedStart.Replace(@""" />", "");
            if (antiForgeryInputTag == removedStart || removedStart == tokenValue)
                throw new InvalidOperationException("Oops! The Html.AntiForgeryToken() method seems to return something I did not expect.");

            input.MergeAttribute("value", tokenValue);
            return new MvcHtmlString(input.ToString());
        }

        public static MvcHtmlString GetUnderline<TModel,TProperty>(this HtmlHelper<TModel> helper,Expression<Func<TModel,TProperty>> expression,bool isdisabled=false,object htmlAttributes=null,string anyclaasses=null)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            var propname = metadata.PropertyName;
            var holder = ModelMetadata.FromLambdaExpression(expression, helper.ViewData).Watermark;

            var name = ModelMetadata.FromLambdaExpression(expression, helper.ViewData).DisplayName;

            TProperty value = (TProperty)metadata.Model;
            string defaultvalue = value?.ToString();
            var formatattr = metadata.ContainerType.GetProperty(propname)?.GetCustomAttributes(typeof(DisplayFormatAttribute), false)?.FirstOrDefault();

            if(formatattr!=null)
            {
                
                defaultvalue = string.Format((formatattr as DisplayFormatAttribute).DataFormatString, value);
            }
            
            TagBuilder inputwrapper = new TagBuilder("div");
            inputwrapper.AddCssClass("form-field");

            TagBuilder fieldwrapper = new TagBuilder("div");
            fieldwrapper.AddCssClass("form-field__control");
            TagBuilder labelinput = new TagBuilder("label");
            TagBuilder thisinput = new TagBuilder("input");
            labelinput.MergeAttribute("for", propname);
            labelinput.AddCssClass("form-field__label");
            labelinput.SetInnerText(name);
            thisinput.MergeAttribute("id", propname);
            thisinput.MergeAttribute("name", propname);
            thisinput.MergeAttribute("type", "text");
            thisinput.AddCssClass("form-field__input");
            thisinput.MergeAttribute("value", defaultvalue);
            if(!string.IsNullOrEmpty(defaultvalue))
            {
                inputwrapper.AddCssClass("form-field--is-filled");
            }
            fieldwrapper.InnerHtml = labelinput.ToString() + thisinput.ToString();
            inputwrapper.InnerHtml = fieldwrapper.ToString();
            
            if (htmlAttributes != null)
                inputwrapper.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            if (anyclaasses != null)
                anyclaasses.Split(' ').ForEach(e => inputwrapper.AddCssClass(e));
            return new MvcHtmlString(inputwrapper.ToString());



        }
        /*
        <div class="form-field">
          <div class="form-field__control">
            <label for="firstname" class="form-field__label">First name</label>
            <input id="firstname" type="text" class="form-field__input" />
          </div>
        </div>          
         */

        public static IEnumerable<SelectListItem> GetSelectList<TValue>(IEnumerable< KeyValuePair<TValue, string>> pairs, TValue selectedvalue, string dummy="All")
        {
            if(!pairs.Any())
            {
                yield return new SelectListItem { Text = $"{Constants.DT.AllItems.Replace("{0}",dummy)}", Value = null, Selected = true };
            }
            foreach(var p in pairs)
            {
                yield return new SelectListItem { Text = p.Value, Value = p.Key?.ToString(), Selected = ( p.Value.Equals(selectedvalue)) };
            }
        }

        public static MvcHtmlString ListBoxMultiSelectFor<TModel, TProperty>(
                this HtmlHelper<TModel> helper,
                Expression<Func<TModel, IEnumerable<TProperty>>> expression,
                IEnumerable<string> selectList,
                object htmlAttributes)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);

            var collection =metadata.Model==null ? new string[] { }.ToArray(): (metadata.Model as IEnumerable<TProperty>).Select(x=> x.ToString());

            List<SelectListItem> items = new List<SelectListItem>();
            if(selectList!=null)
            {
                foreach (var val in selectList)
                {
                    var ra = new SelectListItem { Text = val, Value = val };
                    if (collection != null && collection.Contains(val))
                    {
                        ra.Selected = true;
                    }
                    items.Add(ra);

                }
            }



            return ListBoxMultiSelectFor(helper, expression, items, new RouteValueDictionary(htmlAttributes));
        }

        public static MvcHtmlString DisplayFilterOfRecordFor<TModel, TValue, TProperty>(this HtmlHelper<TModel> helper, Expression<Func<TModel, IEnumerable<TValue>>> expression, Expression<Func<TValue, TProperty>> childexpression,string wrapper="th", object htmlAttributes = null)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);

            var propertyname = PropertyExtensions.GetMemberName(childexpression);
            TagBuilder wrapbuilder = new TagBuilder("input");
            wrapbuilder.Attributes.Add("disabled", "disabled");
            TagBuilder label = new TagBuilder("label");
            if (htmlAttributes != null)
                wrapbuilder.MergeAttributes(new RouteValueDictionary(htmlAttributes));

            var metadataType = typeof(TValue).GetCustomAttributes(typeof(MetadataTypeAttribute), true)
                .OfType<MetadataTypeAttribute>().FirstOrDefault();

            var innermetadata = ModelMetadataProviders.Current.GetMetadataForType(null, metadataType.MetadataClassType);

            var displayattrb = innermetadata.ModelType.GetProperty(propertyname)?.GetCustomAttributes(typeof(DisplayAttribute), false)?.FirstOrDefault();
            if (displayattrb != null)
            {
                var textvalue = (displayattrb as DisplayAttribute).GetName();
                wrapbuilder.Attributes.Add("placeholder", textvalue);                                
                label.SetInnerText(textvalue);
            }
            else
            {
                wrapbuilder.Attributes.Add("placeholder", propertyname);                
                label.SetInnerText(propertyname);
            }
            var outerwrapper = new TagBuilder(wrapper);
            outerwrapper.AddCssClass("filter");
            outerwrapper.InnerHtml += wrapbuilder.ToString() + label.ToString();
            return new MvcHtmlString(outerwrapper.ToString());
        }

        public static MvcHtmlString DisplayNameOfRecordFor<TModel, TValue, TProperty>(this HtmlHelper<TModel> helper, Expression<Func<TModel, IEnumerable<TValue>>> expression, Expression<Func<TValue, TProperty>> childexpression, object htmlAttributes = null, string[] options =null)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            
            var propertyname = PropertyExtensions.GetMemberName(childexpression);

            TagBuilder wrapbuilder = new TagBuilder("span");
            wrapbuilder.Attributes.Add("id", propertyname);
            var sorter = PropertyExtensions.GetTabulatorSorter<TProperty>();
            wrapbuilder.Attributes.Add("sorter", sorter);
            if(options!=null && options.Length >0)
            {
                wrapbuilder.Attributes.Add("options", string.Join("|", options));
            }
            if (htmlAttributes != null)
                wrapbuilder.MergeAttributes(new RouteValueDictionary(htmlAttributes));

            var metadataType = typeof(TValue).GetCustomAttributes(typeof(MetadataTypeAttribute), true)
                .OfType<MetadataTypeAttribute>().FirstOrDefault();

            var innermetadata = ModelMetadataProviders.Current.GetMetadataForType(null, metadataType.MetadataClassType);
            
            var displayattrb = innermetadata.ModelType.GetProperty(propertyname)?.GetCustomAttributes(typeof(DisplayAttribute), false)?.FirstOrDefault();
            if(displayattrb!=null)
            {
                wrapbuilder.SetInnerText((displayattrb as DisplayAttribute).GetName());
            }
            else
            {
                wrapbuilder.SetInnerText(propertyname);
            }
            return new MvcHtmlString(wrapbuilder.ToString());
        }

        public static MvcHtmlString DisplayValueInCommaFor<TModel,TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression,object htmlAttributes=null)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            var value = (TValue)metadata.Model;
            TagBuilder wrapbuilder = new TagBuilder("span");
            if (htmlAttributes != null)
                wrapbuilder.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            if(value!=null)
            {
                var valuestr = value.ToString().Replace("|", ",");
                wrapbuilder.SetInnerText(valuestr);
            }
            return new MvcHtmlString(wrapbuilder.ToString());

        }

        public static MvcHtmlString DisplayValueToTextFor<TModel,TValue>(this HtmlHelper<TModel> helper,Expression<Func<TModel,TValue>> expression,IEnumerable<KeyValuePair<TValue,string>> displaynames, object htmlAttributes=null)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            var value = (TValue)metadata.Model;

            TagBuilder wrapbuilder = new TagBuilder("span");
            if (htmlAttributes != null)
                wrapbuilder.MergeAttributes(new RouteValueDictionary(htmlAttributes));

            if (displaynames.Any(e=> e.Key.Equals(value)))                
            {
                var foundpair = displaynames.First(e => e.Key.Equals(value));
                wrapbuilder.SetInnerText(foundpair.Value);
            }

            return new MvcHtmlString(wrapbuilder.ToString());

        }


        public static MvcHtmlString DisplayPlaceHolderFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {

            var result = ModelMetadata.FromLambdaExpression(expression, html.ViewData).Watermark;

            if (string.IsNullOrEmpty(result))
                result = ModelMetadata.FromLambdaExpression(expression, html.ViewData).DisplayName;

            return new MvcHtmlString(result);
        }

        public static MvcHtmlString ListBoxMultiSelectFor<TModel, TProperty>(
            this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TProperty>> expression,
            IEnumerable<SelectListItem> selectList,
            IDictionary<string, object> htmlAttributes)
        {
            string name = ExpressionHelper.GetExpressionText(expression);

            TagBuilder selectTag = new TagBuilder("select");
            selectTag.MergeAttributes(htmlAttributes);
            selectTag.MergeAttribute("id", name, true);
            selectTag.MergeAttribute("name", name, true);
            foreach (SelectListItem item in selectList)
            {
                TagBuilder optionTag = new TagBuilder("option");
                optionTag.MergeAttribute("value", item.Value);
                if (item.Selected) optionTag.MergeAttribute("selected", "selected");
                optionTag.InnerHtml = item.Text;
                selectTag.InnerHtml += optionTag.ToString();
            }

            return new MvcHtmlString(selectTag.ToString());
        }

        public static IHtmlString RenderDBName(this HtmlHelper htmlHelper)
        {

            var cnstr = ConfigurationManager.ConnectionStrings["HYDwepEntities"].ConnectionString;
            var builder = new DbConnectionStringBuilder();
            builder.ConnectionString = cnstr;
            var catolgDB = new SqlConnectionStringBuilder(builder["provider connection string"].ToString());

            
            return new MvcHtmlString(catolgDB.InitialCatalog);
        }

        public static MvcHtmlString DisplayWithIdFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, string wrapperTag = "div")
        {
            var id = helper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(ExpressionHelper.GetExpressionText(expression));
            return MvcHtmlString.Create(string.Format("<{0} id=\"{1}\">{2}</{0}>", wrapperTag, id, helper.DisplayFor(expression)));
        }

        public static MvcHtmlString ActionLinkMenu(this HtmlHelper htmlHelper, Func<object, HelperResult> template, string actionName, string controllerName, object routeValues = null, object htmlAttributes = null)
        {
            return ActionLinkMenu(htmlHelper, template(null).ToString(), actionName, controllerName, routeValues, htmlAttributes);
        }

        public static MvcHtmlString ActionLinkMenu(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName, object routeValues = null, object htmlAttributes = null)
        {
            var currentAction = htmlHelper.ViewContext.RouteData.GetRequiredString("action");
            var currentController = htmlHelper.ViewContext.RouteData.GetRequiredString("controller");

            var builder = new TagBuilder("li")
            {
                InnerHtml = htmlHelper.ActionLinkRaw(linkText, actionName, controllerName, routeValues, htmlAttributes).ToHtmlString()
            };

            if (controllerName == currentController && actionName == currentAction)
                builder.AddCssClass("active");

            return new MvcHtmlString(builder.ToString());
        }

        private static MvcHtmlString ActionLinkRaw(this HtmlHelper htmlHelper, string rawHtml, string actionName, string controllerName, object routeValues, object htmlAttributes = null)
        {
            var repID = Guid.NewGuid().ToString();
            var lnk = htmlHelper.ActionLink(repID, actionName, controllerName, routeValues, htmlAttributes);
            return MvcHtmlString.Create(lnk.ToString().Replace(repID, rawHtml));
        }

        public static string RenderViewToHtmlString(ControllerBase controller, string viewName, object model)
        {
            controller.ViewData.Model = model;
            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                ViewContext viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(controller.ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}