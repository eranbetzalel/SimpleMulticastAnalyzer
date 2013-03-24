using System;
using System.Net;

namespace Betzalel.SimpleMulticastAnalyzer.Net
{
  public interface INetworkProtocol : IDisposable
  {
    IPEndPoint DestinationEndPoint { get; }

    void Bind(EndPoint bindedIpAddress);
    void SendPackets(int amount);

    void StartListen();
    void StopListen();

    void SetSendBufferSize(int networkBufferSize);
    void SetReceiveBufferSize(int networkBufferSize);
  }
}