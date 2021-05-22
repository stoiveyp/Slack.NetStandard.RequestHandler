using System;
using NSubstitute;
using NSubstitute.Extensions;
using Slack.NetStandard.Endpoint;
using Slack.NetStandard.Interaction;
using Slack.NetStandard.Objects;
using Slack.NetStandard.RequestHandler.Handlers;
using Xunit;

namespace Slack.NetStandard.RequestHandler.Tests
{
    public class ModalTests
    {
        [Fact]
        public void ModalHandlesViewSubmission()
        {
            var modal = Substitute.ForPartsOf<Modal>("testCallback");
            var context = new SlackContext(new SlackInformation(new ViewSubmissionPayload
                {View = new View {CallbackId = "testCallback"}}));
            Assert.True(modal.CanHandle(context));
        }

        [Fact]
        public void ModalHandlesInvalidViewSubmission()
        {
            var modal = Substitute.ForPartsOf<Modal>("testCallback");
            var context = new SlackContext(new SlackInformation(new ViewSubmissionPayload
                { View = new View { CallbackId = "badCallback" } }));
            Assert.False(modal.CanHandle(context));
        }

        [Fact]
        public void ModalHandlesSubModalViewSubmission()
        {
            var modal = Substitute.ForPartsOf<Modal>("testCallback");
            var subModal = Substitute.ForPartsOf<Modal>("subCallback");
            
            var context = new SlackContext(new SlackInformation(new ViewSubmissionPayload
                { View = new View { CallbackId = "subCallback" } }));
            Assert.False(modal.CanHandle(context));
            modal.Modals.Add("secondScreen",subModal);
            Assert.True(modal.CanHandle(context));
        }

        [Fact]
        public void ModalHandlesBlocksAction()
        {
            var modal = Substitute.ForPartsOf<Modal>(string.Empty, new[]{"testAction"});
            var context = new SlackContext(new SlackInformation(new BlockActionsPayload{Actions = new []{new PayloadAction{ActionId="testAction"}}}));
            Assert.True(modal.CanHandle(context));
        }

        [Fact]
        public void ModalHandlesInvalidBlocksAction()
        {
            var modal = Substitute.ForPartsOf<Modal>(string.Empty, new[] { "realAction" });
            var context = new SlackContext(new SlackInformation(new BlockActionsPayload { Actions = new[] { new PayloadAction { ActionId = "testAction" } } }));
            Assert.False(modal.CanHandle(context));
        }
    }
}
