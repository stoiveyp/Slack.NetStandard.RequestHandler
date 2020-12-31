using System;
using System.Threading.Tasks;

namespace Slack.NetStandard.RequestHandler
{
    public delegate Task<TResponse> ErrorInterceptorCall<TResponse>(SlackContext information, Exception ex);

    public interface ISlackErrorInterceptor<TResponse>
    {
        Task<TResponse> Intercept(SlackContext context, ISlackRequestHandler<TResponse> requestHandler, ISlackErrorHandler<TResponse> errorHandler, Exception ex, ErrorInterceptorCall<TResponse> next);
    }
}