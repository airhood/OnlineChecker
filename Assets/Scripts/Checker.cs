using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.Threading;
using Unity.RemoteConfig;

public enum GameUpdate
{
    NoUpdate, RequiredUpdate, NonRequiredUpdate
}

public struct Square
{
    public bool color;
    public string state; // 0: null, 1: white, 2: black
}

public struct Version
{
    public int versionKey;
    public string versionCode;
    public bool required;
}

public class Checker : MonoBehaviourPunCallbacks
{
    [Header("BettleInfo")]
    public bool localPlayerColor;
    public Player opponentPlayer;

    public GamePlayer opponentGamePlayer;

    [Header("Map")]
    public Tilemap tilemap;

    public List<Tile> tiles;

    public bool[,] playerPositions = new bool[8, 8];

    public GameObject Grid;

    public List<Vector2Int> DefaultWhitePosition = new List<Vector2Int>();

    public List<Vector2Int> DefaultBlackPosition = new List<Vector2Int>();

    [Header("Photon")]
    public PhotonView PV;
    public float maxOpponentWaitTime = 7.0f;

    [Header("Chat")]
    public Text chatObject;
    public string chat;
    public InputField ChatInput;

    [Header("Player")]
    public GamePlayer gamePlayer;

    [Header("Notice")]
    NoticeUI notice;

    [Header("Loading")]
    public Animator loadingAnimator;

    bool isWaitingOpponent;
    float opponentWaitTime;


    public struct userAttributes { }
    public struct appAttributes { }

    void Awake()
    {
        ConfigManager.FetchConfigs<userAttributes, appAttributes>(new userAttributes(), new appAttributes());

        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;

        notice = FindObjectOfType<NoticeUI>();
    }

    // Start is called before the first frame update
    void Start()
    {
        notice.SUB("start");

        // 게임 버전 체크
        PhotonNetwork.GameVersion = Application.version;

        Version version = new Version();

        version.versionKey = int.Parse(Application.version);
        version.versionCode = "Dec 0.1";

        switch (CheckVersion(version))
        {
            case GameUpdate.NoUpdate:
                break;
            case GameUpdate.RequiredUpdate:
                break;
            case GameUpdate.NonRequiredUpdate:
                break;
        }
    }

    public void Notice(string msg)
    {
        notice.SUB(msg);
    }

    // Update is called once per frame
    void Update()
    {
        // enter키를 눌렀을 때 채팅 보내기
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SendChat();
        }

        if (isWaitingOpponent)
        {
            opponentWaitTime += Time.deltaTime;

            if (opponentWaitTime >= maxOpponentWaitTime)
            {
                isWaitingOpponent = false;
                opponentWaitTime = 0;

                FindOpponentFailed();
            }
        }
    }

    public void Connect() => PhotonNetwork.ConnectUsingSettings();

    public void Disconnect() => PhotonNetwork.Disconnect();

    public void JoinLobby() => PhotonNetwork.JoinLobby();

    public void LeaveLobby() => PhotonNetwork.LeaveLobby();

    public void JoinRoom() => PhotonNetwork.JoinRandomOrCreateRoom();

    public void LeaveRoom() => PhotonNetwork.LeaveRoom();

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
        isWaitingOpponent = true;
    }

    public override void OnLeftRoom()
    {
        localPlayerColor = true;
    }

    public override void OnJoinedRoom()
    {
        // this will never happen because the max player is set to 2 player so,
        // none of the players can join a room that has 0 player.
        // But in case of bugs, I will not erase this statement.
        if (PhotonNetwork.PlayerList.Length != 2)
        {
            FindOpponentFailed();
        }

        print("JoinedRoom <" + PhotonNetwork.CurrentRoom.Name + ">: No Opponent");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        opponentPlayer = newPlayer;
        print("<" + PhotonNetwork.CurrentRoom.Name + ">" + " OtherPlayerJoined: " + opponentPlayer.NickName);
        PV.RPC("PlayerColor", opponentPlayer, PhotonNetwork.LocalPlayer, localPlayerColor);
        PV.RPC("SendPlayerInformation", opponentPlayer, PhotonNetwork.LocalPlayer,gamePlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        print("<" + PhotonNetwork.CurrentRoom.Name + ">" + " OtherPlayerLeft: " + otherPlayer.NickName);

        OpponentLeft();
    }

    [PunRPC]
    public void PlayerColor(Player sendPlayer, bool color)
    {
        if (PhotonNetwork.LocalPlayer.Equals(sendPlayer)) return;

        localPlayerColor = !color;
    }

    [PunRPC]
    public void SendPlayerInformation(Player sendPlayer,GamePlayer opponentPlayerData)
    {
        if (PhotonNetwork.LocalPlayer == sendPlayer) return;

        setPlayerBoard(opponentPlayerData);
    }

    void setPlayerBoard(GamePlayer opponentPlayerData)
    {
        opponentGamePlayer.Set(opponentPlayerData, true);
    }

    public void FindOpponent()
    {
        JoinRoom();
    }

    void FindOpponentFailed()
    {
        LeaveRoom();

        print("Find room failed");

        notice.SUB("Find opponent failed. Try later");
    }

    void OpponentLeft()
    {
        LeaveRoom();

        print("Opponent Left: No Opponent");

        notice.SUB("Find opponent failed. Try later");
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
    
    [PunRPC]
    public void movePlayer(Vector2Int currentPos, Vector2Int nextPos)
    {
        // 타일 거져오기
        TileBase tile = tilemap.GetTile((Vector3Int)currentPos);

        // 타일 삭제
        tilemap.SetTile((Vector3Int)currentPos, null);

        // 새롭게 타일 생성
        tilemap.SetTile((Vector3Int)nextPos, tile);
    }

    GameUpdate CheckVersion(Version version)
    {
        Version newVersion = GetNewVersion();
        if (newVersion.Equals(version))
        {
            return GameUpdate.NoUpdate;
        } else
        {
            if (newVersion.required)
            {
                return GameUpdate.RequiredUpdate;
            }
            return GameUpdate.NonRequiredUpdate;
        }
    }

    void CallUpdate()
    {

    }

    Version GetNewVersion()
    {
        Version newVersion;

        newVersion.versionKey = ConfigManager.appConfig.GetInt("gameVersionKey");
        newVersion.versionCode = ConfigManager.appConfig.GetString("gameVersionCode");
        newVersion.required = ConfigManager.appConfig.GetBool("isRequiredUpdate");

        return newVersion;
    }

    void ShowLoadingSign()
    {
        loadingAnimator.SetTrigger("loading");
    }

    void StopLoadingSign()
    {
        loadingAnimator.SetTrigger("endloading");
    }

    public void ShowPopUpScreen()
    {

    }

    void OnChatArrived()
    {

    }

    public void PlaySound()
    {

    }
}