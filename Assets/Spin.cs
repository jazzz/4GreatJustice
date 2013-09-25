using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

public class Spin : MonoBehaviour {
    void Update() {
        transform.Rotate(0, Time.deltaTime * 90, 0);
    }
}