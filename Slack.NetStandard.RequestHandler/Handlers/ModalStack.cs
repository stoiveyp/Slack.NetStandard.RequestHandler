using System;
using System.Threading.Tasks;
using Slack.NetStandard.Interaction;
using Slack.NetStandard.WebApi;

namespace Slack.NetStandard.RequestHandler.Handlers
{
    public abstract class ModalStack<TResponse>:ISlackRequestHandler<TResponse>
    {
        protected ModalStack(Modal initialView)
        {
            InitialView = initialView;
        }

        public Modal InitialView { get; set; }

        public abstract Task<TResponse> ConvertResponseAction((ResponseAction Submit, WebApiResponse Update) responses);

        public bool CanHandle(SlackContext context)
        {
            return InitialView.CanHandle(context);
        }

        public async Task<TResponse> Handle(SlackContext context)
        {
            var task = InitialView.Handle(context);
            var result = await task;
            return await ConvertResponseAction(result);
        }
    }
}
