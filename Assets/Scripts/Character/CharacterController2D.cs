using UnityEngine;
using System.Collections;

public class CharacterController2D : MonoBehaviour {

    private const float SkinWidth = 0.002f;
    private const int TotalHorizontalRays = 8;
    private const int TotalVerticalRays = 4;

    private static readonly float SlopeLimitTangent = Mathf.Tan(75f * Mathf.Deg2Rad);

    public LayerMask PlatformMask;
    public ControllerParameters2D DefaultParameters;
    public ControllerState2D State { get; private set; }
    public bool HandleCollisions { get; set; }
    public ControllerParameters2D Parameters { get { return overrideParameters ?? DefaultParameters; } }
    public GameObject StandingOn { get; private set; }
    public Vector3 PlatformVelocity { get; private set; }

    public bool CanJump {
        get {

            if(this.Parameters.JumpRestrictions == ControllerParameters2D.JumpBehaviour.CanJumpAnywhere) {
                return this.jumpIn <= 0;
            } else if(this.Parameters.JumpRestrictions == ControllerParameters2D.JumpBehaviour.CanJumpOnGround) {
                return this.State.IsGrounded;
            }

            return false;

        } }

    public Vector2 Velocity { get { return velocity; } }
    private Vector2 velocity;

    private BoxCollider2D boxCollider;
    private Transform playerTransform;
    private Vector3 playerScale;
    private float horizontalDistanceBetweenRays;
    private float verticalDistanceBetweenRays;
    private ControllerParameters2D overrideParameters;
    private Vector3 raycastTopLeft;
    private Vector3 raycastBottomLeft;
    private Vector3 raycastBottomRight;
    private float jumpIn;
    private Vector3 activeGlobalPlatformPoint;
    private Vector3 activeLocalPlatformPoint;

    public void Awake() {
        this.HandleCollisions = true;
        this.State = new ControllerState2D();
        this.playerTransform = this.transform;
        this.playerScale = this.playerTransform.localScale;
        this.boxCollider = this.GetComponent<BoxCollider2D>();

        float colliderWidth = this.boxCollider.size.x * Mathf.Abs(this.playerTransform.localScale.x) - (2 * SkinWidth);
        this.horizontalDistanceBetweenRays = colliderWidth / (TotalVerticalRays - 1);

        float colliderHeight = this.boxCollider.size.y * Mathf.Abs(this.playerTransform.localScale.y) - (2 * SkinWidth);
        this.verticalDistanceBetweenRays = colliderHeight / (TotalHorizontalRays - 1);
    }

    public void AddForce(Vector2 force) {
        velocity += force;
    }

    public void SetForce(Vector2 force) {
        velocity = force;
    }

    public void SetHorizontalForce(float x) {
        velocity.x = x;
    }

    public void SetVerticalForce(float y) {
        velocity.y = y;
    }

    public void Jump() {
        AddForce(new Vector2(0, this.Parameters.JumpMagnitude));
        this.jumpIn = Parameters.JumpFrequency;
    }

    public void LateUpdate() {
        this.jumpIn -= Time.deltaTime;
        this.velocity.y += this.Parameters.Gravity * Time.deltaTime;
        Move(this.velocity * Time.deltaTime);
    }

