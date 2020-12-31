using System.Threading.Tasks;

namespace Slack.NetStandard.RequestHandler
{
    public interface ISlackRequestHandler<TResponse>
    {
        bool CanHandle(SlackContext context);

        Task<TResponse> Handle(SlackContext context);
    }
}