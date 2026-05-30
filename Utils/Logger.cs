namespace RimWorldModManager.Utils
{
    public interface ILogger
    {
        void Info(string message);
        void Warning(string message);
        void Error(string message);
        void Error(string message, Exception ex);
    }

    public class ConsoleLogger : ILogger
    {
        public void Info(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[INFO] {message}");
            Console.ResetColor();
        }

        public void Warning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[WARN] {message}");
            Console.ResetColor();
        }

        public void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] {message}");
            Console.ResetColor();
        }

        public void Error(string message, Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] {message}");
            Console.WriteLine($"[EXCEPTION] {ex.ToString()}");
            Console.ResetColor();
        }
    }
}