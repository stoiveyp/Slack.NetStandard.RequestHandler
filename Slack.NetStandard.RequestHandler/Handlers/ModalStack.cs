using System.Threading.Tasks;

namespace Slack.NetStandard.RequestHandler.Handlers
{
    public abstract class ModalStack<TResponse>:ISlackRequestHandler<TResponse>
    {
        protected ModalStack(Modal initialView)
        {
            InitialView = initialView;
        }

        public Modal InitialView { get; set; }

        public abstract Task<TResponse> ConvertModalResponse(ModalResult result);

        public bool CanHandle(SlackContext context)
        {
            return InitialView.CanHandle(context);
        }

        public async Task<TResponse> Handle(SlackContext context)
        {
            var task = InitialView.Handle(context);
            var result = await task;
            return await ConvertModalResponse(result);
        }
    }
}
