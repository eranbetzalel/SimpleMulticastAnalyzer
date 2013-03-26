using System;
using System.Linq;
using System.Net;
using Betzalel.SimpleMulticastAnalyzer.Properties;

namespace Betzalel.SimpleMulticastAnalyzer.Config
{
  public class ProgramConfiguration
  {
    private string[] _applicationArguments;

    public DirectionTypes Direction { get; private set; }
    public IPAddress DestinationIpAddress { get; private set; }
    public int DestinationPort { get; private set; }
    public IPAddress SourceIpAddress { get; private set; }
    public int? SourcePort { get; private set; }
    public int PacketSize { get; private set; }
    public int PacketsPerSecond { get; private set; }
    public bool VerifyOrder { get; private set; }
    public int? NetworkBufferSize { get; private set; }
    public bool IsVerbose { get; private set; }

    public bool Initialize(string[] applicationArguments)
    {
      _applicationArguments = applicationArguments;

      try
      {
        if (DoesArgumentExist("h", false))
        {
          ShowUsage();

          return false;
        }

        var isSender = DoesArgumentExist("sender", false);
        var isReceiver = DoesArgumentExist("receiver", false);

        if (isSender)
          Direction = DirectionTypes.Sender;
        else if (isReceiver)
          Direction = DirectionTypes.Receiver;
        else
          throw new Exception("Has to be sender or receiver.");

        SourceIpAddress = (IPAddress)GetArgumentValue("src-ip", false, IPAddress.Parse);
        SourcePort = (int?)GetArgumentValue("src-port", false, int.Parse);
        DestinationIpAddress = (IPAddress)GetArgumentValue("dest-ip", true, IPAddress.Parse);
        DestinationPort = (int)GetArgumentValue("dest-port", true, int.Parse);

        PacketSize = (int?)GetArgumentValue("size", false, int.Parse) ?? 100;
        PacketsPerSecond = (int?)GetArgumentValue("rate", false, int.Parse) ?? 1;
        NetworkBufferSize = (int?)GetArgumentValue("buffer", false, int.Parse);
        VerifyOrder = DoesArgumentExist("verify", false);
        IsVerbose = DoesArgumentExist("verbose", false);
      }
      catch (Exception e)
      {
        ShowUsage(e.Message);

        return false;
      }

      return true;
    }

    private bool DoesArgumentExist(string argumentName, bool isRequired)
    {
      return (bool?)GetArgument<bool>(argumentName, isRequired, false, null) ?? false;
    }

    private object GetArgumentValue<T>(
      string argumentName, bool isRequired, Func<string, T> parsingMethod)
    {
      return GetArgument(argumentName, isRequired, true, parsingMethod);
    }

    private object GetArgument<T>(
      string argumentName, bool isRequired, bool hasValue, Func<string, T> parsingMethod)
    {
      var consoleArgument = "-" + argumentName;

      var argument =
        _applicationArguments.FirstOrDefault(a => a.StartsWith(consoleArgument));

      if (argument == null)
      {
        if (isRequired)
          throw new Exception("Required argument \"" + argumentName + "\" is missing.");

        return null;
      }

      var argumentParts = argument.Split('=');

      if (!hasValue)
      {
        if (argumentParts.Count() > 1)
          throw new Exception("Non value argument \"" + argumentName + "\" has value.");

        return true;
      }

      if (argumentParts.Count() < 2)
        throw new Exception("Value argument \"" + argumentName + "\" has no value.");

      try
      {
        return parsingMethod(argumentParts[1]);
      }
      catch (Exception e)
      {
        throw new Exception("Failed to parse argument \"" + argumentName + "\".", e);
      }
    }

    private static void ShowUsage(string errorMessage = null)
    {
      Console.WriteLine(
        (string.IsNullOrWhiteSpace(errorMessage)
          ? string.Empty
          : "Argument error:\r\n" + errorMessage + "\r\n\r\n") +
            "Usage:\r\n" + Resources.Usage);
    }
  }
}