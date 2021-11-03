using System;
using Xunit;
using Sensor;
using System.Collections.Generic;
using Logger;
using FluentAssertions;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace Elevator.Test
{
    public class Requests
    {
        private DummyLogger logger;
        private readonly LogicBoard logicBoard;

        public Requests()
        {
            logger = new DummyLogger();
            logicBoard = new LogicBoard(logger);
        }

        [Fact]
        public void Upward_AscendASingleLevel_Success()
        {
            var request = "1";

            var queue = logicBoard._queue;
            logicBoard.AddElevatorRequest(request);

            logicBoard.StartProcess();
            while (queue.ActiveRequest())
            {

                Thread.Sleep(500);
            }

            var logLine = logger.Message.Split('\n').ToList().Where(w => w.Contains("Stopping"));
            logLine.First().Should().Contain(" at destination floor: 1");
        }

        [Fact]
        public void Upward_AscendTwoLevels_Success()
        {
            var requests = new List<string> { "1", "2" };

            foreach (var request in requests)
                logicBoard.AddElevatorRequest(request);

            logicBoard.StartProcess();
            while (logicBoard._queue.ActiveRequest())
            {

                Thread.Sleep(500);
            }

            var logLine = logger.Message.Split('\n').ToList().Where(w => w.Contains("Stopping"));
            logLine.Any(line => line.Contains(" at destination floor: 1")).Should().BeTrue();
            logLine.Any(line => line.Contains(" at destination floor: 2")).Should().BeTrue();
        }

        [Fact]
        public void Upward_Ascend_ThenDescend_Success()
        {
            var requests = new List<string> { "1", "2", "1" };

            foreach (var request in requests)
                logicBoard.AddElevatorRequest(request);

            logicBoard.StartProcess();
           

            var logLine = logger.Message.Split('\n').ToList().Where(w => w.Contains("Stopping"));
            logLine.First().Should().Contain(" at destination floor: 1");
            logLine.Any(line => line.Contains(" at destination floor: 2")).Should().BeTrue();
            logLine.Last().Should().Contain(" at destination floor: 1");

        }

        //public static string GetBetween(string strSource, string strStart, string strEnd)
        //{
        //    if (strSource.Contains(strStart) && strSource.Contains(strEnd))
        //    {
        //        int Start, End;
        //        Start = strSource.IndexOf(strStart, 0) + strStart.Length;
        //        End = strSource.IndexOf(strEnd, Start);
        //        return strSource.Substring(Start, End - Start);
        //    }

        //    return "";
        //}

        [Fact]
        public void Upward_ArriveOutOfOrder_Success()
        {
            var requests = new List<string> { "6", "3","1" };


            var queue = logicBoard._queue;
            //do
            //{
                foreach (var request in requests)
                {
                    Thread.Sleep(1000);
                    logicBoard.AddElevatorRequest(request);

                }
            //}
            while (queue.ActiveRequest())
            {
                logicBoard.StartProcess();

                Thread.Sleep(500);
            }


            var p = logger.Message;
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
