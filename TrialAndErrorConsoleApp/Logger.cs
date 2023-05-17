namespace TrialAndErrorConsoleApp
{
    public enum LogLevel
    {
        INFO,
        WARN,
        ERROR
    }

    public static class Logger
    {
        //private static string logFilePath = "log.txt";
        private static string logFilePath = @"C:\Users\nimish.ramteke\source\repos\TrialAndErrorConsoleApp\TrialAndErrorConsoleApp\log.txt";

        public static void LogMessage(LogLevel logLevel, string message)
        {
            string timestamp = DateTime.Now.ToString();

            ConsoleColor consoleColor = GetConsoleColor(logLevel);
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(message);
            Console.ResetColor();

            string logEntry = $"{timestamp} {logLevel.ToString().ToUpper()}: {message}";
            //using (StreamWriter sw = File.AppendText(logFilePath))
            //{
            //    sw.WriteLine(logEntry);
            //}

            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine(logEntry);
            }
        }

        private static ConsoleColor GetConsoleColor(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.INFO:
                    return ConsoleColor.White;
                case LogLevel.WARN:
                    return ConsoleColor.Yellow;
                case LogLevel.ERROR:
                    return ConsoleColor.Red;
                default:
                    return ConsoleColor.White;
            }
        }

        public static void PrintDivider()
        {
            LogMessage(LogLevel.INFO, "------------------------------------------------------------------------------------------------------------------------------------------------");
        }
    }
}
