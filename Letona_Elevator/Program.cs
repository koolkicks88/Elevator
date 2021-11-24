using System;
using System.Threading;
using System.Threading.Tasks;
using Logger;
using Sensor;
using Sensor.Routines;

namespace Letona_Elevator
{
    public class Program
    {
        public ILogger Logger { get; set; }
        public static LogicBoard LogicBoard { get; set; }
        public static KeyPad KeyPad { get; set; } 

        public Program()
        {
            Logger = new EventLogger();
            LogicBoard = new LogicBoard(Logger);
            KeyPad = new KeyPad(Logger);
        }
        static void Main(string[] args)
        {
            Console.WriteLine(" Welcome to Nakatomi Plaza's Elevator. Please enter whether button press is coming from inside or " +
                "outside the elevator (e.g. 4U is coming from the outside). Press Q to exit." +
                " Press M to initiate max weight routine. Press R to exit max weight routine.");
            var program = new Program();
            program.GetInput();
        }

        private void GetInput()
        {
            var exitProgram = Task.Run(() => LogicBoard.StartProcess());
            while (!exitProgram.IsCompleted)
            {
                Insert();
                Thread.Sleep(3000);
            }
        }

        private static void Insert()
        {
            var keyPad = Task.Run(() =>
            {
                var input = Console.In.ReadLineAsync();
                KeyPad.AddElevatorRequest(input.Result);
            });
        }
    }
}
