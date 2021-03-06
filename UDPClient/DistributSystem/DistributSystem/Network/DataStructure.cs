﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace DistributeSystem
{
    public enum MyTask
    {
        None,
        Close,
        StartClient,
        Pack_Send,
        RobotAgent_Para,
        RobotAgent_Pulse
    }
    public enum DSEvent
    {
        None,
        Error,
        StartClient,
        Receive,
        Released,
        Agent
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
    public class DataStructure
    {

    }
}
