using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

// State object for reading client data asynchronously  
public class StateObject
{
    // Client  socket.  
    public Socket workSocket = null;
    // Size of receive buffer.  
    public const int BufferSize = 1024;
    // Receive buffer.  
    public byte[] buffer = new byte[BufferSize];
    // Received data string.  
    public StringBuilder sb = new StringBuilder();

    public void Clear()
    {
        buffer = new byte[BufferSize];
        sb.Clear();

    }
}

public class AsynchronousSocketListener
{
    // Thread signal.  
    public static AutoResetEvent  allDone = new AutoResetEvent (false);

    public static List<StateObject> stateObjectList = new List<StateObject>();


    public AsynchronousSocketListener()
    {
    }

    public static void StartListening()
    {
        // Establish the local endpoint for the socket.  
        // The DNS name of the computer  
        // running the listener is "host.contoso.com".  
        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        for (int i = 0; i < ipHostInfo.AddressList.Length; i++)
        {
            var ip = ipHostInfo.AddressList[i];
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                ipAddress = ip;
                Console.WriteLine("Local IP Address: {0}", ip.ToString());
            }
        }
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

        // Create a TCP/IP socket.  
        Socket listener = new Socket(ipAddress.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);

        // Bind the socket to the local endpoint and listen for incoming connections.  
        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(100);

            while (true)
            {
                // Set the event to nonsignaled state.  
                allDone.Reset();

                // Start an asynchronous socket to listen for connections.  
                Console.WriteLine("Waiting for a connection...(press ctrl + c to quit)");
                listener.BeginAccept(
                    new AsyncCallback(AcceptCallback),
                    listener);

                // Wait until a connection is made before continuing.  
                allDone.WaitOne();
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("\nPress ENTER to continue...");
        Console.Read();

    }

    public static void AcceptCallback(IAsyncResult ar)
    {
        // Signal the main thread to continue.  
        allDone.Set();

        // Get the socket that handles the client request.  
        Socket listener = (Socket)ar.AsyncState;
        Socket handler = listener.EndAccept(ar);

        // Create the state object.  
        StateObject state = new StateObject();
        state.workSocket = handler;
        stateObjectList.Add(state);

        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
            new AsyncCallback(ReadCallback), state);
    }

    public static void ReadCallback(IAsyncResult ar)
    {
        String content = String.Empty;

        // Retrieve the state object and the handler socket  
        // from the asynchronous state object.  
        StateObject state = (StateObject)ar.AsyncState;
        Socket handler = state.workSocket;

        // Read data from the client socket.   
        int bytesRead = handler.EndReceive(ar);

        if (bytesRead > 0)
        {
            // There  might be more data, so store the data received so far.  
            state.sb.Append(Encoding.ASCII.GetString(
                state.buffer, 0, bytesRead));

            // Check for end-of-file tag. If it is not there, read   
            // more data.  
            content = state.sb.ToString();
            if (content.IndexOf("<EOF>") > -1)
            {
                // All the data has been read from the   
                // client. Display it on the console.  
                Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                    content.Length, content);
                // Echo the data back to the client.  
                foreach (var item in stateObjectList)
                {
                    var socket = item.workSocket;
                    Send(socket, content);
                }
                state.Clear();
            }

            // Not all data received. Get more.  
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
            new AsyncCallback(ReadCallback), state);

        }
    }

    private static void Send(Socket handler, String data)
    {
        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device.  
        handler.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), handler);
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket handler = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.  
            int bytesSent = handler.EndSend(ar);
            Console.WriteLine("Sent {0} bytes to client.", bytesSent);

            //handler.Shutdown(SocketShutdown.Both);
            //handler.Close();

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }


}



public class AsynchronousClient
{
    // The port number for the remote device.  
    private const int port = 11000;
    private static Robot robot;
    // AutoResetEvent  instances signal completion.  
    private static AutoResetEvent  connectDone =
        new AutoResetEvent (false);
    private static AutoResetEvent  sendDone =
        new AutoResetEvent (false);
    private static AutoResetEvent  receiveDone =
        new AutoResetEvent (false);

    // The response from the remote device.  
    private static String response = String.Empty;

