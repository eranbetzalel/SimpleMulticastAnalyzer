using System;
using System.Text;
using System.Threading;
using Betzalel.SimpleMulticastAnalyzer.Config;
using Betzalel.SimpleMulticastAnalyzer.Infrastructure;

namespace Betzalel.SimpleMulticastAnalyzer
{
  public class Statistics
  {
    private readonly Log _log;
    private readonly StringBuilder _statisticsSummaryMessage;

    private DirectionTypes _directionType;
    private Timer _printStatisticsTimer;

    private ulong _sentPackets;
    private ulong _receivedPackets;
    private ulong _receivedPacketsMismatchSize;
    private ulong _receivedPacketsUnordered;

    public Statistics(Log log)
    {
      _log = log;
      _statisticsSummaryMessage = new StringBuilder(4096);
    }

    public void StartPrintStatistics(DirectionTypes directionType)
    {
      _directionType = directionType;

      switch (directionType)
      {
        case DirectionTypes.Sender:
          _log.Info(GetSenderSummaryHeader());
          break;
        case DirectionTypes.Receiver:
          _log.Info(GetReceiverSummaryHeader());
          break;
        default:
          throw new ArgumentOutOfRangeException("directionType");
      }

      _printStatisticsTimer = new Timer(PrintStatisticsTask, this, 1000, 1000);
    }

    public void Reset()
    {
      _sentPackets = 0;
      _receivedPackets = 0;
      _receivedPacketsMismatchSize = 0;
      _receivedPacketsUnordered = 0;
    }

    public void SentPacket()
    {
      _sentPackets++;
    }

    public void ReceivedPacket()
    {
      _receivedPackets++;
    }

    public void ReceivedPacketMismatchSize()
    {
      _receivedPacketsMismatchSize++;
    }

    public void ReceivedPacketUnordered(ulong amount)
    {
      _receivedPacketsUnordered += amount;
    }

    public string GetReceiverSummaryHeader()
    {
      return "Statistics - Received Packets |  Mismatch Size  |  Unordered";
    }

    public string GetReceiverSummary()
    {
      return
        string.Format(
          "{0,-16} | {1,-10} ({2}%) | {3,-10} ({4}%)",
          _receivedPackets,
          _receivedPacketsMismatchSize,
          GetPercentage(_receivedPacketsMismatchSize, _receivedPackets),
          _receivedPacketsUnordered,
          GetPercentage(_receivedPacketsUnordered, _receivedPackets));
    }

    public string GetSenderSummaryHeader()
    {
      return "Statistics - Sent packets";
    }

    public string GetSenderSummary()
    {
      return _sentPackets.ToString();
    }

    private static double GetPercentage(ulong part, ulong total)
    {
      return total == 0 ? 0 : Math.Round(part * 100 / (double)total, 2);
    }

    private void PrintStatisticsTask(object state)
    {
      _statisticsSummaryMessage.Clear();

      _statisticsSummaryMessage.Append("Statistics - ");

      switch (_directionType)
      {
        case DirectionTypes.Sender:
          _statisticsSummaryMessage.Append(GetSenderSummary());
          break;
        case DirectionTypes.Receiver:
          _statisticsSummaryMessage.Append(GetReceiverSummary());
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }

      _log.Info(_statisticsSummaryMessage.ToString());
    }

    ~Statistics()
    {
      if (_printStatisticsTimer != null)
        _printStatisticsTimer.Dispose();
    }
  }
}