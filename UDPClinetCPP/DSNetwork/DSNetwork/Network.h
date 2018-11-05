#include <Winsock2.h>  
#pragma comment( lib, "ws2_32.lib" ) 
#include <iostream>
#include <process.h> //_beginthread需要的头文件 即多线程
#pragma warning(disable:4996)
#pragma once
class Network
{
public:
	unsigned short port=11000;
	Network();
	~Network();
	SOCKET sockSrv;

	int Init(unsigned short newPort);
	void SendTo(char* IP,char* Buffer);
	void Colse();
  
};

