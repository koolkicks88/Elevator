namespace Logger
{
    public interface ILogger
    {
        string FileName { get; set; }
        void Write(string message);
    }
}