//  Copyright (C) 2004-2008, Robotics Equipment Corporation GmbH

#define _USE_MATH_DEFINES

#include <opencv2/opencv.hpp>
#include <opencv2/core/core.hpp>
#include <opencv2/highgui/highgui.hpp>
#include <opencv2/imgproc/imgproc.hpp>
#include <cmath>
#include <iostream>
#include <stdlib.h>
#include "cv.h"


#ifdef WIN32
#include <windows.h>
#else
#include <signal.h>
#endif

#include <sstream>
#include <stdio.h>
#include <stdlib.h>
#include <fstream>
#include <string>
#include <vector>

#include "rec/robotino/api2/all.h"

using namespace rec::robotino::api2;
using namespace std;
using namespace cv;

bool _run = true;


#ifdef WIN32 
static BOOL WINAPI sigint_handler(DWORD fdwCtrlType)
{
	switch (fdwCtrlType)
	{
	case CTRL_C_EVENT: // Handle the CTRL-C signal.
		_run = false;
		return TRUE;

	default:
		return FALSE;
	}
}
#else
void sigint_handler(int signum)
{
	_run = false;
}
#endif

class RobotinoData
{
public:

	float vx = 0;
	float vy = 0;

private:
};

RobotinoData FollowBalldata;

class WangJQ
{
public:

	float delt_t;
	float tt;
	float Vx;
	float Vz;

	float Vx_Robot = 0;
	float Vz_Robot = 0;
	float Vsita_Robot = 0;

	float X_Robot = 0;
	float Z_Robot = 0;


	Mat M_int = (Mat_<float>(3, 3) << 620, 0, 320, 0, 620, 240, 0, 0, 1);
	/*Mat M_int = (Mat_<float>(3, 3) << 617.9, 0, 330.6, 0, 619.6, 251.87, 0, 0, 1);*/
	float w_camera = 60;
	float arcsita = 0.356;
	float sita = asin(arcsita);
	float Y_c = 371 - 33.3 - w_camera * arcsita;
	float X_c;
	float Z_c;
	int seq = 0;

	float t[10000];
	float X[10000];
	float Z[10000];

	float FactorofR = 2*10^4;
	Mat R = FactorofR * Mat::eye(Size(2, 2), CV_32FC1);  // R matrix
	Mat C = Mat::eye(Size(4, 2), CV_32FC1);   // C matrix
	Mat measurement = Mat(2, 1, CV_32FC1, Scalar::all(0));

	Mat I = Mat::eye(Size(4, 4), CV_32FC1);
	Mat A = Mat::eye(Size(4, 4), CV_32FC1);
	Mat P = 2000 * (Mat::eye(Size(4, 4), CV_32FC1));
	Mat state = Mat::eye(Size(1, 4), CV_32FC1);
	Mat K = Mat(4, 2, CV_32FC1, Scalar::all(0));            // K matrix
	Mat Q = 5 * Mat::eye(Size(4, 4), CV_32FC1);
	/*Mat w = Mat::eye(Size(1, 4), CV_32FC1);*/

};

WangJQ KalmanData;


class MyCom : public Com
{
public:
	MyCom()
		: Com("Robotino_Wang_new")
	{
	}

	void errorEvent(const char* errorString)
	{
		std::cerr << "Error: " << errorString << std::endl;
	}

	void connectedEvent()
	{
		std::cout << "Connected." << std::endl;
	}

	void connectionClosedEvent()
	{
		std::cout << "Connection closed." << std::endl;
	}

	void logEvent(const char* message, int level)
	{
		std::cout << message << std::endl;
	}

	void pingEvent(float timeMs)
	{
		std::cout << "Ping: " << timeMs << "ms" << std::endl;
	}
};


MyCom com;
Motor motor[3];

class MyBumper : public Bumper
{
public:
	MyBumper()
		: bumped(false)
	{
	}

	void bumperEvent(bool hasContact)
	{
		bumped |= hasContact;
		std::cout << "Bumper has " << (hasContact ? "contact" : "no contact") << std::endl;
	}

	bool bumped;
};


/////////////////////////////////////////////////////////////// Find Ball/////////////////////////////////////////////////////////////////////////////////////////




class MyCamera : public Camera
{


public:
	MyCamera()
	{
	}

