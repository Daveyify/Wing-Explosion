using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class UDPClient : MonoBehaviour, IClient
{
    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;
    public string clientId { get; private set; }

    public event Action<string> OnMessageReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;

    public bool isConnected { get; private set; }

    public async Task ConnectToServer(string ipAddress, int port)
    {
        udpClient = new UdpClient();
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
        isConnected = true;
        _ = ReceiveLoop();
        await SendMessageAsync("CONNECT");
    }

    private async Task ReceiveLoop()
    {
        try
        {
            while (isConnected)
            {
                UdpReceiveResult result = await udpClient.ReceiveAsync();
                string message = Encoding.UTF8.GetString(result.Buffer);

                if (message.StartsWith("CONNECTED|"))
                {
                    clientId = message.Split('|')[1];
                    Debug.Log("[Client] Connected with id: " + clientId);
                    OnConnected?.Invoke();
                    continue;
                }

                OnMessageReceived?.Invoke(message);
            }
        }
        finally
        {
            Disconnect();
        }
    }

    public async Task SendMessageAsync(string message)
    {
        if (!isConnected) return;
        byte[] data = Encoding.UTF8.GetBytes(message);
        await udpClient.SendAsync(data, data.Length, remoteEndPoint);
    }

    public void Disconnect()
    {
        if (!isConnected) return;
        isConnected = false;
        udpClient?.Close();
        udpClient?.Dispose();
        udpClient = null;
        OnDisconnected?.Invoke();
    }

    private async void OnDestroy()
    {
        Disconnect();
        await Task.Delay(100);
    }
}