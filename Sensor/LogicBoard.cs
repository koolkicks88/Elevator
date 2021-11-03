using ElevatorModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sensor.Routines;
using static ElevatorModels.Elevator;
using static ElevatorModels.Button;
using Logger;
using System.Threading;

namespace Sensor
{
    public class LogicBoard
    {
        public LogicBoard(ILogger logger)
        {
            Logger = logger;
            _queue = new QueueRoutine();
            _status = new StatusRoutine(logger);
            _timer = new TimerRoutine();
        }

        private static Floor _floor = new Floor();
        private Elevator _elevator = new Elevator();
        private readonly TimerRoutine _timer;
        public readonly QueueRoutine _queue;
        private StatusRoutine _status;
        private readonly char upwardRequest = 'U';
        private readonly char downwardRequest = 'D';
        private static bool maxWeight;

        public ILogger Logger { get; }

        public void AddElevatorRequest(string input)
        {
            Logger.Write("Button Pressed.. Starting Proccess");
            Logger.Write($"Adding floor request {input} in the queue.");

            AddToDesignatedQueue(input);
        }

        //private void CheckForWeightRestriction()
        //{
        //    if (input.Any(values => values.Contains('Q'))) ;
        //    {
        //        var routime = new MaxWeightRoutine();
        //        routime.CompleteInteralRequests();
        //    }
        //}

        private void AddToDesignatedQueue(string request)
        {
            ParseRequestValues(request, out int destinationLevel, out Button button);

            InsertToQueue(destinationLevel, button);
        }

        private void ParseRequestValues(string request, out int destinationLevel, out Button button)
        {
            destinationLevel = int.Parse(request
                .Trim(new char[] { upwardRequest, downwardRequest, 'Q', }));
            var action = new string(request.ToCharArray().Where(c => !char.IsDigit(c)).ToArray());

            button = new Button
            {
                ButtonPress = destinationLevel,
                Action = action
            };
        }

        private void InsertToQueue(int destinationLevel, Button button)
        {
            var action = Enum.Parse(typeof(ButtonAction),button.Action);
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

        public void StartProcess()
        {
            Thread.Sleep(1000);
            while (_queue.ActiveRequest()) //|| !_shutDownSignal)
            {
                if(_queue.UpwardPeekQueue(out var _))
                {
                    while(_queue.UpwardEmptyQueue())
                        ProcessUpwardQueue();
                }

                if (_queue.DownwardPeekQueue(out var _))
                {
                    while (_queue.DownwardEmptyQueue())
                        ProcessDownwardQueue();
                }
            }

            SetIdleProperties();
            _status.Direction(ElevatorDirection.Idle.ToString());

            Logger.Write("No remaining requests... exiting");
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
                LogMovementStatusAsync(floor);
                AscendSingleLevel(floor);

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
                LogMovementStatusAsync(floor);
                DescendSingleLevel(floor);

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
            _queue.UpwardPeekQueue(out var button);

            if (_floor.CurrentFloor == button.ButtonPress)
                DequeueCurrentDownwardRequests(floor);
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
        private void LogMovementStatusAsync(int currentFloor)
        {
            _status.NextFloor();
            _timer.NextFloorLevel();
            _status.DetermineCurrentFloor(currentFloor);
            _status.PassedFloor(currentFloor);
        }

        private static void AscendSingleLevel(int currentFloor)
        {
            _floor.CurrentFloor = currentFloor + 1;
        }

        private static void DescendSingleLevel(int currentFloor)
        {
            _floor.CurrentFloor = currentFloor - 1;
        }
    }
}