	void imageReceivedEvent(const unsigned char* data,
		unsigned int dataSize,
		unsigned int width,
		unsigned int height,
		unsigned int step)
	{

		// show realtime image without image documents

		Mat frame(height, width, CV_8UC3);  //get frame from the camera

		memcpy(frame.data, data, height*width * 3);

		Mat frame_RGB;
		cvtColor(frame, frame_RGB, CV_BGR2RGB);

		medianBlur(frame_RGB, frame_RGB, 3);

		Mat hsv_image;
		cvtColor(frame_RGB, hsv_image, COLOR_BGR2HSV);

		//Threshold the HSV image, keep only the red pixels
		Mat lower_red_hue_range;
		Mat upper_red_hue_range;
		inRange(hsv_image, Scalar(160, 100, 100), Scalar(179, 255, 255), upper_red_hue_range);

		inRange(hsv_image, Scalar(0, 100, 100), Scalar(10, 255, 255), lower_red_hue_range);

		// Combine the above two images
		Mat red_hue_image;
		addWeighted(lower_red_hue_range, 1.0, upper_red_hue_range, 1.0, 0.0, red_hue_image);

		GaussianBlur(red_hue_image, red_hue_image, Size(9, 9), 2, 2);

		// Use the Hough transform to detect circles in the combined threshold image
		vector<Vec3f> circles;
		HoughCircles(red_hue_image, circles, CV_HOUGH_GRADIENT, 1, red_hue_image.rows / 8, 100, 20, 0, 0);


		for (int i = 0; (static_cast<unsigned int>(i) < circles.size()); i++)
		{


			bool flag = false;

			if (circles.size() > 0 && circles[0][0] > 0)
			{
				flag = true;

				// draw green circle at center of object detected
				cv::circle(frame_RGB, cv::Point((int)circles[i][0], (int)circles[i][1]), 3, cv::Scalar(0, 255, 0), CV_FILLED);

				// draw green circle around object detected
				cv::circle(frame_RGB, cv::Point((int)circles[i][0], (int)circles[i][1]), (int)circles[i][2], cv::Scalar(0, 255, 0), 2);



				KalmanData.Z_c = KalmanData.Y_c / tan(atan((circles[i][1] - KalmanData.M_int.at<float>(1, 2)) / KalmanData.M_int.at<float>(1, 1)) + KalmanData.sita);    //The position of the center of the ball relative to the camera coordinate system


				KalmanData.X_c = (circles[i][0] - KalmanData.M_int.at<float>(0, 2)) / KalmanData.M_int.at<float>(0, 0) * (KalmanData.Z_c / cos(KalmanData.sita));




				KalmanData.t[KalmanData.seq] = com.msecsElapsed();
				KalmanData.X[KalmanData.seq] = KalmanData.X_c;
				KalmanData.Z[KalmanData.seq] = KalmanData.Z_c;




				///////////////////////////////////////////////////////////Kalman Filter //////////////////////////////////////////////////////////////////////////////////////
				
				KalmanData.state = (Mat_<float>(4, 1) << KalmanData.X[0], KalmanData.Z[0], 0, 0);
				

				if (KalmanData.seq > 0)
				{
					KalmanData.delt_t = (KalmanData.t[KalmanData.seq] - KalmanData.t[KalmanData.seq - 1]) / 1000;

					KalmanData.tt = (KalmanData.t[KalmanData.seq] - KalmanData.t[0]) / 1000;

					KalmanData.Vx = (KalmanData.X[KalmanData.seq] - KalmanData.X[0]) / KalmanData.tt;

					KalmanData.Vz = (KalmanData.Z[KalmanData.seq] - KalmanData.Z[0]) / KalmanData.tt;


					KalmanData.measurement = (Mat_<float>(2, 1) << KalmanData.X[KalmanData.seq - 1], KalmanData.Z[KalmanData.seq - 1]);    //Measurement of Data y matrix

					KalmanData.A.at<float>(0, 2) = KalmanData.delt_t;
					KalmanData.A.at<float>(1, 3) = KalmanData.delt_t;

					KalmanData.state = KalmanData.A * KalmanData.state;       // formula 1

					KalmanData.P = KalmanData.A * KalmanData.P * (KalmanData.A.t()) + KalmanData.Q;      // formula 2

					KalmanData.K = (KalmanData.A * KalmanData.P * (KalmanData.C.t())) * ((KalmanData.C * KalmanData.P * (KalmanData.C.t()) + KalmanData.R).inv());       // formula 3

					//KalmanData.Q = KalmanData.K * (KalmanData.measurement - KalmanData.C * KalmanData.state) * ((KalmanData.measurement - KalmanData.C * KalmanData.state).t()) * (KalmanData.K.t()) + KalmanData.P - KalmanData.A * KalmanData.P * KalmanData.A.t();

					

					KalmanData.state = KalmanData.state + KalmanData.K * (KalmanData.measurement - KalmanData.C * KalmanData.state);     // formul 4

					//KalmanData.w = KalmanData.w + (KalmanData.state - KalmanData.state);

					//KalmanData.P = KalmanData.P - KalmanData.K * (KalmanData.R + KalmanData.C * KalmanData.P * (KalmanData.C.t())) * (KalmanData.K.t());  // (    P-K(R+C*P*C')K'    )

					KalmanData.P = (KalmanData.I - KalmanData.K * KalmanData.C) * KalmanData.P * ((KalmanData.I - KalmanData.K * KalmanData.C).t()) + KalmanData.K * (KalmanData.C * KalmanData.P * (KalmanData.C.t()) + KalmanData.R) * (KalmanData.K.t());      // formula 5 

					//KalmanData.P = (KalmanData.I - KalmanData.K * KalmanData.C) * KalmanData.P ; // (   (I-KC)*P   )


					cout << KalmanData.state << '\n ';  // output the state 
					


				}

				KalmanData.seq = KalmanData.seq + 1;


			}
		}


		//cout << "Vz=" << KalmanData.Vz_Robot << ' ' << "Vx=" << KalmanData.Vx_Robot << ' ' << "Vsita=" << KalmanData.Vsita_Robot << '\n';

		namedWindow("Webcam", CV_WINDOW_AUTOSIZE);
		imshow("Webcam", frame_RGB);

		waitKey(10);


		if (isLocalConnection())
		{
			std::cout << "Local connection" << std::endl;
			setFormat(320, 240, "raw");
		}
	}
};


