#include "pch.h"
#include <iostream> 
#include "Network.h"
using namespace std;

int main(int argc, char* argv[]) {
	Network* net = new Network();
	if (net->Init(11000) > 0) {
		cout << "Network Init successful." << endl;
	}

	char s[2048];
	char sIP[20];
	while (true)
	{
		cout << "Please Input target's IP: ";
		cin >> sIP;
		cin.get();
		cout << "Please Input Sending Message：";
		fflush(stdin);
		cin.getline(s, 2047);
		 
		net->SendTo(sIP, s);

	}
	net->Colse();
	return 0;
}