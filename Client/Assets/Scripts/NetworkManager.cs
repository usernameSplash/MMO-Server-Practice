using DummyClient;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    ServerSession _session = new ServerSession();

    void Start()
    {
        Console.WriteLine("ABCD");
        // DNS (Domain Name System)
        string host = "localhost";

        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        Console.WriteLine(ipAddr);

        Connector connector = new Connector();

        connector.Connect(endPoint,
            () => { return _session; },
            1);
    }

    void Update()
    {

    }
}
