using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Betzalel.SimpleMulticastAnalyzer.Config;
using Betzalel.SimpleMulticastAnalyzer.Infrastructure;

namespace Betzalel.SimpleMulticastAnalyzer.Net
{
  public class NetworkNode
  {
    protected readonly Log _log;
    protected readonly ProgramConfiguration _programConfiguration;

    private readonly Lazy<IPAddress[]> _machineAddress;

    protected IPAddress[] MachineAddress
    {
      get { return _machineAddress.Value; }
    }

    public NetworkNode(Log log, ProgramConfiguration programConfiguration)
    {
      _log = log;
      _programConfiguration = programConfiguration;

      _machineAddress =
        new Lazy<IPAddress[]>(() =>
          {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            return host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToArray();
          });
    }
  }
}