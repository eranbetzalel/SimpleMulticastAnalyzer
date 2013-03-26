using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Betzalel.SimpleMulticastAnalyzer.Infrastructure;

namespace Betzalel.SimpleMulticastAnalyzer.Net.Protocols
{
  public class UdpNetworkProtocol : INetworkProtocol
  {
    private const int PacketNumberTypeLength = 8;

    private readonly Log _log;
    private readonly Statistics _statistics;

    private readonly Socket _socket;
    private readonly byte[] _sendBuffer;
    private readonly CancellationTokenSource _listenCancelToken;
    private readonly int _packetDataSize;
    private readonly bool _verifyOrder;
    private ulong _currentPacketNumber;

    public IPEndPoint DestinationEndPoint { get; private set; }

    public UdpNetworkProtocol(
      Log log,
      Statistics statistics,
      IPEndPoint destinationEndPoint,
      UdpNetworkProtocolType udpNetworkProtocolType,
      int packetDataSize,
      bool verifyOrder)
    {
      _log = log;
      _statistics = statistics;
      _packetDataSize = packetDataSize;
      _verifyOrder = verifyOrder;

      if (verifyOrder && _packetDataSize < PacketNumberTypeLength)
        throw new ArgumentOutOfRangeException(
          "packetDataSize",
          packetDataSize,
          "Order verification require packet size bigger than " + PacketNumberTypeLength + ".");

      _sendBuffer = Enumerable.Repeat<byte>(0xFE, _packetDataSize).ToArray();
      _listenCancelToken = new CancellationTokenSource();

      DestinationEndPoint = destinationEndPoint;

      SocketType socketType;
      ProtocolType protocolType;

      switch (udpNetworkProtocolType)
      {
        case UdpNetworkProtocolType.Udp:
          socketType = SocketType.Dgram;
          protocolType = ProtocolType.Udp;
          break;
        case UdpNetworkProtocolType.Pgm:
          socketType = SocketType.Rdm;
          protocolType = (ProtocolType)113;
          break;
        default:
          throw new ArgumentOutOfRangeException("udpNetworkProtocolType");
      }

      _socket = new Socket(AddressFamily.InterNetwork, socketType, protocolType);
    }

    public void Bind(EndPoint bindedIpAddress)
    {
      _socket.Bind(bindedIpAddress);

      _log.Info("Binded to " + bindedIpAddress);
    }

    public void SendPackets(int amount)
    {
      for (var i = 0; i < amount; i++)
      {
        if (_verifyOrder)
          Buffer.BlockCopy(BitConverter.GetBytes(_currentPacketNumber), 0, _sendBuffer, 0, PacketNumberTypeLength);

        _socket.SendTo(_sendBuffer, DestinationEndPoint);

        if (_verifyOrder)
          _currentPacketNumber++;

        _statistics.SentPacket();
      }

      _log.Debug(() => "Packets sent to " + DestinationEndPoint);
    }

    public void StartListen()
    {
      _log.Info("Start listening on " + DestinationEndPoint.Address + "...");

      _socket.SetSocketOption(
        SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(DestinationEndPoint.Address, IPAddress.Any));

      Task.Factory.StartNew(
        StartListenTask, null, _listenCancelToken.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    public void StopListen()
    {
      _log.Info("Stop listening on " + _socket.LocalEndPoint + "...");

      _socket.SetSocketOption(
        SocketOptionLevel.IP, SocketOptionName.DropMembership, new MulticastOption(DestinationEndPoint.Address));

      _listenCancelToken.Cancel(true);
    }

    public void SetSendBufferSize(int networkBufferSize)
    {
      _socket.SendBufferSize = networkBufferSize;
    }

    public void SetReceiveBufferSize(int networkBufferSize)
    {
      _socket.ReceiveBufferSize = networkBufferSize;
    }

    public void Dispose()
    {
      StopListen();

      _listenCancelToken.Dispose();
      _socket.Dispose();
    }

    private void StartListenTask(object state)
    {
      try
      {
        var receiveBuffer = new byte[1500];

        while (!_listenCancelToken.IsCancellationRequested)
        {
          var receiveBufferSize = _socket.Receive(receiveBuffer);

          if (_verifyOrder)
          {
            var receivedPacketNumber = BitConverter.ToUInt64(receiveBuffer, 0);

            if (receivedPacketNumber == 0)
            {
              _statistics.Reset();
            }
            else
            {
              var packetNumberDelta = receivedPacketNumber - _currentPacketNumber;

              if (packetNumberDelta > 1)
                _statistics.ReceivedPacketUnordered(packetNumberDelta - 1);
            }

            _currentPacketNumber = receivedPacketNumber;
          }

          if (receiveBufferSize != _packetDataSize)
            _statistics.ReceivedPacketMismatchSize();

          _statistics.ReceivedPacket();
        }
      }
      catch (OperationCanceledException)
      {
        //  StopListen() was called
      }
      catch (Exception e)
      {
        _log.Error("An error occurred while receiving data.", e);
      }
    }
  }
}