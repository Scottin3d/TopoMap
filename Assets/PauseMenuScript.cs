using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuScript : MonoBehaviour
{
    bool isPaused = false;
    public GameObject pauseMenu = null;
    public GameObject menu = null;
    public CharacterController controller = null;
    // Start is called before the first frame update
    void Start()
    {
        pauseMenu.SetActive(false);
    }

    // Update is called once per frame
    //TODO: Abstract and remove Input
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isPaused) {
            isPaused = !isPaused;
            pauseMenu.SetActive(isPaused);
            //controls.SetActive(false);
            controller.enabled = false;
            ControlTesting.SetPause(true);
            PC_Interface.ToggleLocked();
        }
    }

    public void ResumeGame() {
        isPaused = false;
        pauseMenu.SetActive(isPaused);
        //controls.SetActive(isPaused);
        controller.enabled = true;
        ControlTesting.SetPause(false);
        PC_Interface.ToggleLocked();
    }

    public void EndGame() {
        Application.Quit();
    }
}
