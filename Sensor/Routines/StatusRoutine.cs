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

        public void NextFloor()
        {
            Logger.Write($"Starting moving at: {DateTime.UtcNow}");
        }

        public void PassedFloor(int currentFloor)
        {
            Logger.Write($"Passed floor level {currentFloor} at time: {DateTime.UtcNow}");
        }

        public void StoppingAtDetinationFloor(int destinationFloor)
        {
            Logger.Write($"Stopping at destination floor: " +
                $"{destinationFloor} at time: {DateTime.UtcNow}");
        }

        public void LeavingDetinationFloor(int destinationFloor)
        {
            Logger.Write($"Leaving destination floor: " +
                $"{destinationFloor} at time: {DateTime.UtcNow}");
        }

        public void DetinationFloor(int destinationFloor)
        {
            Logger.Write($"Arrive at floor: {destinationFloor} at time: {DateTime.UtcNow}");
        }

        public void Direction(string direction)
        {
            Logger.Write($"Elevator is currently {direction} ");
        }

        public void DetermineCurrentFloor(int currentFloor)
        {
            Logger.Write($"Current Floor: {currentFloor}");
        }

        public void DetermineNextFloor(int nextFloor)
        {
            Logger.Write($"Next Floor: {nextFloor}");
        }

        public void WaitingForAdditionalRequests()
        {
            Logger.Write($"Waiting on more requests to enqueue");
        }

        public void DetermineElevatorStatus(Elevator elevator)
        {
            Logger.Write($"Current Elavator State is: {elevator.State} " +
                $"and current direction is: {elevator.Direction}");
        }

        public void ShutDownRoutine()
        {
            Logger.Write("No remaining requests... exiting");
        }

        public void MaxWeighRoutine(bool max)
        {
            if(max)
            Logger.Write("Max weight routine initiated." +
                " Elevator no longer taking outside requests");
            else
                Logger.Write("Max weight routine ended." +
                " Elevator is taking outside requests");
        }

        public void RejectedOutsideRequest(Button button)
        {
            Logger.Write($"Outside request {button.ButtonPress} with " +
                $"action {button.Action} has been rejected since max" +
                $"weight routine is active");
        }

        public void InvalidIncomingRequest(int buttonPressed, int low, int high)
        {
            Logger.Write($"Incoming request {buttonPressed}, cannot be completed." +
                   $" Insert levels from {low} and {high}.");
        }

        public void ButtonPressed()
        {
            Logger.Write("Button Pressed.. Starting Proccess");
        }

        public void AddingRequestToQueue(Button button)
        {
            Logger.Write($"Adding floor request {button.ButtonPress}" +
                $" with action {button.Action} in the queue.");
        }

        public void SameLevelRequest(int buttonPress)
        {
            Logger.Write($"The incoming request: {buttonPress} " +
                $"matches the current floor level. Skipping request..");
        }
    }
}
