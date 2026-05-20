using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    [Header("Prefabs")]
    public GameObject localPlayerPrefab;
    public GameObject remotePlayerPrefab;

    [Header("Network")]
    public int port = 7777;

    public bool isHost { get; private set; }
    public string localId { get; private set; }

    private UDPServer server;
    private UDPClient client;

    private readonly Dictionary<string, RemotePlayer> remotePlayers = new();
    private readonly ConcurrentQueue<string> messageQueue = new();
    private PlayerControl localPlayer;

    private readonly Dictionary<string, int> playerPings = new();
    private readonly Dictionary<string, float> pingSentTime = new();

    private float sendInterval = 0.05f;
    private float sendTimer = 0f;


    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        while (messageQueue.TryDequeue(out string msg))
            HandleMessage(msg);

        sendTimer += Time.deltaTime;
        if (localPlayer != null && sendTimer >= sendInterval)
        {
            sendTimer = 0f;
            Vector3 p = localPlayer.transform.position;
            float ry = localPlayer.transform.eulerAngles.y;
            float rx = localPlayer.GetCameraXRotation();
            Send($"POS|{localId}|{p.x:F2}|{p.y:F2}|{p.z:F2}|{rx:F2}|{ry:F2}");
        }
    }

    public async void StartHost()
    {
        isHost = true;

        server = gameObject.AddComponent<UDPServer>();
        await server.StartServer(port);
        Debug.Log("[Net] Server started");

        await SceneManager.LoadSceneAsync("MainGame");

        client = gameObject.AddComponent<UDPClient>();
        client.OnConnected += OnLocalConnected;
        client.OnMessageReceived += msg => messageQueue.Enqueue(msg);
        await client.ConnectToServer("127.0.0.1", port);
    }

    public async void StartClient(string hostIp)
    {
        isHost = false;

        await SceneManager.LoadSceneAsync("MainGame");

        client = gameObject.AddComponent<UDPClient>();
        client.OnConnected += OnLocalConnected;
        client.OnMessageReceived += msg => messageQueue.Enqueue(msg);
        await client.ConnectToServer(hostIp, port);
    }

    private void OnLocalConnected()
    {
        localId = client.clientId;
        Debug.Log("[Net] Local id: " + localId);

        Vector3 spawnPos = new Vector3(Random.Range(-3f, 3f), -32f, Random.Range(-3f, 3f));
        GameObject go = Instantiate(localPlayerPrefab, spawnPos, Quaternion.identity);
        localPlayer = go.GetComponent<PlayerControl>();
        localPlayer.networkId = localId;

        GameManager.Instance.RegisterPlayer(localId, localPlayer);
        string color = localPlayer.GetColorString();
        Send($"SPAWN|{localId}|{spawnPos.x:F2}|{spawnPos.y:F2}|{spawnPos.z:F2}|{color}");
    }

    private void HandleMessage(string msg)
    {
        string[] parts = msg.Split('|');
        switch (parts[0])
        {
            case "SPAWN":
                SpawnRemotePlayer(parts[1],
                    new Vector3(float.Parse(parts[2]), float.Parse(parts[3]), float.Parse(parts[4])),
                    float.Parse(parts[5]), float.Parse(parts[6]), float.Parse(parts[7]));
                break;

            case "DESPAWN":
                DespawnRemotePlayer(parts[1]);
                break;

            case "POS":
                {
                    string id = parts[1];
                    if (id == localId) break;

                    if (!remotePlayers.ContainsKey(id))
                    {
                        Vector3 spawnPos = new Vector3(
                            float.Parse(parts[2]),
                            float.Parse(parts[3]),
                            float.Parse(parts[4])
                        );
                        SpawnRemotePlayer(id, spawnPos, 1f, 1f, 1f);
                    }

                    MoveRemotePlayer(id,
                        new Vector3(float.Parse(parts[2]), float.Parse(parts[3]), float.Parse(parts[4])),
                        float.Parse(parts[5]),
                        float.Parse(parts[6]));
                    break;
                }

            case "PLAYER_JOINED":
                if (localPlayer != null)
                {
                    Vector3 p = localPlayer.transform.position;
                    string color = localPlayer.GetColorString();
                    Send($"SPAWN|{localId}|{p.x:F2}|{p.y:F2}|{p.z:F2}|{color}");
                }
                break;

            case "PUSH":
                    string targetId = parts[1];
                    Vector3 pushDir = new Vector3(
                        float.Parse(parts[2]),
                        float.Parse(parts[3]),
                        float.Parse(parts[4])
                    );
                    PlayerControl target = GameManager.Instance.GetLocalPlayer(targetId);
                    if (target != null)
                        target.RecievePush(pushDir);
                    break;

            case "ELIMINAR":
                GameManager.Instance.HandleDelete(parts[1]);
                break;

            case "PING_REQ":
                Send($"PING_RES|{parts[1]}");
                break;

            case "PING_RES":
                {
                    string pingId = parts[1];
                    if (pingSentTime.TryGetValue(pingId, out float sentTime))
                    {
                        int ms = Mathf.RoundToInt((Time.time - sentTime) * 1000f);
                        playerPings[localId] = ms;
                        pingSentTime.Remove(pingId);
                        Send($"PING_UPDATE|{localId}|{ms}");
                        UIManager.Instance?.UpdateOwnPing(ms);
                    }
                    break;
                }

            case "PING_UPDATE":
                {
                    string pingPlayerId = parts[1];
                    int pingMs = int.Parse(parts[2]);
                    playerPings[pingPlayerId] = pingMs;
                    UIManager.Instance?.RefreshPlayerList();
                    break;
                }

            case "KICK":
                if (parts[1] == localId)
                {
                    Debug.Log("[Net] Fui kickeado");
                    SceneManager.LoadScene("MainMenu");
                }
                else
                {
                    GameManager.Instance.HandleDelete(parts[1]);
                }
                break;

            case "PAUSE":
                UIManager.Instance?.HandlePause(parts[1] == "1");
                break;
        }
    }

    private void SpawnRemotePlayer(string id, Vector3 pos, float r, float g, float b)
    {
        if (id == localId || remotePlayers.ContainsKey(id)) return;
        GameObject go = Instantiate(remotePlayerPrefab, pos, Quaternion.identity);
        RemotePlayer rp = go.GetComponent<RemotePlayer>();
        rp.networkId = id;
        rp.SetColor(r, g, b);
        remotePlayers[id] = rp;
        GameManager.Instance.RegisterPlayer(id, null, rp);
    }

    private void DespawnRemotePlayer(string id)
    {
        if (!remotePlayers.TryGetValue(id, out RemotePlayer rp)) return;
        GameManager.Instance.UnregisterPlayer(id);
        Destroy(rp.gameObject);
        remotePlayers.Remove(id);
    }

    private void MoveRemotePlayer(string id, Vector3 pos, float rx, float ry)
    {
        if (remotePlayers.TryGetValue(id, out RemotePlayer rp))
            rp.SetTarget(pos, rx, ry);
    }
    public void SendPing()
    {
        string pingId = System.Guid.NewGuid().ToString();
        pingSentTime[pingId] = Time.time;
        Send($"PING_REQ|{pingId}");
    }
    public void Send(string message) => _ = client?.SendMessageAsync(message);
    public void BroadcastBombAssign(string id) => Send($"BOMB_ASSIGN|{id}");
    public void BroadcastBombExplode(string id) => Send($"BOMB_EXPLODE|{id}");
    public void BroadcastPush(string id, Vector3 dir) => Send($"PUSH|{id}|{dir.x:F2}|{dir.y:F2}|{dir.z:F2}");
    public void BroadcastEliminar(string id) => Send($"ELIMINAR|{id}");
    public void BroadcastKick(string id) => Send($"KICK|{id}");
    public void BroadcastPause(bool pause) => Send($"PAUSE|{(pause ? "1" : "0")}");
    public Dictionary<string, int> GetPings() => new(playerPings);
}
