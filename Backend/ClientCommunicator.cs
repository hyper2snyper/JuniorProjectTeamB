namespace JuniorProject.Backend
{
	public class ClientCommunicator //Technically speaking, this is an inter-thread communicator.
	{
		static ClientCommunicator _clientCommunicator; //Singleton
		static ClientCommunicator communicator
		{
			get
			{
				_clientCommunicator ??= new ClientCommunicator();
				return _clientCommunicator;
			}
		}
		
		struct ArgumentBundle
		{
			public Delegate @delegate;
			public Type? argType;

			public ArgumentBundle(Delegate @delegate, Type? argType = null)
			{
				this.@delegate = @delegate;
				this.argType = argType;
			}
		}

		struct CallingBundle //Used to track the action to call
		{
			public string name;
			public unsafe bool* done; //This will be done unless there is another thread CallActionWaitFor in a while loop waiting for this action to be completed.
			public object? arg = null;

			public CallingBundle(string name)
			{
				this.name = name;
				unsafe { done = null; }
			}
			public CallingBundle(string name, object arg)
			{
				this = new CallingBundle(name);
				this.arg = arg;
			}

		}

		private int backendThread;
		private Dictionary<string, ArgumentBundle> registeredActions;
		private Queue<CallingBundle> callingQue;


		struct DataBundle
		{
			public object o;
			public Type t;
		}

		private Dictionary<string, DataBundle> storedData;


		private ClientCommunicator()
		{
			registeredActions = new Dictionary<string, ArgumentBundle>();
			callingQue = new Queue<CallingBundle>();
			storedData = new Dictionary<string, DataBundle>();
			backendThread = Thread.CurrentThread.ManagedThreadId;
		}

		//Action Controls

		public static void RegisterAction(string name, Delegate @delegate)
		{
			ArgumentBundle argumentBundle = new ArgumentBundle(@delegate);
			lock (communicator.registeredActions)
			{
				communicator.registeredActions.Add(name, argumentBundle);
			}
		}

		public static void RegisterAction<T>(string name, Delegate @delegate)
		{
			ArgumentBundle argumentBundle = new ArgumentBundle(@delegate, typeof(T));
			argumentBundle.argType = typeof(T);	
			lock (communicator.registeredActions)
			{
				communicator.registeredActions.Add(name, argumentBundle);
			}
		}

		public static void UnregisterAction(string name)
		{
			if (!communicator.registeredActions.ContainsKey(name)) return;
			lock (communicator.registeredActions)
			{
				communicator.registeredActions.Remove(name);
			}
		}

		static bool CanCallAction(string name)
		{
			if (!communicator.registeredActions.ContainsKey(name))
			{
				Debug.Print($"{name} was attempted to be called in CallAction, but there is no callback registered with that name.");
				return false;
			}
			if (Thread.CurrentThread.ManagedThreadId == communicator.backendThread)
			{
				Debug.Print($"{name} was called in CallAction on the same thread as the Backend loop.");
				return false;
			}
			return true;
		}

		static bool CanCallAction<T>(string name)
		{
			if (!CanCallAction(name)) return false;
			ArgumentBundle argumentBundle = communicator.registeredActions[name];
			if (argumentBundle.argType != typeof(T))
			{
				Debug.Print($"{name} was called in CallAction<{typeof(T).Name}>, however, the argument passed was of type {argumentBundle.argType.Name}.");
				return false;
			}
			return true;
		}

		static void _CallAction(CallingBundle calling)
		{
			lock (communicator.callingQue)
			{
				communicator.callingQue.Enqueue(calling); //Queues the action by name;
			}
		}

		public static void CallAction(string name)
		{
			if (!CanCallAction(name)) return;
			CallingBundle calling = new CallingBundle(name);
			_CallAction(calling);
		}

		public static void CallAction<T>(string name, T arg)
		{
			if (!CanCallAction<T>(name)) return;
			CallingBundle calling = new CallingBundle(name, arg);
			_CallAction(calling);
		}

		public static void CallActionWaitFor(string name)
		{
			if (!CanCallAction(name)) return;
			CallingBundle calling = new CallingBundle(name);
			unsafe
			{
				bool finished = false;
				lock (communicator.callingQue)
				{
					calling.done = &finished;
					_CallAction(calling);
				}
				while (!(finished)) ; //Todo, have a default max wait with options in case of infinite loop
			}
		}

		public static void CallActionWaitFor<T>(string name, T arg)
		{
			if (!CanCallAction(name)) return;
			CallingBundle calling = new CallingBundle(name, arg);
			unsafe
			{
				bool finished = false;
				lock (communicator.callingQue)
				{
					calling.done = &finished;
					_CallAction(calling);
				}
				while (!(finished)) ; //Todo, have a default max wait with options in case of infinite loop
			}
		}

		//Data Storage

		public static void RegisterData<T>(string name, object o)
		{
			DataBundle data;
			data.o = o;
			data.t = typeof(T);
			lock (communicator.storedData)
			{
				communicator.storedData[name] = data;
			}
		}

		public static void UnregisterData(string name)
		{
			if (!communicator.storedData.ContainsKey(name)) return;
			lock (communicator.storedData)
			{
				communicator.storedData.Remove(name);
			}
		}


		public static T GetData<T>(string name)
		{
			if (!communicator.storedData.ContainsKey(name))
			{
				Debug.Print($"Variable {name} was attempted to be retrieved from GetData<T>() but was not stored.");
				return default(T);
			}
			lock (communicator.storedData)
			{
				DataBundle data = communicator.storedData[name];
				if (typeof(T) != data.t)
				{
					Debug.Print($"Variable {name} was attempted to be retrieved with the type of {typeof(T).Name} when it is actually a {data.t.Name}");
					return default(T);
				}
				return (T)data.o;
			}
		}

		public static void UpdateData<T>(string name, object newData)
		{
			if (!communicator.storedData.ContainsKey(name))
			{
				Debug.Print($"Variable {name} was attempted to be updated, but was not registered.");
				return;
			}
			lock (communicator.storedData)
			{
				DataBundle data;
				data.t = typeof(T);
				data.o = newData;
				communicator.storedData[name] = data;
			}
		}

		public static void ProcessActions()
		{
			if (communicator.callingQue.Count() == 0) return;
			lock (communicator.callingQue)
			{
				while (communicator.callingQue.Count() > 0)
				{
					CallingBundle c = communicator.callingQue.Dequeue();
					unsafe
					{
						if(c.arg == null)
						{
							communicator.registeredActions[c.name].@delegate.DynamicInvoke();
						} else
						{
							communicator.registeredActions[c.name].@delegate.DynamicInvoke(c.arg);
						}
						if (c.done != null)
						{
							*c.done = true;
						}
					}
				}
			}
		}

	}
}
