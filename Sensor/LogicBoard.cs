using Sensor.Routines;
using static ElevatorModels.Elevator;
using Logger;
using Sensor.Interfaces;
using ElevatorModels;

namespace Sensor
{
    public class LogicBoard : ILogicBoard
    {
        public LogicBoard(ILogger logger)
        {
            Logger = logger;
            _status = new StatusRoutine(logger);
            _timer = new TimerRoutine();
            _elevator = new Elevator();
        }

        public LogicBoard(ILogger logger, IQueueRoutine queue, 
            IFloor floor, ITimerRoutine timer)
        {
            Logger = logger;
            _queue = queue;
            _floor = floor;
            _elevator = new Elevator();
            _status = new StatusRoutine(logger);
            _timer = timer;
        }

        public readonly IQueueRoutine _queue = QueueRoutine.GetQueueRoutine();
        public readonly IFloor _floor = Floor.GetFloorInformation();
        private readonly Elevator _elevator;
        private readonly StatusRoutine _status;
        private readonly ITimerRoutine _timer;
        private int _currentFloor;

        public ILogger Logger { get; }

        public void StartProcess()
        {
            while (_queue.ActiveRequest() || ContinueFunctioning())
            {
                if (_queue.UpwardPeekQueue(out var _))
                    AdvanceLevels();

                if (_queue.DownwardPeekQueue(out var _))
                    DescendLevels();

                _status.WaitingForAdditionalRequests();
                SetIdleProperties();
                _timer.TimerBetweenScanning();
            }

            _queue.DisallowFurtherEnqueuing = false;
            _status.ShutDownRoutine();
        }

        private bool ContinueFunctioning()
        {
            bool continueScanning = true;

            if (_queue.DisallowFurtherEnqueuing)
                continueScanning = false;

            return continueScanning;
        }

        private bool CheckForSameLevelRequest(bool upwards = true)
        {
            bool sameLevelRequest = false;

            int destinationLevel = GetDestinationLevel(upwards);

            if (destinationLevel == _currentFloor
                && _elevator.State == ElevatorState.Stop.ToString())
            {
                if (upwards)
                    _queue.DequeueUpwardRequest();
                else
                    _queue.DequeueDowanwardRequest();

                _status.SameLevelRequest(destinationLevel);
                sameLevelRequest = true;
            }

            return sameLevelRequest;
        }

        private int GetDestinationLevel(bool upwards)
        {
            int destinationLevel;
            if (upwards)
            {
                _queue.UpwardPeekQueue(out var currentRequest);
                destinationLevel = currentRequest.ButtonPress;
            }
            else
            {
                _queue.DownwardPeekQueue(out var currentRequest);
                destinationLevel = currentRequest.ButtonPress;
            }

            return destinationLevel;
        }

        private bool EnqueueUpwardMissedFloorRequest()
        {
            bool missedFloorRequest = false;
            _queue.UpwardPeekQueue(out var currentRequest);

            if (currentRequest.ButtonPress < _currentFloor
                && _elevator.Direction == ElevatorDirection.Up.ToString())
            {
                DequeuAndReorderDownwardQueue(currentRequest);
                missedFloorRequest = true;
            }

            return missedFloorRequest;
        }

        private bool EnqueueDownwardMissedFloorRequest()
        {
            bool missedFloorRequest = false;
            _queue.DownwardPeekQueue(out var currentRequest);

            if (currentRequest.ButtonPress > _currentFloor
                   && _elevator.Direction == ElevatorDirection.Down.ToString())
            {
                DequeAndReorderUpwardQueue(currentRequest);
                missedFloorRequest = true;
            }

            return missedFloorRequest;
        }

        private void DequeAndReorderUpwardQueue(Button currentRequest)
        {
            _queue.DequeueDowanwardRequest();
            _queue.EnqueueUpwardRequest(currentRequest);
            _queue.ReviseUpwardQueueOrder(_currentFloor);
        }

        private void DequeuAndReorderDownwardQueue(Button currentRequest)
        {
            _queue.DequeueUpwardRequest();
            _queue.EnqueueDownwardRequest(currentRequest);
            _queue.ReviseDownwardQueueOrder(_currentFloor);
        }

