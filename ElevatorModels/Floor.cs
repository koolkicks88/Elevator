using System;

namespace ElevatorModels
{
    public class Floor
    {
        public Floor()
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
        public int DestinationFloor { get; set; }
    }
}
