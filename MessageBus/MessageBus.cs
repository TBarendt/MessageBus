//------------------------------------------------------------------------------
// MIT License
//
// Copyright (c) 2024 Tobias Barendt
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Diagnostics;

//------------------------------------------------------------------------------
// MessageBus
//
// The message bus is a central point for communication between different parts
// of the application. It is used to decouple different parts of the application
// and to allow for a more modular design.
//
public class MessageBus
{
	//--------------------------------------------------------------------------
	// Singleton instance
	private static MessageBus instance = new MessageBus();

	//--------------------------------------------------------------------------
	// Message Scopes
	private Dictionary<string, MessageDispatcher> m_scopes = new();

	//--------------------------------------------------------------------------
	// GetDispatcher
	//
	// Returns the message dispatcher for the given scope. IF scoped does not 
	// already exist it will be created.
	//
	//--------------------------------------------------------------------------
	public static MessageDispatcher GetDispatcher() => instance.Internal_GetDispatcher("");
	public static MessageDispatcher GetDispatcher(string scope) => instance.Internal_GetDispatcher(scope);
	private MessageDispatcher Internal_GetDispatcher(string scope)
	{
		if(m_scopes.TryGetValue(scope, out var dispatcher))
			return dispatcher;
		
		m_scopes[scope] = new MessageDispatcher(scope);
		return m_scopes[scope];
	}
	//--------------------------------------------------------------------------
	// Dispatch
	//
	// Unscoped dispatch, will dispatch to all unscoped subscribers
	// Use GetDispatcher for scoped dispatching
	//
	//--------------------------------------------------------------------------
	public static void Dispatch<MessageType>(params object[] args) where MessageType : System.Delegate => instance.Internal_DispatchEvent(typeof(MessageType).GetHashCode(), args);
	public static void Dispatch(System.Type messageType, params object[] args) => instance.Internal_DispatchEvent(messageType.GetHashCode(), args);
	private void Internal_DispatchEvent(int type, params object[] args)
	{
		if(!m_scopes.ContainsKey(""))Internal_GetDispatcher("");
		m_scopes[""].Dispatch(type, args);
	}

	//--------------------------------------------------------------------------
	// Subscribe
	//--------------------------------------------------------------------------
	public static void Subscribe<MessageType>(MessageType subscriber) where MessageType : System.Delegate => instance.Internal_Subscribe("", subscriber);
	public static void Subscribe<MessageType>(string scope, MessageType subscriber) where MessageType : System.Delegate => instance.Internal_Subscribe(scope, subscriber);
	private void Internal_Subscribe<MessageType>(string scope, MessageType subscriber) where MessageType : System.Delegate
	{
		if(!m_scopes.ContainsKey(scope))Internal_GetDispatcher(scope);
		m_scopes[scope].Subscribe(subscriber);
	}

	//--------------------------------------------------------------------------
	// Unsubscribe
	//--------------------------------------------------------------------------
	public static void Unsubscribe<MessageType>(MessageType subscriber) where MessageType : System.Delegate => instance.Internal_Unsubscribe("", subscriber);
	public static void Unsubscribe<MessageType>(string scope, MessageType subscriber) where MessageType : System.Delegate => instance.Internal_Unsubscribe(scope, subscriber);
	private void Internal_Unsubscribe<MessageType>(string scope, MessageType subscriber) where MessageType : System.Delegate
	{
		if(!m_scopes.ContainsKey(scope))return ;
		m_scopes[scope].Unsubscribe(subscriber);
	}
}

//------------------------------------------------------------------------------
// IMessageDispatcher
//
// Interface for the message dispatcher, handy for making proxy dispatchers.
//
//------------------------------------------------------------------------------
public interface IMessageDispatcher
{
	public void Dispatch<MessageType>(params object[] args) where MessageType : System.Delegate;
	public void Dispatch(System.Type messageType, params object[] args);
	public void Subscribe<MessageType>(MessageType subscriber) where MessageType : System.Delegate;
	public void Unsubscribe<MessageType>(MessageType subscriber) where MessageType : System.Delegate;
}

//------------------------------------------------------------------------------
// MessageDispatcher
//
// The message dispatcher is used to dispatch messages to the correct handlers.
//
//------------------------------------------------------------------------------
public class MessageDispatcher : IMessageDispatcher
{
	//--------------------------------------------------------------------------
	// Properties
	public string Scope {get; private set;}

	//--------------------------------------------------------------------------
	// Subscribers
	private Dictionary<int, HashSet<System.Delegate>> m_subscribers = new();

	//--------------------------------------------------------------------------
	// Cache
	private List<System.Delegate> m_delegateList = new(8);

	//--------------------------------------------------------------------------
	// Constructor
	//--------------------------------------------------------------------------
	public MessageDispatcher(string scope)
	{
		Scope = scope;
	}

	//--------------------------------------------------------------------------
	// Subscribe
	//--------------------------------------------------------------------------
	public void Subscribe<MessageType>(MessageType subscriber) where MessageType : System.Delegate
	{
		// Find message type set
		int type = subscriber.GetType().GetHashCode();
		if(!m_subscribers.TryGetValue(type, out var messageSet))
		{
			// First time we see this message type, create a new set
			messageSet = new HashSet<System.Delegate>();
			m_subscribers[type] = messageSet;
		}

		// Add listener
		Debug.Assert(!messageSet.Contains(subscriber as System.Delegate), "Listener added twice! [" + subscriber.GetType() + "]");
		messageSet.Add(subscriber as System.Delegate);
	}

	//--------------------------------------------------------------------------
	// Unsubscribe
	//--------------------------------------------------------------------------
	public void Unsubscribe<MessageType>(MessageType subscriber) where MessageType : System.Delegate
	{
		// Find message type set
		int type = subscriber.GetType().GetHashCode();
		if(m_subscribers.TryGetValue(type, out var messageSet))
		{
			messageSet.Remove(subscriber as System.Delegate);
			if(messageSet.Count == 0)
				m_subscribers.Remove(type);
		}
	}
	
	//--------------------------------------------------------------------------
	// DispatchEvent
	//--------------------------------------------------------------------------
	public void Dispatch<MessageType>(params object[] args) where MessageType : System.Delegate => Internal_DispatchEvent(typeof(MessageType).GetHashCode(), args);
	public void Dispatch(System.Type messageType, params object[] args) => Internal_DispatchEvent(messageType.GetHashCode(), args);
	public void Dispatch(int type, params object[] args) => Internal_DispatchEvent(type, args);
	private void Internal_DispatchEvent(int type, params object[] args)
	{
		if(m_subscribers.TryGetValue(type, out var messageSet))
		{
			if(messageSet.Count > 0)
			{
				m_delegateList.Clear();
				foreach(System.Delegate dg in messageSet)m_delegateList.Add(dg);

				for(int i = 0; i < m_delegateList.Count; i++)
				{
					try
					{
						m_delegateList[i].Method.Invoke(m_delegateList[i].Target, args);
					}
					catch(System.Exception e)
					{
						Debug.Assert(false, "Error dispatching message: " + e.Message);
					}
				}
			}
		}
	}
}
