using Xunit;
using Logger;
using Sensor.Routines;
using ElevatorModels;
using Moq;
using FluentAssertions;

namespace Elevator.Test.Routines
{
    public class KeyPadTest
    {
        private TestLogger logger;

        public KeyPadTest()
        {
            logger = new TestLogger();
        }
        private Mock<IFloor> GetFloorMock(int currentFloor)
        {
            Mock<IFloor> mockFloor = new Mock<IFloor>();
            mockFloor.Setup(c => c.GetCurrentFloor()).Returns(currentFloor);
            return mockFloor;
        }

        private Mock<IQueueRoutine> GetQueueMock(bool disallowFurtherEnqueuing = false)
        {
            Mock<IQueueRoutine> mockQueue = new Mock<IQueueRoutine>();
            mockQueue.Setup(c => c.DisallowFurtherEnqueuing).Returns(disallowFurtherEnqueuing);
            return mockQueue;
        }

        [Fact]
        public void Constructor_NotNull()
        {
            Mock<IFloor> mockFloor = GetFloorMock(currentFloor: 1);
            Mock<IQueueRoutine> mockQueue = GetQueueMock();
            Mock<ILogger> mockLogger = new Mock<ILogger>();

            var keyPad = new KeyPad(mockLogger.Object, mockQueue.Object, mockFloor.Object);

            Assert.NotNull(keyPad);
        }

        [Fact]
        public void ConstructorWithOnlyLogger_NotNull()
        {
            Mock<ILogger> mockLogger = new Mock<ILogger>();

            var logicBoard = new KeyPad(mockLogger.Object);

            Assert.NotNull(logicBoard);
        }

        [Fact]
        public void UpwardQueue_Success()
        {
            const string input = "1";

            Mock<IFloor> mockFloor = GetFloorMock(currentFloor: 0);
            Mock<IQueueRoutine> mockQueue = GetQueueMock();

            var keyPad = new KeyPad(logger, mockQueue.Object, mockFloor.Object);
            keyPad.AddElevatorRequest(input);

            mockQueue.Verify(mockQueue => mockQueue.EnqueueUpwardRequest(It.IsAny<Button>()), Times.Once);
        }

        [Fact]
        public void DownwardQueue_Success()
        {
            const string input = "0";

            Mock<IFloor> mockFloor = GetFloorMock(currentFloor: 1);
            Mock<IQueueRoutine> mockQueue = GetQueueMock();

            var keyPad = new KeyPad(logger, mockQueue.Object, mockFloor.Object);
            keyPad.AddElevatorRequest(input);

            mockQueue.Verify(mockQueue => mockQueue.EnqueueDownwardRequest(It.IsAny<Button>()), Times.Once);
        }

        [Fact]
        public void DownwardQueue_OutsideRequest_Success()
        {
            const string input = "0U";

            Mock<IFloor> mockFloor = GetFloorMock(currentFloor: 1);
            Mock<IQueueRoutine> mockQueue = GetQueueMock();

            var keyPad = new KeyPad(logger, mockQueue.Object, mockFloor.Object);
            keyPad.AddElevatorRequest(input);

            mockQueue.Verify(mockQueue => mockQueue.EnqueueDownwardRequest(It.IsAny<Button>()), Times.Once);
        }

        [Fact]
        public void UpwardQueue_OutsideRequest_Success()
        {
            const string input = "1U";

            Mock<IFloor> mockFloor = GetFloorMock(currentFloor: 0);
            Mock<IQueueRoutine> mockQueue = GetQueueMock();

            var keyPad = new KeyPad(logger, mockQueue.Object, mockFloor.Object);
            keyPad.AddElevatorRequest(input);

            mockQueue.Verify(mockQueue => mockQueue.EnqueueUpwardRequest(It.IsAny<Button>()), Times.Once);
        }

        [Fact]
        public void ShutdownRequest_Success()
        {
            const string input = "Q";
            const bool disallowEnqueue = true;

            Mock<IFloor> mockFloor = GetFloorMock(currentFloor: 0);
            Mock<IQueueRoutine> mockQueue = GetQueueMock(disallowEnqueue);
            
            var keyPad = new KeyPad(logger, mockQueue.Object, mockFloor.Object);
            keyPad.AddElevatorRequest(input);

            mockQueue.Verify(mockQueue => mockQueue.EnqueueUpwardRequest(It.IsAny<Button>()), Times.Never);
            mockQueue.Verify(mockQueue => mockQueue.EnqueueDownwardRequest(It.IsAny<Button>()), Times.Never);
        }

        [Fact]
        public void Enable_MaxWeight_Success()
        {
            const string input = "M";
            Mock<IFloor> mockFloor = GetFloorMock(currentFloor: 0);
            Mock<IQueueRoutine> mockQueue = GetQueueMock();

            var keyPad = new KeyPad(logger, mockQueue.Object, mockFloor.Object);
            keyPad.AddElevatorRequest(input);

            keyPad.AddElevatorRequest("1D");

            mockQueue.Verify(mockQueue => mockQueue.EnqueueUpwardRequest(It.IsAny<Button>()), Times.Never);
            mockQueue.Verify(mockQueue => mockQueue.EnqueueDownwardRequest(It.IsAny<Button>()), Times.Never);
        }

        [Fact]
        public void Enable_MaxWeight_ValidRequest_Success()
        {
            const string input = "M";
            Mock<IFloor> mockFloor = GetFloorMock(currentFloor: 0);
            Mock<IQueueRoutine> mockQueue = GetQueueMock();

            var keyPad = new KeyPad(logger, mockQueue.Object, mockFloor.Object);
            keyPad.AddElevatorRequest(input);

            keyPad.AddElevatorRequest("1");

            mockQueue.Verify(mockQueue => mockQueue.EnqueueUpwardRequest(It.IsAny<Button>()), Times.Once);
         }

        [Fact]
        public void Disable_MaxWeight_Success()
        {
            const string input = "M";
            Mock<IFloor> mockFloor = GetFloorMock(currentFloor: 2);
            Mock<IQueueRoutine> mockQueue = GetQueueMock();

            var keyPad = new KeyPad(logger, mockQueue.Object, mockFloor.Object);
            keyPad.AddElevatorRequest(input);

            keyPad.AddElevatorRequest("R");
            keyPad.AddElevatorRequest("1D");

            mockQueue.Verify(mockQueue => mockQueue.EnqueueUpwardRequest(It.IsAny<Button>()), Times.Once);
        }

        [Fact]
        public void Upward_HighValueOutofBounce_LogError()
        {
            const string input = "13";

            Mock<IFloor> mockFloor = GetFloorMock(currentFloor: 0);
            Mock<IQueueRoutine> mockQueue = GetQueueMock();

            var keyPad = new KeyPad(logger, mockQueue.Object, mockFloor.Object);
            keyPad.AddElevatorRequest(input);

            logger.Message.Should().Contain("cannot be completed");
        }

        [Fact]
        public void Upward_LowValueOutofBounce_LogError()
        {
            const string input = "-1";

            Mock<IFloor> mockFloor = GetFloorMock(currentFloor: 0);
            Mock<IQueueRoutine> mockQueue = GetQueueMock();

            var keyPad = new KeyPad(logger, mockQueue.Object, mockFloor.Object);
            keyPad.AddElevatorRequest(input);

            logger.Message.Should().Contain("cannot be completed");
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
