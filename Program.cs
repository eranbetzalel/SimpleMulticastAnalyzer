using System;
using Betzalel.SimpleMulticastAnalyzer.Config;
using Betzalel.SimpleMulticastAnalyzer.Infrastructure;
using Betzalel.SimpleMulticastAnalyzer.Net;

namespace Betzalel.SimpleMulticastAnalyzer
{
  class Program
  {
    static void Main(string[] args)
    {
      var log = new Log();
      var statistics = new Statistics(log);

      var programConfiguration = new ProgramConfiguration();

      if (!programConfiguration.Initialize(args))
        return;

      log.IsVerbose = programConfiguration.IsVerbose;

      INetworkNode networkNode;

      switch (programConfiguration.Direction)
      {
        case DirectionTypes.Sender:
          {
            networkNode = new Sender(log, statistics, programConfiguration);

            log.Info("Application initialized - press Escape to exit.");
          }
          break;
        case DirectionTypes.Receiver:
          {
            networkNode = new Receiver(log, statistics, programConfiguration);

            log.Info("Application initialized - press Escape to exit.");
          }
          break;
        default:
          throw new Exception("Invalid network node direction.");
      }

      networkNode.Start();
      statistics.StartPrintStatistics(programConfiguration.Direction);

      while (Console.ReadKey(true).Key != ConsoleKey.Escape)
      {
      }

      log.Info("Application shutting down...");
    }
  }
}
