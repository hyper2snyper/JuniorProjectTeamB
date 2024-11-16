﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JuniorProject.Backend
{
	internal class BackendMain
	{
		public void BackendStart()
		{
			Console.WriteLine($"Back end thread started on thread {Thread.CurrentThread.ManagedThreadId}.");
			Thread.CurrentThread.Name = "backend";
			DatabaseManager.LoadDB("LocalData\\BackendDatabase.db");
			
		}
	}
}
