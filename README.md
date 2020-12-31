# Slack.NET.RequestHandlers

Slack apps can get large and complicated and editing long switch statements can be error prone. This library allows you to isolate functionality into RequestHandlers. A request handler is an isolated piece of logic that you want your Slack app to run based on a particular condition (it's slash command, it's a specific type of event with extra conditions).

# So what is a request handler?

From a code point of view it's any class that implements the following interface

```csharp
public interface ISlackRequestHandler<TResponse>
{
	bool CanHandle(SlackContext information);
	Task<TResponse> Handle(SlackContext information);
}
```

The way this works is that when brought together in a pipeline and a request is processed, each of the request handlers has its `CanHandle` method executed in declaration order. The first handler that returns true is selected, and the handler logic in the `Handle` method is executed to generated the necessary response.

Here's a few examples of a request handler

__Slash Command:__
```csharp
public class EchoCommand:SlashCommandHandler<Message>
{
    public WeatherCommand():base("/echo")

    public Task<Message> Handle(SlackContext context){
        var restOfMessage = context.Command.Text;
        return new Message{Text = restOfMessage };
    }
}
```

__EventCallback in a lambda proxy__
```csharp
public class OnAppHomeOpenedEvent:EventCallbackHandler<AppHomeOpened,APIGatewayProxyResponse>
{
    public override Task<APIGatewayProxyResponse> Handle(SlackContext context)
    {
        //Add event to queue here .....
        return new APIGatewayProxyResponse{StatusCode=200};
    }
}
```

# Executing your request handlers

To execute your request handlers you build a SlackPipeline and register each of your RequestHanders. As we've said order here is important - it will allow you to make handlers that deal with subtle differences in functionality and you can register the most specific first (such as a slash command with formatted text and a slash command with no extra info).

```csharp
var pipeline = new SlackPipeline(
    new[]{
        new SlashCommandWithText(),
        new SlashCommandWithoutText(),
        new IMMessageSentToApp()
    }
)

return await pipeline.Process(slashContext);
```

Side note - another advantage of having handlers perform logic is that your executing environment doesn't need to know about the logic its executing, functionality can be tweaked and reordered by the order of the handlers without any alterations to the project that handles the actual Slack requests.

# Pre-packaged handlers
Although you can create handlers for yourself if you wish, there are several types of handler already available as base classes.

*    SlashCommandHandler - CanHandle looks for a command with a specific name
*    InteractionHandler - looks for a specific type of interaction payload (GlobalShortcut, ViewSubmision etc.) and performs any extra checks
*    EventHandler - looks for a specific type of event from the Events API
*    EventCallbackHandler - handles specific types of event callback (most events are of this type except UrlVerification and AppRateLimited)
*    AlwaysTrueRequestHandler - Good as a final item in the list, a catch all that always returns true to ensure you never have requests fail without some handled response
