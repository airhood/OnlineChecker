using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using Photon.Chat;
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
    [Header("Map")]
    public Tilemap tilemap;

    public List<Tile> tiles;

    public bool[,] playerPositions = new bool[8, 8];

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
        PhotonNetwork.GameVersion = Application.version;

        if (!CheckVersion(PhotonNetwork.GameVersion))
        {
            CallUpdate();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SendChat();
        }
    }

    public void Connect() => PhotonNetwork.ConnectUsingSettings();

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

    public override void OnConnected()
    {

    }
    
    public override void OnJoinedLobby()
    {

    }

    public override void OnCreatedRoom()
    {

    }

    public override void OnJoinedRoom()
    {

    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {

    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {

    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = "";
    }

    IEnumerator SetDefaultPlayerPosition()
    {
        return null;
    }

    IEnumerator setPlayer(Vector2Int pos, bool color, bool state)
    {
        Tile tile = new Tile();

        if (color ? tile = tiles[3] : tiles[4]) ;

        if (state)
        {
            tilemap.SetTile((Vector3Int)pos, tile);
        }

        return null;
    }

    bool CheckVersion(string version)
    {
        return true;
    }

    void CallUpdate()
    {

    }
}