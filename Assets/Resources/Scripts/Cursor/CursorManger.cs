using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManger : MonoBehaviour
{
    private bool ifPaused = false;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        LockUnlockCursor();
    }

    private void LockUnlockCursor()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (!ifPaused)
            {
                UnlockCursor();
                ifPaused = true;
            }
            else
            {
                LockCursor();
                ifPaused = false;
            }
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
