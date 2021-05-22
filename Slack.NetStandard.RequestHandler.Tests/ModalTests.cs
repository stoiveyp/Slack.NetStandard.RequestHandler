using System;
using System.Threading.Tasks;
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
        public void ModalCanHandleViewSubmission()
        {
            var modal = Substitute.ForPartsOf<Modal>("testCallback");
            var context = new SlackContext(new SlackInformation(new ViewSubmissionPayload
                {View = new View {CallbackId = "testCallback"}}));
            Assert.True(modal.CanHandle(context));
        }

        [Fact]
        public void ModalCanHandleInvalidViewSubmission()
        {
            var modal = Substitute.ForPartsOf<Modal>("testCallback");
            var context = new SlackContext(new SlackInformation(new ViewSubmissionPayload
                { View = new View { CallbackId = "badCallback" } }));
            Assert.False(modal.CanHandle(context));
        }

        [Fact]
        public void ModalCanHandleSubModalViewSubmission()
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
        public void ModalCanHandleBlocksAction()
        {
            var modal = Substitute.ForPartsOf<Modal>(string.Empty, new[]{"testAction"});
            var context = new SlackContext(new SlackInformation(new BlockActionsPayload{Actions = new []{new PayloadAction{ActionId="testAction"}}}));
            Assert.True(modal.CanHandle(context));
        }

        [Fact]
        public void ModalCanHandleInvalidBlocksAction()
        {
            var modal = Substitute.ForPartsOf<Modal>(string.Empty, new[] { "realAction" });
            var context = new SlackContext(new SlackInformation(new BlockActionsPayload { Actions = new[] { new PayloadAction { ActionId = "testAction" } } }));
            Assert.False(modal.CanHandle(context));
        }

        [Fact]
        public async Task ModalHandlesViewSubmission()
        {
            var modal = Substitute.ForPartsOf<Modal>("testCallback");
            modal.Configure().Submit(Arg.Any<ViewSubmissionPayload>(), Arg.Any<SlackContext>())
                .Returns(new ResponseActionClear());
            var context = new SlackContext(new SlackInformation(new ViewSubmissionPayload
                { View = new View { CallbackId = "testCallback" } }));

            context.Items.Add(modal.ModalHandlerId, "submit");
            var response = await modal.Handle(context);

            Assert.IsType<ResponseActionClear>(response);
            await modal.Received(1).Submit(Arg.Any<ViewSubmissionPayload>(), Arg.Any<SlackContext>());
        }

        [Fact]
        public async Task ModalHandlesUpdateSubmission()
        {
            var modal = Substitute.ForPartsOf<Modal>("testCallback");
            modal.Configure().Update(Arg.Any<BlockActionsPayload>(), Arg.Any<SlackContext>())
                .Returns(new ResponseActionClear());
            var context = new SlackContext(new SlackInformation(new BlockActionsPayload()));

            context.Items.Add(modal.ModalHandlerId, "update");
            var response = await modal.Handle(context);

            Assert.IsType<ResponseActionClear>(response);
            await modal.Received(1).Update(Arg.Any<BlockActionsPayload>(), Arg.Any<SlackContext>());
        }

        [Fact]
        public async Task SubModalCallsWithParent()
        {
            var modal = Substitute.ForPartsOf<Modal>("testCallback");
            modal.Configure().Submit(Arg.Any<ViewSubmissionPayload>(), Arg.Any<SlackContext>(), Arg.Any<Modal>())
                .Returns(new ResponseActionClear());
            var parentModal = Substitute.ForPartsOf<Modal>("parentCallback");
            parentModal.Modals.Add("secondary", modal);

            var context = new SlackContext(new SlackInformation(new ViewSubmissionPayload()));
            context.Items.Add(modal.ModalHandlerId, "submit");

            var response = await modal.Handle(context);

            Assert.IsType<ResponseActionClear>(response);
            await modal.Received(1).Submit(Arg.Any<ViewSubmissionPayload>(), Arg.Any<SlackContext>(),Arg.Any<Modal>());
        }
    }
}
