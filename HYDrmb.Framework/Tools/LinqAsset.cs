using HYDrmb.Abstraction;
using HYDrmb.Framework.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HYDrmb.Framework.Tools
{
    public static class LinqAsset
    {
        public const string OrderByAsc = "OrderBy";
        public const string OrderByDesc = "OrderByDescending";

        public const string ThenByAsc = "ThenBy";
        public const string ThenByDesc = "ThenByDescending";

        public const string Eq = "Eq";
        public const string NotEq = "NotEq";
        public const string GreaterOrEq = "GreaterOrEq";
        public const string LessOrEq = "LessOrEq";
        public const string Less = "Less";
        public const string LikeOp = "LikeOp";
        public const string CheckListOp = "CheckListOp";
        public const string StartOp = "StartOp";
        public const string EndOp = "EndOp";


        public static readonly MethodInfo StringContainsMethod = MethodOf(() => "".Contains(default(string)));
        public static readonly MethodInfo StringStartsWithMethod = MethodOf(() => "".StartsWith(default(string))); //typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
        public static readonly MethodInfo StringEndsWithMethod = MethodOf(() => "".EndsWith(default(string))); //typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
        public static readonly MethodInfo AnyMethod = MethodOf(() => Enumerable.Any(default(IEnumerable<object>), default(Func<object, bool>))).GetGenericMethodDefinition(); //typeof(Enumerable).GetMethods().Single(m => m.Name == "Any" && m.GetParameters().Length == 2);
        //private static readonly MethodInfo LikePatternMethod = typeof(SqlFunctions).GetMethod("Like", BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase, null, new Type[] { typeof(string), typeof(string) }, null);

        public static readonly MethodInfo LikePatternMethod = typeof(SqlFunctions).GetMethod("PATINDEX", BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase, null, new Type[] { typeof(string), typeof(string) }, null);


        public static MethodInfo orderByMethodForIQ = typeof(Queryable).GetMethods().Single(m => m.Name == "OrderBy" && m.GetParameters().Length == 2);

        public static MethodInfo orderByDescendingMethodForIQ = typeof(Queryable).GetMethods().Single(m => m.Name == "OrderByDescending" && m.GetParameters().Length == 2);


        public static MethodInfo thenByMethodForIQ = typeof(Queryable).GetMethods().Single(m => m.Name == "ThenBy" && m.GetParameters().Length == 2);


        public static MethodInfo thenByDescendingMethodForIQ = typeof(Queryable).GetMethods().Single(m => m.Name == "ThenByDescending" && m.GetParameters().Length == 2);




        public static MethodInfo orderByMethod =
            MethodOf(() => Enumerable.OrderBy(default(IEnumerable<object>), default(Func<object, object>)))
                .GetGenericMethodDefinition();

        public static MethodInfo orderByDescendingMethod =
            MethodOf(() => Enumerable.OrderByDescending(default(IEnumerable<object>), default(Func<object, object>)))
                .GetGenericMethodDefinition();

        public static MethodInfo thenByMethod =
            MethodOf(() => Enumerable.ThenBy(default(IOrderedEnumerable<object>), default(Func<object, object>)))
                .GetGenericMethodDefinition();

        public static MethodInfo thenByDescendingMethod =
            MethodOf(() => Enumerable.ThenByDescending(default(IOrderedEnumerable<object>), default(Func<object, object>)))
                .GetGenericMethodDefinition();

        public static MethodInfo MethodOf<T>(Expression<Func<T>> method)
        {
            MethodCallExpression mce = (MethodCallExpression)method.Body;
            MethodInfo mi = mce.Method;
            return mi;
        }


        public static string ToTraceString<T>(this IQueryable<T> t)
        {
            string sql = "";
            ObjectQuery<T> oqt = t as ObjectQuery<T>;
            if (oqt != null)
                sql = oqt.ToTraceString();
            return sql;
        }


        /// <summary>
        /// load the navigation properties
        /// </summary>
        /// <param name="ctx">db context</param>
        /// <param name="result">starting entity </param>
        /// <param name="navpaths">navigation paths of entity</param>
        public static void LoadRelatedFor(DbContext ctx, object result, params string[] navpaths)
        {
            navpaths = navpaths.Select((x, i) => $"{i}!{x}").ToArray();
            var instanceBag = new Queue<object>();
            var navStack = new Stack<string>(navpaths.Reverse());
            instanceBag.Enqueue(result);
            while (navStack.Count > 0)
            {
                var p = navStack.Pop();
                var nextnav = string.Empty;
                if (p.Contains("."))
                {
                    var navs = p.Split(new[] { '.' },StringSplitOptions.RemoveEmptyEntries );
                    nextnav = navs[1];
                    p = navs[0];
                }
                while (instanceBag.Count > 0)
                {
                    var instance = instanceBag.Dequeue();

                    var typeOfinst = instance.GetType();
                    var nameOfproperty = p.Substring(p.IndexOf("!") + 1);
                    if (!typeOfinst.GetProperties().Any(y => y.Name.Equals(nameOfproperty)))
                        break;

                    var propOfp = typeOfinst.GetProperty(nameOfproperty);
                    var isCollectionOfChild = typeof(IEnumerable).IsAssignableFrom(propOfp.PropertyType);

                    if (isCollectionOfChild)
                        ctx.Entry(instance).Collection(nameOfproperty).Load();
                    else
                        ctx.Entry(instance).Reference(nameOfproperty).Load();


                    var instancegetter = PropertyExtensions.FindGetter(instance.GetType(), nameOfproperty);

                    if (instancegetter != null)
                    {
                        instance = instancegetter(instance);
                        if (instance != null)
                        {
                            if (isCollectionOfChild)
                            {
                                var c = (instance as IEnumerable).Cast<object>().Count();
                                if (c == 0)
                                    break;
                                foreach (var insItem in instance as IEnumerable)
                                {
                                    instanceBag.Enqueue(insItem);

                                }
                                var listofnav = navpaths.ToList();
                                var nextindex = listofnav.IndexOf(nextnav);
                                var restofnav = listofnav.Skip(int.Parse(p.Substring(0, p.IndexOf("!"))) + 1).ToArray();
                                navStack = ReCreateStack(navStack, c, string.IsNullOrEmpty(nextnav) ? nextnav : $"{p}.{nextnav}", (listofnav.Count > nextindex + 1 && nextindex != -1) ? listofnav[nextindex + 1] : null, restofnav, p);


                                break;
                            }
                            else
                            {
                                instanceBag.Enqueue(instance);
                                var listofnav = navpaths.ToList();
                                var nextindex = listofnav.IndexOf(nextnav);
                                var restofnav = listofnav.Skip(int.Parse(p.Substring(0, p.IndexOf("!"))) + 1).ToArray();
                                navStack = ReCreateStack(navStack, 1, string.IsNullOrEmpty(nextnav) ? nextnav : $"{p}.{nextnav}", (listofnav.Count > nextindex + 1 && nextindex != -1) ? listofnav[nextindex + 1] : null, restofnav, p);

                                break;
                            }
                        }


                    }

                }


            }

        }

        private static Stack<string> ReCreateStack(Stack<string> orgstack, int count, string nextnav, string tailnav, string[] restofnav, string currentnav)
        {
            if (!string.IsNullOrEmpty(nextnav))
            {
                var childnav = nextnav.Split('.').LastOrDefault();
                if (!string.IsNullOrEmpty(childnav))
                {
                    var headarr = orgstack.ToArray().Where(y => y.Contains(".")).ToArray();
                    var tailarr = orgstack.ToArray().Where(y => (!y.Contains(".") && (string.IsNullOrEmpty(tailnav) || y != tailnav))).ToArray();
                    if (!string.IsNullOrEmpty(tailnav))
                    {
                        childnav = string.Join(".", new[] { childnav, tailnav });
                    }

                    orgstack.Clear();
                    var newstack = headarr.Concat(Enumerable.Repeat(childnav, count)).Concat(tailarr).Reverse();
                    return new Stack<string>(newstack);
                }


            }


            if (restofnav.Length == 1)
            {
                orgstack = new Stack<string>(restofnav.Reverse());
                while (--count > 0)
                {
                    orgstack.Push(orgstack.Peek());
                }

            }
            else if (restofnav.Length > 1)
            {
                orgstack.Clear();
                var restnavs = restofnav.Skip(2).Reverse();
                foreach (var n in restnavs)
                {
                    orgstack.Push(n);
                }
                var newnav = string.Join(".", restofnav.Take(2));
                while (count-- > 0)
                {
                    orgstack.Push(newnav);
                }
            }
            return orgstack;
        }
    }

    public static class LinqHelper
    {



        public static Expression CreateBaseExpr(Type paramtype, string symbol = "e")
        {

            return Expression.Parameter(paramtype, symbol);
        }

        public static Expression CreateRootParameter<T>(string symbol = "e") where T : class
        {
            return Expression.Parameter(typeof(T), symbol);
        }

        public static IQueryable Search(this IQueryable query, Expression exprBase, NameValueCollection nvSet, string tablename)
        {
            Expression lamdaWhereClause = null;
            if (!string.IsNullOrEmpty(tablename))
                lamdaWhereClause = ExpressionExtensions.GetNavigationPropertyExpression(exprBase, nvSet, tablename.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries));
            // this case never happen?!
            else
                lamdaWhereClause = ExpressionExtensions.GetNavigationPropertyExpression(exprBase, nvSet);

            MethodCallExpression whereMthdCall = Expression.Call(typeof(Queryable), "Where", new Type[] { exprBase.Type }, query.Expression, lamdaWhereClause);
            query = query.Provider.CreateQuery(whereMthdCall);
            return query;
        }



        public static IQueryable QuerySearch(IQueryable query, Expression exprBase, string strJSON, Dictionary<String, String> ignoreList = null)
        {
            var inputSortedList = JSONtoDynamicHelper.CreateJSONObjList<JsonSearchClass>(strJSON);
            //filter out the ignoreList Value
            List<JsonSearchClass> sortedList = new List<JsonSearchClass>();
            if (ignoreList != null)
            {
                foreach (var searchValue in inputSortedList)
                {
                    Boolean isVal = true;
                    foreach (string value in ignoreList.Select(x => x.Value))
                    {
                        if (value == searchValue.key)
                        {
                            isVal = false;

                        }
                    }

                    if (isVal)
                    {
                        sortedList.Add(searchValue);
                    }
                }
            }
            else
            {
                sortedList = inputSortedList;
            }


            foreach (var record in JSONtoDynamicHelper.GroupListwithTableNames(sortedList))
            {
                var nvSet = JSONtoDynamicHelper.PrepareTablesNVSet(record);
                // nvSet.Key is the table name
                query = query.Search(exprBase, nvSet.Value, nvSet.Key);
            }
            return query;
        }



        public static IQueryable<TSource> WhereIf<TSource>(this IQueryable<TSource> source, bool condition, Expression<Func<TSource, bool>> expression)
        {
            if (condition)
                return source.Where(expression);
            else
                return source;
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string prop, string order)
        {
            var type = typeof(T);
            var property = type.GetProperty(prop, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExp = Expression.Lambda(propertyAccess, parameter);
            MethodCallExpression resultExp = Expression.Call(typeof(Queryable),
                order.Equals("asc", StringComparison.InvariantCultureIgnoreCase) ? "OrderBy" : "OrderByDescending",
                new Type[] { type, property.PropertyType }, source.Expression, Expression.Quote(orderByExp));
            return source.Provider.CreateQuery<T>(resultExp);
        }


        public static IEnumerable<T> BuildOrderBys<T>(this IEnumerable<T> source, string jsonvalues)
        {
            if (string.IsNullOrEmpty(jsonvalues))
                return source;
            var sorter = JSONtoDynamicHelper.CreateJSONObjList<JSort>(jsonvalues);
            var sortdescs = sorter.Select(y => new SortDescription { Direction = y.Order == "desc" ? ListSortDirection.Descending : ListSortDirection.Ascending, PropertyName = y.Column }).ToArray();

            return BuildOrderBys(source, sortdescs).AsQueryable();

        }


        /* 2017-07-05      - Andrew Ng
        *  Task            - Search Revamp
        *                  - to concate generic orderby and thenby method
        */
        public static IEnumerable<T> BuildOrderBys<T>(this IEnumerable<T> source, params SortDescription[] properties)
        {
            if (properties == null || properties.Length == 0) return source;

            var typeOfT = typeof(T);

            Type t = typeOfT;

            IOrderedEnumerable<T> result = null;
            var thenBy = false;

            foreach (var item in properties
                .Select(prop => new { PropertyInfo = t.GetProperty(prop.PropertyName), prop.Direction }))
            {
                var oExpr = Expression.Parameter(typeOfT, "o");
                var propertyInfo = item.PropertyInfo;
                var propertyType = propertyInfo.PropertyType;
                var isAscending = item.Direction == ListSortDirection.Ascending;

                if (thenBy)
                {
                    var prevExpr = Expression.Parameter(typeof(IOrderedEnumerable<T>), "prevExpr");
                    var expr1 = Expression.Lambda<Func<IOrderedEnumerable<T>, IOrderedEnumerable<T>>>(
                        Expression.Call(
                            (isAscending ? LinqAsset.thenByMethod : LinqAsset.thenByDescendingMethod).MakeGenericMethod(typeOfT, propertyType),
                            prevExpr,
                            Expression.Lambda(
                                typeof(Func<,>).MakeGenericType(typeOfT, propertyType),
                                Expression.MakeMemberAccess(oExpr, propertyInfo),
                                oExpr)
                            ),
                        prevExpr)
                        .Compile();

                    result = expr1(result);
                }
                else
                {
                    var prevExpr = Expression.Parameter(typeof(IEnumerable<T>), "prevExpr");
                    var expr1 = Expression.Lambda<Func<IEnumerable<T>, IOrderedEnumerable<T>>>(
                        Expression.Call(
                            (isAscending ? LinqAsset.orderByMethod : LinqAsset.orderByDescendingMethod).MakeGenericMethod(typeOfT, propertyType),
                            prevExpr,
                            Expression.Lambda(
                                typeof(Func<,>).MakeGenericType(typeOfT, propertyType),
                                Expression.MakeMemberAccess(oExpr, propertyInfo),
                                oExpr)
                            ),
                        prevExpr)
                        .Compile();

                    result = expr1(source);
                    thenBy = true;
                }
            }
            return result;
        }



        /// <summary>
        /// return paging query
        /// </summary>
        /// <typeparam name="T">type of queryable</typeparam>
        /// <param name="query">queryable</param>
        /// <param name="pageNo">page no</param>
        /// <param name="pageSize">page size</param>
        /// <param name="isAll">no paging if true</param>
        /// <returns></returns>
        public static IQueryable<T> GetPagingOrAll<T>(this IQueryable<T> query, int pageNo, int pageSize, bool isAll)
        {
            if (isAll)
                return query;
            return query.Skip((pageNo - 1) * pageSize).Take(pageSize);
        }

        public static IQueryable<T> BuildOrder<T>(this IQueryable<T> source, params SortDescription[] properties)
        {
            if (properties == null || properties.Length == 0) return source;

            var thenBy = false;
            var queryExpr = source.Expression;
            var worktype = typeof(T);
            foreach (var item in properties
                .Select(prop => new { PropertyInfo = worktype.GetProperty(prop.PropertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance), prop.Direction }))
            {
                var paremeterExpr = Expression.Parameter(worktype, "o");
                var propertyInfo = item.PropertyInfo;
                //property = "SomeProperty"
                var propertyExpr = Expression.Property(paremeterExpr, item.PropertyInfo);
                var selectorExpr = Expression.Lambda(propertyExpr, paremeterExpr);
                var propertyType = propertyInfo.PropertyType;
                var isAscending = item.Direction == ListSortDirection.Ascending;
                var currentOrderIs = !thenBy ? LinqAsset.OrderByAsc : LinqAsset.ThenByAsc;


                queryExpr = Expression.Call(
                        //type to call method on
                        typeof(Queryable),
                        //method to call
                        isAscending ? currentOrderIs : $"{currentOrderIs}Descending",
                        //generic types of the order by method
                        new Type[] {
                source.ElementType,
                propertyType },
                        //existing expression to call method on
                        queryExpr,
                        //method parameter, in our case which property to order on
                        selectorExpr);

                thenBy = true;

            }
            return source.Provider.CreateQuery<T>(queryExpr);
        }

        /// <summary>
        /// return sort of queryable
        /// </summary>
        /// <param name="source">queryable</param>
        /// <param name="worktype">type of queryable</param>
        /// <param name="properties">sorting arguments</param>
        /// <returns></returns>
        public static IQueryable BuildSort(this IQueryable source, Type worktype, params SortDescription[] properties)
        {

            if (properties == null || properties.Length == 0) return source;

            var thenBy = false;
            var queryExpr = source.Expression;

            foreach (var item in properties
                .Select(prop => new { PropertyInfo = worktype.GetProperty(prop.PropertyName), prop.Direction }))
            {
                var paremeterExpr = Expression.Parameter(worktype, "o");
                var propertyInfo = item.PropertyInfo;
                //property = "SomeProperty"
                var propertyExpr = Expression.Property(paremeterExpr, item.PropertyInfo);
                var selectorExpr = Expression.Lambda(propertyExpr, paremeterExpr);
                var propertyType = propertyInfo.PropertyType;
                var isAscending = item.Direction == ListSortDirection.Ascending;
                var currentOrderIs = !thenBy ? LinqAsset.OrderByAsc : LinqAsset.ThenByAsc;


                queryExpr = Expression.Call(
                        //type to call method on
                        typeof(Queryable),
                        //method to call
                        isAscending ? currentOrderIs : $"{currentOrderIs}Descending",
                        //generic types of the order by method
                        new Type[] {
                source.ElementType,
                propertyType },
                        //existing expression to call method on
                        queryExpr,
                        //method parameter, in our case which property to order on
                        selectorExpr);

                thenBy = true;

            }
            return source.Provider.CreateQuery(queryExpr);
        }
    }
}
