using SigniflowMiddlewareCSharp.Loggings;

namespace ConsoleUI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MyLogs myLogs = new MyLogs();
            myLogs.LogError("This is an error message...");
            myLogs.LogInfo("This is an error info...");
            myLogs.LogWarning("This is an error warning...");
        }
    }
}
