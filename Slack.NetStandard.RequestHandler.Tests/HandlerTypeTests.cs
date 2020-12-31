using System;
using System.Collections.Generic;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using NSubstitute;
using Slack.NetStandard.Endpoint;
using Slack.NetStandard.EventsApi;
using Slack.NetStandard.EventsApi.CallbackEvents;
using Slack.NetStandard.Interaction;
using Slack.NetStandard.Objects;
using Slack.NetStandard.RequestHandler.Handlers;
using Xunit;

namespace Slack.NetStandard.RequestHandler.Tests
{
    public class HandlerTypeTests
    {
        [Fact]
        public void CommandHandler()
        {
            var handler = Substitute.For<SlashCommandHandler<object>>("weather");
            Assert.True(handler.CanHandle(new SlackContext(new SlackInformation(new SlashCommand(new Dictionary<string,string>{{"command","/weather"}})))));
        }

        [Fact]
        public void InteractionHandler()
        {
            var handler = Substitute.For<InteractionHandler<BlockActionsPayload, object>>();
            Assert.True(handler.CanHandle(new SlackContext(new SlackInformation(new BlockActionsPayload{Type = InteractionType.BlockActions}))));
        }

        [Fact]
        public void InteractionHandlerWithAdditionalCheck()
        {
            var handler = Substitute.For<InteractionHandler<BlockActionsPayload, object>>(new Func<BlockActionsPayload, bool>(b => b.User != null));
            Assert.True(handler.CanHandle(new SlackContext(new SlackInformation(new BlockActionsPayload { Type = InteractionType.BlockActions,User=new UserSummary() }))));
        }

        [Fact]
        public void InteractionHandlerWithFalseAdditionalCheck()
        {
            var handler = Substitute.For<InteractionHandler<BlockActionsPayload, object>>(new Func<BlockActionsPayload,bool>(b => b.User != null));
            Assert.False(handler.CanHandle(new SlackContext(new SlackInformation(new BlockActionsPayload { Type = InteractionType.BlockActions }))));
        }

        [Fact]
        public void EventTypeHandler()
        {
            var handler = Substitute.For<SlackEventHandler<UrlVerification, object>>();
            Assert.True(handler.CanHandle(new SlackContext(new SlackInformation(new UrlVerification{Type = UrlVerification.EventType}))));
        }

        [Fact]
        public void EventTypeHandlerWithCheck()
        {
            var handler = Substitute.For<SlackEventHandler<UrlVerification, object>>(new Func<UrlVerification,bool>(uv => uv.Challenge == "ABC"));
            Assert.True(handler.CanHandle(new SlackContext(new SlackInformation(new UrlVerification { Type = UrlVerification.EventType,Challenge = "ABC"}))));
        }
        [Fact]
        public void FalseEventTypeHandlerWithCheck()
        {
            var handler = Substitute.For<SlackEventHandler<UrlVerification, object>>(new Func<UrlVerification, bool>(uv => uv.Challenge == "BCA"));
            Assert.False(handler.CanHandle(new SlackContext(new SlackInformation(new UrlVerification { Type = UrlVerification.EventType, Challenge = "ABC" }))));
        }

        [Fact]
        public void EventCallbackTypeHandler()
        {
            var eventType = new EventCallback
                {Type = EventCallbackBase.EventType, Event = new AppHomeOpened {Type = AppHomeOpened.EventType}};
            var handler = Substitute.For<EventCallbackHandler<AppHomeOpened, object>>();
            Assert.True(handler.CanHandle(new SlackContext(new SlackInformation(eventType))));
        }

        [Fact]
        public void EventCallbackTypeHandlerWithCheck()
        {
            var eventType = new EventCallback
                { Type = EventCallbackBase.EventType, Event = new AppHomeOpened { Type = AppHomeOpened.EventType , Tab="messages"} };
            var handler = Substitute.For<EventCallbackHandler<AppHomeOpened, object>>(new Func<AppHomeOpened,bool>(aho => aho.Tab == "messages"));
            Assert.True(handler.CanHandle(new SlackContext(new SlackInformation(eventType))));
        }
        [Fact]
        public void FalseCallbackEventTypeHandlerWithCheck()
        {
            var eventType = new EventCallback
                { Type = EventCallbackBase.EventType, Event = new AppHomeOpened { Type = AppHomeOpened.EventType, Tab="messages" } };
            var handler = Substitute.For<EventCallbackHandler<AppHomeOpened, object>>(new Func<AppHomeOpened, bool>(aho => aho.Tab == "home"));
            Assert.False(handler.CanHandle(new SlackContext(new SlackInformation(eventType))));
        }

        [Fact]
        public void NullInteractionHandler()
        {
            var handler = Substitute.For<InteractionHandler<InteractionPayload,object>>();
            Assert.False(handler.CanHandle(new SlackContext(new SlackInformation(SlackRequestType.UnknownRequest))));
        }

        [Fact]
        public void NullCommandHandler()
        {
            var handler = Substitute.For<SlashCommandHandler<object>>("weather");
            Assert.False(handler.CanHandle(new SlackContext(new SlackInformation(SlackRequestType.UnknownRequest))));
        }

        [Fact]
        public void NullEventHandler()
        {
            var handler = Substitute.For<SlackEventHandler<UrlVerification,object>>();
            Assert.False(handler.CanHandle(new SlackContext(new SlackInformation(SlackRequestType.UnknownRequest))));
        }
    }
}
