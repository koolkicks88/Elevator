using System;

namespace ElevatorModels
{
    public class Elevator
    {
        public Elevator()
        {
            Direction = ElevatorDirection.Idle.ToString();
            State = ElevatorState.Stop.ToString();
        }

        public ElevatorState ElevatorCurrentState;
        public ElevatorDirection ElevatorCurrentDirection;

        public string State
        {
            get { return ElevatorCurrentState.ToString(); }
            set
            {
                ElevatorCurrentState = value switch
                {
                    "Stop" => ElevatorState.Stop,
                    "Moving" => ElevatorState.Moving,
                    _ => throw new Exception("invalid elevator state"),
                };
            }
        }

        public string Direction
        {
            get { return ElevatorCurrentDirection.ToString(); }
            set
            {
                ElevatorCurrentDirection = value switch
                {
                    "Up" => ElevatorDirection.Up,
                    "Down" => ElevatorDirection.Down,
                    "Idle" => ElevatorDirection.Idle,
                    _ => throw new Exception("invalid elevator state"),
                };
            }
        }

        public void SetCurrentData(Elevator elevator)
        {
            Direction = elevator.Direction;
            State = elevator.State;
        }

        public enum ElevatorState
        {
            Stop,
            Moving
        }

        public enum ElevatorDirection
        {
            Up, 
            Down,
            Idle
        }
    }
}