    public static void StartClient(string ip, bool listener = false,Robot bot=null)
    {
        // Connect to a remote device.  
        try
        {
            // Establish the remote endpoint for the socket.  
            // The name of the   
            // remote device is "host.contoso.com".  
            //IPHostEntry ipHostInfo = Dns.GetHostEntry("host.contoso.com");

            ip.Replace(" ", string.Empty);
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.  
            Socket client = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Connect to the remote endpoint.  
            client.BeginConnect(remoteEP,
                new AsyncCallback(ConnectCallback), client);
            connectDone.WaitOne();

            if (bot != null)
            {
                robot = bot;
            }

            while (true)
            {
                if (!listener)
                {

                    var custom = Console.ReadLine();
                    if (custom == "q") { break; }
                    // Send test data to the remote device.  
                    custom += "<EOF>";
                    Send(client, custom);
                    sendDone.WaitOne();
                }

                // Receive the response from the remote device.  
                Receive(client);
                receiveDone.WaitOne();
               

            }

            // Release the socket.  
            client.Shutdown(SocketShutdown.Both);
            client.Close();

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket client = (Socket)ar.AsyncState;

            // Complete the connection.  
            client.EndConnect(ar);

            Console.WriteLine("Socket connected to {0}",
                client.RemoteEndPoint.ToString());

            // Signal that the connection has been made.  
            connectDone.Set();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private static void Receive(Socket client)
    {
        try
        {
            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = client;

            // Begin receiving the data from the remote device.  
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReceiveCallback), state);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the state object and the client socket   
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.workSocket;
            // Read data from the client socket.   
            int bytesRead = client.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.  
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read   
                // more data.  
                var content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the   
                    // client. Display it on the console.  
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                        content.Length, content);
                    if (content.Contains("rot"))
                    {
                        robot.Start();
                    }

                    receiveDone.Set();
                    state.Clear();
                }
                else
                {
                    // Not all data received. Get more.  
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
                }
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private static void Send(Socket client, String data)
    {
        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device.  
        client.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), client);
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket client = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.  
            int bytesSent = client.EndSend(ar);
            Console.WriteLine("Sent {0} bytes to server.", bytesSent);

            // Signal that all bytes have been sent.  
            sendDone.Set();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }


    
}
public class Robot
{

    [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern int Com_construct();

    [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool Com_setAddress(int id, [MarshalAs(UnmanagedType.LPStr)] string address);

    [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool Com_connect(int id);

    [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool Com_destroy(int id);

    [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern int OmniDrive_construct();

    [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool OmniDrive_setComId(int OdometryId, int ComId);

    [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool OmniDrive_setVelocity(int OmniDriveId, float Vx, float Vy, float Omega);

    [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool OmniDrive_destroy(int OmniDriveId);

    [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern float PowerManagement_voltage(int id);
    [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern int PowerManagement_construct();
    [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool PowerManagement_setComId(int id, int comId);
    [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool PowerManagement_destroy(int id);

    public int comId, driverID, powerID;
    public void Init(string ip)
    {
         comId = Com_construct();
        Console.WriteLine("comId={0}", comId);

        //bool ok = Com_setAddress(comId, "127.0.0.1");
        //bool ok = Com_setAddress(comId, "169.254.7.210");
        bool ok = Com_setAddress(comId, ip);
        Console.WriteLine("Com_setAddress returned {0}", ok);

        ok = Com_connect(comId);
        Console.WriteLine("Com_connect returned {0}", ok);

        driverID = OmniDrive_construct();
        Console.WriteLine("OmniDrive_construct returned {0}", driverID);

        ok = OmniDrive_setComId(driverID, comId);
        Console.WriteLine("OmniDrive_setComId returned {0}", ok);

        powerID = PowerManagement_construct();
        Console.WriteLine("PowerManagement_construct returned {0}", powerID);

        ok = PowerManagement_setComId(powerID, comId);
        Console.WriteLine("PowerManagement_setComId returned {0}", ok);


    }

    ~Robot()
    {
        var ok = OmniDrive_destroy(driverID);
        Console.WriteLine("OmniDrive_destroy returned {0}", ok);
        Console.ReadKey();

        ok = Com_destroy(comId);
        Console.WriteLine("Com_destroy returned {0}", ok);
        Console.ReadKey();
    }
    public void Start( )
    {
            var ok = OmniDrive_setVelocity(driverID, 0.00f, 0.0f, 1f);
            Console.WriteLine("OmniDrive_setVelocity returned {0}", ok);
            var value = PowerManagement_voltage(powerID);
            Console.WriteLine("PowerManagement_voltage returned {0}", value);
         

    }
}