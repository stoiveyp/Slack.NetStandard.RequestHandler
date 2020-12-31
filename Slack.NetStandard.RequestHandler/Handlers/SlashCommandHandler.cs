using System;
using System.Threading.Tasks;

namespace Slack.NetStandard.RequestHandler.Handlers
{
    public abstract class SlashCommandHandler<TResponse>:ISlackRequestHandler<TResponse>
    {
        protected SlashCommandHandler(string command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }
            Command = (command.StartsWith("/") ? string.Empty : "/") + command;
        }

        public string Command { get; }

        public bool CanHandle(SlackContext context)
        {
            return context?.Command?.Command == Command;
        }

        public abstract Task<TResponse> Handle(SlackContext context);
    }
}
