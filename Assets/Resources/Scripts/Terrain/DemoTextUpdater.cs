using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DemoTextUpdater : MonoBehaviour
{
    public static DemoTextUpdater current;
    public Text currentProcess = null;
    public Text currentChunk = null;
    public Text heightmapRes = null;
    public Text chunkRes = null;
    public Text numberOfChunks = null;
    public Text miscText = null;


    private void Awake() {
        Progress("");
        Chunk("");
        MiscText("");
        current = this;
    }

    public void Progress(string p) {
        currentProcess.text = p;
    }

    public void Chunk(string c) {
        currentChunk.text = c;
    }

    public void MiscText(string s) {
        miscText.text = s;
    }
}
