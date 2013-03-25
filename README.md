Simple Multicast Analyzer
===========================

This applicaiton can send/receive multicast packets and analayze their status in real time.

Usage
======

    Use as sender:

      SimpleMulticastAnalyzer.exe -sender -dest-ip=X -dest-port=X [-src-ip=X] [-src-port=X] [-size=X] [-rate=X] 
      [-buffer=X] [-verify] [-verbose]

    Use as receiver:

      SimpleMulticastAnalyzer.exe -receiver -dest-ip=X -dest-port=X [-size=X] [-rate=X] [-buffer=X] [-verify] [-verbose]

    -h            Shows the usage screen.
    -sender       Set as a sender network node.
    -receiver     Set as a receiver network node.
    -src-ip       Source IP (bind to specific network adapter).
    -src-port     Source port number.
    -dest-ip      Remote destination IP.
    -dest-port    Remote destination port number.
    -size         Packet size (bytes) to send/receive.
    -rate         Packet to send/receive per second.
    -buffer       Network buffer size (bytes) for pending incoming/outgoing packets.
    -verify       Verify the packets order.
    -verbose      Print debug information.

Download
===========
Latest version: [SimpleMulticastAnalyzer.exe](https://docs.google.com/file/d/0B_zYyPNRGrlMcUpITFA4a1hyVm8/edit?usp=sharing).
