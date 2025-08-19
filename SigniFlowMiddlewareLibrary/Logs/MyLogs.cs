using Serilog;

namespace SigniflowMiddlewareCSharp.Loggings
{
    public class MyLogs
    {
        string connString;
        public MyLogs( string connectionString )
        {
            this.connString = connectionString;

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
