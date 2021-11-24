using ElevatorModels;
using Logger;
using Sensor.Interfaces;
using System.Linq;
using static ElevatorModels.Button;

namespace Sensor.Routines
{
    public class KeyPadRoutine : IKeyPadRoutine
    {

        public KeyPadRoutine(ILogger logger)
        {
            Logger = logger;
            _status = new StatusRoutine(logger);
        }

        public KeyPadRoutine(ILogger logger, IQueueRoutine queue, IFloor floor)
        {
            Logger = logger;
            _queue = queue;
            _floor = floor;
            _status = new StatusRoutine(logger);
        }

        public readonly IQueueRoutine _queue = QueueRoutine.GetQueueRoutine();
        public readonly IFloor _floor = Floor.GetFloorInformation();
        private readonly StatusRoutine _status;

        public ILogger Logger { get; }

        const char upwardRequest = 'U';
        const char downwardRequest = 'D';
        const char shutdownRequest = 'Q';
        const char maxWeight = 'M';
        const char resetMaxWeight = 'R';
        const int LowestLevel = 0;
        const int HighestLevel = 12;
        private bool maxWeightRoutine = false;

        public void AddElevatorRequest(string input)
        {
            CheckForShutDownSignal(input);

            if (!CheckForMaxWeighRoutine(input))
            {
                if (!_queue.DisallowFurtherEnqueuing)
                {
                    _status.ButtonPressed();
                    ValidateRequest(input);
                }
            }
        }

        private bool CheckForMaxWeighRoutine(string input)
        {
            bool containsKeyValue = false;

            if (input.Contains(maxWeight))
            {
                containsKeyValue = true;
                maxWeightRoutine = true;
                _status.MaxWeighRoutine(true);
            }
            else if (input.Contains(resetMaxWeight))
            {
                containsKeyValue = true;
                maxWeightRoutine = false;
                _status.MaxWeighRoutine(false);
            }

            return containsKeyValue;
        }

        private void CheckForShutDownSignal(string input)
        {
            if(input.Contains(shutdownRequest))
                _queue.DisallowFurtherEnqueuing = true;
        }

        private void ValidateRequest(string request)
        {
            var button = ParseRequestValues(request);

            if (button.ButtonPress >= LowestLevel && button.ButtonPress <= HighestLevel)
                RouteToQueue(button);
            else
                _status.InvalidIncomingRequest(button.ButtonPress, LowestLevel, HighestLevel);
        }

        private void RouteToQueue(Button button)
        {
            if (!maxWeightRoutine)
                InsertToQueue(button);
            else if (maxWeightRoutine && button.Action
                == ButtonAction.Internal.ToString())
                InsertToQueue(button);
            else
                _status.RejectedOutsideRequest(button);
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
            _status.AddingRequestToQueue(button);

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
