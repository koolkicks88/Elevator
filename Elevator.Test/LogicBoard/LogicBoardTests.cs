using Xunit;
using Sensor;
using System.Collections.Generic;
using Logger;
using FluentAssertions;
using System.Linq;
using Sensor.Routines;
using ElevatorModels;
using Moq;
using static ElevatorModels.Button;

namespace Elevator.Test
{
    public class LogicBoardTests
    {
        private TestLogger logger;

        public LogicBoardTests()
        {
            logger = new TestLogger();
        }

        private static Mock<ITimerRoutine> MockTimerRoutine()
        {
            Mock<ITimerRoutine> timer = new Mock<ITimerRoutine>();
            timer.Setup(c => c.NextFloorLevel());
            timer.Setup(c => c.StopAtFloorLevel());
            timer.Setup(c => c.TimerBetweenScanning());
            return timer;
        }

        private static Mock<IQueueRoutine> MockAscendingQueue(Button button, bool reviseQueue = false)
        {
            Mock<IQueueRoutine> mockQueue = new Mock<IQueueRoutine>();
            mockQueue.Setup(c => c.EnqueueUpwardRequest(button));
            mockQueue.SetupSequence(c => c.ActiveRequest()).Returns(true).Returns(false);
            mockQueue.Setup(c => c.UpwardPeekQueue(out button)).Returns(true);
            mockQueue.SetupSequence(c => c.UpwardEmptyQueue()).Returns(true).Returns(true).Returns(false);
            mockQueue.Setup(c => c.DisallowFurtherEnqueuing).Returns(true);
            mockQueue.Setup(c => c.ReviseUpwardQueueOrder(It.IsAny<int>())).Returns(reviseQueue);
            return mockQueue;
        }

        private static Mock<IQueueRoutine> MockDescendingQueue(Button button, bool reviseQueue = false)
        {
            Mock<IQueueRoutine> mockQueue = new Mock<IQueueRoutine>();
            mockQueue.SetupSequence(c => c.ActiveRequest()).Returns(true).Returns(false);
            mockQueue.Setup(c => c.DownwardPeekQueue(out button)).Returns(true);
            mockQueue.SetupSequence(c => c.DownwardEmptyQueue()).Returns(true).Returns(true).Returns(false);
            mockQueue.Setup(c => c.DisallowFurtherEnqueuing).Returns(true);
            mockQueue.Setup(c => c.ReviseDownwardQueueOrder(It.IsAny<int>())).Returns(reviseQueue);
            mockQueue.Setup(c => c.EnqueueDownwardRequest(button));

            return mockQueue;
        }

        private static Mock<IFloor> MockAssending(int currentFloor, int destination)
        {
            Mock<IFloor> mockFloor = new Mock<IFloor>();
            mockFloor.SetupSequence(ss => ss.NextLevel).Returns(currentFloor + 1).Returns(destination + 1);
            mockFloor.SetupSequence(ss => ss.GetCurrentFloor()).Returns(currentFloor).Returns(destination);
            mockFloor.Setup(c => c.AscendSingleLevel(1));
            return mockFloor;
        }
        private static Mock<IFloor> MockDescending(int currentFloor, int destination)
        {
            Mock<IFloor> mockFloor = new Mock<IFloor>();
            mockFloor.SetupSequence(c => c.GetCurrentFloor()).Returns(currentFloor).Returns(destination);
            mockFloor.SetupSequence(ss => ss.NextLevel).Returns(currentFloor - 1).Returns(destination - 1);
            mockFloor.Setup(c => c.DescendSingleLevel(It.IsAny<int>()));
            return mockFloor;
        }

