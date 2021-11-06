using System;
using Xunit;
using Sensor;
using System.Collections.Generic;
using Logger;
using FluentAssertions;
using System.Threading;
using System.Linq;
using Sensor.Routines;
using ElevatorModels;
using Moq;

namespace Elevator.Test
{
    public class Requests
    {
        private DummyLogger logger;
        private readonly LogicBoard logicBoard;
        private readonly KeyPadRoutine keyPad;

        public Requests()
        {
            logger = new DummyLogger();
            logicBoard = new LogicBoard(logger);
            keyPad = new KeyPadRoutine(logger, Floor.GetFloorInformation());
        }

        [Fact]
        public void Upward_AscendASingleLevel_Success()
        {
            var request = "1";
            //var keyPad = new KeyPadRoutine(logger, Floor.GetFloorInformation());

            //System.Reflection.FieldInfo instance = typeof(Floor)
            //    .GetField("_instance", System.Reflection.BindingFlags.Static
            //    | System.Reflection.BindingFlags.NonPublic);
            //Mock<Floor> mockSingleton = new Mock<Floor>();
            //instance.SetValue(null, null);//mockSingleton.Object);

            keyPad.AddElevatorRequest(request);

            logicBoard.StartProcess();

            var logLine = logger.Message.Split('\n').ToList().Where(w => w.Contains("Stopping"));
            AssertFirstDestinationLevel(logLine, 1);
        }

        [Fact]
        public void Upward_AscendTwoLevels_Success()
        {
            var requests = new List<string> { "1", "2" };
             var logicBoard = new LogicBoard(logger);
            
            //System.Reflection.FieldInfo instance = typeof(Floor)
            //   .GetField("_instance", System.Reflection.BindingFlags.Static
            //   | System.Reflection.BindingFlags.NonPublic);
            //Mock<IFloor> mockSingleton = new Mock<IFloor>();
            //instance.SetValue(null, mockSingleton.Object);

            foreach (var request in requests)
                keyPad.AddElevatorRequest(request);

            logicBoard.StartProcess();
           
            var logLine = logger.Message.Split('\n').ToList().Where(w => w.Contains("Stopping"));
            AssertFirstDestinationLevel(logLine, 1);
            AssertLastDestinationLevel(logLine, 2);
        }

        [Fact]
        public void Upward_RemovesDuplicateRequest_Success()
        {
            var requests = new List<string> { "1", "2", "1" };
            foreach (var request in requests)
                keyPad.AddElevatorRequest(request);

            logicBoard.StartProcess();

            var logLine = logger.Message.Split('\n').ToList().Where(w => w.Contains("Stopping"));
            AssertFirstDestinationLevel(logLine, 1);
            AssertLastDestinationLevel(logLine, 2);
        }

        [Fact]
        public void Upward_InternalAndExternalRequest_Success()
        {
            var requests = new List<string> { "1", "4U"};

            foreach (var request in requests)
                keyPad.AddElevatorRequest(request);

            logicBoard.StartProcess();

            var logLine = logger.Message.Split('\n').ToList().Where(w => w.Contains("Stopping"));
            AssertFirstDestinationLevel(logLine, 1);
            AssertLastDestinationLevel(logLine, 4);
        }

        [Fact]
        public void Upward_AllExternalRequest_Success()
        {
            var requests = new List<string> { "1U", "4U" };

            foreach (var request in requests)
                keyPad.AddElevatorRequest(request);

            logicBoard.StartProcess();

            var logLine = logger.Message.Split('\n').ToList().Where(w => w.Contains("Stopping"));
            AssertSpecificLogLine(logLine, 1);
            AssertSpecificLogLine(logLine, 4);
        }

        [Fact]
        public void Downward_AllExternalRequest_Success()
        {
            const int firstDestination = 6;
            const int secondDestination = 5;
            var requests = new List<string> { $"{firstDestination}", $"{secondDestination}" + "D" };

            foreach (var request in requests)
                keyPad.AddElevatorRequest(request);

            logicBoard.StartProcess();

            var logLine = logger.Message.Split('\n').ToList().Where(w => w.Contains("Stopping"));
            AssertFirstDestinationLevel(logLine, firstDestination);
            AssertLastDestinationLevel(logLine, secondDestination);
        }

        [Fact]
        public void Shutdown_CompletesCurrentQueue_Success()
        {
            const int firstDestination = 2;
            const int secondDestination = 1;
            const string shutdownCommand = "Q";
            var requests = new List<string> { $"{firstDestination}", $"{secondDestination}" + "D" ,
                shutdownCommand };

            foreach (var request in requests)
                keyPad.AddElevatorRequest(request);

            logicBoard.StartProcess();

            var logLine = logger.Message.Split('\n').ToList().Where(w => w.Contains("Stopping"));
            AssertFirstDestinationLevel(logLine, firstDestination);
            AssertLastDestinationLevel(logLine, secondDestination);
            logger.Message.Split('\n').ToList().Any(l => l.Contains($"Shutdown routine initiated"))
               .Should().BeTrue();
        }

        [Fact]
        public void Upward_ArriveOutOfOrder_Success()
        {
            var requests = new List<string> { "6", "3", "1" };

            foreach (var request in requests)
            {
                Thread.Sleep(1000);
                keyPad.AddElevatorRequest(request);
            }

            logicBoard.StartProcess();
            var p = logger.Message;
        }

        private static void AssertSpecificLogLine(IEnumerable<string> logLine, int floor)
        {
            logLine.Any(line => line.Contains($" at destination floor: {floor}"))
                .Should().BeTrue();
        }

        private static void AssertFirstDestinationLevel(IEnumerable<string> logLine, int floor)
        {
            logLine.First().Should().Contain($" at destination floor: {floor}");
        }

        private static void AssertLastDestinationLevel(IEnumerable<string> logLine, int floor)
        {
            logLine.Last().Should().Contain($" at destination floor: {floor}");
        }

        internal class DummyLogger : ILogger
        {
            internal string Message;
            public string FileName { get; set; }

            internal DummyLogger()
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
