using UnityEngine;
using System.Collections;

public class Butterman : MonoBehaviour {

    private bool isFacingRight;
    private CharacterController2D controller;
    private float normalizedHorizontalSpeed;

    public float MaxSpeed = 8f;
    public float SpeedAccelerationOnGround = 10f;
    public float SpeedAccelerationInAir = 5f;

    public void Start() {
        this.controller = this.GetComponent<CharacterController2D>();
        this.isFacingRight = transform.localScale.x > 0;
    }

    public void Update() {
        //handle input and update controller
        this.HandleInput();

        float movementFactor = this.controller.State.IsGrounded ? this.SpeedAccelerationOnGround : this.SpeedAccelerationInAir;
        this.controller.SetHorizontalForce(Mathf.Lerp(this.controller.Velocity.x, this.normalizedHorizontalSpeed * this.MaxSpeed, Time.deltaTime * movementFactor));
    }

    private void HandleInput() {

        if (Input.GetKey(KeyCode.D)) {
            this.normalizedHorizontalSpeed = 1;
            if (!this.isFacingRight) {
                this.Flip();
            }
        } else if (Input.GetKey(KeyCode.A)) {
            this.normalizedHorizontalSpeed = -1;
            if (this.isFacingRight) {
                this.Flip();
            }
        } else {
            this.normalizedHorizontalSpeed = 0;
        }

        if(this.controller.CanJump && Input.GetKey(KeyCode.Space)) {
            this.controller.Jump();
        }

    }

    private void Flip() {
        this.transform.localScale = new Vector3(-this.transform.localScale.x, this.transform.localScale.y, this.transform.localScale.z);
        this.isFacingRight = this.transform.localScale.x > 0;
    }

}
