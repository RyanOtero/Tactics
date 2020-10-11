using System;
using System.Collections.Generic;
using UnityEngine;

public enum Direction { North, South, East, West, None }

public class Cursor : MonoBehaviour {

    public static Transform location;
    public static Tile ctile;
    public static Tile ltile;
    public Material painterMaterial;
    public GameObject CursorOrb;
    public static bool isRestricted;


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

    public static void ChangeCursorColors(bool isYellow = false) {
        Color orb;
        Color projectorColor;
        if (!isYellow) {
            orb = new Color(0.027f, 0.378f, 0.943f);
            projectorColor = new Color(.434f, .533f, .766f);

        } else {
            orb = new Color(1f, 1f, 0f);
            projectorColor = new Color(.719f, .738f, .323f);
        }
        Instance.CursorOrb.GetComponent<Renderer>().sharedMaterial.color = orb;
        Instance.painterMaterial.SetColor("_PrimaryTintColor", projectorColor);
    }


    public static void GoSouth() {
        FindTile(Direction.South);
        location.position = new Vector3(ctile.x, ctile.height, ctile.y);
    }

    public static void GoNorth() {
        FindTile(Direction.North);
        location.position = new Vector3(ctile.x, ctile.height, ctile.y);
    }

    public static void GoWest() {
        FindTile(Direction.West);
        location.position = new Vector3(ctile.x, ctile.height, ctile.y);
    }

    public static void GoEast() {
        FindTile(Direction.East);
        location.position = new Vector3(ctile.x, ctile.height, ctile.y);
    }

    public static void FindTile(Direction d) {
        int x = 0;
        int y = 0;
        switch (d) {
            case Direction.North:
                y++;
                break;
            case Direction.South:
                y--;
                break;
            case Direction.East:
                x++;
                break;
            case Direction.West:
                x--;
                break;
            default:
                break;
        }
        ctile = new Tile(BattleManager.terrainTiles.Find(q => q.x == (location.position.x + x) && q.y == (location.position.z + y)));
        ctile.height += 1.5f;
    }
}