    private void Move(Vector2 delataMovement) {
        bool wasGrounded = this.State.IsCollidingBelow;
        State.Reset();

        //handle collisions and modify the movement
        //accordingly
        if (this.HandleCollisions) {
            this.HandlePlatforms();
            this.CalculateRayOrigins();

            if(delataMovement.y < 0 && wasGrounded) {
                this.HandleVerticalSlope(ref delataMovement);
            }

            if(Mathf.Abs(delataMovement.x) > 0.001f) {
                this.MoveHorizontally(ref delataMovement);
            }

            this.MoveVertically(ref delataMovement);

        }

        //by here the respective move function will have modified
        //delta movement to prevent the character from moving
        //by handling the collisions
        this.playerTransform.Translate(delataMovement, Space.World);

        if(Time.deltaTime > 0) {
            this.velocity = delataMovement / Time.deltaTime;
        }

        this.velocity.x = Mathf.Min(this.velocity.x, Parameters.MaxVelocity.x);
        this.velocity.y = Mathf.Min(this.velocity.y, Parameters.MaxVelocity.y);

        //if(this.State.ISMovingUpSlope) {
        //    this.velocity.y = 0;
        //}

        if(this.StandingOn != null) {
            this.activeGlobalPlatformPoint = this.playerTransform.position;
            this.activeLocalPlatformPoint = this.StandingOn.transform.InverseTransformPoint(this.playerTransform.position);
        }
    }

    public void OnTriggerEnter2D(Collider2D other) {

    }

    public void OnTriggerExit2D(Collider2D other) {

    }

    private void HandlePlatforms() {
        if (this.StandingOn != null) {
            Vector3 newGlobalPlatformPoint = this.StandingOn.transform.TransformPoint(this.activeLocalPlatformPoint);
            Vector3 moveDistance = newGlobalPlatformPoint - this.activeGlobalPlatformPoint;

            if (moveDistance != Vector3.zero) {
                this.playerTransform.Translate(moveDistance, Space.World);
            }

            PlatformVelocity = (newGlobalPlatformPoint - this.activeGlobalPlatformPoint) / Time.deltaTime;

        } else {

            PlatformVelocity = Vector3.zero;
        }

        this.StandingOn = null;
    }

    //Invoked in late update to precompute where rays will be shot
    //out from
    private void CalculateRayOrigins() {
        var size = new Vector2(this.boxCollider.size.x * Mathf.Abs(this.playerScale.x), this.boxCollider.size.y * Mathf.Abs(this.playerScale.y)) / 2;
        var center = new Vector2(this.boxCollider.offset.x * this.playerScale.x, this.boxCollider.offset.y * this.playerScale.y);

        this.raycastTopLeft = this.playerTransform.position + new Vector3(center.x - size.x + SkinWidth, center.y + size.y - SkinWidth);
        this.raycastBottomRight = this.playerTransform.position + new Vector3(center.x + size.x - SkinWidth, center.y - size.y + SkinWidth);
        this.raycastBottomLeft = this.playerTransform.position + new Vector3(center.x - size.x + SkinWidth, center.y - size.y + SkinWidth);
    }

    //needs to manipulate deltamovement pass in as reference
    //so copy is ignored
    private void MoveHorizontally(ref Vector2 deltaMovement) {
        bool isGoingRight = deltaMovement.x > 0;

        float rayDistance = Mathf.Abs(deltaMovement.x) + SkinWidth;
        Vector2 rayDirection = isGoingRight ? Vector2.right : Vector2.left;
        Vector2 rayOrigin = isGoingRight ? this.raycastBottomRight : this.raycastBottomLeft;

        for(int i = 0; i < TotalHorizontalRays; i++) {
            Vector2 rayVector = new Vector2(rayOrigin.x, rayOrigin.y + (i * this.verticalDistanceBetweenRays));
            Debug.DrawRay(rayVector, rayDirection * rayDistance, Color.red);

            RaycastHit2D raycastHit = Physics2D.Raycast(rayVector, rayDirection, rayDistance, PlatformMask);
            if (!raycastHit) {
                continue;
            }

            if(i == 0 && this.HandleHorizontalSlope(ref deltaMovement, Vector2.Angle(raycastHit.normal, Vector2.up), isGoingRight)) {
                break;
            }

            deltaMovement.x = raycastHit.point.x - rayVector.x;
            rayDistance = Mathf.Abs(deltaMovement.x);

            if (isGoingRight) {
                deltaMovement.x -= SkinWidth;
                State.IsCollidingRight = true;
            } else {
                deltaMovement.x += SkinWidth;
                State.IsCollidingLeft = true;
            }

        }
    }

