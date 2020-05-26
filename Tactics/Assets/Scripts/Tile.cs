using System;
using System.Collections.Generic;
using UnityEngine;

public class Tile : IComparable<Tile>, IEquatable<Tile> {

    #region Fields

    public int x;
    public int y;
    public int initialDistance;
    public int xOffset;
    public int yOffset;
    public int additionalCost;
    public int Cost { get { return path.Count - 1 + additionalCost; } private set { Cost = value; } }
    public List<Tile> adjacent;
    public List<Tile> path;
    public float height;

    #endregion

    #region Constructors

    public Tile(Tile t) {
        x = t.x;
        y = t.y;
        height = t.height;
        adjacent = new List<Tile>(t.adjacent);
        path = new List<Tile>(t.path);
    }

    public Tile(Vector3 vector3) {
        x = (int)vector3.x;
        y = (int)vector3.z;
        height = (float)Math.Round(vector3.y, 2);
        adjacent = new List<Tile>();
        path = new List<Tile>();
    }

    public Tile(Vector3 vector3, int _cost) {
        x = Mathf.RoundToInt(vector3.x);
        y = Mathf.RoundToInt(vector3.z);
        height = (float)Math.Round(vector3.y, 2);
        adjacent = new List<Tile>();
        path = new List<Tile>();
    }

    public Tile(Vector2 vector2) {
        x = Mathf.RoundToInt(vector2.x);
        y = Mathf.RoundToInt(vector2.y);
        adjacent = new List<Tile>();
        path = new List<Tile>();
        GetHeight();
    }

    public Tile(Vector2 vector2, int _cost) {
        x = Mathf.RoundToInt(vector2.x);
        y = Mathf.RoundToInt(vector2.y);
        adjacent = new List<Tile>();
        path = new List<Tile>();
        GetHeight();
    }

    public Tile(float _x, float _y) {
        x = Mathf.RoundToInt(_x);
        y = Mathf.RoundToInt(_y);
        adjacent = new List<Tile>();
        path = new List<Tile>();
        GetHeight();
    }

    public Tile(float _x, float _y, int _cost) {
        x = Mathf.RoundToInt(_x);
        y = Mathf.RoundToInt(_y);
        adjacent = new List<Tile>();
        path = new List<Tile>();
        GetHeight();
    }

    #endregion

    #region Methods

    public string PrintPath() {
        string pathString = "\nPath\n";
        foreach (Tile tile in path) {
            pathString += "(" + tile.x + ", " + tile.y + ") ";
        }
        return pathString;
    }

    public string PrintAdjacent() {
        string adjString = "\nAdjacent\n";
        foreach (Tile tile in adjacent) {
            adjString += "(" + tile.x + ", " + tile.y + ") ";
        }
        return adjString;
    }

    public void GetHeight() {
        int layermask = 1 << 8;
        RaycastHit hit;
        Ray restrictRay = new Ray(new Vector3(x, 20, y), Vector3.down);
        if (Physics.Raycast(restrictRay, out hit, 21f, layermask)) {
            height = (float)Math.Round(hit.point.y,5);
            //Debug.Log(hit.point + " " + hit.collider);
        }
    }

    #endregion

    #region Implementations and Overrides

    public static bool operator ==(Tile a, Tile b) {
        if (ReferenceEquals(a, b)) {
            return true;
        }
        if (ReferenceEquals(null, a)) {
            return false;
        }
        if (ReferenceEquals(null, b)) {
            return false;
        }
        return ((a.x == b.x) && (a.y == b.y) && (a.height == b.height));
    }


    public static bool operator !=(Tile a, Tile b) {
        return !(a == b);
    }

    public override string ToString() {
        return string.Format("Tile: {0}, {1}; height: {2}, range: {3}", x, y, height, additionalCost) + PrintPath() + PrintAdjacent();
    }

    public override bool Equals(object obj) {
        return Equals(obj as Tile);
    }

    public bool Equals(Tile other) {
        return other != null &&
               x == other.x &&
               y == other.y &&
               initialDistance == other.initialDistance &&
               xOffset == other.xOffset &&
               yOffset == other.yOffset &&
               EqualityComparer<List<Tile>>.Default.Equals(adjacent, other.adjacent) &&
               EqualityComparer<List<Tile>>.Default.Equals(path, other.path) &&
               height == other.height;
    }

    public override int GetHashCode() {
        var hashCode = 461372102;
        hashCode = hashCode * -1521134295 + x.GetHashCode();
        hashCode = hashCode * -1521134295 + y.GetHashCode();
        hashCode = hashCode * -1521134295 + initialDistance.GetHashCode();
        hashCode = hashCode * -1521134295 + xOffset.GetHashCode();
        hashCode = hashCode * -1521134295 + yOffset.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<List<Tile>>.Default.GetHashCode(adjacent);
        hashCode = hashCode * -1521134295 + EqualityComparer<List<Tile>>.Default.GetHashCode(path);
        hashCode = hashCode * -1521134295 + height.GetHashCode();
        return hashCode;
    }

    int IComparable<Tile>.CompareTo(Tile other) {
        if (initialDistance < other.initialDistance) {
            return 1;
        } else if (initialDistance > other.initialDistance) {
            return -1;
        } else {
            return 0;
        }
    }
}

#endregion

public class InitialDistanceComparer : IComparer<Tile> {
    public int Compare(Tile a, Tile b) {
        if (a.initialDistance > b.initialDistance) {
            return 1;
        } else if (a.initialDistance == b.initialDistance) {
            if (a.yOffset > b.yOffset) {
                return 1;
            } else if (a.yOffset < b.yOffset) {
                return -1;
            } else {
                if (a.xOffset > b.xOffset) {
                    return 1;
                } else if (a.xOffset < b.xOffset) {
                    return -1;
                }
                return 0;
            }
        } else {
            return -1;
        }
    }
}
