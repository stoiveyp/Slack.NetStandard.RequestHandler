using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Slack.NetStandard.Interaction;
using Slack.NetStandard.Objects;

namespace Slack.NetStandard.RequestHandler.Handlers
{
    public abstract class Modal : IParentModal
    {
        protected Modal(string callbackId) : this(callbackId, bp => false) { }

        protected Modal(string callbackId, params string[] updateActionIds) : this(callbackId, bp => IsBlockAction(bp, updateActionIds))
        {

        }

        protected Modal(string callbackId, Func<SlackContext, bool> updateCheck)
        {
            CallbackId = callbackId;
            UpdateCheck = updateCheck;
        }

        public Func<SlackContext, bool> UpdateCheck { get; set; }

        public readonly string ModalHandlerId = Guid.NewGuid().ToString("N");
        public List<Modal> Modals = new();

        public string CallbackId { get; set; }
        public virtual bool CanHandle(SlackContext context)
        {
            if (IsViewSubmission(context))
            {
                context.Items[ModalHandlerId] = "submit";
                return true;

            }

            if (UpdateCheck(context))
            {
                context.Items[ModalHandlerId] = "update";
                return true;
            }

            return Modals.Any(m => m.CanHandle(context));
        }

        protected virtual bool IsViewSubmission(SlackContext context)
        {
            return context.Interaction is ViewSubmissionPayload view && view.View.CallbackId == CallbackId;
        }

        protected static bool IsBlockAction(SlackContext context, string[] actionIds)
        {
            return actionIds.Any() && context.Interaction is BlockActionsPayload blocks &&
                   blocks.Actions.Any(pa => actionIds.Contains(pa.ActionId));
        }

        public virtual Task<ResponseAction> Submit(ViewSubmissionPayload payload, SlackContext context, IParentModal parent = null)
        {
            return Task.FromResult(default(ResponseAction));
        }

        protected abstract Task<ISlackApiClient> GetClient(string teamId);

        public virtual async Task Update(BlockActionsPayload blockActions, SlackContext context,
            IParentModal parent = null)
        {
            var view = await GenerateView(context);
            var client = await GetClient(blockActions.Team.ID);

            if (parent == null)
            {
                await client.View.Open(blockActions.TriggerId, view);
            }
            else
            {
                await client.View.Push(blockActions.TriggerId, view);
            }
        }

        public abstract Task<View> GenerateView(object context = null);

        protected virtual async Task<ResponseAction> HandleFromParent(SlackContext context, IParentModal parent)
        {
            if (!context.Items.ContainsKey(ModalHandlerId))
            {
                return await Modals.First(m => context.Items.ContainsKey(m.ModalHandlerId)).HandleFromParent(context, this);
            }

            if ((string) context.Items[ModalHandlerId] == "submit")
            {
                return await Submit((ViewSubmissionPayload) context.Interaction, context, parent);
            }

            await Update((BlockActionsPayload)context.Interaction, context, parent);
            return null;
        }

        public virtual async Task<ResponseAction> Handle(SlackContext context)
        {
            if (!context.Items.ContainsKey(ModalHandlerId))
            {
                return await Modals.First(m => context.Items.ContainsKey(m.ModalHandlerId)).HandleFromParent(context, this);
            }

            if ((string) context.Items[ModalHandlerId] == "submit")
            {
                return await Submit((ViewSubmissionPayload) context.Interaction, context);
            }
            await Update((BlockActionsPayload)context.Interaction, context);
            return null;
        }
    }
}