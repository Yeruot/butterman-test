using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FollowPath : MonoBehaviour {
    
    public enum FollowType
    {
        MoveTowards,
        Lerp
    }

    public FollowType Type = FollowType.MoveTowards;
    public Path PathDefinition;
    public float Speed = 1;
    public float MaxPaddingToGoal = 0.1f;

    private IEnumerator<Transform> CurrentPoint;

    public void Start() {
        if(this.PathDefinition == null) {
            //Shouldn't ever happen
            Debug.LogError("Path Not Set");
            return;
        }

        //Get the enumerator for the paths and initialize to the first value
        this.CurrentPoint = this.PathDefinition.GetPathEnumerator();
        this.CurrentPoint.MoveNext();

        if(this.CurrentPoint == null) {
            Debug.LogError("Path has no points in it");
        }

        this.transform.position = this.CurrentPoint.Current.position;

    }

    public void Update() {
        if(this.CurrentPoint == null || this.CurrentPoint.Current == null) {
            Debug.LogError("This path is fucked yo");
        }

        if(this.Type == FollowType.MoveTowards) {
            this.transform.position = Vector3.MoveTowards(this.transform.position, this.CurrentPoint.Current.position, Time.deltaTime * this.Speed);
        } else if (this.Type == FollowType.Lerp) {
            this.transform.position = Vector3.Lerp(this.transform.position, this.CurrentPoint.Current.position, Time.deltaTime * this.Speed);
        }

        float distanceSquared = (this.transform.position - this.CurrentPoint.Current.position).sqrMagnitude;

        if(distanceSquared < (this.MaxPaddingToGoal * this.MaxPaddingToGoal)) {
            this.CurrentPoint.MoveNext();
        }
    }

}
