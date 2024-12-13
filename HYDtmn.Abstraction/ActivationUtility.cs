
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HYDtmn.Abstraction
{
    public static class ActivationUtility
    {

        public delegate T ObjectActivator<T>(params object[] args);

        private static object GetValue(MemberExpression member)
        {
            
            var objectMember = Expression.Convert(member, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
        }

        public static KeyValuePair<ConstructorInfo, object[]> FindConstructor<T, E>(params Expression<Func<E>>[] args)
        {
            object[] parmValues = new object[] { };

            ConstructorInfo ctor = typeof(T).GetConstructors().Where(x => x.GetParameters().Length == 0).FirstOrDefault();

            //Return parameterless constructor
            if (args.Length == 0)
                return new KeyValuePair<ConstructorInfo, object[]>(ctor, new object[] { });

            //Otherwise, find correct Constructor with parameters matching by name

            var argparms =
                args.Where(x => x.Body is MemberExpression)
                .Select(x => new Tuple<string, object>(((MemberExpression)x.Body).Member.Name, GetValue((MemberExpression)x.Body)));

            var argnames = argparms.Select(k => k.Item1).ToList();
            var parmDict = typeof(T).GetConstructors().Select((x, i) => new DictionaryEntry(new Tuple<int, ConstructorInfo>(i, x), x.GetParameters().Select(y => y.Name))).ToDictionary(x => (Tuple<int, ConstructorInfo>)x.Key, x => (IEnumerable<string>)x.Value);
            foreach (var d in parmDict)
            {
                var parms = (IEnumerable<string>)d.Value;
                if (parms.Except(argnames).Count() == 0 && argnames.Except(parms).Count() == 0)
                {
                    ctor = d.Key.Item2;
                    break;
                }

            }
            if (ctor != null)
            {
                parmValues = ctor.GetParameters().Join(argparms, x => x.Name, y => y.Item1, (x, y) => new KeyValuePair<int, object>(x.Position, y.Item2)).OrderBy(x => x.Key).Select(x => x.Value).ToArray();
            }


            return new KeyValuePair<ConstructorInfo, object[]>(ctor, parmValues);

        }

        public static ObjectActivator<T> GetActivator<T>(this ConstructorInfo ctor)
        {
            Type type = ctor.DeclaringType;
            ParameterInfo[] paramsInfo = ctor.GetParameters();

            //create a single param of type object[]
            ParameterExpression param =
                Expression.Parameter(typeof(object[]), "args");

            Expression[] argsExp =
                new Expression[paramsInfo.Length];

            //pick each arg from the params array 
            //and create a typed expression of them
            for (int i = 0; i < paramsInfo.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type paramType = paramsInfo[i].ParameterType;

                Expression paramAccessorExp =
                    Expression.ArrayIndex(param, index);

                Expression paramCastExp =
                    Expression.Convert(paramAccessorExp, paramType);

                argsExp[i] = paramCastExp;
            }

            //make a NewExpression that calls the
            //ctor with the args we just created
            NewExpression newExp = Expression.New(ctor, argsExp);

            //create a lambda with the New
            //Expression as body and our param object[] as arg
            LambdaExpression lambda =
                Expression.Lambda(typeof(ObjectActivator<T>), newExp, param);

            //compile it
            ObjectActivator<T> compiled = (ObjectActivator<T>)lambda.Compile();
            return compiled;
        }

    }
}
