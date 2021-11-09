using Sensor.Routines;
using static ElevatorModels.Elevator;
using Logger;
using System.Threading;
using Sensor.Interfaces;
using ElevatorModels;

namespace Sensor
{
    public class LogicBoard : SensorBaseClass, ILogicBoard
    {
        public LogicBoard(ILogger logger)
        {
            Logger = logger;
            _status = new StatusRoutine(logger);
            _timer = new TimerRoutine();
            _elevator = new Elevator();
        }
        
        private Elevator _elevator;
        private StatusRoutine _status;
        private readonly TimerRoutine _timer;
        private readonly bool shutDownSignal;

        public ILogger Logger { get; }

        public void StartProcess()
        {
            while (_queue.ActiveRequest() || ContinueFunctioning())
            {

                if (_queue.UpwardPeekQueue(out var _))
                {
                    while(_queue.UpwardEmptyQueue())
                        ProcessUpwardQueue();
                }

                if (_queue.DownwardPeekQueue(out var _))
                {
                    while (_queue.DownwardEmptyQueue())
                        ProcessDownwardQueue();
                }

                _status.WaitingForAdditionalRequests();
                SetIdleProperties();
                Thread.Sleep(2000);
            }

            _queue.disallowFurtherEnqueuing = false;
            Logger.Write("No remaining requests... exiting");
        }

        private bool ContinueFunctioning()
        {
            bool continueScanning = true;

            if (shutDownSignal)
                continueScanning = false;

            return continueScanning;
        }

        private void ProcessUpwardQueue()
        {
            _queue.UpwardPeekQueue(out var currentRequest);

            var destination = currentRequest.ButtonPress;
            if (destination > _floor.CurrentFloorLevel)
                StartUpwardMotion();
            else
                StartDownwardMotion();
        }

        private void ProcessDownwardQueue()
        {
             _queue.DownwardPeekQueue(out var currentRequest);

            var destination = currentRequest.ButtonPress;
            if (destination < _floor.CurrentFloorLevel)
                StartDownwardMotion();
            else
                StartDownwardMotion();
        }

        internal void StartDownwardMotion()
        {
            SetDownwardProperties();

            SetElevatorDownwardProjection();
        }

        internal void StartUpwardMotion()
        {
            SetUpwardProperties();

            SetElevatorUpwardProjection();
        }

        private void SetElevatorDownwardProjection()
        {
            int levelsToAscend = SetLevelsToDescend();

            if (levelsToAscend == _floor.CurrentFloor)
                CheckForDownwardDestination(levelsToAscend);

            DescendLevels(levelsToAscend);
        }

        private void SetElevatorUpwardProjection()
        {
            int levelsToAscend = SetLevelsToAscend();

            if (levelsToAscend == _floor.CurrentFloor)
                CheckForUpwardDestination(levelsToAscend);

            AdvanceLevels(levelsToAscend);
        }

        private int SetLevelsToDescend()
        {
            _queue.DownwardPeekQueue(out var currentButton);
            return currentButton.ButtonPress;
        }

        private int SetLevelsToAscend()
        {
            _queue.UpwardPeekQueue(out var currentButton);
            return currentButton.ButtonPress;
        }

        private void AdvanceLevels( int levelsToAscend)
        {
            var currentFloor = _floor.CurrentFloor;
            var levelsToAdvance = levelsToAscend - currentFloor;

            for (int floor = currentFloor; floor < currentFloor + levelsToAdvance; floor++)
            {
                LogMovementStatus(floor);
                _floor.AscendSingleLevel(floor);

                if (_queue.ReviseUpwardQueueOrder(_floor.CurrentFloor))
                    SetElevatorUpwardProjection();

                CheckForUpwardDestination(floor);
            }
        }

        private void DescendLevels(int levelsToDescend)
        {
            var currentFloor = _floor.CurrentFloor;

            var levelsToAdvance = currentFloor - levelsToDescend;

            for (int floor = currentFloor; floor > currentFloor - levelsToAdvance; floor--)
            {
                LogMovementStatus(floor);
                _floor.DescendSingleLevel(floor);

                if (_queue.ReviseDownwardQueueOrder(_floor.CurrentFloor))
                    SetElevatorDownwardProjection();

                CheckForDownwardDestination(floor);
            }
        }

        private void CheckForUpwardDestination(int floor)
        {
            _queue.UpwardPeekQueue(out var button);
            var currentDestination = button.ButtonPress;

            if (_floor.CurrentFloor == currentDestination)
                DequeueCurrentUpwardRequest(currentDestination);
        }

        private void DequeueCurrentUpwardRequest(int floor)
        {
            LogDestination(floor);
            _queue.DequeueUpwardRequest();
        }

        private void CheckForDownwardDestination(int floor)
        {
            _queue.DownwardPeekQueue(out var button);
            var currentDestination = button.ButtonPress;

            if (_floor.CurrentFloor == currentDestination)
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
            _status.NextFloor();
            _timer.NextFloorLevel();
            _status.DetermineCurrentFloor(_floor.CurrentFloor);
            _status.DetermineNextFloor(_floor.NextLevel);
            _status.PassedFloor(currentFloor);
        }
    }
}
