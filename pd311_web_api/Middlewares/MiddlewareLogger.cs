using System.Text;
using System.Text.Json;

namespace pd311_web_api.Middlewares
{
    public class MiddlewareLogger
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MiddlewareLogger> _logger;
        private readonly string _logFilePath = "logs/requests.log";

        public MiddlewareLogger(RequestDelegate next, ILogger<MiddlewareLogger> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            string logEntry = JsonSerializer.Serialize(new
            {
                Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                IP = context.Connection.RemoteIpAddress?.ToString(),
                Method = context.Request.Method,
                URL = context.Request.Path
            });

            _logger.LogInformation(logEntry);
            await WriteLogToFile(logEntry);

            await _next(context);
        }

        private async Task WriteLogToFile(string log)
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(_logFilePath)!;
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                await File.AppendAllTextAsync(_logFilePath, log + Environment.NewLine, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to write log to file: {ex.Message}");
            }
        }
    }
}
