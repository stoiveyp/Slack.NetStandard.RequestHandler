using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using Slack.NetStandard.Endpoint;
using Slack.NetStandard.RequestHandler.Handlers;
using Xunit;

namespace Slack.NetStandard.RequestHandler.Tests
{
    public class GroupedRequestHandlerTests
    {
        [Fact]
        public void FailedGroupSkipsChildren()
        {
            var child = Substitute.For<ISlackRequestHandler<object>>();
            child.CanHandle(Arg.Any<SlackContext>()).Returns(true);
            var group = new GroupedRequestHandler<object>(_ => false,child);
            var result = group.CanHandle(new SlackContext(new SlackInformation(SlackRequestType.UnknownRequest)));
            Assert.False(result);
            child.DidNotReceive().CanHandle(Arg.Any<SlackContext>());
        }

        [Fact]
        public void TrueGroupChecksHandler()
        {
            var child = Substitute.For<ISlackRequestHandler<object>>();
            child.CanHandle(Arg.Any<SlackContext>()).Returns(true);
            var group = new GroupedRequestHandler<object>(_ => true, child);
            var result = group.CanHandle(new SlackContext(new SlackInformation(SlackRequestType.UnknownRequest)));
            Assert.True(result);
            child.Received(1).CanHandle(Arg.Any<SlackContext>());
        }

        [Fact]
        public void TrueGroupFalseHandlerReturnsFalse()
        {
            var child = Substitute.For<ISlackRequestHandler<object>>();
            child.CanHandle(Arg.Any<SlackContext>()).Returns(false);
            var group = new GroupedRequestHandler<object>(_ => true, child);
            var result = group.CanHandle(new SlackContext(new SlackInformation(SlackRequestType.UnknownRequest)));
            Assert.False(result);
            child.Received(1).CanHandle(Arg.Any<SlackContext>());
        }

        [Fact]
        public async Task TrueHandlerExecutesHandler()
        {
            var child = Substitute.For<ISlackRequestHandler<object>>();
            child.CanHandle(Arg.Any<SlackContext>()).Returns(true);
            child.Handle(Arg.Any<SlackContext>()).Returns((object)null);
            var group = new GroupedRequestHandler<object>(_ => true, child);

            var context = new SlackContext(new SlackInformation(SlackRequestType.UnknownRequest));
            Assert.True(group.CanHandle(context));
            var result = await group.Handle(context);
            await child.Received(1).Handle(Arg.Any<SlackContext>());
            Assert.Null(result);
        }

        [Fact]
        public async Task NoCheckReturnsDefault()
        {
            var child = Substitute.For<ISlackRequestHandler<object>>();
            child.CanHandle(Arg.Any<SlackContext>()).Returns(true);
            child.Handle(Arg.Any<SlackContext>()).Returns(new object());
            var group = new GroupedRequestHandler<object>(_ => true, child);

            var context = new SlackContext(new SlackInformation(SlackRequestType.UnknownRequest));
            var result = await group.Handle(context);
            await child.DidNotReceive().Handle(Arg.Any<SlackContext>());
            Assert.Null(result);
        }
    }
}
