using System;
using System.Threading;
using System.Threading.Tasks;
using Logger;
using Sensor;

namespace Letona_Elevator
{
    public class Program
    {
        public ILogger Logger { get; set; }
        public static LogicBoard LogicBoard { get; set; }

        public Program()
        {
            Logger = new EventLogger();
            LogicBoard = new LogicBoard(Logger);
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Please enter whether button press is coming from inside or " +
                "outside the elevator (e.g. 4U is coming from the outside). Press Q to exit");
            var program = new Program();

            while (true)
            {
                Thread.Sleep(1000);
                GetInputAsync(program);
            }
        }

        private static void GetInputAsync(Program program)
        {
            LogicBoard.StartProcess();
            Task.Run(() =>
            {
                var input = Console.In.ReadLineAsync();
                LogicBoard.AddElevatorRequest(input.Result);
            });
        }
    }
}
