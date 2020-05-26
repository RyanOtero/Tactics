using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using static TacticsUtil;
using TMPro;
using UnityEngine.Events;

//todo: player direction during movement, Character Builder, context menu, unit classes, AI


public enum PhaseOfTurn { None, SelectUnit, Confirm, SelectAction, Move, Attack, Prayer, Item, SelectPrayer, SelectItem }

public sealed class BattleManager : MonoBehaviour {
    public static BattleManager Instance { get; private set; } = null;

    #region Fields
    public static int checkpoint;

    //Terrain, list of all units and list of units that have not moved this round
    public static List<GameObject> unitList;
    public static List<GameObject> activeUnitList;

    private static int round;
    public static GameObject terrain;
    public static float jumpScale;

    //list of tiles, movement range, target range, projectors
    public static List<Tile> terrainTiles;
    public static List<Tile> availableMoves;
    public static List<Tile> availableTargets;

    public static List<Tile> targetArea;
    private static GameObject movePainter;
    private static GameObject rangePainter;


    public static GameObject selectedUnit;
    public static GameObject targetUnit;
    public static Tile selectedTile;
    public static Tile targetTile;
    //original location of unit at beginning of movement
    public static Vector3 lastLocation;
    public static GameObject cursor;
    private static PhaseOfTurn phase;
    public static bool isPlayerTurn;
    //for testing orders
    private static float heightThreshold;
    public TextMeshProUGUI phaseLabel;
    public static PhaseOfTurn PreviousPhase { get; private set; }
    public static PhaseOfTurn Phase {
        get { return phase; }
        private set {
            PreviousPhase = Phase;
            phase = value;
        }
    }
    public static SaveData saveData;

    #endregion

    #region Methods

    void Awake() {

        //Singleton Check
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
        //Initialization
        unitList = new List<GameObject>();
        activeUnitList = new List<GameObject>();
        foreach (GameObject unit in GameObject.FindGameObjectsWithTag("EnemyUnit")) {
            unitList.Add(unit);
            activeUnitList.Add(unit);
        }
        foreach (GameObject unit in GameObject.FindGameObjectsWithTag("PlayerUnit")) {
            unitList.Add(unit);
            activeUnitList.Add(unit);
        }

        terrain = GameObject.FindGameObjectWithTag("Terrain");
        availableMoves = new List<Tile>();
        availableTargets = new List<Tile>();
        terrainTiles = new List<Tile>();
        ///////Have to make system to order units, probably by speed but will have to incorporate double turns
        unitList.Reverse();
        activeUnitList.Reverse();
        selectedUnit = null;
        targetUnit = null;
        selectedTile = null;
        targetTile = null;
        terrainTiles = GetTerrainTiles();
        round = 1;
        isPlayerTurn = true;
        Phase = PhaseOfTurn.SelectUnit;
        PreviousPhase = PhaseOfTurn.None;
        jumpScale = 4f;
        heightThreshold = .26f;
        if (saveData != null) LoadGame(saveData.Slot);
        cursor = Instantiate(Resources.Load("Prefab/Cursor", typeof(GameObject)), new Vector3(GetX(activeUnitList[0]),
            GetY(activeUnitList[0]) + 1f, GetZ(activeUnitList[0])), Quaternion.Euler(0, 0, 0)) as GameObject;
    }

    void Update() {
        //remove inactive units  
        foreach (GameObject unit in unitList) {
            if (unit != null && unit.GetComponent<Unit>().ActiveUnit == false) {
                activeUnitList.Remove(unit);
            }
        }

        //if every unit has moved, increment the round number and replenish active unit list
        if (activeUnitList.Count == 0) {
            Endround();
        }
    }

    #region TurnManagement

    public static GameObject SelectUnit() {
        GameObject unit = null;
        //if cursor is over unit
        if (isPlayerTurn == true) {
            foreach (GameObject pUnit in GameObject.FindGameObjectsWithTag("PlayerUnit")) {
                if (GetX(cursor) == GetX(pUnit) && GetZ(cursor) == GetZ(pUnit) && pUnit.GetComponent<Unit>().ActiveUnit == true) {
                    unit = pUnit;
                }
            }
        } else {
            foreach (GameObject eUnit in GameObject.FindGameObjectsWithTag("EnemyUnit")) {
                if (GetX(cursor) == GetX(eUnit) && GetZ(cursor) == GetZ(eUnit) && eUnit.GetComponent<Unit>().ActiveUnit == true) {
                    unit = eUnit;
                }
            }
        }

        return unit;
    }

