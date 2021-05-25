using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkData : MonoBehaviour
{
    private MapChunk mapChunk = null;
    public MapChunk MapChunk { get => mapChunk; set => mapChunk = value; }

    public IEnumerator AskIfVisible()
    {
        while (true)
        {
            if(mapChunk != null)
            {
                ScaleLine.IsChunkVisible(this);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
}