        [Fact]
        public void Upward_AscendLevel_Success()
        {
            var button = new Button { Action = ButtonAction.Up.ToString(), ButtonPress = 1 };
            Mock<IFloor> mockFloor = MockAssending(currentFloor:0, destination: button.ButtonPress);
            Mock<IQueueRoutine> mockQueue = MockAscendingQueue(button);
            Mock<ITimerRoutine> timer = MockTimerRoutine();

            var logicBoard = new LogicBoard(logger, mockQueue.Object,
                mockFloor.Object, timer.Object);
            logicBoard.StartProcess();

            var logLine = logger.Message.Split('\n').ToList().Where(w => w.Contains("Stopping"));
            AssertFirstDestinationLevel(logLine, 1);
        }

        [Fact]
        public void Constructor_NotNull()
        {
            const int destinationLevel = 0;
            var button = new Button { Action = ButtonAction.Down.ToString(), ButtonPress = destinationLevel };
            Mock<IFloor> mockFloor = MockDescending(currentFloor: 1, destinationLevel);
            Mock<IQueueRoutine> mockQueue = MockDescendingQueue(button);
            Mock<ITimerRoutine> timer = MockTimerRoutine();
            Mock<ILogger> mockLogger = new Mock<ILogger>();

            var logicBoard = new LogicBoard(mockLogger.Object, mockQueue.Object,
                mockFloor.Object, timer.Object);
            
            Assert.NotNull(logicBoard);
        }

        [Fact]
        public void ConstructorWithOnlyLogger_NotNull()
        {
            Mock<ILogger> mockLogger = new Mock<ILogger>();

            var logicBoard = new LogicBoard(mockLogger.Object);

            Assert.NotNull(logicBoard);
        }

        [Fact]
        public void Downward_DescendLevel_Success()
        {
            const int destinationLevel = 0;
            var button = new Button { Action = ButtonAction.Down.ToString(), ButtonPress = destinationLevel };
            Mock<IFloor> mockFloor = MockDescending(currentFloor: 1, destinationLevel);
            Mock<IQueueRoutine> mockQueue = MockDescendingQueue(button);
            Mock<ITimerRoutine> timer = MockTimerRoutine();

            var logicBoard = new LogicBoard(logger, mockQueue.Object,
                mockFloor.Object, timer.Object);
            logicBoard.StartProcess();

            var logLine = logger.Message.Split('\n').ToList().Where(w => w.Contains("Stopping"));
            AssertFirstDestinationLevel(logLine, destinationLevel);
        }

        [Fact]
        public void Downward_ReviseQueue_Success()
        {
            const int destinationLevel = 0;
            var button = new Button { Action = ButtonAction.Down.ToString(), ButtonPress = destinationLevel };
            Mock<IFloor> mockFloor = MockDescending(currentFloor: 1, destination: 0);
            Mock<IQueueRoutine> mockQueue = MockDescendingQueue(button, reviseQueue: true);
            Mock<ITimerRoutine> timer = MockTimerRoutine();

            var logicBoard = new LogicBoard(logger, mockQueue.Object,
                mockFloor.Object, timer.Object);
            logicBoard.StartProcess();

            var logLine = logger.Message.Split('\n').ToList().Where(w => w.Contains("Stopping"));
            AssertFirstDestinationLevel(logLine, destinationLevel);
        }

        [Fact]
        public void Upward_ReviseQueue_Success()
        {
            const int destinationLevel = 1;
            var button = new Button { Action = ButtonAction.Down.ToString(), ButtonPress = destinationLevel };
            Mock<IFloor> mockFloor = MockAssending(currentFloor: 0, destination: destinationLevel);
            Mock<IQueueRoutine> mockQueue = MockAscendingQueue(button, reviseQueue: true);
            Mock<ITimerRoutine> timer = MockTimerRoutine();

            var logicBoard = new LogicBoard(logger, mockQueue.Object,
                mockFloor.Object, timer.Object);
            logicBoard.StartProcess();

            var logLine = logger.Message.Split('\n').ToList().Where(w => w.Contains("Stopping"));
            AssertFirstDestinationLevel(logLine, destinationLevel);
        }

