using System.Threading;

namespace Sensor.Routines
{
    public class TimerRoutine : ITimerRoutine
    {
        private readonly int timeToReachNextFloorLevel = 3;
        private readonly int timeToStopAtDestinationFloorLevel = 1;
        private readonly int timeToWaitBetweenIteration = 3;
        private readonly int milisecondsMultiplier = 1000;

        public void NextFloorLevel() => Thread.Sleep(timeToReachNextFloorLevel * milisecondsMultiplier);

        public void StopAtFloorLevel() => Thread.Sleep(timeToStopAtDestinationFloorLevel * milisecondsMultiplier);
        
        public void TimerBetweenScanning() => Thread.Sleep(timeToWaitBetweenIteration * milisecondsMultiplier);
    }
}
