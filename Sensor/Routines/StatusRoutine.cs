using System;
using System.Collections.Generic;
using ElevatorModels;
using Logger;

namespace Sensor
{
    public class StatusRoutine
    {
        public ILogger Logger { get; }

        public StatusRoutine(ILogger logger)
        {
            Logger = logger;
        }

        internal void DeterimineOrderOfStops(Queue<Button> queue)
        {
            var list = new List<int>();
            foreach (var stops in queue)
                list.Add(stops.ButtonPress);

            Logger.Write($"Current order of stops:{String.Join(',',list)}");
        }

        internal void NextFloor()
        {
            Logger.Write($"Starting moving at: {DateTime.UtcNow}");
        }

        internal void PassedFloor(int currentFloor)
        {
            Logger.Write($"Passed floor level {currentFloor} at time: {DateTime.UtcNow}");
        }

        internal void StoppingAtDetinationFloor(int destinationFloor)
        {
            Logger.Write($"Stopping at destination floor: " +
                $"{destinationFloor} at time: {DateTime.UtcNow}");
        }

        internal void LeavingDetinationFloor(int destinationFloor)
        {
            Logger.Write($"Leaving destination floor: " +
                $"{destinationFloor} at time: {DateTime.UtcNow}");
        }

        internal void DetinationFloor(int destinationFloor)
        {
            Logger.Write($"Arrive at floor: {destinationFloor} at time: {DateTime.UtcNow}");
        }

        internal void Direction(string direction)
        {
            Logger.Write($"Elevator is currently {direction} ");
        }

        internal void DetermineCurrentFloor(int currentFloor)
        {
            Logger.Write($"Current Floor: {currentFloor}");
        }

        internal void DetermineNextFloor(int nextFloor)
        {
            Logger.Write($"Next Floor: {nextFloor}");
        }

        internal void WaitingForAdditionalRequests()
        {
            Logger.Write($"Waiting on more requests to enqueue");
        }

        internal void DetermineElevatorStatus(Elevator elevator)
        {
            Logger.Write($"Current Elavator State is: {elevator.State} " +
                $"and current direction is: {elevator.Direction}");
        }

        internal void ShutDownRoutine()
        {
            Logger.Write("Shutdown routine initiated");
        }

        internal void MaxWeighRoutine()
        {
            Logger.Write("Max weight routine initiated." +
                " Elevator no longer taking requests");
        }
    }
}
