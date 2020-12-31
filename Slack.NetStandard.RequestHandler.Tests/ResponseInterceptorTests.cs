using System.Threading.Tasks;
using NSubstitute;
using Slack.NetStandard.Endpoint;
using Slack.NetStandard.RequestHandler.Handlers;
using Xunit;

namespace Slack.NetStandard.RequestHandler.Tests
{
    public class ResponseInterceptorTests
    {

        ISlackRequestHandler<object> Handler { get; }
        public ResponseInterceptorTests()
        {
            Handler = Substitute.For<AlwaysTrueSlackRequestHandler<object>>();
        }

        [Fact]
        public async Task EmptyInterceptorReturnsHandler()
        {
            var expected = new object();
            Handler.Handle(Arg.Any<SlackContext>()).Returns(expected);
            var request = new SlackPipeline<object>(new[] { Handler }, null);
            var actual = await request.Process(new SlackInformation(SlackRequestType.UnknownRequest));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task OneInterceptorCallsCorrectly()
        {
            var expected = new object();
            var before = string.Empty;
            Handler.Handle(Arg.Any<SlackContext>()).Returns(c =>
            {
                before = before + "2";
                return expected;
            });

            var interceptor = Substitute.For<ISlackRequestInterceptor<object>>();
            interceptor.Intercept(Arg.Any<SlackContext>(), Arg.Any<ISlackRequestHandler<object>>(),Arg.Any<RequestInterceptorCall<object>>()).Returns(c => {
                before = before + "1";
                var actual = c.Arg<RequestInterceptorCall<object>>()(c.Arg<SlackContext>());
                before = before + "3";
                return actual;
            });

            var request = new SlackPipeline<object>(new[] { Handler }, null, new[] { interceptor }, null);
            await request.Process(new SlackInformation(SlackRequestType.UnknownRequest));
            Assert.Equal("123", before);
        }

        [Fact]
        public async Task TwoInterceptorCallsCorrectly()
        {
            var expected = new object();
            var before = string.Empty;
            Handler.Handle(Arg.Any<SlackContext>()).Returns(c =>
            {
                before = before + "3";
                return expected;
            });

            var interceptor = Substitute.For<ISlackRequestInterceptor<object>>();
            interceptor.Intercept(Arg.Any<SlackContext>(), Arg.Any<ISlackRequestHandler<object>>(),Arg.Any<RequestInterceptorCall<object>>()).Returns(c => {
                before = before + "1";
                var actual = c.Arg<RequestInterceptorCall<object>>()(c.Arg<SlackContext>());
                before = before + "5";
                return actual;
            });

            var secondInterceptor = Substitute.For<ISlackRequestInterceptor<object>>();
            secondInterceptor.Intercept(Arg.Any<SlackContext>(), Arg.Any<ISlackRequestHandler<object>>(), Arg.Any<RequestInterceptorCall<object>>()).Returns(c => {
                Assert.Equal(Handler, c.Arg<ISlackRequestHandler<object>>());
                before = before + "2";
                var actual = c.Arg<RequestInterceptorCall<object>>()(c.Arg<SlackContext>());
                before = before + "4";
                return actual;
            });

            var request = new SlackPipeline<object>(new[] { Handler },
                null,
                new[] { interceptor, secondInterceptor }, null);
            await request.Process(new SlackInformation(SlackRequestType.UnknownRequest));
            Assert.Equal("12345", before);
        }

        [Fact]
        public async Task TwoInterceptorCallsInOrder()
        {
            var expected = new object();
            var before = string.Empty;
            Handler.Handle(Arg.Any<SlackContext>()).Returns(c =>
            {
                before = before + "3";
                return expected;
            });

            var interceptor = Substitute.For<ISlackRequestInterceptor<object>>();
            interceptor.Intercept(Arg.Any<SlackContext>(), Arg.Any<ISlackRequestHandler<object>>(),Arg.Any<RequestInterceptorCall<object>>()).Returns(c => {
                before = before + "1";
                var actual = c.Arg<RequestInterceptorCall<object>>()(c.Arg<SlackContext>());
                before = before + "5";
                return actual;
            });

            var secondInterceptor = Substitute.For<ISlackRequestInterceptor<object>>();
            secondInterceptor.Intercept(Arg.Any<SlackContext>(), Arg.Any<ISlackRequestHandler<object>>(),Arg.Any<RequestInterceptorCall<object>>()).Returns(c => {
                before = before + "2";
                var actual = c.Arg<RequestInterceptorCall<object>>()(c.Arg<SlackContext>());
                before = before + "4";
                return actual;
            });

            var request = new SlackPipeline<object>(new[] { Handler }, null, new[] { secondInterceptor, interceptor }, null);
            await request.Process(new SlackInformation(SlackRequestType.UnknownRequest));
            Assert.Equal("21354", before);
        }
    }
}
