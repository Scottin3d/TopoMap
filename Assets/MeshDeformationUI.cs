using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeshDeformationUI : MonoBehaviour
{
    public Text deformationStregth = null;
    public Text deformationRadius = null;

    private void Start() {
        MeshManipulation.ChangeStrength += HandleStrengthText;
        MeshManipulation.ChangeRadius += HandleRadiusText;
    }

    public void HandleStrengthText(string str) {
        deformationStregth.text = str;
    }

    public void HandleRadiusText(string str) {
        deformationRadius.text = str;
    }
}
