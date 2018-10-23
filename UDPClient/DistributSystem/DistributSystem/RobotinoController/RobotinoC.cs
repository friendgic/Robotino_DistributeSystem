using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace RobotinoController
{
    public class CDLL
    {
        #region Com
        /// <summary>
        /// Construct an interface for communicating to one Robotino
        /// </summary>
        /// <returns>Returns the ID of the newly constructed communication interface.</returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Com_construct();

        /// <summary>
        /// Destroy the communication interface assigned to id
        /// </summary>
        /// <param name="id">The id of the communication interface to be destroyed</param>
        /// <returns>Returns TRUE (1) on success. Returns FALSE (0) if the given ComId is invalid.</returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Com_destroy(int id);

        /// <summary>
        /// The default to connect to Robotino is 172.26.1.1 (port can be omitted.
        /// To connect to RobotinoSim running at localhost use 127.0.0.1:8080 (or higher ports).
        /// </summary>
        /// <param name="id">The ComId returned by Com_construct().</param>
        /// <param name="address">A null terminated string containing the (IP)-address (plus port) of Robotino.</param>
        /// <returns>Returns TRUE (1) on success. Returns FALSE (0) if the given ComId is invalid.</returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Com_setAddress(int id, [MarshalAs(UnmanagedType.LPStr)] string address);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">The ComId returned by Com_construct().</param>
        /// <param name="addressBuffer">Will contain the currently active server address set with Com_setAddress() as '\0' terminated string.</param>
        /// <param name="addressBuffersSize">The size of addressBuffer.</param>
        /// <returns></returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Com_address(int id, [MarshalAs(UnmanagedType.LPStr)] string addressBuffer, uint addressBuffersSize);

        /// <summary>
        /// running on your machine. The default port is 8080, but you might change this.
        /// </summary>
        /// <param name="id">The ComId returned by Com_construct().</param>
        /// <param name="port"> The image server port. To be able to receive images from Robotino there is a UDP server</param>
        /// <returns></returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Com_setImageServerPort(int id, int port);

        /// <summary>
        /// Establish the communication. Call Com_setAddress() first.
        /// </summary>
        /// <param name="id">The ComId returned by Com_construct().</param>
        /// <returns></returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Com_connect(int id);

        /// <summary>
        /// Stop communication.
        /// </summary>
        /// <param name="id">The ComId returned by Com_construct().</param>
        /// <returns>Returns TRUE (1) on success. Returns FALSE (0) if the given ComId is invalid.</returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Com_disconnect(int id);

        /// <summary>
        /// Check if the communication is established.
        /// </summary>
        /// <param name="id">The ComId returned by Com_construct().</param>
        /// <returns> Returns TRUE (1) if the communication is active. Returns FALSE (0) if the communication is inactive or if the given ComId is invalid.</returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Com_isConnected(int id);


        #endregion

        #region OmniDrive
        /// <summary>
        /// Construct an OmniDrive object
        /// </summary>
        /// <returns>Returns the ID of the newly constructed OmniDrive object.</returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int OmniDrive_construct();

        /// <summary>
        /// Destroy the OmniDrive object assigned to id
        /// </summary>
        /// <param name="OmniDriveId">The id of the OmniDrive object to be destroyed</param>
        /// <returns> Returns TRUE (1) on success. Returns FALSE (0) if the given OmniDriveId is invalid.</returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool OmniDrive_destroy(int OmniDriveId);

        /// <summary>
        /// Associated an OmniDrive object with a communication interface, i.e. binding the OmniDrive to a specific Robotino
        /// </summary>
        /// <param name="OdometryId"></param>
        /// <param name="ComId"></param>
        /// <returns> Returns TRUE (1) on success. Returns FALSE (0) if the given OmniDriveId or ComId is invalid.</returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool OmniDrive_setComId(int OdometryId, int ComId);

        /// <summary>
        /// Drive Robotino associated with id
        /// </summary>
        /// <param name="OmniDriveId"></param>
        /// <param name="Vx">Velocity in x-direction in m/s</param>
        /// <param name="Vy">Velocity in y-direction in m/s</param>
        /// <param name="Omega">Angular velocity in deg/s</param>
        /// <returns></returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool OmniDrive_setVelocity(int OmniDriveId, float Vx, float Vy, float Omega);

        /// <summary>
        /// Project the velocity of the robot in cartesian coordinates to single motor speeds.
        /// </summary>
        /// <param name="OmniDriveId"></param>
        /// <param name="m1">The resulting speed of motor 1 in rpm</param>
        /// <param name="m2">The resulting speed of motor 2 in rpm</param>
        /// <param name="m3">The resulting speed of motor 3 in rpm</param>
        /// <param name="vx">Velocity in x-direction in mm/s</param>
        /// <param name="vy">Velocity in y-direction in mm/s</param>
        /// <param name="omega">Angular velocity in deg/s</param>
        /// <returns></returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool OmniDrive_project(int OmniDriveId, [In, Out]  float m1, [In, Out] float m2, [In, Out] float m3, float vx, float vy, float omega);
        #endregion

        #region PowerManagement
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int PowerManagement_construct();

        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool PowerManagement_destroy(int id);

        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool PowerManagement_setComId(int id, int comId);

        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern float PowerManagement_current(int id);

        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern float PowerManagement_voltage(int id);

        #endregion

        #region Bumper
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Bumper_construct();

        #endregion

        #region DistanceSensor
        /// <summary>
        /// Construct an distance sensor object
        /// </summary>
        /// <param name="n">The input number. Range [0; numDistanceSensors()-1]</param>
        /// <returns></returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Bumper_construct(uint n);

        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int DistanceSensor_construct(uint n);

        /// <summary>
        /// Destroy the Digital input object assigned to id
        /// </summary>
        /// <param name="DistanceSensorId">The id of the distance sensor object to be destroyed</param>
        /// <returns></returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DistanceSensor_destroy(int DistanceSensorId);

        /// <summary>
        /// Associated a distance sensor object with a communication interface, i.e. binding the distance sensor to a specific Robotino
        /// </summary>
        /// <param name="DistanceSensorId"></param>
        /// <param name="comID"></param>
        /// <returns></returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DistanceSensor_setComId(int DistanceSensorId, int comID);

        /// <summary>
        ///  Sets the number of this distance sensor.
        /// </summary>
        /// <param name="DistanceSensorId"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DistanceSensor_setSensorNumber(int DistanceSensorId, int n);

        /// <summary>
        /// Returns the number of distance sensors.
        /// </summary>
        /// <returns></returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int numDistanceSensors();

        /// <summary>
        /// Returns the current value of the specified input device.
        /// </summary>
        /// <param name="DistanceSensorId"></param>
        /// <returns></returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern float DistanceSensor_voltage(int DistanceSensorId);

        /// <summary>
        /// Returns the heading of this distance sensor.
        /// </summary>
        /// <param name="DistanceSensorId"></param>
        /// <returns>The heading in degrees. [0; 360]</returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint DistanceSensor_heading(int DistanceSensorId);

        #endregion

        #region Camera
        /// <summary>
        /// Construct an Camera object
        /// </summary>
        /// <returns></returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Camera_construct();

        /// <summary>
        /// Destroy the Camera object assigned to id
        /// </summary>
        /// <param name="id"> The id of the Camera object to be destroyed</param>
        /// <returns></returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Camera_destroy(uint id);

        /// <summary>
        /// Associated an Camera object with a communication interface, i.e. binding the Camera to a specific Robotino
        /// </summary>
        /// <param name="id"></param>
        /// <param name="comId"></param>
        /// <returns></returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Camera_setComId(uint id, uint comId);

        /// <summary>
        /// Grab image.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>return Returns TRUE (1) if a new image is available since the last call of Camera_grab. Returns FALSE (0) otherwise.</returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Camera_grab(uint id);

        /// <summary>
        /// Size of image aquired by grab.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Camera_imageSize(uint id, [In, Out]uint width, [In, Out] uint height);

        /// <summary>
        /// Get Robotino's camera image. Do not forget to call Camera_setStreaming( id, TRUE )  and Camera_grab first.
        /// Get the size of the image by Camera_imageSize first.imageBufferSize must be at least 3*width* height.The image
        /// copied to image buffer is an interleaved RGB image width 3 channels and 1 byte per channel.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="imageBuffer"> The image is copied to imageBuffer.</param>
        /// <param name="imageBufferSize"> The size (number of bytes) of imageBuffer.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <returns></returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Camera_getImage(uint id, byte[] imageBuffer, uint imageBufferSize, [In, Out]uint width, [In, Out]uint height);


        /// <summary>
        /// Start/Stop streaming of camera images
        /// </summary>
        /// <param name="id"></param>
        /// <param name="streaming">streaming If TRUE (1) streaming is started. Otherwise streaming is stopped.</param>
        /// <returns></returns>
        [DllImport("rec_robotino_api2.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Camera_setStreaming(uint id, bool streaming);
        #endregion
    }

    public class Robotino
    {
        private int com;
        private int omniDrive;
        private int[] distanceSensor=new int[9];

        public delegate void ChangedEvent(float speedX, float speedY, float rot);
        public event ChangedEvent mChangedEvent;

        public void Connect(string ip)
        {
            com = CDLL.Com_construct();

            if (!CDLL.Com_setAddress(com, ip)) throw new Exception("SetAddress error");
            if (!CDLL.Com_connect(com)) throw new Exception("Cann't connect with robotino");

            omniDrive = CDLL.OmniDrive_construct();
            for (int i = 0; i < 9; i++)
            {
                distanceSensor[i] = CDLL.DistanceSensor_construct((uint)i);
                if (!CDLL.DistanceSensor_setComId(distanceSensor[i], com)) throw new Exception("Cann't set DistanceSensor");
            }
            if (!CDLL.OmniDrive_setComId(omniDrive, com)) throw new Exception("Cann't set OmniDrive");
             
         
        }
        public void DisConnect()
        {
            if (!CDLL.Com_disconnect(com)) throw new Exception("Disconnect fail!");
            if (!CDLL.OmniDrive_destroy(omniDrive)) throw new Exception("Disconnect fail!");
            for(int i = 0; i < 9; i++)
            {
            if (!CDLL.DistanceSensor_destroy(distanceSensor[i])) throw new Exception("Disconnect fail!");

            }

        }
        public void ManualControlMove(float speedX, float speedY, float rot)
        {
            if (mChangedEvent != null)
            {
                mChangedEvent.Invoke(speedX, speedY,  rot);
            }
            if (!CDLL.OmniDrive_setVelocity(omniDrive, speedY, speedX, rot)) throw new Exception("Control error"); ;
        }
        public float ReadDistanceSensor(int n)
        {
            return CDLL.DistanceSensor_voltage(distanceSensor[n]);

        }
    }
}