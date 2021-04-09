using System;
using System.Threading.Tasks;
using Slack.NetStandard.EventsApi;
using Slack.NetStandard.EventsApi.CallbackEvents;

namespace Slack.NetStandard.RequestHandler.Handlers
{
    public abstract class EventCallbackHandler<TEventType, TResponse> : ISlackRequestHandler<TResponse> where TEventType : ICallbackEvent
    {
        protected EventCallbackHandler()
        {

        }

        protected EventCallbackHandler(Func<TEventType, bool> typeCheck)
        {
            TypeCheck = typeCheck;
        }

        private Func<TEventType, bool> TypeCheck { get; }

        public bool CanHandle(SlackContext context)
        {
            return context.Event is EventCallback {Event: TEventType callback} && (TypeCheck ?? (t => true))(callback);
        }

        public virtual Task<TResponse> Handle(SlackContext context)
        {
            var callback = (EventCallback)context.Event;
            var evt = (TEventType)callback.Event;
            return HandleEvent(callback, evt, context.Tag);
        }

        protected abstract Task<TResponse> HandleEvent(EventCallback callback, TEventType evt, object tag = null);
    }
}