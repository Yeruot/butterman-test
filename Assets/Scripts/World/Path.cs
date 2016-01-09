using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Path : MonoBehaviour {

    public Transform[] Points;

    public IEnumerator<Transform> GetPathEnumerator() {
        if(this.Points == null || this.Points.Length < 1) {
            yield break;
        }

        int index = 0;
        int direction = 1;

        while (true) {

            yield return this.Points[index];

            if(this.Points.Length == 1) {
                continue;
            } else if(index <= 0) {
                direction = 1;
            } else if(index >= (Points.Length - 1)) {
                direction = -1;
            }
            index = index + direction;
        }

    }

    public void OnDrawGizmos() {

        if (this.Points == null || this.Points.Length < 2) return;

        for(int i = 0; i < this.Points.Length; i++) {

            if(this.Points[i] != null) {
                Gizmos.DrawIcon(this.Points[i].position, "PathPosition");

                if(i >= 1) {
                    Gizmos.DrawLine(this.Points[i - 1].position, this.Points[i].position);
                }
            }
        }

    }

}
