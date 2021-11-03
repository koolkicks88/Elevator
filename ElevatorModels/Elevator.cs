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

        private readonly object _elevatorSettings = new object();

        public ElevatorState ElevatorCurrentState;
        public ElevatorDirection ElevatorCurrentDirection;

        public bool ReachedMaxWeight { get; set; }
        public string State
        {
            get { return ElevatorCurrentState.ToString(); }
            set
            {
                switch (value)
                {
                    case "Stop":
                        ElevatorCurrentState = ElevatorState.Stop;
                        break;
                    case "Moving":
                        ElevatorCurrentState = ElevatorState.Moving;
                        break;
                    default:
                        throw new Exception("invalid elevator state");
                }
            }
        }

        public string Direction
        {
            get { return ElevatorCurrentDirection.ToString(); }
            set
            {
                switch (value)
                {
                    case "Up":
                        ElevatorCurrentDirection = ElevatorDirection.Up;
                        break;
                    case "Down":
                        ElevatorCurrentDirection = ElevatorDirection.Down;
                        break;
                    case "Idle":
                        ElevatorCurrentDirection = ElevatorDirection.Idle;
                        break;
                    default:
                        throw new Exception("invalid elevator state");
                }
            }
        }

        public void SetCurrentData(Elevator elevator)
        {
            lock (_elevatorSettings)
            {
                Direction = elevator.Direction;
                State = elevator.State;
            }
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
