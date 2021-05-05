using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OccluderTrigger : MonoBehaviour
{
   
    public enum Occlusion { enable_Zone, disable_Zone}
    public Occlusion OcclusionAction;

    private Occluder Occl;

    private void Start() {
        Occl = GetComponentInParent<Occluder>();
    }

    private void OnTriggerEnter(Collider other) {
        if(OcclusionAction == Occlusion.enable_Zone && other.tag.Equals("GameController")) {
            Occl.enableZone();
        }
    }

    private void OnTriggerExit(Collider other) {
        if (OcclusionAction == Occlusion.disable_Zone && other.tag.Equals("GameController")) {
            Occl.disableZone();
        }
    }

    // neccessary for initial enable of a Zone
    private void OnTriggerStay(Collider other) {
        if(Time.timeSinceLevelLoad < 3 && other.tag.Equals("GameController")) {
            Occl.enableZone();
        }
        
    }
}
