using System;
using System.Collections.Generic;
using Slack.NetStandard.Endpoint;
using Slack.NetStandard.EventsApi;
using Slack.NetStandard.Interaction;

namespace Slack.NetStandard.RequestHandler
{
    public class SlackContext
    {
        private SlackInformation _info;

        public SlackContext(SlackInformation info)
        {
            _info = info ?? throw new ArgumentNullException(nameof(info), "SlackInformation required");
        }

        public SlackRequestType Type => _info.Type;
        public SlashCommand Command => _info.Command;
        public InteractionPayload Interaction => _info.Interaction;
        public Event Event => _info.Event;

        public Dictionary<string, object> Items = new();
        public object Tag { get; set; }
    }
}
