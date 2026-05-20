using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class UDPServer : MonoBehaviour, IServer
{
    private UdpClient udpServer;
    private readonly ConcurrentDictionary<string, IPEndPoint> clients = new();

    public event Action<string> OnMessageReceived;                 
    public event Action OnConnected;                                
    public event Action OnDisconnected;                            
    public event Action<string, string> OnMessageReceivedFromClient; 
    public event Action<string> OnClientConnected;
    public event Action<string> OnClientDisconnected;

    public bool isServerRunning { get; private set; }

    public Task StartServer(int port)
    {
        udpServer = new UdpClient(port);
        isServerRunning = true;
        Debug.Log("[Server] Started on port " + port);
        _ = ReceiveLoop();
        return Task.CompletedTask;
    }

    private async Task ReceiveLoop()
    {
        try
        {
            while (isServerRunning)
            {
                UdpReceiveResult result = await udpServer.ReceiveAsync();
                string raw = Encoding.UTF8.GetString(result.Buffer);
                IPEndPoint sender = result.RemoteEndPoint;

                string clientId = sender.ToString();

                if (raw == "CONNECT")
                {
                    if (!clients.ContainsKey(clientId))
                    {
                        clients[clientId] = sender;
                        Debug.Log("[Server] Client connected: " + clientId);
                        await SendToAsync("CONNECTED|" + clientId, sender);
                        await BroadcastExceptAsync("PLAYER_JOINED|" + clientId, clientId);
                        OnConnected?.Invoke();
                        OnClientConnected?.Invoke(clientId);
                    }
                    continue;
                }

                if (raw.StartsWith("DISCONNECT|"))
                {
                    string id = raw.Split('|')[1];
                    clients.TryRemove(id, out _);
                    await BroadcastExceptAsync("DESPAWN|" + id, id);
                    OnClientDisconnected?.Invoke(id);
                    continue;
                }

                await BroadcastExceptAsync(raw, clientId);
                OnMessageReceived?.Invoke(raw);
                OnMessageReceivedFromClient?.Invoke(clientId, raw);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[Server] ReceiveLoop error: " + e.Message);
        }
        finally
        {
            Disconnect();
        }
    }

    public async Task SendToAsync(string message, IPEndPoint endpoint)
    {
        if (!isServerRunning) return;
        byte[] data = Encoding.UTF8.GetBytes(message);
        await udpServer.SendAsync(data, data.Length, endpoint);
    }

    public async Task BroadcastAsync(string message)
    {
        foreach (var kvp in clients)
            await SendToAsync(message, kvp.Value);
    }

    public async Task BroadcastExceptAsync(string message, string excludeClientId)
    {
        foreach (var kvp in clients)
            if (kvp.Key != excludeClientId)
                await SendToAsync(message, kvp.Value);
    }

    public Task SendMessageAsync(string message) => BroadcastAsync(message);

    public List<string> GetConnectedClients() => new(clients.Keys);

    public void Disconnect()
    {
        if (!isServerRunning) return;
        isServerRunning = false;
        udpServer?.Close();
        udpServer?.Dispose();
        udpServer = null;
        OnDisconnected?.Invoke();
        Debug.Log("[Server] Stopped");
    }

    private async void OnDestroy()
    {
        Disconnect();
        await Task.Delay(100);
    }
}