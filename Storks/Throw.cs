using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Storks.Encoders;

namespace Storks
{

    internal static class Throw
    {
        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the parameter given is null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameterExpression">An action pointing to a parameter</param>
        /// <exception cref="ArgumentNullException">If parameter given by parameterExpression is null</exception>
        internal static void IfNull<T>(Expression<Func<T>> parameterExpression)
            where T : class
        {
            if (parameterExpression.Compile()() == null)
            {
                var name = GetParameterName(parameterExpression);
                throw new ArgumentNullException(name);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the parameter given is null or empty
        /// </summary>
        /// <param name="parameterExpression">An action pointing to a parameter</param>
        /// <exception cref="ArgumentNullException">If parameter given by parameterExpression is null</exception>
        internal static void IfNullOrEmpty(Expression<Func<string>> parameterExpression)
        {
            if (string.IsNullOrEmpty(parameterExpression.Compile()()))
            {
                var name = GetParameterName(parameterExpression);
                throw new ArgumentNullException(name);
            }
        }

        private static string GetParameterName<T>(Expression<Func<T>> parameterExpression)
        {
            var body = (MemberExpression)parameterExpression.Body;
            return body.Member.Name;
        }
    }
}