        private void AdvanceLevels()
        {
            _currentFloor = _floor.GetCurrentFloor();

            while (_queue.UpwardEmptyQueue())
            {
                if (CheckForSameLevelRequest())
                    continue;

                SetUpwardProperties();

                if (EnqueueUpwardMissedFloorRequest())
                    continue;

                LogMovementStatus(_currentFloor);
                _floor.AscendSingleLevel(_currentFloor);
                _currentFloor = _floor.GetCurrentFloor();

                _queue.ReviseUpwardQueueOrder(_currentFloor);

                if (_queue.UpwardPeekQueue(out var _))
                    CheckForUpwardDestination();
            }
        }

        private void DescendLevels()
        {
            _currentFloor = _floor.GetCurrentFloor();

            while (_queue.DownwardEmptyQueue())
            {
                if (CheckForSameLevelRequest(upwards:false))
                    continue;

                SetDownwardProperties();

                if (EnqueueDownwardMissedFloorRequest())
                    continue;

                LogMovementStatus(_currentFloor);
                _floor.DescendSingleLevel(_currentFloor);
                _currentFloor = _floor.GetCurrentFloor();

                _queue.ReviseDownwardQueueOrder(_currentFloor);

                if (_queue.DownwardPeekQueue(out var _))
                    CheckForDownwardDestination();
            }
        }

        private bool CheckForUpwardDestination()
        {
            _queue.UpwardPeekQueue(out var button);
            var currentDestination = button.ButtonPress;

            if (_currentFloor == currentDestination) { 
                DequeueCurrentUpwardRequest(currentDestination);
                return true;
            }

            return false;
        }

        private void DequeueCurrentUpwardRequest(int floor)
        {
            LogDestination(floor);
            _queue.DequeueUpwardRequest();
        }

        private void CheckForDownwardDestination()
        {
            _queue.DownwardPeekQueue(out var button);
            var currentDestination = button.ButtonPress;

            if (_currentFloor == currentDestination)
                DequeueCurrentDownwardRequests(currentDestination);
        }

        private void DequeueCurrentDownwardRequests(int floor)
        {
            _queue.DequeueDowanwardRequest();
            LogDestination(floor);
        }

        private void LogDestination(int floor)
        {
            _status.DetinationFloor(floor);
            _status.StoppingAtDetinationFloor(floor);
            _timer.StopAtFloorLevel();
            ArrivedAtDestination();
        }

        internal void ArrivedAtDestination()
        {
            _elevator.State = ElevatorState.Stop.ToString();
            _elevator.Direction = ElevatorDirection.Idle.ToString();
            _status.DetermineElevatorStatus(_elevator);
        }

        private void SetIdleProperties()
        {
            _elevator.State = ElevatorState.Stop.ToString();
            _elevator.Direction = ElevatorDirection.Idle.ToString();
            _elevator.SetCurrentData(_elevator);
            _status.DetermineElevatorStatus(_elevator);
        }

        private void SetUpwardProperties()
        {
            _elevator.State = ElevatorState.Moving.ToString();
            _elevator.Direction = ElevatorDirection.Up.ToString();
            _elevator.SetCurrentData(_elevator);
            _status.DetermineElevatorStatus(_elevator);
        }

        private void SetDownwardProperties()
        {
            _elevator.State = ElevatorState.Moving.ToString();
            _elevator.Direction = ElevatorDirection.Down.ToString();
            _elevator.SetCurrentData(_elevator);
            _status.DetermineElevatorStatus(_elevator);
        }
        private void LogMovementStatus(int currentFloor)
        {
            var nextLevel = DeterminNextLevel(currentFloor);

            _status.NextFloor();
            _timer.NextFloorLevel();
            _status.DetermineCurrentFloor(_currentFloor);
            _status.DetermineNextFloor(nextLevel == -1 ? 0 : nextLevel);
            _status.PassedFloor(currentFloor);
        }

        private int DeterminNextLevel(int currentFloor)
        {
            int next;
            if (_elevator.Direction == ElevatorDirection.Up.ToString())
               next = currentFloor == 0 ? 1 : currentFloor + 1;
            else 
                next = currentFloor == 12 ? 11 : currentFloor - 1;

            return next;
        }
    }
}
