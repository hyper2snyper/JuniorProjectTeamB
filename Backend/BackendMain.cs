﻿using JuniorProject.Backend.WorldData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JuniorProject.Backend
{
	internal class BackendMain
	{

		World mainWorld;


		public void BackendStart()
		{
			Console.WriteLine($"Back end thread started on thread {Thread.CurrentThread.ManagedThreadId}.");
			Thread.CurrentThread.Name = "backend";
			DatabaseManager.LoadDB("LocalData\\BackendDatabase.db");
			mainWorld = new World();
			

		}
	}
}
