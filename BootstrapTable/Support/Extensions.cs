using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;
using System.Globalization;

namespace BootstrapTable.Support
{
    /// <exclude/>
    internal static class Extensions
    {
        /// <exclude/>
        public static string ToStringLower(this object value)
        {
            return value != null ? value.ToString().ToLower() : null;
        }

        /// <exclude/>
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var t in enumerable)
            {
                action(t);
            }
        }

        /// <exclude/>
        public static string SplitCamelCase(this string str)
        {
            return Regex.Replace(Regex.Replace(str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
        }

        /// <exclude/>
        public static IDictionary<string, object> HtmlAttributesToDictionary(this object htmlAttributes)
        {
            return htmlAttributes as IDictionary<string, object> ?? HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
        }

        /// <exclude/>
        public static Boolean IsAnonymousType(this Type type)
        {
            Boolean hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Count() > 0;
            Boolean nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            return hasCompilerGeneratedAttribute && nameContainsAnonymousType;
        }

        /// <exclude/>
        public static IHtmlString ToSerializedString(this object data)
        {
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return new HtmlString((serializer.Serialize(data)));
        }

        /// <exclude/>
        public static string GetName<TModel, TProperty>(this Expression<Func<TModel, TProperty>> field)
        {
            var member = field.Body as MemberExpression;
            return member.Member.Name;
        }

        /// <exclude/>
        public static string GetDisplayName<TModel, TProperty>(this Expression<Func<TModel, TProperty>> field)
        {
            var member = field.Body as MemberExpression;
            
            DisplayAttribute display = member.Member.GetCustomAttribute(typeof(DisplayAttribute)) as DisplayAttribute;
            if (display != null && !string.IsNullOrEmpty(display.GetName()))
                return display.GetName();
            else
                return member.Member.Name.SplitCamelCase();
        }

        /// <exclude/>
        public static Dictionary<PropertyInfo,DisplayAttribute> GetSortedProperties(this Type type)
        {
            int order = int.MaxValue - type.GetProperties().Count();
            var metadataType = type.GetCustomAttributes(typeof(MetadataTypeAttribute), true)
                .OfType<MetadataTypeAttribute>().FirstOrDefault();
            var metaData = (metadataType != null)
                ? ModelMetadataProviders.Current.GetMetadataForType(null, metadataType.MetadataClassType)
                : ModelMetadataProviders.Current.GetMetadataForType(null, type);

            return type.GetProperties()
                .Select(p =>
                {

                    var metadisplay = metaData.ModelType.GetProperty(p.Name)?.GetCustomAttributes(typeof(DisplayAttribute), false)?.FirstOrDefault() as DisplayAttribute;
                    var display = p.GetCustomAttributes(typeof(DisplayAttribute), true).FirstOrDefault() as DisplayAttribute;
                    display = metadisplay ?? display;

                    return new
                    {
                        display = display,
                        property = p,
                        order = display != null ?
                            (display.GetOrder() ?? ++order) : ++order
                    };
                })
                .OrderBy(o => o.order)
                .ToDictionary(x => x.property, x => x.display);
        }
    }
}
