using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace DistributeSystem
{
    public enum NetworkType
    {
        None, Server, Client
    }
    public enum MyTask
    {
        None,
        StartServer,
        WaitForAConnection,
        StartClient,
        ServerClose,
        ClientClose,
        ClientSend,
        ServerSend,
        ServerBoardCast,
        ServerMultiSend
    }
    public enum DSEvent
    {
        None,
        StartServerSuccessful,
        StartServerFailure,
        StartClientSuccessful,
        StartClientFailure,
        ClientSendAndReceive,
        Error,
        ClientSend
    }
    public class NetworkConfig
    {
        public NetworkType networkType = NetworkType.None;
        public string localIP = "";
        public string serverIP = "";
        public int port = 11000;
    }
    public class StateObject
    {
        // Client  socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 1024;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public List<byte> data = new List<byte>();

        public void Clear()
        {
            buffer = new byte[BufferSize];
            data = new List<byte>();
        }
    }
}
