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

# Grouping Common Conditions

If you start having a large number of handlers, you can group common expressions to try and skip groups of handlers if you know they'll all return false.

You can do this with a GroupedRequestHandler

```csharp
var pipeline = new SlackPipeline<object>(
            new GroupedRequestHandler<object>(
                context => context.Command != null,
                new SlashCommandWithText(),
                new SlashCommandWithoutText())
);
```

# Pre-packaged handlers
Although you can create handlers for yourself if you wish, there are several types of handler already available as base classes.

*    SlashCommandHandler - CanHandle looks for a command with a specific name
*    InteractionHandler - looks for a specific type of interaction payload (GlobalShortcut, ViewSubmision etc.) and performs any extra checks
*    EventHandler - looks for a specific type of event from the Events API
*    EventCallbackHandler - handles specific types of event callback (most events are of this type except UrlVerification and AppRateLimited)
*    AlwaysTrueRequestHandler - Good as a final item in the list, a catch all that always returns true to ensure you never have requests fail without some handled response

# Creating Modal Stacks
To help manage a stack of modal views, you can create ModalStack handlers within your application. These maintain modal view hierarchies.

__Modal Stacks__

To create a modal stack you start by creating a class that will handle all your modal views. 

This must inherit from ModalStack<T> and helps translate to your end result

```csharp
public class GatewayResponseStack : ModalStack<APIGatewayProxyResponse>
    {
        public GatewayResponseStack(Modal initialView) : base(initialView)
        {
        }

        public override Task<APIGatewayProxyResponse> ConvertResponseAction(ModalResult result)
        {
            APIGatewayProxyResponse response = result switch
            {
                { Submit: not null } => //Submission result handled here,
                { Update: not null } => //Update result (open, push, update) handled here,
                _ => //Default response is a positive "all clear"
            };

            return Task.FromResult(response);
        }
    }
```

__Modals__

Once you have your stack, then you can create a tree of Modal views that represent the stack you're trying to create.

Modal classes handle scenarios such as

* If a view lower down the tree is recognised then the view is correctly pushed onto the current stack.
* If an action ID is one of those attached to the current view, the modal will re-generate the view and update itself
* View submission is sent to the `Submit` method
* Updates via block actions are sent to the 'Update' method

You nest your modal sub-classes together to represent up to three levels of view stack Slack allows

```csharp
public EditItemStack() : base(
            new EditItem( 
                new EditItemAction(
                    new ItemActionResult())))
        {
        }
    }
```


