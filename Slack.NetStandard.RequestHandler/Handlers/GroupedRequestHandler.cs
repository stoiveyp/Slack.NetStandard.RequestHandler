using System;
using System.Linq;
using System.Threading.Tasks;

namespace Slack.NetStandard.RequestHandler.Handlers
{
    public class GroupedRequestHandler<T>:ISlackRequestHandler<T>
    {
        public GroupedRequestHandler(Func<SlackContext,bool> groupCheck, params ISlackRequestHandler<T>[] handlers)
        {
            GroupCheck = groupCheck;
            Handlers = handlers;
        }

        public Func<SlackContext, bool> GroupCheck { get; }
        public ISlackRequestHandler<T>[] Handlers { get; }
        public readonly string GroupHandlerId = Guid.NewGuid().ToString("N");

        public bool CanHandle(SlackContext context)
        {
            if (!GroupCheck(context)) return false;

            var handler = Handlers.FirstOrDefault(h => h.CanHandle(context));
            if (handler != null)
            {
                context.Items[GroupHandlerId] = handler;
                return true;
            }

            return false;
        }

        public Task<T> Handle(SlackContext context) => context.Items.ContainsKey(GroupHandlerId) 
            ? ((ISlackRequestHandler<T>)context.Items[GroupHandlerId]).Handle(context) 
            : Task.FromResult(default(T));
        
    }
}
