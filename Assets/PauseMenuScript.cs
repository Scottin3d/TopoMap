using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuScript : MonoBehaviour
{
    bool isPaused = false;
    public GameObject pauseMenu = null;
    public GameObject menu = null;
    public GameObject controls = null;
    // Start is called before the first frame update
    void Start()
    {
        pauseMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            isPaused = !isPaused;
            pauseMenu.SetActive(isPaused);
            controls.SetActive(false);
        }
    }

    public void ResumeGame() {
        isPaused = false;
        pauseMenu.SetActive(isPaused);
        controls.SetActive(isPaused);
    }

    public void EndGame() {
        Application.Quit();
    }
}