    private static void Endround() {
        //replenish active unit list and increment round number
        foreach (GameObject unit in unitList) {
            unit.GetComponent<Unit>().ActiveUnit = true;
            activeUnitList.Add(unit);
        }
        round++;
    }
    #endregion

    #region UtilMethods

    public static void PaintRange(List<Tile> tileList, string resourcePath) {
        //destroy any existing painters, then repaint the move range
        ClearPainters();
        foreach (Tile coord in tileList) {
            GameObject mp = Instantiate(Resources.Load(resourcePath, typeof(GameObject)), new Vector3(coord.x, 10, coord.y), Quaternion.Euler(90, 0, 0)) as GameObject;
        }
    }

    //destroy all painters
    public static void ClearPainters() {
        foreach (GameObject ptr in GameObject.FindGameObjectsWithTag("Painter")) {
            Destroy(ptr);
        }
    }

    private static List<Tile> GetTerrainTiles() {
        List<Tile> moves = new List<Tile>();
        //Grid size: i is x-axis,  j is z-axis
        for (int i = 0; i < terrain.GetComponent<Renderer>().bounds.size.x; i++) {
            for (int j = 0; j < terrain.GetComponent<Renderer>().bounds.size.z; j++) {
                Tile t = new Tile(i, j);
                moves.Add(t);
                //GameObject n = GameObject.Instantiate(Resources.Load("Prefab/Number"), new Vector3(t.x, t.height + .05f, t.y), Quaternion.Euler(90, 0, 0)) as GameObject;
                //n.GetComponent<TextMesh>().text = string.Format("{0}, {1}", i, j);
            }
        }
        return moves;
    }

    public void ChangePhase(int phaseInt) {
        //public enum Phase { None, Select_Unit, Confirm, Select_Action, Move, Attack, Prayer, Item }
        Phase = (PhaseOfTurn)phaseInt;
        UpdatePhaseLabel();

    }

    public static void ChangePhase(PhaseOfTurn phase) {
        //public enum Phase { None, Select_Unit, Confirm, Select_Action, Move, Attack, Prayer, Item }
        Phase = phase;
        UpdatePhaseLabel();
    }

    private static void UpdatePhaseLabel() {
        //public enum Phase { None, Select_Unit, Confirm, Select_Action, Move, Attack, Prayer, Item }
        switch (phase) {
            case PhaseOfTurn.SelectUnit:
                Instance.phaseLabel.text = "Select Unit";
                break;
            case PhaseOfTurn.Confirm:
                Instance.phaseLabel.text = "Confirm?";
                break;
            case PhaseOfTurn.SelectAction:
                Instance.phaseLabel.text = "Select Action";
                break;
            case PhaseOfTurn.Move:
                Instance.phaseLabel.text = "Move";
                break;
            case PhaseOfTurn.Attack:
                Instance.phaseLabel.text = "Attack";
                break;
            case PhaseOfTurn.Prayer:
                Instance.phaseLabel.text = "Use Prayer";
                break;
            case PhaseOfTurn.Item:
                Instance.phaseLabel.text = "Use Item";
                break;
            case PhaseOfTurn.SelectPrayer:
                Instance.phaseLabel.text = "Select Prayer";
                break;
            case PhaseOfTurn.SelectItem:
                Instance.phaseLabel.text = "Select Item";
                break;
            case PhaseOfTurn.None:
            default:
                Instance.phaseLabel.text = "Ahh, Shit!";
                break;
        }
    }

    public void SetSelectedUnitInactive() {
        selectedUnit.GetComponent<Unit>().ActiveUnit = false;
    }

    



    #endregion

    #region Movement

