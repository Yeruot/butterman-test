using UnityEngine;
using System.Collections;

public class Butterman : MonoBehaviour {

    [SerializeField] Rigidbody2D playerBody;
    [SerializeField] float maxSpeed;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetButtonDown("Jump")) {
            this.playerBody.velocity = new Vector3(this.playerBody.velocity.x, this.playerBody.velocity.y + 10);
        }

	}

    void FixedUpdate() {
        this.playerBody.velocity = new Vector3(Input.GetAxis("Horizontal") * this.maxSpeed, this.playerBody.velocity.y);
    }
}
