using Slack.NetStandard.Interaction;
using Slack.NetStandard.WebApi;

namespace Slack.NetStandard.RequestHandler.Handlers
{
    public class ModalResult
    {
        public ModalResult(ResponseAction submit, WebApiResponse update)
        {
            Submit = submit;
            Update = update;
        }

        public WebApiResponse Update { get; }

        public ResponseAction Submit { get; }
    }
}