    public static List<Tile> GetMoveRange(GameObject unit) {
        //get variables
        Transform trans = unit.transform;
        int movement = unit.GetComponent<Unit>().movement;
        Tile unitLocation = new Tile(trans.position.x, trans.position.z);
        List<Tile> moves = new List<Tile>();

        //check through terrain tiles for tiles to include in tile list
        terrainTiles.ForEach(t => {
            //Area bounded between four linear inequalities with player location as offset from 0,0
            int a = t.x - unitLocation.x;
            int b = t.y - unitLocation.y;
            if ((a <= -b + movement) && (a >= -b - movement) && (a <= b + movement) && (a >= b - movement)) {
                moves.Add(new Tile(t));
            }
        });

        foreach (Tile t in moves) {
            //set tile variables 
            t.initialDistance = CalcCost(unitLocation, t);
            t.xOffset = CalcXOffset(unitLocation, t);
            t.yOffset = CalcYOffset(unitLocation, t);

        }

        //sort list so tiles closest to unit are first
        //remove enemy tiles from range
        //calculate paths to tiles in list
        //remove ally tiles from range after path calculation
        moves.Sort(new InitialDistanceComparer());

        if (isPlayerTurn) {
            RemoveOccupiedTiles(moves, unit, false, true);
            GetPaths(moves, unit);
            RemoveOccupiedTiles(moves, unit, true, false);
        } else {
            RemoveOccupiedTiles(moves, unit, true, false);
            GetPaths(moves, unit);
            RemoveOccupiedTiles(moves, unit, false, true);
        }

        return moves;
    }

    private static void GetPaths(List<Tile> list, GameObject unit) {
        GetAdjacency(list, unit);

        //trace 4 times to get optimal paths
        for (int i = 0; i < 4; i++) {
            TracePaths(list, unit);
        }
        //Remove tiles that exceed range due to height restriction, tiles with no path, and origin tile
        list.RemoveAll(i => OverHeight(i, unit));
        list.RemoveAll(i => i.path.Count == 0);
        list.RemoveAt(0);

        ////////////////////////////Debug: label tiles with numbers
        //list.ForEach(t => {
        //    GameObject n = GameObject.Instantiate(numberProjector, new Vector3(t.x, t.height + .05f, t.y), Quaternion.Euler(90, 0, 0));
        //    n.GetComponent<TextMesh>().text = ;
        //});


    }

    private static bool OverHeight(Tile tile, GameObject unit) {
        tile.additionalCost = 0;
        //set tile range to movement range of unit
        float height = 0f;
        for (int x = 0; x < tile.path.Count; x++) {
            //if first tile in path set height to tile height
            if (tile.path[x] == tile.path[0]) {
                height = tile.path[x].height;
                //if difference of current tile height and last tile height > heightThreshold 
            } else if (tile.path[x].height - height > heightThreshold) {
                //if current tile is within height threshold of 2nd previous tile, mark previous as flag tile to jump over
                if (x > 1 && Math.Abs(tile.path[x].height - tile.path[x - 2].height) < heightThreshold) {
                    tile.path[x - 1] = new Tile(new Vector3(-1f, tile.path[x].height, -1f));
                    //increment additionalCost 
                } else {
                    int add = (int)((tile.path[x].height - height) * jumpScale);
                    tile.additionalCost += add;
                }
            }


            //set height to current tile height
            height = tile.path[x].height;
        }

        //return a boolean for List.RemoveAll()
        if (tile.path.Count - 1 + tile.additionalCost > unit.GetComponent<Unit>().movement) {
            return true;
        } else {
            return false;
        }
    }

    private static void TracePaths(List<Tile> list, GameObject unit) {
        foreach (Tile current in list) {
            //if current tile is unit coordinate, generate path for it and each adjacent tile
            if (current == list[0]) {
                if (current.path.Count == 0) {
                    current.path.Add(current);
                }
                foreach (Tile adjacent in current.adjacent) {
                    if (adjacent.path.Count == 0) {
                        OverHeight(adjacent, unit);
                        adjacent.path.Add(current);
                        adjacent.path.Add(adjacent);
                    }
                }
            } else {
                //if current has already received a path
                if (current.path.Count > 0) {
                    foreach (Tile adjacent in current.adjacent) {
                        //if adjacent tiles don't have paths or have worse paths, generate path
                        // overwrite path if actual movement cost is higher that proposed movement cost or 
                        // costs are equal but proposed carries over a better range
                        if (adjacent.path.Count == 0 || adjacent.Cost > current.Cost + 1 || adjacent.Cost == current.Cost + 1 && current.additionalCost < adjacent.additionalCost) {
                            adjacent.path.Clear();
                            adjacent.path.AddRange(current.path);
                            adjacent.path.Add(adjacent);
                            OverHeight(adjacent, unit);
                        }
                    }
                }
            }
        }
    }

