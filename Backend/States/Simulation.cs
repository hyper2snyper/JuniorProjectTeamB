﻿using JuniorProject.Backend.Agents;
using JuniorProject.Backend.Helpers;
using JuniorProject.Backend.WorldData;
using System.Drawing;
using System.IO;


namespace JuniorProject.Backend.States
{
    internal class Simulation : IState
    {
        World world;
        bool paused = true;

        ulong lastTick;
        ulong timePerTick = (10000) * 100; //Ticks are in units of 100 nanoseconds. 10,000 in a millisecond.
        ulong tickCount = 0;

        public void CreateWorld(string seed, float freq, float amp, int octaves, float seaLevel, float treeLine)
        {
            world = new World();
            ClientCommunicator.UpdateData<string>("LoadingMessage", "Generating World");
            world.GenerateWorld(32, new Vector2Int(1024, 1024), seed, freq, amp, octaves, seaLevel, treeLine);
        }

        public void LoadFromFile(string filename)
        {
            Map map = new Map(); //This loads biomes
            Serializer serializer = new Serializer(filename);
            serializer.arbitraryPostRead += (ref FileStream f) =>
            {
                Stream stream = new MemoryStream();
                f.CopyTo(stream);
                stream.Seek(0, SeekOrigin.Begin);
                ClientCommunicator.UpdateData<string>("LoadingMessage", "Loading Image From File");
                map.LoadMap((Bitmap)Bitmap.FromStream(stream));

            };
            ClientCommunicator.UpdateData<string>("LoadingMessage", "Loading From File");
            Unit.LoadUnitTemplates();
            Dictionary<Type, List<Serializable>> loadedObjects = serializer.Load();
            world = (World)loadedObjects[typeof(World)][0];
            world.LoadWorld(map);
        }

        public IState Loop()
        {
            if (paused) return this;

            ulong currentTick = (uint)DateTime.Now.Ticks;
            if ((lastTick + timePerTick) > currentTick) return this;
            Debug.RePrint($"Tick count [{tickCount}]");
            tickCount++;
            lastTick = currentTick;
            world.Update(tickCount);
            return this;
        }

        public void EnterState()
        {
            Debug.Print("Entered Simulation State.");
            lastTick = (uint)DateTime.Now.Ticks;
            ClientCommunicator.RegisterAction("TogglePause", TogglePause);
            ClientCommunicator.RegisterAction("Step", Step);
            ClientCommunicator.RegisterData<bool>("Paused", paused);

        }

        public void TogglePause()
        {
            paused = !paused;
            ClientCommunicator.UpdateData<bool>("Paused", paused);
        }

        public void Step()
        {
            if (!paused) return;
            tickCount++;
            world.Update(tickCount);
        }

        public void ExitState()
        {
            world.FreeWorld();
            ClientCommunicator.UnregisterAction("TogglePause");
            ClientCommunicator.UnregisterAction("Step");
            ClientCommunicator.UnregisterData("Paused");
        }

    }
}