MyCamera camera;

OmniDrive omniDrive;
MyBumper bumper;

void init(const std::string& hostname)
{
	// Initialize the actors

	// Connect
	std::cout << "Connecting...";
	com.setAddress(hostname.c_str());

	com.connectToServer(true);

	if (false == com.isConnected())
	{
		std::cout << std::endl << "Could not connect to " << com.address() << std::endl;
#ifdef WIN32
		std::cout << "Press any key to exit..." << std::endl;
		rec::robotino::api2::waitForKey();
#endif
		rec::robotino::api2::shutdown();
		exit(1);
	}
	else
	{
		std::cout << "success" << std::endl;
	}
}

void drive()
{
	Bumper bumper;
	Motor motor[3];

	float s0;
	float s1;
	float s2;

	float m0;
	float m1;
	float m2;

	motor[0].setMotorNumber(2);
	motor[1].setMotorNumber(0);
	motor[2].setMotorNumber(1);

	float rr = 62;
	float RR = 187;
	float i_gear = 32;

	// Velocity of Omindrive
	float Omi1 = -60 * i_gear*sin(60 * 3.14 / 180) / (2 * 3.14*rr);
	float Omi2 = 60 * i_gear*sin(30 * 3.14 / 180) / (2 * 3.14*rr);
	float Omi3 = RR * i_gear / (6 * rr);
	float Omi4 = 0;
	float Omi5 = -60 * i_gear / (2 * 3.14*rr);
	float Omi6 = RR * i_gear / (6 * rr);
	float Omi7 = 60 * i_gear*sin(60 * 3.14 / 180) / (2 * 3.14*rr);
	float Omi8 = 60 * i_gear*sin(30 * 3.14 / 180) / (2 * 3.14*rr);
	float Omi9 = RR * i_gear / (6 * rr);

	// Position of Omindrive
	float OmTemp1 = -2048 * i_gear * sin(60 * 3.14 / 180) / (2 * 3.14 * rr);
	float OmTemp2 = 2048 * i_gear * sin(30 * 3.14 / 180) / (2 * 3.14 * rr);
	float OmTemp3 = 2048 * RR * i_gear / (360 * rr);
	float OmTemp4 = 0;
	float OmTemp5 = -2048 * i_gear / (2 * 3.14 * rr);
	float OmTemp6 = 2048 * RR*i_gear / (360 * rr);
	float OmTemp7 = 2048 * i_gear * sin(60 * 3.14 / 180) / (2 * 3.14 * rr);
	float OmTemp8 = 2048 * i_gear * sin(30 * 3.14 / 180) / (2 * 3.14 * rr);
	float OmTemp9 = 2048 * RR * i_gear / (360 * rr);

	double OmindriveMatrixTemp_v[9] = { Omi1, Omi2, Omi3, Omi4, Omi5, Omi6, Omi7, Omi8, Omi9 };
	Mat OmindiveMatrix_v = Mat(3, 3, CV_64F, OmindriveMatrixTemp_v);

	Mat OmindiveMatrixPinv_v;
	invert(OmindiveMatrix_v, OmindiveMatrixPinv_v, DECOMP_SVD);	//Pseudoinverse of the Omnidrive Matrix

	Mat Motor_N_v = Mat(3, 1, CV_64F);

	Mat Velocity = Mat(3, 1, CV_64F);

	double OmindriveMatrixTemp_p[9] = { OmTemp1, OmTemp2, OmTemp3, OmTemp4, OmTemp5, OmTemp6, OmTemp7, OmTemp8, OmTemp9 };
	Mat OmindiveMatrix_p = Mat(3, 3, CV_64F, OmindriveMatrixTemp_p);

	Mat OmindiveMatrixPinv_p;
	invert(OmindiveMatrix_p, OmindiveMatrixPinv_p, DECOMP_SVD);	//Pseudoinverse of the Omnidrive Matrix

	Mat Motor_N_p = Mat(3, 1, CV_64F);

	Mat Position = Mat(3, 1, CV_64F);


	while (com.isConnected() && false == bumper.value() && _run)
	{


		s0 = motor[1].actualVelocity();
		s1 = motor[2].actualVelocity();
		s2 = motor[0].actualVelocity();

		m0 = motor[1].actualPosition();
		m1 = motor[2].actualPosition();
		m2 = motor[0].actualPosition();


		Motor_N_p.at<double>(0, 0) = m0;
		Motor_N_p.at<double>(1, 0) = m1;
		Motor_N_p.at<double>(2, 0) = m2;



		Motor_N_v.at<double>(0, 0) = s0;
		Motor_N_v.at<double>(1, 0) = s1;
		Motor_N_v.at<double>(2, 0) = s2;



		Mat Velocity = OmindiveMatrixPinv_v * Motor_N_v;
		Mat Position = OmindiveMatrixPinv_p * Motor_N_p;

		KalmanData.Vz_Robot = Velocity.at<double>(0, 0);
		KalmanData.Vx_Robot = Velocity.at<double>(1, 0);
		KalmanData.Vsita_Robot = Velocity.at<double>(2, 0);

		KalmanData.Z_Robot = Position.at<double>(0, 0);
		KalmanData.X_Robot = Position.at<double>(1, 0);





		//if (com.msecsElapsed() < 3000)
		//{
		//	FollowBalldata.vx = 0;
		//	FollowBalldata.vy = 0;

		//}
		if (com.msecsElapsed() > 0 && com.msecsElapsed() < 6000)
		{
			FollowBalldata.vx = 0;
			FollowBalldata.vy = 0;
		}
		if (com.msecsElapsed() > 6000)
		{
			FollowBalldata.vx = 0;
			FollowBalldata.vy = 0;

		}


		/////////////////////////////////////////////////////////////////For TXT/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		fstream file("mydata1.txt", ios::in | ios::out | ios::app);
		if (file.is_open())
		{

			//file << KalmanData.X_c << " " << KalmanData.Z_c << " " << KalmanData.state.at<float>(0, 0) << " " << KalmanData.state.at<float>(1, 0) << " " << KalmanData.X_Robot << " " << KalmanData.Z_Robot << " " << com.msecsElapsed() << " \n";
			
			//file << KalmanData.state.at<float>(2, 0) << " " << KalmanData.state.at<float>(3, 0) << " " << KalmanData.Vx_Robot << ' ' << KalmanData.Vz_Robot << ' ' << com.msecsElapsed() << " \n";

			file << KalmanData.state.at<float>(2, 0) << " " << KalmanData.state.at<float>(3, 0) << " " << KalmanData.Vx << ' ' << KalmanData.Vz << ' ' << com.msecsElapsed() << " \n";
		}
		else
		{
			std::cout << "error when opening file";
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


		omniDrive.setVelocity(FollowBalldata.vx, FollowBalldata.vy, 0);

		com.processEvents();
		rec::robotino::api2::msleep(100);
	}
}

void destroy()
{
	com.disconnectFromServer();
}

int main(int argc, char **argv)
{
	std::string hostname = "172.26.1.1";
	if (argc > 1)
	{
		hostname = argv[1];
	}

#ifdef WIN32
	::SetConsoleCtrlHandler((PHANDLER_ROUTINE)sigint_handler, TRUE);
#else
	struct sigaction act;
	memset(&act, 0, sizeof(act));
	act.sa_handler = sigint_handler;
	sigaction(SIGINT, &act, NULL);
#endif

	try
	{
		init(hostname);
		drive();
		destroy();
	}
	catch (const rec::robotino::api2::RobotinoException& e)
	{
		std::cerr << "Com Error: " << e.what() << std::endl;
	}
	catch (const std::exception& e)
	{
		std::cerr << "Error: " << e.what() << std::endl;
	}
	catch (...)
	{
		std::cerr << "Unknow Error" << std::endl;
	}

	rec::robotino::api2::shutdown();

#ifdef WIN32
	std::cout << "Press any key to exit..." << std::endl;
	rec::robotino::api2::waitForKey();
#endif
}
