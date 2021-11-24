using System.Runtime.CompilerServices;

namespace ElevatorModels
{
    public class Floor : IFloor
    {
        private static Floor _instance;

        public static Floor GetFloorInformation()
        {
            if (_instance == null)
            {
                _instance = new Floor();
            }
            return _instance;
        }

        private Floor()
        {
            CurrentFloorLevel = 0;
        }

        private readonly object _floorLocation = new object();

        public void SetFloorSettings(int currentFloor)
        {
            lock (_floorLocation)
            {
                CurrentFloorLevel = currentFloor;
            }
        }

        private int _currentFloor;
        public int CurrentFloor
        {
            get
            {
                lock (_floorLocation)
                {
                    return _currentFloor;
                }
            }
            set
            {
                lock (_floorLocation)
                {
                    CurrentFloorLevel = value;
                    _currentFloor = value;
                }
            }
        }

        public int GetCurrentFloor()
        {
            return CurrentFloor;
        }

        public int CurrentFloorLevel { get; set; }
        public int NextLevel { get; set; }

        public void AscendSingleLevel(int currentFloor)
        {
            CurrentFloor = currentFloor + 1;
            NextLevel = currentFloor == 0
                ? 1 : currentFloor + 2;
        }

        public void DescendSingleLevel(int currentFloor)
        {
            CurrentFloor = currentFloor - 1;
            NextLevel = currentFloor == 12
                ? 11 : currentFloor - 2;
        }
    }
}
