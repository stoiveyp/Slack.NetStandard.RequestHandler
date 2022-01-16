using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Slack.NetStandard.Endpoint;
using Slack.NetStandard.RequestHandler.Interceptors;

namespace Slack.NetStandard.RequestHandler
{
    public class SlackPipeline<TResponse>
    {
        public List<ISlackRequestHandler<TResponse>> RequestHandlers { get; set; } = new();
        public List<ISlackErrorHandler<TResponse>> ErrorHandlers { get; set; } = new();

        public LinkedList<ISlackRequestInterceptor<TResponse>> RequestInterceptors = new ();
        public LinkedList<ISlackErrorInterceptor<TResponse>> ErrorInterceptors = new ();

        public bool RequestHandlerTriggersErrorHandlers { get; set; } = true;

        public SlackPipeline()
        {
            RequestHandlers = new List<ISlackRequestHandler<TResponse>>();
            ErrorHandlers = new List<ISlackErrorHandler<TResponse>>();
            RequestInterceptors = new LinkedList<ISlackRequestInterceptor<TResponse>>();
            ErrorInterceptors = new LinkedList<ISlackErrorInterceptor<TResponse>>();
        }

        public SlackPipeline(params ISlackRequestHandler<TResponse>[] requestHandlers):this(requestHandlers, null, null, null) { }

        public SlackPipeline(IEnumerable<ISlackRequestHandler<TResponse>> requestHandlers) : this(requestHandlers, null, null, null)
        {
        }

        public SlackPipeline(IEnumerable<ISlackRequestHandler<TResponse>> requestHandlers, IEnumerable<ISlackErrorHandler<TResponse>> errorHandlers) : this(requestHandlers, errorHandlers, null, null) { }


        public SlackPipeline(IEnumerable<ISlackRequestHandler<TResponse>> requestHandlers,
            IEnumerable<ISlackErrorHandler<TResponse>> errorHandlers,
            IEnumerable<ISlackRequestInterceptor<TResponse>> requestInterceptors,
            IEnumerable<ISlackErrorInterceptor<TResponse>> errorInterceptors)
        {
            RequestHandlers = requestHandlers?.ToList() ?? new ();
            ErrorHandlers = errorHandlers?.ToList() ?? new ();
            RequestInterceptors = requestInterceptors == null ? new () : new LinkedList<ISlackRequestInterceptor<TResponse>>(requestInterceptors);
            ErrorInterceptors = errorInterceptors == null ? new () : new LinkedList<ISlackErrorInterceptor<TResponse>>(errorInterceptors);
        }

        public Task<TResponse> Process(Socket.Envelope envelope, object tag = null)
        {
            if (envelope == null)
            {
                throw new ArgumentNullException(nameof(envelope), "Envelope is required");
            }

            var context = new SlackContext(envelope.ToSlackInformation())
            {
                Tag = tag
            };
            context.Items.Add("envelope",envelope);
            return Process(context);
        }

        public Task<TResponse> Process(SlackInformation information, object tag = null)
        {
            if (information == null)
            {
                throw new ArgumentNullException(nameof(information), "Slack context is required");
            }

            return Process(new SlackContext(information){Tag = tag});
        }

        private async Task<TResponse> Process(SlackContext information)
        {
            ISlackRequestHandler<TResponse> candidate = null;
            try
            {
                candidate = RequestHandlers.FirstOrDefault(h => h?.CanHandle(information) ?? false);
                if (candidate == null)
                {
                    throw new SlackRequestHandlerNotFoundException();
                }

                return await new SlackRequestInterceptor<TResponse>(RequestInterceptors.First, candidate).Intercept(information);
            }
            catch (SlackRequestHandlerNotFoundException) when (!RequestHandlerTriggersErrorHandlers)
            {
                throw;
            }
            catch (Exception) when (!ErrorHandlers?.Any() ?? false)
            {
                throw;
            }
            catch (Exception ex)
            {
                var errorCandidate = ErrorHandlers.FirstOrDefault(eh => eh?.CanHandle(information, ex) ?? false);
                if (errorCandidate == null)
                {
                    throw;
                }

                return await new SlackErrorInterceptor<TResponse>(ErrorInterceptors.First, candidate, errorCandidate).Intercept(information, ex);
            }
        }
    }
}
