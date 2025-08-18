using Serilog;

namespace SigniflowMiddlewareCSharp.Loggings
{
    public class MyLogs
    {
        // Initialize the logger in a static constructor to ensure it's set up once for the class
        static MyLogs()
        {
            var connectionString = "server=en33-jhb.za-dns.com;uid=sparksch_dev;pwd=Password00@11;database=sparksch_csharp;";

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("Logs/log.log", rollingInterval: RollingInterval.Day)
                .WriteTo.MySQL(
                    connectionString: $"{connectionString}", tableName: "MyApplicationLogsSparkSchools")
                .CreateLogger();
        }

        // Log message 
        public void LogInfo(string message)
        {
            Log.Information($"{message}");
        }
        
         // Log an error
        public void LogError(string errorMessage)
        {
            Log.Error($"{errorMessage}");
        }

        
         // Log a warning
        public void LogWarning(string warningMessage)
        {
            Log.Warning($"{warningMessage}");
        }
    }
}
