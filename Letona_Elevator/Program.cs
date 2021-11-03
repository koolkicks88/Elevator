using System;
using System.Collections.Generic;
using System.Linq;
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
                "outside the elevator (I or O) followed by the level number. Press Q to exit");
            var program = new Program();

            while (true)
            {
                Thread.Sleep(1000);
                GetInputAsync(program);
            }


            //todo: works

            //while (true)
            //{
            //    Console.WriteLine("incorrect input: must enter level followed by I or O. (To quit enter Q)");
            //    var input = Console.ReadLine();
            //    var program = new Program();
            //    program.InitiateElevator(new List<string> { input });
            //    //program.InitiateElevator(input.Split(',').ToList());
            //}


            //if (input.Length > 3)
            //    Console.WriteLine("incorrect input: must enter level followed by I or O. (To quit enter Q)");

            //program.InitiateElevator(input.Split(',').ToList());
        }

        private static void GetInputAsync(Program program)
        {
            LogicBoard.StartProcess();
            Task.Run(() =>
            {
                var input = Console.In.ReadLineAsync();
                //foreach (var i in input)
                LogicBoard.AddElevatorRequest(input.Result);
                // return null;
            });
        }



        public void InitiateElevator(List<string> input)
        {
           

           
        }

        //private async Task<string> GetInputAsync()
        //{
        //    return Task.Run(() => Console.ReadLine());
        //}
    }
}
