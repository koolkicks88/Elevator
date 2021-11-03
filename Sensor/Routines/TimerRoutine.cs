using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Sensor.Routines
{
    internal class TimerRoutine
    {
        private readonly int timeToReachNextFloorLevel = 3;
        private readonly int timeToStopAtDestinationFloorLevel = 1;

           
        internal void NextFloorLevel() => Thread.Sleep(timeToReachNextFloorLevel * 1000);

        internal void StopAtFloorLevel() => Thread.Sleep(timeToStopAtDestinationFloorLevel * 1000);

        internal Stopwatch Timer()
        {
            var stopWatch = new Stopwatch();
            return stopWatch;
        }

        //if( currentTime.Second <= currentTime.AddSeconds(timeToReachNextFloorLevel).Second)
        //{
        //}
    }
}
