using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Unity.RemoteConfig;
using System.Diagnostics;

public enum GameUpdate
{
    NoUpdate, RequiredUpdate, NonRequiredUpdate, Error
}

public enum Square
{
    none, white, black
}

public struct Version
{
    public int versionKey;
    public string versionCode;
    public bool requiredUpdate;
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
    public NoticeUI notice;

    [Header("Loading")]
    public Animator loadingAnimator;

    bool isWaitingOpponent;
    float opponentWaitTime;

    [Header("Screens/Panels")]
    public GameObject LoadingScreen;
    public GameObject TitleScreen;
    public GameObject FindOpponentScreen;
    public GameObject GameScreen;

    [Header("Game")]
    public int versionKey = 1;


    Square[,] map = new Square[8, 8];


    public struct userAttributes { }
    public struct appAttributes { }

    void Awake()
    {
        ConfigManager.FetchConfigs<userAttributes, appAttributes>(new userAttributes(), new appAttributes());

        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    // Start is called before the first frame update
    void Start()
    {
        notice = FindObjectOfType<NoticeUI>();

        notice.SUB("start");

        // 게임 버전 체크
        PhotonNetwork.GameVersion = Application.version;

        Version version = new Version();

        version.versionCode = Application.version;
        version.versionKey = versionKey;

        switch (CheckVersion(version))
        {
            case GameUpdate.NoUpdate:
                break;
            case GameUpdate.RequiredUpdate:
                CallUpdate();
                break;
            case GameUpdate.NonRequiredUpdate:
                AskForUpdate(GetNewVersion());
                break;
        }

        print("Datapath: " + Application.dataPath);
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

        notice.SUB(Application.dataPath);
    }

    public void Connect() => PhotonNetwork.ConnectUsingSettings();

    public void Disconnect() => PhotonNetwork.Disconnect();

    public void JoinLobby() => PhotonNetwork.JoinLobby();

    public void LeaveLobby() => PhotonNetwork.LeaveLobby();

    public void JoinRoom() => PhotonNetwork.JoinRandomOrCreateRoom();

    public void LeaveRoom() => PhotonNetwork.LeaveRoom();

    public void SendChat()
    {
        PV.RPC("ChatCall", RpcTarget.All, PhotonNetwork.LocalPlayer, gamePlayer, ChatInput.text.Replace("\n", string.Empty));
    }

    [PunRPC]
    public void ChatCall(Player sendPlayer, GamePlayer sendGamePlayer, string msg)
    {
        chat += "\n" + msg;
        chatObject.text = chat;

        if (!sendPlayer.Equals(PhotonNetwork.LocalPlayer))
        {
            OnChatArrived(sendPlayer, sendGamePlayer, msg);
        }
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

        for(int i = 0; i < DefaultBlackPosition.Count; i++)
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
            for(int y = -4; y < 5; y++)
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
        else
        {
            tilemap.SetTile((Vector3Int)pos, null);
        }
    }
    
    [PunRPC]
    public void movePlayer(Vector2Int currentPos, Vector2Int nextPos)
    {
        // 타일 거져오기
        TileBase tile = tilemap.GetTile((Vector3Int)currentPos);

        // 타일 삭제
        tilemap.SetTile((Vector3Int)currentPos, null);
        Square temp = map[currentPos.x, currentPos.y];
        map[currentPos.x, currentPos.y] = Square.none;

        // 새롭게 타일 생성
        tilemap.SetTile((Vector3Int)nextPos, tile);
        map[nextPos.x, nextPos.y] = temp;
    }

    GameUpdate CheckVersion(Version version)
    {
        Version newVersion = GetNewVersion();
        if (newVersion.Equals(version))
        {
            return GameUpdate.NoUpdate;
        } else
        {
            if (version.versionKey < newVersion.versionKey)
            {
                if (newVersion.requiredUpdate)
                {
                    return GameUpdate.RequiredUpdate;
                }
                return GameUpdate.NonRequiredUpdate;
            }
            return GameUpdate.Error;
        }
    }

    void CallUpdate()
    {
        notice.SUB("There is new update. Start updating... (Game will quit)");
        string path = Application.dataPath + "/../GameUpdater.exe";
        Process.Start(path);
        Application.Quit();
    }

    void AskForUpdate(Version version)
    {

    }

    Version GetNewVersion()
    {
        Version newVersion;

        newVersion.versionKey = ConfigManager.appConfig.GetInt("gameVersionKey");
        newVersion.versionCode = ConfigManager.appConfig.GetString("gameVersionCode");
        newVersion.requiredUpdate = ConfigManager.appConfig.GetBool("isRequiredUpdate");

        return newVersion;
    }

    public void ShowLoadingSign()
    {
        loadingAnimator.SetTrigger("loading");
    }

    public void StopLoadingSign()
    {
        loadingAnimator.SetTrigger("endloading");
    }

    public void ShowPopUpScreen()
    {

    }

    void OnChatArrived(Player sendPlayer, GamePlayer sendGamePlayer, string msg)
    {
        // play sound ex)띨롱
    }

    public void SetScreenState(string name, bool state)
    {
        GameObject[] searchResult = GameObject.FindGameObjectsWithTag("Screen");

        foreach (GameObject screen in searchResult)
        {
            if (screen.name == name)
            {
                screen.SetActive(state);
            }
        }
    }

    public void SetScreen(string name)
    {
        GameObject[] searchResult = GameObject.FindGameObjectsWithTag("Screen");

        foreach (GameObject screen in searchResult)
        {
            if (screen.name != name)
            {
                screen.SetActive(false);
            }
            else
            {
                screen.SetActive(true);
            }
        }
    }

    private void OnApplicationPause(bool pause)
    {

    }

    [System.Obsolete]
    private void OnApplicationQuit()
    {

    }
}