using ElevatorModels;
using Logger;
using Sensor.Interfaces;
using System;
using System.Linq;
using static ElevatorModels.Button;

namespace Sensor.Routines
{
    public class KeyPadRoutine : IKeyPadRoutine
    {
        public KeyPadRoutine(ILogger logger)
        {
            Logger = logger;
        }

        public KeyPadRoutine(ILogger logger, IQueue keyPadRoutine)
        {
            Logger = logger;
            //_floor = floor;
            _floor.ResetForTesting();
        }

        public static readonly QueueRoutine _queue = QueueRoutine.GetQueueRoutine();
        public static readonly Floor _floor = Floor.GetFloorInformation();

        public ILogger Logger { get; }
        private readonly char upwardRequest = 'U';
        private readonly char downwardRequest = 'D';
        private readonly char shutdownRequest = 'Q';

        public void AddElevatorRequest(string input)
        {
            CheckForShutDownSignal(input);

            if (!_queue.disallowFurtherEnqueuing)
            {
                Logger.Write("Button Pressed.. Starting Proccess");
                Logger.Write($"Adding floor request {input} in the queue.");
                AddToDesignatedQueue(input);
            }
        }

        private void CheckForShutDownSignal(string input)
        {
            if(input.Contains(shutdownRequest))
                _queue.disallowFurtherEnqueuing = true;
        }

        private void AddToDesignatedQueue(string request)
        {
            var button = ParseRequestValues(request);
            InsertToQueue(button);
        }

        private Button ParseRequestValues(string request)
        {
            var requestButton = new Button
            {
                ButtonPress = int.Parse(request
                .Trim(new char[] { upwardRequest, downwardRequest })),
                Action = new string(request.ToCharArray()
                .Where(c => !char.IsDigit(c)).ToArray())
            };

            return requestButton;
        }

        private void InsertToQueue(Button button)
        {
            var action = Enum.Parse(typeof(ButtonAction), button.Action);
            switch (action)
            {
                case ButtonAction.Up:
                    _queue.EnqueueUpwardRequest(button);
                    break;
                case ButtonAction.Internal:
                    CalculateInternalQueue(button);
                    break;
                case ButtonAction.Down:
                    _queue.EnqueueDownwardRequest(button);
                    break;
                default:
                    throw new Exception("incorrect button action type");
            }
        }

        private void CalculateInternalQueue(Button button)
        {
            if (_floor.CurrentFloor <
            button.ButtonPress)
                _queue.EnqueueUpwardRequest(button);
            else
            {
                _queue.EnqueueDownwardRequest(button);
            }
        }
    }
}
