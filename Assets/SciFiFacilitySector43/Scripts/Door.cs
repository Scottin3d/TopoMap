using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {
    private int trDoorOpen = Animator.StringToHash("DoorOpen");
    private int trDoorClose = Animator.StringToHash("DoorClose");
    private Animator animator;
    private AudioSource audioSource;

	void Start() {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
	}


	void OnTriggerEnter(Collider c) {
        openDoor(c);
    }

	void OnTriggerExit(Collider c) {
        closeDoor(c);
	}

    public void openDoor(Collider c) {
        if (c.tag.Equals("GameController")) {
            audioSource.Play();
            animator.SetTrigger(trDoorOpen);

        }
    }
    public void closeDoor(Collider c) {
        if (c.tag.Equals("GameController")) {
            audioSource.Play();
            animator.SetTrigger(trDoorClose);
        }
    }


}
