using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend
{
    public class ClientCommunicator
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
        public delegate void Callback();

        struct CallingBundle //Used to track the action to call
        {
            public string name;
            public unsafe bool* done; //This will be done unless there is another thread CallActionWaitFor in a while loop waiting for this action to be completed.

            public CallingBundle(string name)
            {
                this.name = name;
                unsafe { done = null; }
            }

        }

        private int backendThread;
        private Dictionary<string, Callback> registeredActions;
        private Queue<CallingBundle> callingQue;


        struct DataBundle
        {
            public object o;
            public Type t;
        }

        private Dictionary<string, DataBundle> storedData;


        private ClientCommunicator()
        {
            registeredActions = new Dictionary<string, Callback>();
            callingQue = new Queue<CallingBundle>();
            storedData = new Dictionary<string, DataBundle>();
            backendThread = Thread.CurrentThread.ManagedThreadId;
        }

        //Action Controls

        public static void RegisterAction(string name, Callback c)
        {
            lock (communicator.registeredActions)
            {
                communicator.registeredActions.Add(name, c);
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
                    communicator.registeredActions[c.name].Invoke();
                    unsafe
                    {
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
