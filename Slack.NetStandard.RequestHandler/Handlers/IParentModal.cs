using System.Threading.Tasks;
using Slack.NetStandard.Objects;

namespace Slack.NetStandard.RequestHandler.Handlers
{
    public interface IParentModal
    {
        Task<View> GenerateView(object context = null);
    }
}