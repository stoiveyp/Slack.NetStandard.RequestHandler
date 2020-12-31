using System;
using System.Threading.Tasks;

namespace Slack.NetStandard.RequestHandler.Handlers
{
    public abstract class AlwaysTrueSlackErrorHandler<TResponse> : ISlackErrorHandler<TResponse>
    {
        public bool CanHandle(SlackContext context, Exception exception) => true;

        public abstract Task<TResponse> Handle(SlackContext context, Exception exception);
    }
}