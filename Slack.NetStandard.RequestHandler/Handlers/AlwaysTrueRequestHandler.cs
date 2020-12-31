using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Slack.NetStandard.RequestHandler.Handlers
{
    public abstract class AlwaysTrueSlackRequestHandler<TResponse>:ISlackRequestHandler<TResponse>
    {
        public bool CanHandle(SlackContext context) => true;

        public abstract Task<TResponse> Handle(SlackContext context);
    }
}
