using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend
{
	public class ClientCommunicator
	{
		static ClientCommunicator _clientCommunicator;
		static ClientCommunicator communicator { 
			get { 
				_clientCommunicator ??= new ClientCommunicator();
				return _clientCommunicator;
			} 
		}
		public delegate void Callback();
		public delegate void CallbackDone(ref bool done);

		struct CallingBundle
		{
			public string name;
			public unsafe bool* done;

			public CallingBundle(string name)
			{
				this.name = name;
				unsafe { done = null; }
			}

		}

		private Dictionary<string, Callback> registeredActions;
		private Queue<CallingBundle> callingQue;


		private ClientCommunicator()
		{
			registeredActions = new Dictionary<string, Callback>();
			callingQue = new Queue<CallingBundle>();
		}

		public static void RegisterAction(string name, Callback c)
		{
			lock(communicator.registeredActions)
			{
				communicator.registeredActions.Add(name, c);
			}
		}

		public static void UnregisterAction(string name)
		{
			if (!communicator.registeredActions.ContainsKey(name)) return;
			lock(communicator.registeredActions)
			{
				communicator.registeredActions.Remove(name);
			}
		}

		static void _CallAction(CallingBundle calling)
		{
			if(!communicator.registeredActions.ContainsKey(calling.name))
			{
				Console.WriteLine($"{calling.name} was attempted to be called in _CallAction, but there is no callback registered with that name.");
				return;
			}
			lock(communicator.callingQue)
			{
				communicator.callingQue.Enqueue(calling); //Queues the action by name;
			}
		}

		public static void CallAction(string name)
		{
			CallingBundle calling = new CallingBundle(name);
			_CallAction(calling);
		}

		public static void CallActionWaitFor(string name)
		{
			CallingBundle calling = new CallingBundle(name);
			unsafe
			{
				
				bool finished = false;
				lock (communicator.callingQue)
				{
					calling.done = &finished;
					_CallAction(calling);
				}
				while (!(finished)) ;
			}
		}

		public static void ProcessActions()
		{
			if (communicator.callingQue.Count() == 0) return;
			lock(communicator.callingQue)
			{
				CallingBundle c = communicator.callingQue.Dequeue();
				communicator.registeredActions[c.name].Invoke();
				unsafe
				{
					if(c.done != null)
					{
						*c.done = true;
					}
				}
			}
		}

	}
}
