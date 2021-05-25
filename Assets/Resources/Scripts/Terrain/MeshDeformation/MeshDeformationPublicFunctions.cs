using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public partial class MeshManipulation : MonoBehaviour {

    public static event Action<string> ChangeStrength;
    public static event Action<string> ChangeRadius;

    public void ChangeDeformationStrength(float f) {
        deformationStrength += f;
        deformationStrength = (deformationStrength >= 2.5f) ? 2.5f : deformationStrength;
        ChangeStrength?.Invoke(deformationStrength.ToString());
    }

    public void ChangeDeformationRadius(float f) {
        radius += f;
        radius = (radius <= radiusMin) ? radiusMin : radius;
        ChangeRadius?.Invoke(radius.ToString());
    }
}
