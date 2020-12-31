using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Slack.NetStandard.Endpoint;

namespace Slack.NetStandard.RequestHandler.Interceptors
{
    internal class SlackRequestInterceptor<TResponse> : ISlackRequestInterceptor<TResponse>
    {
        public SlackRequestInterceptor(LinkedListNode<ISlackRequestInterceptor<TResponse>> node, ISlackRequestHandler<TResponse> handler)
        {
            Node = node;
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public LinkedListNode<ISlackRequestInterceptor<TResponse>> Node { get; }
        public ISlackRequestHandler<TResponse> Handler { get; }

        public Task<TResponse> Intercept(SlackContext information)
        {
            if (Node == null)
            {
                return Handler.Handle(information);
            }

            var interceptor = Node.Value;

            return interceptor.Intercept(information, Handler, new SlackRequestInterceptor<TResponse>(Node.Next, Handler).Intercept);
        }

        public Task<TResponse> Intercept(SlackContext request, ISlackRequestHandler<TResponse> handler, RequestInterceptorCall<TResponse> next)
        {
            return Node.Value.Intercept(request, handler, next);
        }
    }
}
