using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JuniorProject
{
	class Debug
	{

		static int backendThread;
		static int frontendThread;

		[Conditional("DEBUG")]
		public static void SetThreads(int backendThread, int frontendThread)
		{
			Debug.backendThread = backendThread;
			Debug.frontendThread = frontendThread;
		}
		
		[Conditional("DEBUG")]
		public static void Print(string message)
		{
			string caller = Thread.CurrentThread.Name;
			Console.WriteLine($"[{DateTime.Now.TimeOfDay}][{caller}]: {message}");
		}



	}
}
