using System.Linq;
using System.Net;
using Betzalel.SimpleMulticastAnalyzer.Config;
using Betzalel.SimpleMulticastAnalyzer.Infrastructure;
using Betzalel.SimpleMulticastAnalyzer.Net.Protocols;

namespace Betzalel.SimpleMulticastAnalyzer.Net
{
  public class Receiver : NetworkNode, INetworkNode
  {
    private readonly INetworkProtocol _networkProtocol;

    public Receiver(Log log, Statistics statistics, ProgramConfiguration programConfiguration)
      : base(log, programConfiguration)
    {
      var destinationEndPoint =
        new IPEndPoint(_programConfiguration.DestinationIpAddress, programConfiguration.DestinationPort);

      _networkProtocol =
        new UdpNetworkProtocol(
          log, statistics, destinationEndPoint, UdpNetworkProtocolType.Udp, _programConfiguration.PacketSize,
          _programConfiguration.VerifyOrder);

      var bindedIp =
        MachineAddress.FirstOrDefault(i => i.Equals(_programConfiguration.SourceIpAddress)) ?? IPAddress.Any;

      var bindedPort = programConfiguration.SourcePort ?? programConfiguration.DestinationPort;

      _networkProtocol.Bind(new IPEndPoint(bindedIp, bindedPort));

      if (programConfiguration.NetworkBufferSize.HasValue)
        _networkProtocol.SetReceiveBufferSize(programConfiguration.NetworkBufferSize.Value);
    }

    public void Start()
    {
      _log.Info("Start receiving packets from " + _networkProtocol.DestinationEndPoint + ".");

      _networkProtocol.StartListen();
    }

    public void Stop()
    {
      _networkProtocol.StopListen();

      _log.Info("Receiving packets stopped.");
    }
  }
}