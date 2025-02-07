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
using HYDrmb.Abstraction;
using HYDrmb.jobweb.Models;
using log4net.Util.TypeConverters;
using Newtonsoft.Json.Linq;
using OpenXmlPowerTools;
using WebGrease.Css.Ast.Selectors;
using WebGrease.Css.Extensions;
using static HYDrmb.jobweb.Tools.MaterialHelper;

namespace HYDrmb.jobweb.Tools
{

    public static class BundleExtensions
    {
        public static void IncludeInOrder(this ScriptBundle bundle,BundleCollection collection,params string[] scripts)
        {
            var mybundle = bundle.Include(scripts);
            mybundle.Orderer = new AsIsBundleOrderer();
            collection.Add(mybundle);

        }
    }

    public static class MaterialHelper
    {
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
                yield return new SelectListItem { Text = $"{Constants.UI.AllItems.Replace("{0}",dummy)}", Value = null, Selected = true };
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