    private static void RemoveOccupiedTiles(List<Tile> tileList, GameObject unit, bool removePlayer, bool removeEnemy) {
        //shoot a ray from above all tiles, if it hits a player/enemy, remove the tile from the list
        if (removePlayer) {
            tileList.RemoveAll(tile => {
                RaycastHit hit;
                Ray restrictRay = new Ray(new Vector3(tile.x, 50, tile.y), Vector3.down);
                if (Physics.Raycast(restrictRay, out hit, 55f)) {
                    if (hit.collider.tag == "PlayerUnit") {
                        return true;
                    }
                }
                return false;
            });

        }
        if (removeEnemy) {
            tileList.RemoveAll(tile => {
                RaycastHit hit;
                Ray restrictRay = new Ray(new Vector3(tile.x, 50, tile.y), Vector3.down);
                if (Physics.Raycast(restrictRay, out hit, 55f)) {
                    if (hit.collider.tag == "EnemyUnit") {
                        return true;
                    }
                }
                return false;
            });

        }
    }


    private static void GetAdjacency(List<Tile> list, GameObject unit) {
        for (int a = 0; a < list.Count; a++) {
            Tile tile = list[a];
            //add tile to the East if it exists and is within height range
            Tile tileToAdd = list.Find(x => (x.x == tile.x + 1 && x.y == tile.y && tile.height >= x.height - unit.GetComponent<Unit>().jump / jumpScale));
            if (tileToAdd != null) {
                tile.adjacent.Add(tileToAdd);
            }
            //add tile to the West if it exists and is within height range
            tileToAdd = list.Find(x => (x.x == tile.x - 1 && x.y == tile.y && tile.height >= x.height - unit.GetComponent<Unit>().jump / jumpScale));
            if (tileToAdd != null) {
                tile.adjacent.Add(tileToAdd);
            }
            //add tile to the North if it exists and is within height range
            tileToAdd = list.Find(x => (x.x == tile.x && x.y == tile.y + 1 && tile.height >= x.height - unit.GetComponent<Unit>().jump / jumpScale));
            if (tileToAdd != null) {
                tile.adjacent.Add(tileToAdd);
            }
            //add tile to the South if it exists and is within height range
            tileToAdd = list.Find(x => (x.x == tile.x && x.y == tile.y - 1 && tile.height >= x.height - unit.GetComponent<Unit>().jump / jumpScale));
            if (tileToAdd != null) {
                tile.adjacent.Add(tileToAdd);
            }
        }
        //remove unreachable tiles
        list.RemoveAll(x => x.adjacent.Count == 0);
    }
    #endregion

    #region Targeting

    public static List<Tile> GetTargetRange(GameObject unit, bool removePlayer, bool removeEnemy) {
        //get variables
        Transform trans = unit.transform;
        int tRange = unit.GetComponent<Unit>().targetRange;
        Tile unitLocation = new Tile(trans.position.x, trans.position.z);
        List<Tile> targetRange = new List<Tile>();
        terrainTiles.ForEach(t => {
            int a = t.x - unitLocation.x;
            int b = t.y - unitLocation.y;

            switch (unit.GetComponent<Unit>().targetStyle) {
                case TargetStyle.Band:
                    if ((a == -b + tRange) && (a <= b + tRange) && (a >= b - tRange) ||
                    (a == -b - tRange) && (a <= b + tRange) && (a >= b - tRange) ||
                    (a == b + tRange) && (a <= -b + tRange) && (a >= -b - tRange) ||
                    (a == b - tRange) && (a <= -b + tRange) && (a >= -b - tRange)) {
                        targetRange.Add(new Tile(t));
                    }
                    break;
                case TargetStyle.Cross:
                    int xOffset = CalcXOffset(t, unitLocation);
                    int yOffset = CalcYOffset(t, unitLocation);
                    if (t.x == unitLocation.x && yOffset <= tRange || t.y == unitLocation.y && xOffset <= tRange) {
                        targetRange.Add(new Tile(t));
                    }
                    break;
                case TargetStyle.Diamond:
                    if ((a <= -b + tRange) && (a >= -b - tRange) && (a <= b + tRange) && (a >= b - tRange)) {
                        targetRange.Add(new Tile(t));
                    }
                    break;
                default:
                    break;
            }

        });

        //foreach (Tile t in targetRange) {
        //    //set tile variables 
        //    t.initialDistance = CalcCost(unitLocation, t);
        //    t.xOffset = CalcXOffset(unitLocation, t);
        //    t.yOffset = CalcYOffset(unitLocation, t);

        //}

        //sort list so tiles closest to unit are first
        //remove enemy tiles from range
        //calculate paths to tiles in list
        //remove ally tiles from range after path calculation
        targetRange.Sort(new InitialDistanceComparer());

        if (isPlayerTurn) {
            RemoveOccupiedTiles(targetRange, unit, removePlayer, removeEnemy);
        } else {
            RemoveOccupiedTiles(targetRange, unit, removeEnemy, removePlayer);
        }
        return targetRange;
    }

