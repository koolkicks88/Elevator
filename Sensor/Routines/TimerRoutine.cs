using System.Threading;

namespace Sensor.Routines
{
    internal class TimerRoutine
    {
        private readonly int timeToReachNextFloorLevel = 3;
        private readonly int timeToStopAtDestinationFloorLevel = 1;
        private readonly int milisecondsMultiplier = 1000;
           
        internal void NextFloorLevel() => Thread.Sleep(timeToReachNextFloorLevel * milisecondsMultiplier);

        internal void StopAtFloorLevel() => Thread.Sleep(timeToStopAtDestinationFloorLevel * milisecondsMultiplier);
    }
}