        [Fact]
        public void Upward_RequestPassedCurrentLevel_DequeueAndReviseQueue_Success()
        {
            const int destinationLevel = 1;
            var button = new Button { Action = ButtonAction.Down.ToString(), ButtonPress = destinationLevel };
            Mock<IFloor> mockFloor = MockAssending(currentFloor: 2, destination: destinationLevel);
            Mock<IQueueRoutine> mockQueue = MockAscendingQueue(button);
            Mock<ITimerRoutine> timer = MockTimerRoutine();

            var logicBoard = new LogicBoard(logger, mockQueue.Object,
                mockFloor.Object, timer.Object);
            logicBoard.StartProcess();

            mockQueue.Verify(mockQueue => mockQueue.DequeueUpwardRequest(), Times.Once);
            mockQueue.Verify(mockQueue => mockQueue.EnqueueDownwardRequest(It.IsAny<Button>()), Times.Once);
            mockQueue.Verify(mockQueue => mockQueue.ReviseDownwardQueueOrder(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public void Downward_RequestPassedCurrentLevel_DequeueAndReviseQueue_Success()
        {
            const int destinationLevel = 3;
            var button = new Button { Action = ButtonAction.Down.ToString(), ButtonPress = destinationLevel };
            Mock<IFloor> mockFloor = MockAssending(currentFloor: 2, destination: destinationLevel);
            Mock<IQueueRoutine> mockQueue = MockDescendingQueue(button);
            Mock<ITimerRoutine> timer = MockTimerRoutine();

            var logicBoard = new LogicBoard(logger, mockQueue.Object,
                mockFloor.Object, timer.Object);
            logicBoard.StartProcess();

            mockQueue.Verify(mockQueue => mockQueue.DequeueDowanwardRequest(), Times.Once);
            mockQueue.Verify(mockQueue => mockQueue.EnqueueUpwardRequest(It.IsAny<Button>()), Times.Once);
            mockQueue.Verify(mockQueue => mockQueue.ReviseUpwardQueueOrder(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public void Downward_SameLevelRequest_DequeueAndReviseQueue_Success()
        {
            const int destinationLevel = 2;
            var button = new Button { Action = ButtonAction.Down.ToString(), ButtonPress = destinationLevel };
            Mock<IFloor> mockFloor = MockDescending(currentFloor: 2, destination: destinationLevel);
            Mock<IQueueRoutine> mockQueue = MockDescendingQueue(button);
            Mock<ITimerRoutine> timer = MockTimerRoutine();

            var logicBoard = new LogicBoard(logger, mockQueue.Object,
                mockFloor.Object, timer.Object);
            logicBoard.StartProcess();

            mockQueue.Verify(mockQueue => mockQueue.DequeueDowanwardRequest(), Times.Once);
        }

        [Fact]
        public void Upward_SameLevelRequest_DequeueAndReviseQueue_Success()
        {
            const int destinationLevel = 2;
            var button = new Button { Action = ButtonAction.Down.ToString(), ButtonPress = destinationLevel };
            Mock<IFloor> mockFloor = MockAssending(currentFloor: 2, destination: destinationLevel);
            Mock<IQueueRoutine> mockQueue = MockAscendingQueue(button);
            Mock<ITimerRoutine> timer = MockTimerRoutine();

            var logicBoard = new LogicBoard(logger, mockQueue.Object,
                mockFloor.Object, timer.Object);
            logicBoard.StartProcess();

            mockQueue.Verify(mockQueue => mockQueue.DequeueUpwardRequest(), Times.Once);
        }

        private static void AssertFirstDestinationLevel(IEnumerable<string> logLine, int floor)
        {
            logLine.First().Should().Contain($" at destination floor: {floor}");
        }

        internal class TestLogger : ILogger
        {
            internal string Message;
            public string FileName { get; set; }

            internal TestLogger()
            {
                Message = string.Empty;
            }

            public void Write(string message)
            {
                Message += "\n" + " " + message;
            }
        }
    }
}
