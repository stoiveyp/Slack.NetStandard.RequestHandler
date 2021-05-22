using Slack.NetStandard.Objects;

namespace Slack.NetStandard.RequestHandler.Handlers
{
    public interface IParentModal
    {
        View GenerateView(object context = null);
    }
}