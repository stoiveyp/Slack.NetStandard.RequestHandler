using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Slack.NetStandard.RequestHandler.Interceptors
{
    public class SlackErrorInterceptor<TResponse> :ISlackErrorInterceptor<TResponse>
    {
        public SlackErrorInterceptor(LinkedListNode<ISlackErrorInterceptor<TResponse>> node, ISlackRequestHandler<TResponse> requestHandler, ISlackErrorHandler<TResponse> handler)
        {
            Node = node;
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
            RequestHandler = requestHandler;
        }

        public ISlackRequestHandler<TResponse> RequestHandler { get; set; }

        public LinkedListNode<ISlackErrorInterceptor<TResponse>> Node { get; }
        public ISlackErrorHandler<TResponse> Handler { get; }

        public Task<TResponse> Intercept(SlackContext context, Exception ex)
        {
            if (Node == null)
            {
                return Handler.Handle(context, ex);
            }

            var interceptor = Node.Value;
            return interceptor.Intercept(context, RequestHandler, Handler, ex, new SlackErrorInterceptor<TResponse>(Node.Next, RequestHandler, Handler).Intercept);
        }

        public Task<TResponse> Intercept(SlackContext context, ISlackRequestHandler<TResponse> requestHandler, ISlackErrorHandler<TResponse> errorHandler, Exception ex, ErrorInterceptorCall<TResponse> next)
        {
            return Node.Value.Intercept(context, requestHandler, errorHandler, ex, next);
        }
    }
}
