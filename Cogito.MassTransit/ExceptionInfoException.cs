using System;

using MassTransit;

namespace Cogito.MassTransit
{

    /// <summary>
    /// Encapsulates a <see cref="global::MassTransit.ExceptionInfo"/>.
    /// </summary>
    public class ExceptionInfoException : Exception
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="exceptionInfo"></param>
        public ExceptionInfoException(ExceptionInfo exceptionInfo) :
            base(exceptionInfo.Message, exceptionInfo.InnerException != null ? new ExceptionInfoException(exceptionInfo.InnerException) : null)
        {
            ExceptionInfo = exceptionInfo;
        }

        /// <summary>
        /// Wrapped <see cref="global::MassTransit.ExceptionInfo"/> instance.
        /// </summary>
        public ExceptionInfo ExceptionInfo { get; }

    }

}
