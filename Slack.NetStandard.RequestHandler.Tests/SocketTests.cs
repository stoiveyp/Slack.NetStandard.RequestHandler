using System.Threading.Tasks;
using NSubstitute;
using Slack.NetStandard.Interaction;
using Slack.NetStandard.RequestHandler.Handlers;
using Slack.NetStandard.Socket;
using Xunit;

namespace Slack.NetStandard.RequestHandler.Tests
{
    public class SocketTests
    {
        [Fact]
        public async Task EnvelopePassedAsItem()
        {
            var errorHandler = Substitute.For<AlwaysTrueSlackRequestHandler<object>>();
            var request = new SlackPipeline<object>(errorHandler);
            var response = await request.Process(new Envelope{Payload = new BlockActionsPayload()});
            await errorHandler.Received().Handle(Arg.Is<SlackContext>(c => c.Items.ContainsKey("envelope") && c.Items["envelope"] != null));
        }
    }
}
