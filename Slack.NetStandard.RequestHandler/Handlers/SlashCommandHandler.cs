using System;
using System.Threading.Tasks;
using Slack.NetStandard.Interaction;

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

        protected SlashCommandHandler(string command, Func<SlashCommand, bool> typeCheck) : this(command)
        {
            TypeCheck = typeCheck;
        }

        private Func<SlashCommand, bool> TypeCheck { get; }


        public string Command { get; }

        public bool CanHandle(SlackContext context)
        {
            return context?.Command?.Command == Command && (TypeCheck ?? (t => true))(context?.Command);
        }

        public virtual Task<TResponse> Handle(SlackContext context)
        {
            return HandleCommand(context.Command, context.Tag);
        }

        protected abstract Task<TResponse> HandleCommand(SlashCommand command, object tag = null);
    }
}
