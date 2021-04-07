using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPS : MonoBehaviour
{
    private Text text;
    float frameCount = 0;
    float fps = 0.0f;
    float updateRate = 1.0f;  // 4 updates per sec.
    float nextUpdate = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
        nextUpdate = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        frameCount++;
        if (Time.time > nextUpdate) {
            nextUpdate += 1.0f / updateRate;
            fps = frameCount * updateRate;
            frameCount = 0f;
            text.text = fps.ToString("N0");
        }
        
    }
}
