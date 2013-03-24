using System;
using System.Linq;
using System.Net;
using Betzalel.SimpleMulticastAnalyzer.Config;
using Betzalel.SimpleMulticastAnalyzer.Infrastructure;
using Betzalel.SimpleMulticastAnalyzer.Net.Protocols;

namespace Betzalel.SimpleMulticastAnalyzer.Net
{
  public class Sender : NetworkNode, INetworkNode
  {
    private readonly INetworkProtocol _networkProtocol;
    private readonly HighResolutionTimer _sendDataTask;

    public Sender(Log log, Statistics statistics, ProgramConfiguration programConfiguration)
      : base(log, programConfiguration)
    {
      var destinationEndPoint =
        new IPEndPoint(_programConfiguration.DestinationIpAddress, programConfiguration.DestinationPort);

      _networkProtocol =
        new UdpNetworkProtocol(
          log, statistics, destinationEndPoint, UdpNetworkProtocolType.Udp, _programConfiguration.PacketSize,
          _programConfiguration.VerifyOrder);

      _sendDataTask =
        new HighResolutionTimer
          {
            Mode = TimerMode.Periodic,
            Period = 1000,
            Resolution = 0,
            IsAsync = true
          };

      _sendDataTask.Tick += (sender, args) => SendData();

      var bindedIp =
        MachineAddress.FirstOrDefault(i => i.Equals(_programConfiguration.SourceIpAddress)) ?? IPAddress.Any;

      var bindedPort = programConfiguration.SourcePort ?? 0;

      _networkProtocol.Bind(new IPEndPoint(bindedIp, bindedPort));

      if (programConfiguration.NetworkBufferSize.HasValue)
        _networkProtocol.SetSendBufferSize(programConfiguration.NetworkBufferSize.Value);
    }

    public void Start()
    {
      _log.Info("Start sending packets to " + _networkProtocol.DestinationEndPoint + ".");

      if (_sendDataTask.IsRunning)
        throw new Exception("The sender is already running.");

      _sendDataTask.Start();
    }

    public void Stop()
    {
      if (!_sendDataTask.IsRunning)
        throw new Exception("The sender is not running.");

      _sendDataTask.Stop();
      _sendDataTask.Dispose();

      _log.Info("Sending packets stopped.");
    }

    private void SendData()
    {
      try
      {
        _networkProtocol.SendPackets(_programConfiguration.PacketsPerSecond);
      }
      catch (Exception e)
      {
        _log.Error("Failed to send packets.", e);
      }
    }
  }
}