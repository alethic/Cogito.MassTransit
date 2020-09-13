using System;
using System.Linq.Expressions;

namespace Cogito.MassTransit
{

    /// <summary>
    /// Provides extension methods for working with <see cref="Expression"/>s.
    /// </summary>
    static class ExpressionExtensions
    {

        /// <summary>
        /// Replaces the given <paramref name="source"/> <see cref="Expression"/> with the specified <paramref name="target"/> <see cref="Expression"/>.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Expression Replace(this Expression expression, Expression source, Expression target)
        {
            return new ReplacerVisitor(source, target).Visit(expression);
        }

        /// <summary>
        /// Transforms the expression.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        class ReplacerVisitor : ExpressionVisitor
        {

            readonly Expression source;
            readonly Expression target;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="source"></param>
            /// <param name="target"></param>
            public ReplacerVisitor(Expression source, Expression target)
            {
                this.source = source ?? throw new ArgumentNullException(nameof(source));
                this.target = target ?? throw new ArgumentNullException(nameof(target));
            }

            public override Expression Visit(Expression node)
            {
                return node == source ? target : base.Visit(node);
            }

        }

    }

}
