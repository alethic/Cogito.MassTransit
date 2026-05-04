using System;

using MassTransit;

using NSubstitute;

namespace Cogito.MassTransit.Tests
{

    public class ExceptionInfoExceptionTests
    {

        static ExceptionInfo CreateExceptionInfo(string message, string? exceptionType = null, ExceptionInfo? inner = null)
        {
            var info = Substitute.For<ExceptionInfo>();
            info.Message.Returns(message);
            info.ExceptionType.Returns(exceptionType ?? typeof(Exception).FullName!);
            info.InnerException.Returns(inner);
            info.StackTrace.Returns(string.Empty);
            info.Source.Returns(string.Empty);
            return info;
        }

        [Fact]
        public void Captures_message_from_exception_info()
        {
            var info = CreateExceptionInfo("boom");

            var ex = new ExceptionInfoException(info);

            Assert.Equal("boom", ex.Message);
            Assert.Same(info, ex.ExceptionInfo);
            Assert.Null(ex.InnerException);
        }

        [Fact]
        public void Wraps_inner_exception_info_recursively()
        {
            var inner = CreateExceptionInfo("inner");
            var outer = CreateExceptionInfo("outer", inner: inner);

            var ex = new ExceptionInfoException(outer);

            Assert.Equal("outer", ex.Message);
            var innerEx = Assert.IsType<ExceptionInfoException>(ex.InnerException);
            Assert.Equal("inner", innerEx.Message);
            Assert.Same(inner, innerEx.ExceptionInfo);
            Assert.Null(innerEx.InnerException);
        }

    }

}
