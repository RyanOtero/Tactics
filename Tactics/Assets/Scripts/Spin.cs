using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour {

    public int rate;

    void Update() {
        transform.eulerAngles = new Vector3(0, transform.rotation.eulerAngles.y + rate * Time.deltaTime, 0);
    }
}
