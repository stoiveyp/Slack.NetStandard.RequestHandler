using System;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Slack.NetStandard.Endpoint;
using Slack.NetStandard.RequestHandler.Handlers;
using Slack.NetStandard.Socket;
using Xunit;

namespace Slack.NetStandard.RequestHandler.Tests
{
    public class ExceptionHandlingTests
    {
        [Fact]
        public async Task ArgumentNullExceptionIfNoEnvelope()
        {
            var request = new SlackPipeline<object>();
            await Assert.ThrowsAsync<ArgumentNullException>(() => request.Process((Envelope)null));
        }

        [Fact]
        public async Task ArgumentNullExceptionIfNoRequest()
        {
            var request = new SlackPipeline<object>();
            await Assert.ThrowsAsync<ArgumentNullException>(() => request.Process((SlackInformation)null));
        }

        [Fact]
        public async Task EmptyRequestHandlersHandledByErrors()
        {
            var request = new SlackPipeline<object>();
            await Assert.ThrowsAsync<SlackRequestHandlerNotFoundException>(() => request.Process(new SlackInformation(SlackRequestType.UnknownRequest)));
        }

        [Fact]
        public async Task EmptyRequestHandlerThrowsWhenSet()
        {
            var errorHandler = Substitute.For<AlwaysTrueSlackErrorHandler<object>>();
            errorHandler.Handle(Arg.Any<SlackContext>(), Arg.Any<Exception>()).Returns(new object());
            var request = new SlackPipeline<object>(null, new[] { errorHandler }) { RequestHandlerTriggersErrorHandlers = false };
            await Assert.ThrowsAsync<SlackRequestHandlerNotFoundException>(() => request.Process(new SlackInformation(SlackRequestType.UnknownRequest)));
        }

        [Fact]
        public async Task EmptyErrorHandlerThrowsOnException()
        {
            var requestHandler = Substitute.For<AlwaysTrueSlackRequestHandler<object>>();
            requestHandler.Handle(Arg.Any<SlackContext>()).Throws<InvalidOperationException>();
            var request = new SlackPipeline<object>(new[] { requestHandler });
            await Assert.ThrowsAsync<InvalidOperationException>(() => request.Process(new SlackInformation(SlackRequestType.UnknownRequest)));
        }

        [Fact]
        public async Task NoValidRequestHandlerThrowsException()
        {
            var requestHandler = Substitute.For<ISlackRequestHandler<object>>();
            requestHandler.CanHandle(Arg.Any<SlackContext>()).Returns(false);
            var request = new SlackPipeline<object>(new[] { requestHandler });
            await Assert.ThrowsAsync<SlackRequestHandlerNotFoundException>(() => request.Process(new SlackInformation(SlackRequestType.UnknownRequest)));
        }

        [Fact]
        public async Task NoValidErrorHandlerThrowsException()
        {
            var requestHandler = Substitute.For<ISlackRequestHandler<object>>();
            requestHandler.CanHandle(Arg.Any<SlackContext>()).Returns(false);

            var errorHandler = Substitute.For<ISlackErrorHandler<object>>();
            errorHandler.CanHandle(Arg.Any<SlackContext>(), Arg.Any<Exception>()).Returns(false);

            var request = new SlackPipeline<object>(new[] { requestHandler }, new[] { errorHandler });
            await Assert.ThrowsAsync<SlackRequestHandlerNotFoundException>(() => request.Process(new SlackInformation(SlackRequestType.UnknownRequest)));
        }

        [Fact]
        public async Task ValidErrorHandlerReturnsResponse()
        {
            var errorHandler = Substitute.For<AlwaysTrueSlackErrorHandler<object>>();
            errorHandler.Handle(Arg.Any<SlackContext>(), Arg.Any<Exception>()).Returns(new object());
            var request = new SlackPipeline<object>(null, new[] { errorHandler });
            var response = await request.Process(new SlackInformation(SlackRequestType.UnknownRequest));
            Assert.NotNull(response);
        }

        [Fact]
        public async Task InvalidResponseValidErrorHandlerReturnsResponse()
        {
            var requestHandler = Substitute.For<ISlackRequestHandler<object>>();
            requestHandler.CanHandle(Arg.Any<SlackContext>()).Returns(false);

            var errorHandler = Substitute.For<AlwaysTrueSlackErrorHandler<object>>();
            errorHandler.Handle(Arg.Any<SlackContext>(), Arg.Any<Exception>()).Returns(new object());
            var request = new SlackPipeline<object>(new[] { requestHandler }, new[] { errorHandler });
            var response = await request.Process(new SlackInformation(SlackRequestType.UnknownRequest));
            Assert.NotNull(response);
        }

        [Fact]
        public async Task SpecificExceptionHandlerPicks()
        {
            var requestHandler = Substitute.For<AlwaysTrueSlackRequestHandler<object>>();
            requestHandler.Handle(Arg.Any<SlackContext>()).Throws<InvalidOperationException>();


            var specificError = Substitute.For<ISlackErrorHandler<object>>();
            specificError.CanHandle(Arg.Any<SlackContext>(), Arg.Any<InvalidOperationException>()).Returns(true);
            specificError.Handle(Arg.Any<SlackContext>(), Arg.Any<Exception>()).Returns(new object());
            var errorHandler = Substitute.For<AlwaysTrueSlackErrorHandler<object>>();


            var request = new SlackPipeline<object>(new[] { requestHandler }, new ISlackErrorHandler<object>[] { specificError, errorHandler });
            var response = await request.Process(new SlackInformation(SlackRequestType.UnknownRequest));
            Assert.NotNull(response);
        }
    }
}
