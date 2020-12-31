using System;
using System.Threading.Tasks;
using Slack.NetStandard.EventsApi;

namespace Slack.NetStandard.RequestHandler.Handlers
{
    public abstract class SlackEventHandler<TEventType, TResponse> : ISlackRequestHandler<TResponse> where TEventType : Event
    {
        protected SlackEventHandler()
        {

        }

        protected SlackEventHandler(Func<TEventType, bool> typeCheck)
        {
            TypeCheck = typeCheck;
        }

        private Func<TEventType, bool> TypeCheck { get; }

        public bool CanHandle(SlackContext context)
        {
            return context.Event is TEventType payload && (TypeCheck ?? (t => true))(payload);
        }

        public abstract Task<TResponse> Handle(SlackContext context);
    }
}