    public static GameObject TargetOnTile(Tile targetTile, List<GameObject> unitList) {
        return unitList.Find(u => {
            Tile t = new Tile(u.transform.position.x, u.transform.position.z);
            if (t == targetTile) return true;
            return false;
        });
    }

    #endregion

    #region Actions
    public static UnityEvent UsePrayer;
    public static UnityEvent UseItem;
    public static void Attack() {
        Unit sUnit = selectedUnit.GetComponent<Unit>();
        Unit tUnit = targetUnit.GetComponent<Unit>();
        int damage = 0;
        //play attack animation
        Random.InitState((int)Time.time);
        if (Random.value > .08f && Random.value < .92f) {
            Random.InitState((int)Time.time);
            damage = (int)Random.Range(sUnit.attack * .6f, sUnit.attack);
            Debug.Log("Successful hit!");
        } else if (Random.value >= .92f) {
            damage = (int)(sUnit.attack * 1.5f);
            Debug.Log("Critical hit!");
        } else {
            Debug.Log("Miss!");
        }
        //play recieve damage animation
        tUnit.HP -= damage;
    }
    #endregion

    #region Data Management
    //Still testing////////////////////////////////////////////////////////////////////////////////////////
    public void SaveGame() {
        if (saveData == null) {
            saveData = new SaveData();
            saveData.RecordUnitData(unitList);
        } else {
            saveData.UnitDataList.Clear();
            saveData.RecordUnitData(unitList);
        }
        saveData.Inventory = ItemManager.Instance.inventory;
        saveData.GoldTotal = ItemManager.Instance.goldTotal;
        //Save checkpoint, money, unlocked content

        //temp code for saving level instead of checkpoint
        saveData.Checkpoint = checkpoint;
        saveData.Save();
    }

    public static void LoadGame(int slot) {
        saveData = SaveData.Load(slot);
        checkpoint = saveData.Checkpoint;
        ItemManager.Instance.inventory = saveData.Inventory;
        ItemManager.Instance.goldTotal = saveData.GoldTotal;
        //load checkpoint

        //temp code for loading level instead of checkpoint
        for (int i = unitList.Count - 1; i > -1; i--) {
            Destroy(unitList[i]);
        }
        unitList.Clear();
        activeUnitList.Clear();
        saveData.UnitDataList.ForEach(u => {
            GameObject unit;
            if (u.IsPlayer) {
                unit = Instantiate(Resources.Load("Prefab/PlayerUnit", typeof(GameObject)), new Vector3(u.PosRot[0, 0], u.PosRot[0, 1], u.PosRot[0, 2]),
                    Quaternion.Euler(u.PosRot[1, 0], u.PosRot[1, 1], u.PosRot[1, 2])) as GameObject;
                unit.GetComponent<Unit>().LoadUnitData(u);
            } else {
                unit = Instantiate(Resources.Load("Prefab/EnemyUnit", typeof(GameObject)), new Vector3(u.PosRot[0, 0], u.PosRot[0, 1], u.PosRot[0, 2]),
                    Quaternion.Euler(u.PosRot[1, 0], u.PosRot[1, 1], u.PosRot[1, 2])) as GameObject;
                unit.GetComponent<Unit>().LoadUnitData(u);
            }
            unitList.Add(unit);
        });
        activeUnitList = new List<GameObject>(unitList);

    }

    #endregion

    #endregion

}




