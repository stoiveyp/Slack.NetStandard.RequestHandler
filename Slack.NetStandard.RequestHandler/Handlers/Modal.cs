using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Slack.NetStandard.Interaction;
using Slack.NetStandard.Objects;

namespace Slack.NetStandard.RequestHandler.Handlers
{
    public abstract class Modal
    {
        protected Modal(string callbackId):this(callbackId, bp => false){}

        protected Modal(string callbackId, params string[] updateActionIds):this(callbackId, bp => IsBlockAction(bp, updateActionIds))
        {
            
        }

        protected Modal(string callbackId, Func<SlackContext, bool> updateCheck)
        {
            CallbackId = callbackId;
            UpdateCheck = updateCheck;
        }

        public Func<SlackContext, bool> UpdateCheck { get; set; }

        internal readonly string ModalId = Guid.NewGuid().ToString("N");
        public Dictionary<string,Modal> Modals = new ();

        public string CallbackId { get; set; }
        public virtual bool CanHandle(SlackContext context)
        {
            if (IsViewSubmission(context))
            {
                context.Items[ModalId] = "view";
                return true;

            }

            if (UpdateCheck(context))
            {
                context.Items[ModalId] = "update";
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

        public virtual Task<ResponseAction> Submit(ViewSubmissionPayload payload, SlackContext context)
        {
            return Task.FromResult(default(ResponseAction));
        }

        public virtual Task<ResponseAction> Update(BlockActionsPayload blockActions, SlackContext context)
        {
            return Task.FromResult(default(ResponseAction));
        }

        protected abstract View InitialView(object context = null);

        public Task<ResponseAction> Handle(SlackContext context)
        {
            if (!context.Items.ContainsKey(ModalId))
            {
                return Modals.First(m => context.Items.ContainsKey(m.Value.ModalId)).Value.Handle(context);
            }

            return (string)context.Items[ModalId] == "view" ? 
                Submit((ViewSubmissionPayload)context.Interaction, context) : 
                Update((BlockActionsPayload) context.Interaction, context);
        }
    }
}