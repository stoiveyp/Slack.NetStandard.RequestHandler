using System;
using System.Threading.Tasks;
using Slack.NetStandard.Interaction;

namespace Slack.NetStandard.RequestHandler.Handlers
{
    public abstract class InteractionHandler<TPayloadType,TResponse>:ISlackRequestHandler<TResponse> where TPayloadType : InteractionPayload
    {
        protected InteractionHandler()
        {

        }

        protected InteractionHandler(Func<TPayloadType, bool> typeCheck)
        {
            TypeCheck = typeCheck;
        }

        private Func<TPayloadType, bool> TypeCheck { get; }

        public bool CanHandle(SlackContext context)
        {
            return context.Interaction is TPayloadType payload && (TypeCheck ?? (t => true))(payload);
        }

        public abstract Task<TResponse> Handle(SlackContext context);
    }
}
