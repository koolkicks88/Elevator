namespace ElevatorModels
{
    public interface IFloor
    {
        int CurrentFloor { get; set; }
        int CurrentFloorLevel { get; set; }
        int NextLevel { get; set; }
        int GetCurrentFloor();
        void AscendSingleLevel(int currentFloor);
        void DescendSingleLevel(int currentFloor);
        void SetFloorSettings(int currentFloor);
    }
}