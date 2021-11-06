using ElevatorModels;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static ElevatorModels.Button;

namespace Sensor.Routines
{
    public class KeyPadRoutine
    {
        public KeyPadRoutine(ILogger logger)
        {
            Logger = logger;
            _status = new StatusRoutine(logger);
        }

        public KeyPadRoutine(ILogger logger, Floor floor)
        {
            Logger = logger;
            _floor = floor;
            _status = new StatusRoutine(logger);
            _floor.ResetForTesting();
            _elevator.ShutDownSignal = false;
        }

        private static readonly Elevator _elevator = Elevator.GetElevatorInformation();
        private readonly Floor _floor = Floor.GetFloorInformation();
        private static readonly QueueRoutine _queue = QueueRoutine.GetQueueRoutine();

        public ILogger Logger { get; }
        private readonly StatusRoutine _status;
        private readonly char upwardRequest = 'U';
        private readonly char downwardRequest = 'D';
        private readonly char shutdownRequest = 'Q';

        public void AddElevatorRequest(string input)
        {
            CheckForShutDownSignal(input);

            if (!_elevator.ShutDownSignal)
            {
                Logger.Write("Button Pressed.. Starting Proccess");
                Logger.Write($"Adding floor request {input} in the queue.");
                AddToDesignatedQueue(input);
            }
        }

        private void CheckForShutDownSignal(string input)
        {
            if (input.Contains(shutdownRequest))
            {
                _elevator.ShutDownSignal = true;
                _status.ShutDownRoutine();
            }
        }

        private void AddToDesignatedQueue(string request)
        {
            ParseRequestValues(request, out int destinationLevel, out Button button);

            InsertToQueue(destinationLevel, button);
        }

        private void ParseRequestValues(string request, out int destinationLevel, out Button button)
        {
            destinationLevel = int.Parse(request
                .Trim(new char[] { upwardRequest, downwardRequest }));
            var action = new string(request.ToCharArray()
                .Where(c => !char.IsDigit(c)).ToArray());

            button = new Button
            {
                ButtonPress = destinationLevel,
                Action = action
            };
        }

        private void InsertToQueue(int destinationLevel, Button button)
        {
            var action = Enum.Parse(typeof(ButtonAction), button.Action);
            switch (action)
            {
                case ButtonAction.Up:
                    _queue.EnqueueUpwardRequest(button);
                    break;
                case ButtonAction.Internal:
                    CalculateInternalQueue(button, destinationLevel);
                    break;
                case ButtonAction.Down:
                    _queue.EnqueueDownwardRequest(button);
                    break;
                default:
                    throw new Exception("incorrect button action type");
            }
        }

        private void CalculateInternalQueue(Button button, int destinationLevel)
        {
            if (_floor.CurrentFloor <
            destinationLevel)
                _queue.EnqueueUpwardRequest(button);
            else
            {
                _queue.EnqueueDownwardRequest(button);
            }
        }
    }
}
