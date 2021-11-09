using ElevatorModels;
using Logger;
using Sensor.Routines;

namespace Sensor
{
    public abstract class SensorBaseClass
    {
        public SensorBaseClass()
        {
        }

        public static readonly Floor _floor = Floor.GetFloorInformation();
        public static readonly QueueRoutine _queue = QueueRoutine.GetQueueRoutine();

    }
}
