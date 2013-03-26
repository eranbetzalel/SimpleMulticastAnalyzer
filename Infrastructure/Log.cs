using System;

namespace Betzalel.SimpleMulticastAnalyzer.Infrastructure
{
  public class Log
  {
    public bool IsVerbose { get; set; }

    public void Debug(Func<string> messageMethod)
    {
      if (!IsVerbose)
        return;

      LogMessage("DEBUG", messageMethod());
    }

    public void Info(string message)
    {
      LogMessage("INFO", message);
    }

    public void Warn(string message)
    {
      LogMessage("WARN", message);
    }

    public void Error(string message)
    {
      LogMessage("ERROR", message);
    }

    public void Error(string message, Exception exception)
    {
      LogMessage("ERROR", message + "\r\n\r\nException details:\r\n" + exception);
    }

    private static void LogMessage(string logType, string message)
    {
      Console.WriteLine("{0,-22} - {1,-5} - {2}", DateTime.Now.ToString("dd/MM/yy HH:mm:ss.ffff"), logType, message);
    }
  }
}