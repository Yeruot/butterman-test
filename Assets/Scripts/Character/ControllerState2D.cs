using UnityEngine;
using System.Collections;

public class ControllerState2D {
    
    public bool IsCollidingRight { get; set; }
    public bool IsCollidingLeft { get; set; }
    public bool IsCollidingAbove { get; set; }
    public bool IsCollidingBelow { get; set; }
    public bool IsMovingDownSlope { get; set; }
    public bool ISMovingUpSlope { get; set; }
    public bool IsGrounded { get { return this.IsCollidingBelow; } }
    public float SlopeAngle { get; set; }

    public bool HasCollisions { get { return this.IsCollidingRight || this.IsCollidingLeft || this.IsCollidingBelow || this.IsCollidingAbove; } }
    
    public void Reset() {

        this.IsCollidingRight =
            this.IsCollidingLeft =
            this.IsCollidingBelow =
            this.IsCollidingAbove =
            this.IsMovingDownSlope = false;

        SlopeAngle = 0;
    }

    public override string ToString() {
        return string.Format(
            "(controller: r:{0} l:{1} a:{2} b:{3} down-slope:{4} up-slope:{5} angle{6}",
            this.IsCollidingRight,
            this.IsCollidingLeft,
            this.IsCollidingAbove,
            this.IsCollidingBelow,
            this.IsMovingDownSlope,
            this.ISMovingUpSlope,
            this.SlopeAngle);
    }
}
