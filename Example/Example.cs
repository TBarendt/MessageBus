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

public class Example
{
	//--------------------------------------------------------------------------
	// Messages
	public delegate void ExampleMessage();
	public delegate void ExampleArguments(int x, int y);

	//--------------------------------------------------------------------------
	// Constructor
	//--------------------------------------------------------------------------
	public Example()
	{
		// Example of how to use it unscoped
		UnscopedExample();

		// Example on how to use it with a scope
		ScopedExample();

		// Example showing weak references
		WeakExample();

	}

	//--------------------------------------------------------------------------
	// UnscopedExample
	//--------------------------------------------------------------------------
	private void UnscopedExample()
	{
		//----------------------------------------------------------------------
		// Subscribe to a few messages
		MessageBus.Subscribe<ExampleMessage>(OnExampleMessage);
		MessageBus.Subscribe<ExampleArguments>(OnExampleArguments);

		//----------------------------------------------------------------------
		// Dispatch messages
		MessageBus.Dispatch<ExampleMessage>();
		MessageBus.Dispatch<ExampleArguments>(100, 200);

		//----------------------------------------------------------------------
		// Remove subscriptions
		MessageBus.Unsubscribe<ExampleMessage>(OnExampleMessage);
		MessageBus.Unsubscribe<ExampleArguments>(OnExampleArguments);
	}

	
	//--------------------------------------------------------------------------
	// ScopedExample
	//--------------------------------------------------------------------------
	private void ScopedExample()
	{
		//----------------------------------------------------------------------
		// Subscribe to a few messages
		MessageBus.Subscribe<ExampleMessage>("ExampleScope", OnExampleMessage);
		MessageBus.Subscribe<ExampleArguments>("ExampleScope", OnExampleArguments);


		//----------------------------------------------------------------------
		// Dispatch messages
		var scopedDispatcher = MessageBus.GetDispatcher("ExampleScope");

		scopedDispatcher.Dispatch<ExampleMessage>();
		scopedDispatcher.Dispatch<ExampleArguments>(64, 128);

		//----------------------------------------------------------------------
		// Remove subscriptions
		MessageBus.Unsubscribe<ExampleMessage>("ExampleScope", OnExampleMessage);
		MessageBus.Unsubscribe<ExampleArguments>("ExampleScope", OnExampleArguments);
	}
	
	//--------------------------------------------------------------------------
	// OnExampleMessage
	//--------------------------------------------------------------------------
	private void OnExampleMessage()
	{
		Console.WriteLine("OnExampleMessage");
	}

	//--------------------------------------------------------------------------
	// OnExampleArguments
	//--------------------------------------------------------------------------
	private void OnExampleArguments(int x, int y)
	{
		Console.WriteLine("ExampleArguments with " + x + " and " + y);
	}

	//--------------------------------------------------------------------------
	// WeakExample
	//--------------------------------------------------------------------------
	private void WeakExample()
	{
		//----------------------------------------------------------------------
		// Subscribe 
		SubscribeWeak();

		//----------------------------------------------------------------------
		// Dispatch messages
		MessageBus.Dispatch<ExampleMessage>();
		
		//----------------------------------------------------------------------
		// Force garbage collection
		GC.Collect();

		//----------------------------------------------------------------------
		// Dispatch messages
		MessageBus.Dispatch<ExampleMessage>();
	}

	//--------------------------------------------------------------------------
	// SubscribeWeak
	//--------------------------------------------------------------------------
	private void SubscribeWeak()
	{
		WeakExample weakExample = new WeakExample();
		MessageBus.Subscribe<ExampleMessage>(weakExample.OnExampleMessage);
	}
}

public class WeakExample
{
	//--------------------------------------------------------------------------
	// Destructor
	//--------------------------------------------------------------------------
	~WeakExample()
	{
		Console.WriteLine("Weak: Destructor");
	}
	//--------------------------------------------------------------------------
	// OnExampleMessage
	//--------------------------------------------------------------------------
	public void OnExampleMessage()
	{
		Console.WriteLine("Weak: OnExampleMessage");
	}
}