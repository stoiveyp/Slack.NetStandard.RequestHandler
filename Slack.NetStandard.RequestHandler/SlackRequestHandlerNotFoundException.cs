using System;

namespace Slack.NetStandard.RequestHandler
{
    public class SlackRequestHandlerNotFoundException : Exception
    {
        private const string RequestHandlerExceptionMessage = "No matching request handler found";

        public SlackRequestHandlerNotFoundException() : base(RequestHandlerExceptionMessage)
        {
        }

        public SlackRequestHandlerNotFoundException(Exception innerException) : base(RequestHandlerExceptionMessage, innerException)
        {
        }
    }
}