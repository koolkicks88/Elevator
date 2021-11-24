using ElevatorModels;
using System.Collections.Generic;

namespace Sensor.Routines
{
    public interface IQueueRoutine
    {
        bool DisallowFurtherEnqueuing { get; set; }
        bool ActiveRequest();
        Queue<Button> CurrentUpwardQueue();
        Queue<Button> CurrentDownwardQueue();
        Button DequeueDowanwardRequest();
        Button DequeueUpwardRequest();
        bool DownwardEmptyQueue();
        bool DownwardPeekQueue(out Button button);
        void EnqueueDownwardRequest(Button button);
        void EnqueueUpwardRequest(Button button);
        bool ReviseDownwardQueueOrder(int currentFloor);
        bool ReviseUpwardQueueOrder(int currentFloor);
        bool UpwardEmptyQueue();
        bool UpwardPeekQueue(out Button button);
    }
}