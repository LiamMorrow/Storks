using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Storks.Encoders;

namespace Storks
{
    /// <summary>
    /// A collection of methods for useful functions when working with Stork
    /// </summary>
    public static class StorkHelpers
    {
        /// <summary>
        /// Registers encoders for many standard types
        /// </summary>
        /// <param name="controller">The controller to register the encoders to</param>
        public static void RegisterDefaultEncoders(this IStoreBackedPropertyController controller)
        {
            ThrowIfNull(() => controller);
            controller.RegisterEncoder(new StringStoreBackedPropertyEncoder());
        }

        private static void ThrowIfNull<T>(Expression<Func<T>> parameterExpression)
            where T : class
        {
            if (parameterExpression.Compile()() == null)
            {
                string name = GetParameterName(parameterExpression);
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
