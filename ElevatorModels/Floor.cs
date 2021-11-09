using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Sensor")]
namespace ElevatorModels
{
    public class Floor
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

        static Floor()
        {
            _instance = new Floor();
        }

        internal void ResetForTesting()
        {
            CurrentFloor = 0;
            NextLevel = 1;
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

        public int CurrentFloorLevel { get; set; }
        public int NextLevel { get; set; }

        public void AscendSingleLevel(int currentFloor)
        {
            CurrentFloor = currentFloor + 1;
            NextLevel = currentFloor == 0
                ? 0 : currentFloor + 2;
        }

        public void DescendSingleLevel(int currentFloor)
        {
            CurrentFloor = currentFloor - 1;
            NextLevel = currentFloor == 0 
                ? 0 : currentFloor - 2;
        }
    }
}
