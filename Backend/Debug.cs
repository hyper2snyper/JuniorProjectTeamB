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
    }
}
