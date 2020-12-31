using System.Threading.Tasks;

namespace Slack.NetStandard.RequestHandler
{
    public delegate Task<TResponse> RequestInterceptorCall<TResponse>(SlackContext information);

    public interface ISlackRequestInterceptor<TResponse>
    {
        public Task<TResponse> Intercept(SlackContext information, ISlackRequestHandler<TResponse> handler, RequestInterceptorCall<TResponse> next);
    }
}