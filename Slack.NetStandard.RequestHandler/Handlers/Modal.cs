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
        public Dictionary<string, Modal> Modals = new();

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

            return Modals.Any(m => m.Value.CanHandle(context));
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

        public virtual async Task<ResponseAction> Update(BlockActionsPayload blockActions, SlackContext context, IParentModal parent = null)
        {
            var view = await GenerateView(context);

            if (parent == null)
            {
                var client = await GetClient(blockActions.Team.ID);
                await client.View.Open(blockActions.TriggerId, view);
            }

            return new ResponseActionPush(view);
        }

        public abstract Task<View> GenerateView(object context = null);

        protected virtual Task<ResponseAction> HandleFromParent(SlackContext context, IParentModal parent)
        {
            if (!context.Items.ContainsKey(ModalHandlerId))
            {
                return Modals.First(m => context.Items.ContainsKey(m.Value.ModalHandlerId)).Value.HandleFromParent(context, this);
            }

            return (string)context.Items[ModalHandlerId] == "submit" ?
                Submit((ViewSubmissionPayload)context.Interaction, context, parent) :
                Update((BlockActionsPayload)context.Interaction, context, parent);
        }

        public virtual Task<ResponseAction> Handle(SlackContext context)
        {
            if (!context.Items.ContainsKey(ModalHandlerId))
            {
                return Modals.First(m => context.Items.ContainsKey(m.Value.ModalHandlerId)).Value.HandleFromParent(context, this);
            }

            return (string)context.Items[ModalHandlerId] == "submit" ?
                Submit((ViewSubmissionPayload)context.Interaction, context) :
                Update((BlockActionsPayload)context.Interaction, context);
        }
    }
}