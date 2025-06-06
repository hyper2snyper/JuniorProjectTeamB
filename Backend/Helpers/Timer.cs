﻿using System.Numerics;

namespace JuniorProject.Backend.Helpers
{
    public class Timer<T> where T : INumber<T>
    {
        T start;
        readonly T delay;

        public Timer(T start, T delay)
        {
            this.start = start;
            this.delay = delay;
        }

        public bool Tick(T current)
        {
            if (current >= (start + delay))
            {
                start = current;
                return true;
            }
            return false;
        }

    }
}
