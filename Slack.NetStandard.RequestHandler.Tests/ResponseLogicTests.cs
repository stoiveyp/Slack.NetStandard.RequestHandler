using System;
using System.Threading.Tasks;
using NSubstitute;
using Slack.NetStandard.Endpoint;
using Xunit;

namespace Slack.NetStandard.RequestHandler.Tests
{
    public class ResponseLogicTests
    {
        [Fact]
        public async Task ValidFirstRequestReturnsResponse()
        {
            var expected = new object();
            var requestHandler = Substitute.For<ISlackRequestHandler<object>>();
            requestHandler.CanHandle(Arg.Any<SlackContext>()).Returns(true);
            requestHandler.Handle(Arg.Any<SlackContext>()).Returns(expected);

            var errorHandler = Substitute.For<ISlackErrorHandler<object>>();
            errorHandler.CanHandle(Arg.Any<SlackContext>(), Arg.Any<Exception>()).Returns(false);

            var request = new SlackPipeline<object>(new[] { requestHandler }, new[] { errorHandler });
            var actual = await request.Process(new SlackInformation(SlackRequestType.UnknownRequest));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ValidSecondRequestReturnsSecondResponse()
        {
            var expected = new object();

            var alwaysFalse = Substitute.For<ISlackRequestHandler<object>>();
            alwaysFalse.CanHandle(Arg.Any<SlackContext>()).Returns(false);

            var requestHandler = Substitute.For<ISlackRequestHandler<object>>();
            requestHandler.CanHandle(Arg.Any<SlackContext>()).Returns(true);
            requestHandler.Handle(Arg.Any<SlackContext>()).Returns(expected);

            var errorHandler = Substitute.For<ISlackErrorHandler<object>>();
            errorHandler.CanHandle(Arg.Any<SlackContext>(), Arg.Any<Exception>()).Returns(false);

            var request = new SlackPipeline<object>(new[] { alwaysFalse, requestHandler }, new[] { errorHandler });
            var actual = await request.Process(new SlackInformation(SlackRequestType.UnknownRequest));
            Assert.Equal(expected, actual);
        }
    }
}
