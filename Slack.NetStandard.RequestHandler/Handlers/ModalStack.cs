using System;
using System.Threading.Tasks;
using Slack.NetStandard.Interaction;

namespace Slack.NetStandard.RequestHandler.Handlers
{
    public abstract class ModalStack<T>:ISlackRequestHandler<T>
    {
        protected ModalStack(Modal initialView)
        {
            InitialView = initialView;
        }

        public Modal InitialView { get; set; }

        public abstract Task<T> ConvertResponseAction(ResponseAction responseAction);

        public abstract Task<T> NoResponse();

        public bool CanHandle(SlackContext context)
        {
            return InitialView.CanHandle(context);
        }

        public async Task<T> Handle(SlackContext context)
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
