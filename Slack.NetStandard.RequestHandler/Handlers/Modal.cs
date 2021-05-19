using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Slack.NetStandard.Interaction;
using Slack.NetStandard.Objects;

namespace Slack.NetStandard.RequestHandler.Handlers
{
    public abstract class Modal: ISlackRequestHandler<ResponseAction>
    {
        protected Modal(string callbackId, params string[] actionIds)
        {
            CallbackId = callbackId;
            ActionIds = new List<string>(actionIds);
        }

        internal readonly string ModalId = Guid.NewGuid().ToString("N");
        public List<Modal> Modals = new ();
        public List<string> ActionIds { get; }

        public string CallbackId { get; set; }
        public bool CanHandle(SlackContext context)
        {
            if (IsViewSubmission(context))
            {
                context.Items[ModalId] = "view";
                return true;

            }

            if (IsBlockAction(context))
            {
                context.Items[ModalId] = "blocks";
                return true;
            }

            return Modals.Any(m => m.CanHandle(context));
        }

        protected virtual bool IsViewSubmission(SlackContext context)
        {
            return context.Interaction is ViewSubmissionPayload view && view.View.CallbackId == CallbackId;
        }

        protected virtual bool IsBlockAction(SlackContext context)
        {
            return ActionIds.Any() && context.Interaction is BlockActionsPayload blocks &&
                   blocks.Actions.Any(pa => ActionIds.Contains(pa.ActionId));
        }

        public virtual Task<ResponseAction> SubmitView(ViewSubmissionPayload payload, SlackContext context)
        {
            return Task.FromResult(default(ResponseAction));
        }

        public virtual Task<ResponseAction> UpdateView(BlockActionsPayload blockActions, SlackContext context)
        {
            return Task.FromResult(default(ResponseAction));
        }

        public Task<ResponseAction> Handle(SlackContext context)
        {
            if (!context.Items.ContainsKey(ModalId))
            {
                return Modals.First(m => context.Items.ContainsKey(m.ModalId)).Handle(context);
            }

            return context.Items[ModalId] == "view" ? SubmitView((ViewSubmissionPayload)context.Interaction, context) : UpdateView((BlockActionsPayload) context.Interaction, context);
        }
    }
}