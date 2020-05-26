using System.Collections.Generic;
using UnityEngine;

public enum Direction { North, South, East, West, None }

public class Cursor : MonoBehaviour {
    
    public static Transform location;
    public static Tile ctile;
    public static Tile ltile;
    public static Cursor Instance { get; private set; } = null;



    void Awake() {

        //Singleton Check
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    void Start() {
        location = transform;
        ctile = null;
        ltile = null;
    }


    public static void GoSouth() {
        FindTile(Direction.South);
        location.position = new Vector3(ctile.x, ctile.height, ctile.y);
        ReportTile();
    }

    public static void GoNorth() {
        FindTile(Direction.North);
        location.position = new Vector3(ctile.x, ctile.height, ctile.y);
        ReportTile();
    }

    public static void GoWest() {
        FindTile(Direction.West);
        location.position = new Vector3(ctile.x, ctile.height, ctile.y);
        ReportTile();
    }

    public static void GoEast() {
        FindTile(Direction.East);
        location.position = new Vector3(ctile.x, ctile.height, ctile.y);
        ReportTile();
    }

    public static void FindTile(Direction d) {
        int X = 0;
        int Y = 0;
        switch (d) {
            case Direction.North:
                Y++;
                break;
            case Direction.South:
                Y--;
                break;
            case Direction.East:
                X++;
                break;
            case Direction.West:
                X--;
                break;
            default:
                break;
        }
        ctile = new Tile(BattleManager.terrainTiles.Find(q => q.x == (location.position.x + X) && q.y == (location.position.z + Y)));
        ctile.height += 1.5f;
    }

    public static void ReportTile() {
        if (BattleManager.terrainTiles.Count > 0) {
            Tile t = BattleManager.availableMoves.Find(tile => tile.x == ctile.x && tile.y == ctile.y);
            if (t != null) {
                //Debug.Log(t);
            }
        }
    }
}