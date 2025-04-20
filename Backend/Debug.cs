using JuniorProject.Backend;
using JuniorProject.Backend.Helpers;
using JuniorProject.Backend.WorldData;
using JuniorProject.Frontend.Components;
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
        static bool rewriting = false;

        [Conditional("DEBUG")]
        public static void Print(string message)
        {
            string caller = Thread.CurrentThread.Name;
            Console.WriteLine($"[{DateTime.Now.TimeOfDay}][{caller}]: {message}");
            rewriting = false;
        }

        [Conditional("DEBUG")]
        public static void RePrint(string message)
        {
            string caller = Thread.CurrentThread.Name;
            if (rewriting)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
            }
            Console.WriteLine($"[{DateTime.Now.TimeOfDay}][{caller}]: {message}                     ");
            rewriting = true;
        }

        [Conditional("DEBUG")]
        public static void AddDebugCircle(string color, int x, int y)
        {
            string caller = Thread.CurrentThread.Name;
            if (color != "Red" && color != "Yellow" && color != "Green")
            {
                Console.WriteLine($"[{DateTime.Now.TimeOfDay}][{caller}]: Cannot add debug circle with color {color}");
                return;
            }

            World w = ClientCommunicator.GetData<World>("World");
            w.debugCircles.Add(new GenericDrawable(new Vector2Int(x, y), $"{color}DebugCircle", GenericDrawable.DrawableType.Debug, $"debugCircle:{x}-{y}"));
            w.RedrawAction.Invoke();
        }
    }
}
