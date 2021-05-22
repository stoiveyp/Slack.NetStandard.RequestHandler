using System;
using System.Threading.Tasks;
using Slack.NetStandard.Interaction;

namespace Slack.NetStandard.RequestHandler.Handlers
{
    public abstract class ModalStack<TResponse>:ISlackRequestHandler<TResponse>
    {
        protected ModalStack(Modal initialView)
        {
            InitialView = initialView;
        }

        public Modal InitialView { get; set; }

        public abstract Task<TResponse> ConvertResponseAction(ResponseAction responseAction);

        public abstract Task<TResponse> NoResponse();

        public bool CanHandle(SlackContext context)
        {
            return InitialView.CanHandle(context);
        }

        public async Task<TResponse> Handle(SlackContext context)
        {
            var task = InitialView.Handle(context);
            if (task == null)
            {
                return await NoResponse();
            }

            var result = await task;
            return await (result != null ? ConvertResponseAction(result) : NoResponse());
        }
    }
}