    private void MoveVertically(ref Vector2 deltaMovement) {

        bool isGoingUp = deltaMovement.y > 0;
        float rayDistance = Mathf.Abs(deltaMovement.y) + SkinWidth;
        Vector2 rayDirection = isGoingUp ? Vector2.up : Vector2.down;
        Vector2 rayOrigin = isGoingUp ? this.raycastTopLeft : this.raycastBottomLeft;

        //need to have ray origin be at where we want to be
        rayOrigin.x += deltaMovement.x;

        float standingOnDistance = float.MaxValue;

        for(int i = 0; i < TotalVerticalRays; i++) {
            Vector2 rayVector = new Vector2(rayOrigin.x + (i * this.horizontalDistanceBetweenRays), rayOrigin.y);
            Debug.DrawRay(rayVector, rayDirection * rayDistance, Color.red);

            RaycastHit2D raycastHit = Physics2D.Raycast(rayVector, rayDirection, rayDistance, this.PlatformMask);
            if (!raycastHit) {
                continue;
            }

            if (!isGoingUp) {
                float verticalDistanceToHit = this.playerTransform.position.y - raycastHit.point.y;

                if(verticalDistanceToHit < standingOnDistance) {
                    standingOnDistance = verticalDistanceToHit;
                    this.StandingOn = raycastHit.collider.gameObject;
                }

            }

            deltaMovement.y = raycastHit.point.y - rayVector.y;
            rayDistance = Mathf.Abs(deltaMovement.y);

            if (isGoingUp) {
                deltaMovement.y -= SkinWidth;
                this.State.IsCollidingAbove = true;
            } else {
                deltaMovement.y += SkinWidth;
                this.State.IsCollidingBelow = true;
            }

            if(!isGoingUp && deltaMovement.y > 0.0001f) {
                this.State.ISMovingUpSlope = true;
            }

            if(rayDistance < SkinWidth + 0.0001f) {
                break;
            }

        }

    }

    private void HandleVerticalSlope(ref Vector2 deltaMovement) {
        float center = (this.raycastBottomLeft.x + this.raycastBottomRight.x) / 2;

        float slopeDistance = SlopeLimitTangent * (this.raycastBottomRight.x - center);
        Vector2 slopeRayVector = new Vector2(center, this.raycastBottomLeft.y);

        Debug.DrawRay(slopeRayVector, Vector3.down * slopeDistance, Color.yellow);

        RaycastHit2D raycastHit = Physics2D.Raycast(slopeRayVector, Vector3.down, slopeDistance, PlatformMask);

        if (!raycastHit) {
            return;
        }

        bool isMovingDownSlope = Mathf.Sign(raycastHit.normal.x) == Mathf.Sign(deltaMovement.x);

        if (isMovingDownSlope) {
            return;
        }

        float angle = Vector2.Angle(raycastHit.normal, Vector2.up);
        if(Mathf.Abs(angle) < 0.0001f) {
            return;
        }

        State.IsMovingDownSlope = true;
        State.SlopeAngle = angle;
        deltaMovement.y = raycastHit.point.y - slopeRayVector.y;

    }

    private bool HandleHorizontalSlope(ref Vector2 deltaMovement, float angle, bool isGoingRight) {
        if (Mathf.RoundToInt(angle) == 90) return false;

        if (angle > Parameters.SlopeLimit) {
            deltaMovement.x = 0;
            return true;
        }

        if(deltaMovement.y > .07) {
            return true;
        }

        deltaMovement.x += isGoingRight ? -SkinWidth : SkinWidth;
        deltaMovement.y = Mathf.Abs(Mathf.Tan(angle * Mathf.Deg2Rad) * deltaMovement.x);
        this.State.ISMovingUpSlope = true;
        this.State.IsCollidingBelow = true;
        return true;

    }
}
