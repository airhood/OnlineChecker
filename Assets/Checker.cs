using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using Photon.Chat;
using UnityEngine.Tilemaps;

public class Checker : MonoBehaviourPunCallbacks
{
    [Header("Components")]
    public PhotonView PV;

    [Header("Map")]
    public Tilemap tilemap;

    public List<Tile> tiles;

    public bool[,] playerPositions = new bool[8, 8];

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