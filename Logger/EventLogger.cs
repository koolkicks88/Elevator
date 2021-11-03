using System.IO;

namespace Logger
{
    public class EventLogger : ILogger
    {
        private static readonly object LogFile = new object();
        public string FileName { get; set; }
        public EventLogger()
        {
            FileName = ".\\Elevator_Activity.log";
        }

        public void Write(string message)
        {
            lock (LogFile)
            {
                using (var writer = File.AppendText(FileName))
                {
                    writer.WriteLine(message);
                }
            }
        }
    }
}
