# MessageBus
A C# scoped message bus system that is lightweight, type safe and easy to use.


---

## Install
This is a single file library, place <B>MessageBus/MessageBus.cs</B> any where in your project.


---
## Usage without scope
<B>Step #1</B>
Declare your message types as delegates

```
public MyMessages
{
	public delegate void LogText(string text);
}
```
> The delegates can be in any class

<B>Step #2</B>
Create a function that will listen to the message

```
public MyListener
{
	private void OnLogText(string text)
	{
		Console.WriteLine("Log " + text);
	}
}
```

<B>Step #3</B>
Subscribe to the message

```

public MyListener
{
	public MyListener()
	{
		MessageBus.Subscribe<MyMessages.LogText>(OnLogText);
	}
	
	private void OnLogText(string text)
	{
		Console.WriteLine("Log " + text);
	}
}

```

<B>Step #4</B>
Dispatch messages
```
MessageBus.Dispatch<MyMessages.LogText>("Hello world!");

```

<B>Step #5</B> 
Unsubscribe from messages
```
MessageBus.Unsubscribe<MyMessages.LogText>(OnLogText);
```
> Subscriptions are kept as WeakReferences so if the subscriber gets finalized the subscription is automatically removed.

---
## Usage with scope
To subscribe to a scoped message bus, use a string for the scope
```
MessageBus.Subscribe<MyMessages.LogText>("scopeName", OnLogText);
```

To dispatch a message to a scooped bus

```
var dispatcher = MessageBus.GetDispatcher("scopeName");
dispatcher.Dispatch<MyMessages.LogText>("Hello scoped world!");
```
> The <B>MessageDispatcher</B> can be used for dependency injection so the dispatcher fo messages doesn't need to know the scope










