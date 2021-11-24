namespace Sensor.Routines
{
    public interface ITimerRoutine
    {
        void NextFloorLevel();
        void StopAtFloorLevel();
        void TimerBetweenScanning();
    }
}