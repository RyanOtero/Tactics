using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNavigationController : MonoBehaviour {

    public bool isMoving;
    public bool isAtOffset;
    public int i;
    public List<Tile> path;
    float time;
    Vector3 nextPoint;
    Vector3 startPoint;
    Vector3 endPoint;
    public int moveSpeed;
    Vector3 center;
    Vector3 srel;
    Vector3 erel;
    float delay;
    public float threshold;
    public static Vector3 startRotation;
    
    void Awake() {
        i = 1;
        moveSpeed = 4;
        delay = .25f;
        threshold = .26f;
    }

    void Update() {
        //get variables
        float heightDifference = nextPoint.y - startPoint.y;
        Vector3 startPointOffset = startPoint + new Vector3(0, heightDifference, 0);
        Vector3 endPointOffset = nextPoint + new Vector3(0, -heightDifference, 0);

        if (isMoving) {
            //BattleManager.cursor.SetActive(false);

            //initialize variables if this is the first frame of movement
            if (path == null) path = BattleManager.selectedTile.path;
            if (startPoint == Vector3.zero) GetStartPoint();
            if (nextPoint == Vector3.zero) GetNextPoint(i);
            if (endPoint == Vector3.zero) GetEndPoint();
            
            //increment the timer
            time += Time.deltaTime;

            //handle unit rotation
            if (transform.position.x == nextPoint.x) {
                if (transform.position.z > nextPoint.z) {
                    transform.eulerAngles = new Vector3(0, 180f, 0);
                } else if (transform.position.z < nextPoint.z) {
                    transform.eulerAngles = new Vector3(0, 0, 0);
                }
            } else if (transform.position.z == nextPoint.z) {
                if (transform.position.x > nextPoint.x) {
                    transform.eulerAngles = new Vector3(0, -90f, 0);
                } else if (transform.position.x < nextPoint.x) {
                    transform.eulerAngles = new Vector3(0, 90f, 0);
                }
            }

            //////////////This block deals with jumping over a single low tile in the path 
            // check if this tile is the flag tile to jump over, if so make endpoint next tile in path
            if (nextPoint.x == -1 && nextPoint.z == -1) {
                i++;
                GetNextPoint(i);
            }
            if (TacticsUtil.CalcCost(startPoint, nextPoint) > 1) {
                //wait for the timer to pass the delay
                if (time < delay) {
                    return;
                }
                //once past the delay, jump over flag tile
                if (time >= delay && transform.position != nextPoint) {
                    center = (startPoint + nextPoint) / 2f;
                    center -= Vector3.up * .5f;
                    srel = startPoint - center;
                    erel = nextPoint - center;
                    transform.position = Vector3.Slerp(srel, erel, (time - delay) * moveSpeed * .6f) + center;
                    return;
                }
                
            }


            //if the path raises above the threshold to jump up
            if (heightDifference > threshold) {
                //raise to height of next tile after wait period
                if (isAtOffset == false && transform.position != startPointOffset) {
                    if (time >= delay) transform.position = Vector3.Lerp(startPoint, startPointOffset, (time - delay) * moveSpeed * 1.8f);
                //reset timer when at height
                } else if (isAtOffset == false && transform.position == startPointOffset) {
                    isAtOffset = true;
                    time = 0f;
                //arc to endpoint
                } else if (isAtOffset == true) {
                    center = (startPoint + nextPoint) / 2f;
                    center -= Vector3.up * .005f;
                    srel = startPointOffset - center;
                    erel = nextPoint - center;
                    transform.position = Vector3.Slerp(srel, erel, time * moveSpeed * .9f) + center;
                }

            //if the path is within height threshold
            } else if (heightDifference <= threshold && heightDifference >= -threshold) {
                transform.position = Vector3.Lerp(startPoint, nextPoint, time * moveSpeed);

                //if the path drops below the threshold to jump down
            } else if (heightDifference < -threshold) {
                //arc to endpoint + height offset after wait period
                if (isAtOffset == false && transform.position != endPointOffset) {
                    center = (startPoint + endPointOffset) / 2f;
                    center -= Vector3.up * .005f;
                    srel = startPoint - center;
                    erel = endPointOffset - center;
                    if (time >= delay) transform.position = Vector3.Slerp(srel, erel, (time - delay) * moveSpeed * .9f) + center;
                //reset timer when at endpoint + height offset
                } else if (isAtOffset == false && transform.position == endPointOffset) {
                    isAtOffset = true;
                    time = 0f;
                //drop to endpoint
                } else if (isAtOffset == true) {
                    transform.position = Vector3.Lerp(endPointOffset, nextPoint, time * moveSpeed * 1.8f);
                }
            }
            //when done with path, set variables to null/0
            if (transform.position == endPoint) {
                //BattleManager.cursor.SetActive(true);
                Reset();
                isAtOffset = false;
            //if at endpoint, move to next startpoint and endpoint
            } else if (transform.position == nextPoint) {
                isAtOffset = false;
                i++;
                time = 0f;
                GetStartPoint();
                GetNextPoint(i);
            }
        }
    }

    #region Methods
    public void GetStartPoint() {
        startPoint = transform.position;
    }

    public void GetNextPoint(int i) {
        nextPoint = new Vector3(path[i].x, path[i].height + .5f, path[i].y);
    }

    public void GetEndPoint() {
        endPoint = new Vector3(path[path.Count - 1].x, path[path.Count - 1].height + .5f, path[path.Count - 1].y);
    }


    public void Reset() {
        //BattleManager.cursor.SetActive(true);
        isMoving = false;
        startPoint = Vector3.zero;
        nextPoint = Vector3.zero;
        path = null;
        endPoint = Vector3.zero;
        time = 0f;
        i = 1;
    }
    #endregion

}

