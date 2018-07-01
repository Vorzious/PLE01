using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookWalk : MonoBehaviour {
    public Transform VRCamera;
    public float toggleAngle = 10f;
    public float speed = 1.5f;
    public bool moveForward;
    private CharacterController cc;

	// Use this for initialization
	void Start () {
        cc = GetComponent<CharacterController>();
	}
	
	// Update is called once per frame
	void Update () {
		if(VRCamera.eulerAngles.x >= toggleAngle && VRCamera.eulerAngles.x <= 90f) {
            moveForward = true;
        } else {
            moveForward = false;
        }

        if (moveForward) {
            Vector3 forward = VRCamera.TransformDirection(Vector3.forward);
            cc.SimpleMove(forward * speed);
        }
	}
}
