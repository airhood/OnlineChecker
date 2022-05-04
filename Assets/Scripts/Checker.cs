using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public struct Square
{
    public bool color;
    public string state; // 0: null, 1: white, 2: black
}

public enum SquareType
{
    whitePlayer, blackPlayer
}

public class Checker : MonoBehaviourPunCallbacks
{
    [Header("BettleInfo")]
    public bool playerColor;

    [Header("Map")]
    public Tilemap tilemap;

    public List<Tile> tiles;

    public bool[,] playerPositions = new bool[8, 8];

    public GameObject Grid;

    public List<Vector2Int> DefaultWhitePosition = new List<Vector2Int>();

    public List<Vector2Int> DefaultBlackPosition = new List<Vector2Int>();

    [Header("Photon")]
    public PhotonView PV;

    [Header("Chat")]
    public Text chatObject;
    public string chat;
    public InputField ChatInput;


    void Awake()
    {
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    // Start is called before the first frame update
    void Start()
    {
        // ���� ���� üũ
        PhotonNetwork.GameVersion = Application.version;

        if (!CheckVersion(PhotonNetwork.GameVersion))
        {
            CallUpdate();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // enterŰ�� ������ �� ä�� ������
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SendChat();
        }
    }

    public void Connect() => PhotonNetwork.ConnectUsingSettings();

    public void JoinLobby() => PhotonNetwork.JoinLobby();

    public void JoinRoom() => PhotonNetwork.JoinRandomOrCreateRoom();

    public void SendChat()
    {
        PV.RPC("ChatCall", RpcTarget.All, ChatInput.text.Replace("\n", string.Empty));
    }

    [PunRPC]
    public void ChatCall(string msg)
    {
        chat += "\n" + msg;
        chatObject.text = chat;
    }

    public override void OnConnectedToMaster()
    {
        print("ConnectedToMaster");
        JoinLobby();
    }
    
    public override void OnJoinedLobby()
    {
        print("Joined Lobby");
    }

    public override void OnCreatedRoom()
    {

    }

    public override void OnJoinedRoom()
    {
        print("JoinedRoom: " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        print("<" + PhotonNetwork.CurrentRoom.Name + ">" + " OtherPlayerJoined: " + newPlayer.NickName);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        print("<" + PhotonNetwork.CurrentRoom.Name + ">" + " OtherPlayerLeft: " + otherPlayer.NickName);
    }

    public void sendPlayerColor(Player sendPlayer, bool color)
    {
        if (PhotonNetwork.LocalPlayer.Equals(sendPlayer)) return;

        playerColor = !color;
    }

    IEnumerator SetDefaultPlayerPosition()
    {
        for(int i = 0; i < DefaultWhitePosition.Count; i++)
        {
            setPlayer(DefaultWhitePosition[i], true, true);
        }

        for (int i = 0; i < DefaultBlackPosition.Count; i++)
        {
            setPlayer(DefaultBlackPosition[i], false, true);
        }

        return null;
    }

    IEnumerator ClearMap()
    {
        Vector3Int[] positions = new Vector3Int[4 * 4 * 4];

        int n = 0;

        for(int x = -4; x < 5; x++)
        {
            for (int y = -4; y < 5; y++)
            {
                if (x != 0 && y != 0)
                {
                    positions[n] = new Vector3Int(x, y, 0);
                    n++;
                }
            }
        }

        tilemap.SetTiles(positions, null);

        return null;
    }

    void setPlayer(Vector2Int pos, bool color, bool state)
    {
        Tile tile = new Tile();

        tile = color ? tiles[1] : tiles[2];

        if (state)
        {
            tilemap.SetTile((Vector3Int)pos, tile);
        }
    }
    
    void movePlayer(Vector2Int currentPos, Vector2Int nextPos)
    {
        // Ÿ�� ��������
        TileBase tile = tilemap.GetTile((Vector3Int)currentPos);

        // Ÿ�� ����
        tilemap.SetTile((Vector3Int)currentPos, null);

        // ���Ӱ� Ÿ�� ����
        tilemap.SetTile((Vector3Int)nextPos, tile);
    }

    bool CheckVersion(string version)
    {
        return true;
    }

    void CallUpdate()
    {

    }
}