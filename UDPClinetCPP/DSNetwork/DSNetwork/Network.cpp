#include "pch.h"


#include "Network.h"

using namespace std;
void RecvMain(void * p)
{
	char s[2048];
	sockaddr_in sfrom = { 0 };
	int slen = sizeof(sfrom);//in out
	SOCKET sock = (SOCKET)p;
	int n = 0;
	while ((n = recvfrom(sock, s, sizeof(s), 0, (sockaddr*)&sfrom, &slen)) > 0)
	{
		s[n] = '\0';
		cout << endl << inet_ntoa(sfrom.sin_addr) << "-" << htons(sfrom.sin_port) << ": " << s << endl;

		//AFTER RECEIVE:


	}
}
Network::Network()
{
}


Network::~Network()
{
}


int Network::Init(unsigned short newPort)
{
	WORD wVersionRequested;
	WSADATA wsaData;
	int err;

	wVersionRequested = MAKEWORD(1, 1);

	err = WSAStartup(wVersionRequested, &wsaData);
	if (err != 0) {
		return -1;
	}

	if (LOBYTE(wsaData.wVersion) != 1 ||
		HIBYTE(wsaData.wVersion) != 1) {
		WSACleanup();
		return -1;
	}

	// create new socket
	sockSrv = socket(AF_INET, SOCK_DGRAM, 0);
	if (INVALID_SOCKET == sockSrv)
	{
		cout << "Socket Error：" << WSAGetLastError << endl;
		return -1;
	} 

	SOCKADDR_IN local;
	//local.sin_addr.Sun.S_addr = inet_addr("192.168.1.1");
	local.sin_addr.S_un.S_addr = htonl(INADDR_ANY);
	local.sin_family = AF_INET;
	local.sin_port = htons((u_short)(newPort));

	int a = bind(sockSrv, (SOCKADDR*)&local, sizeof(SOCKADDR));
	
	port = newPort;

	_beginthread(RecvMain, 1024 * 1024, (void*)sockSrv);//多线程，再次调用int main主函数 实现两个线程

	return 1;
} 
 
void Network::Colse() { 
	closesocket(sockSrv);
	WSACleanup();
}
void Network::SendTo(char* IP, char* Buffer) {
	struct sockaddr_in sa = { AF_INET,htons(port) };
	sa.sin_addr.S_un.S_addr = inet_addr(IP);
	sa.sin_port = htons(port);
	sendto(sockSrv,Buffer, strlen(Buffer), 0, (sockaddr*)&sa, sizeof(sa));
}

