using System;
using System.Threading.Tasks;

namespace Slack.NetStandard.RequestHandler
{
    public interface ISlackErrorHandler<TResponse>
    {
        bool CanHandle(SlackContext context, Exception exception);

        Task<TResponse> Handle(SlackContext context, Exception exception);
    }
}