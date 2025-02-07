using HYDrmb.Abstraction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HYDrmb.Framework.Tools
{
    public class ParameterVisitor : ExpressionVisitor
    {
        public Expression Parameter
        {
            get; private set;
        }
        protected override Expression VisitParameter(ParameterExpression node)
        {
            Parameter = node;
            return node;
        }
    }
    /// <summary>
    /// </summary>
    /// <see cref="http://stackoverflow.com/questions/17960103/reduce-an-expression-by-inputting-a-parameter"/>
    public class ResolveParameterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _param;
        private readonly object _value;

        public ResolveParameterVisitor(ParameterExpression param, object value)
        {
            _param = param;
            _value = value;
        }

        public Expression ResolveLocalValues(Expression exp)
        {
            return Visit(exp);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node.Type == _param.Type && node.Name == _param.Name
                && node.Type.IsSimpleType())
            {
                return Expression.Constant(_value);
            }

            return base.VisitParameter(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var parameters = node.Parameters.Where(p => p.Name != _param.Name && p.Type != _param.Type).ToList();
            return Expression.Lambda(Visit(node.Body), parameters);
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            if (m.Expression != null
                && m.Expression.NodeType == ExpressionType.Parameter
                && m.Expression.Type == _param.Type && ((ParameterExpression)m.Expression).Name == _param.Name)
            {
                object newVal;
                if (m.Member is FieldInfo)
                    newVal = ((FieldInfo)m.Member).GetValue(_value);
                else if (m.Member is PropertyInfo)
                    newVal = ((PropertyInfo)m.Member).GetValue(_value, null);
                else
                    newVal = null;
                return Expression.Constant(newVal);
            }

            return base.VisitMember(m);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            return base.VisitUnary(node);
        }
    }

    public static class ExpressionExtensions
    {





        private static string GetPropertyNavigation(this Expression body, bool throwOnMethod = true)
        {
            var memberInfos = body.GetMembersInfo();
            // var sb = new StringBuilder();
            var str = string.Empty;

            foreach (var memberInfo in memberInfos)
            {
                if (memberInfo == null) continue;

                str += (ParseMemberPath(memberInfo));
            }

            return str;
            /*
            var memberInfo = body.GetMemberInfo(throwOnMethod: throwOnMethod);
            if (memberInfo == null)
                return string.Empty;

            return ParseMemberPath(memberInfo);*/
        }
        private static MemberExpression GetLamdaMember(Expression body, bool throwOnMethod = true)
        {
            if (body.NodeType == ExpressionType.Convert)
            {
                return ((UnaryExpression)body).Operand as MemberExpression;
            }
            else if (body.NodeType == ExpressionType.MemberAccess)
            {
                return body as MemberExpression;
            }
            else if (body.NodeType == ExpressionType.Call)
            {
                var methodCall = body as System.Linq.Expressions.MethodCallExpression;
                StringBuilder sb = new StringBuilder();
                foreach (var param in methodCall.Arguments)
                {
                    var path = ParseMemberPath(GetMemberInfo(param));
                    sb.AppendLine(path);
                }
            }

            // unhandled.
            if (throwOnMethod)
                throw new ArgumentException("method");
            return null;
        }
        public static string ParseMemberPath(List<MemberExpression> expressions)
        {
            string ret = "";
            for (var i = 0; i < expressions.Count; i++)
            {
                if (i != 0)
                {
                    ret += ".";
                }

                ret += expressions[i].Member.Name;
            }

            return ret;
        }

        public static List<MemberExpression> GetMemberInfo(this Expression method, bool throwOnMethod = true)
        {
            // cast the lamba expression.
            LambdaExpression lambda = method as LambdaExpression;
            if (lambda == null)
                throw new ArgumentNullException("method");

            // return value.
            var ret = new List<MemberExpression>();

            // top.

            var member = GetLamdaMember(lambda.Body, throwOnMethod: throwOnMethod);
            if (member == null)
                return null;


            ret.Insert(0, member);

            // each parent.
            while (ret[0].Expression.NodeType != ExpressionType.Parameter)
            {
                member = GetLamdaMember(ret[0].Expression, throwOnMethod: throwOnMethod);

                if (member == null)
                    continue;
                ret.Insert(0, member);
            }



            return ret;
        }

        private static List<List<MemberExpression>> GetMembersInfo(this Expression method, bool throwOnMethod = true)
        {
            LambdaExpression lambda = method as LambdaExpression;
            if (lambda == null)
                throw new ArgumentNullException("method");

            List<List<MemberExpression>> res = new List<List<MemberExpression>>();
            if (lambda.Body.NodeType == ExpressionType.Call)
            {
                var methodCall = lambda.Body as System.Linq.Expressions.MethodCallExpression;
                foreach (var param in methodCall.Arguments)
                {
                    res.Add(GetMemberInfo(param, throwOnMethod));
                }
            }
            else
                res.Add(GetMemberInfo(lambda));

            return res;
        }

        /// <summary>
        /// Convert an expression with two parameters to one parameter, 
        /// by filling in (resolving) the first parameter with an instance provided.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <see cref="http://stackoverflow.com/questions/17960103/reduce-an-expression-by-inputting-a-parameter"/>
        public static Expression<Func<TData, bool>> ConvertExpression<TData>(
            this Expression<Func<TData, TData, bool>> expression,
            TData model)
        {
            ParameterExpression modelParameterEx = expression.Parameters.FirstOrDefault();
            if (modelParameterEx == null)
                throw new InvalidOperationException("Unable to find paremeter of the expression.");
            var newExpression = Expression.Lambda<Func<TData, bool>>(
                new ResolveParameterVisitor(modelParameterEx, model)
                .ResolveLocalValues(expression.Body), expression.Parameters.Where(p => p != modelParameterEx));
            return newExpression;
        }





        private static Expression GenerateLambda(Expression exprBase, Expression predicate)
        {
            var resultParameterVisitor = new ParameterVisitor();
            resultParameterVisitor.Visit(exprBase);
            var resultParameter = resultParameterVisitor.Parameter;
            return Expression.Lambda(predicate, (ParameterExpression)resultParameter);
        }

        public static Expression GenerateCompareLambda(string searchString, Type dbType, MemberExpression dbFieldMember, String methodCall = LinqAsset.LikeOp)
        {
            // Check if a search criterion was provided
            // Then "and" it to the predicate.
            // e.g. predicate = predicate.And(x => x.firstName.Contains(searchCriterion.FirstName)); ...
            // Create an "x" as TDbType
            var dbTypeParameter = Expression.Parameter(dbType, @"x");
            // Get at x.firstName
            //var dbFieldMember = Expression.MakeMemberAccess(dbTypeParameter, dbFieldMemberInfo);
            // Create the criterion as a constant
            searchString = string.Format("{0}", searchString);

            

            var criterionConstant = new Expression[] { Expression.Constant(searchString) };
            // Create the MethodCallExpression like x.firstName.Contains(criterion)
            MethodInfo methodHandler = LinqAsset.LikePatternMethod;
            #region
            switch (methodCall)
            {
                case LinqAsset.StartOp:
                    methodHandler = LinqAsset.StringStartsWithMethod;
                    break;
                case LinqAsset.EndOp:
                    methodHandler = LinqAsset.StringEndsWithMethod;
                    break;
                default:
                    methodHandler = LinqAsset.LikePatternMethod;
                    break;
            }
            #endregion
            //methodHandler = typeof(System.Data.Linq.SqlClient.SqlMethods).GetMethods().Where(x => x.Name == "Like").First();
            var containsCall = Expression.Call(methodHandler, criterionConstant[0], dbFieldMember);
            // Create a lambda like x => x.firstName.Contains(criterion)
            //var lambda = Expression.Lambda(containsCall, dbTypeParameter) ;
            // Apply!            

            return containsCall;
        }


        public static Expression GetNavigationPropertyExpression(Expression exprBase, NameValueCollection nvSetValues, params string[] properties)
        {
            Expression resultExpression = null;
            Expression childParameter, navigationPropertyPredicate;
            Type childType = null;

            if (properties.Count() > 0)
            {
                // arrange the base path
                exprBase = Expression.Property(exprBase, properties[0]);
                var isCollection = typeof(IEnumerable).IsAssignableFrom(exprBase.Type);
                // check if it´s a collection for further use as predicate during MethodCallExpression
                if (isCollection)
                {
                    childType = exprBase.Type.GetGenericArguments()[0];
                    childParameter = Expression.Parameter(childType, childType.Name);
                }
                else
                    childParameter = exprBase;

                // skip current property and get navigation property expression recursively
                if (properties.Count() > 1)
                {
                    var innerProperties = properties.Skip(1).ToArray();
                    navigationPropertyPredicate = GetNavigationPropertyExpression(childParameter, nvSetValues, innerProperties);
                    if (isCollection)
                    {
                        //build methodexpressioncall                    
                        var anyMethod = LinqAsset.AnyMethod.MakeGenericMethod(childType);
                        navigationPropertyPredicate = Expression.Call(anyMethod, exprBase, navigationPropertyPredicate);
                        resultExpression = GenerateLambda(exprBase, navigationPropertyPredicate);
                    }
                    else
                        resultExpression = navigationPropertyPredicate;
                }
                else
                {
                    resultExpression = GetNavigationPropertyExpression(childParameter, nvSetValues);
                    if (isCollection)
                    {
                        var anyMethod = LinqAsset.AnyMethod.MakeGenericMethod(childType);
                        navigationPropertyPredicate = Expression.Call(anyMethod, exprBase, resultExpression);
                        resultExpression = GenerateLambda(exprBase, navigationPropertyPredicate);
                    }
                }
            }
            else
            {
                var equalExpr = Expression.Equal(Expression.Constant(1), Expression.Constant(1));
                foreach (var exp in GenerateExpr(exprBase, nvSetValues))
                {
                    if (exp.ToString().ToUpper().Contains("PATINDEX"))
                    {
                        var value = Expression.Constant(0, typeof(int?));
                        var greaterThan = Expression.GreaterThan(exp, value);
                        equalExpr = Expression.AndAlso(equalExpr, greaterThan);
                    }
                    else
                        equalExpr = Expression.AndAlso(equalExpr, exp);
                }
                resultExpression = GenerateLambda(exprBase, equalExpr);
            }
            return resultExpression;
        }

        public static IEnumerable<Expression> GenerateExpr(Expression exprBase, NameValueCollection nvSetValues, string prefix = "__")
        {
            Type entityType = exprBase.Type;
            var props = entityType.GetProperties();
            foreach (var key in nvSetValues.AllKeys)
            {
                string functionKey = string.Empty;
                string convertedKey = key;
                if (key.StartsWith("__"))
                {

                    convertedKey = key.Substring(key.LastIndexOf(prefix) + prefix.Length);
                    functionKey = key.Replace(convertedKey, string.Empty).Replace(prefix, string.Empty);

                }

                var columnProperty = props.FirstOrDefault(x => x.Name.Equals(convertedKey, StringComparison.InvariantCultureIgnoreCase));

                //skip if no corresponding property
                if (columnProperty == null)
                    continue;
                //throw new Exception("Cannot find the property:" + convertedKey);

                object paramValue = Convert.ChangeType(nvSetValues[key], TypeExtensions.BaseType(columnProperty.PropertyType));
                MemberExpression columnExpr = Expression.Property(exprBase, columnProperty);
                //barbara
                if (paramValue.ToString().Contains("%") || paramValue.ToString().Contains("_"))
                    functionKey = LinqAsset.LikeOp;
                //20170807 Barbara Add Less for DateTime To Search
                if (key.EndsWith("DT_TO"))
                {
                    paramValue = ((DateTime)paramValue).Date.AddDays(1);
                    functionKey = LinqAsset.Less;
                }
                switch (functionKey)
                {
                    case LinqAsset.NotEq: //Add By Jonathan LO
                        yield return Expression.NotEqual(columnExpr, Expression.Constant(paramValue, columnProperty.PropertyType));
                        break;
                    case LinqAsset.GreaterOrEq:
                        yield return Expression.GreaterThanOrEqual(columnExpr, Expression.Constant(paramValue, columnProperty.PropertyType));
                        break;
                    case LinqAsset.LessOrEq:
                        yield return Expression.LessThanOrEqual(columnExpr, Expression.Constant(paramValue, columnProperty.PropertyType));
                        break;
                    //20170807 Barbara Add Less for DateTime To Search
                    case LinqAsset.Less:
                        yield return Expression.LessThan(columnExpr, Expression.Constant(paramValue, columnProperty.PropertyType));
                        break;
                    case LinqAsset.LikeOp:
                    case LinqAsset.StartOp:
                    case LinqAsset.EndOp:
                        yield return GenerateCompareLambda(paramValue.ToString(), entityType, columnExpr, functionKey);
                        break;
                    case LinqAsset.CheckListOp:
                        Expression OrExp = Expression.Equal(Expression.Constant(0), Expression.Constant(1));
                        var valuelist = paramValue.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var val in valuelist)
                        {
                            OrExp = Expression.OrElse(OrExp, Expression.Equal(columnExpr, Expression.Constant(val, typeof(string))));
                        }
                        if (valuelist.Length > 0)
                        {
                            yield return OrExp;
                        }
                        break;
                    default:
                        yield return Expression.Equal(columnExpr, Expression.Constant(paramValue, columnProperty.PropertyType));
                        break;
                }
            }
        }


    }
}
