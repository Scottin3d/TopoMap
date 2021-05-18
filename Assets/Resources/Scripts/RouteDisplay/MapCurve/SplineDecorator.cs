using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineDecorator : MonoBehaviour
{
	//From https://catlikecoding.com/unity/tutorials/curves-and-splines/

	private static SplineDecorator _sd;
	public BezierSpline spline;
	private bool lookForward;

	void Awake()
    {
		if (_sd == null) _sd = this;
    }

	/// <summary>
    /// Decorates the Bezier spline using the list of game objects
    /// </summary>
    /// <param name="items">The list of game objects to be used as decoration</param>
	public static void Decorate(List<GameObject> items)
    {
		if (_sd.spline == null) return;
		if (items == null || items.Count == 0)
		{
			return;
		}
		float stepSize = items.Count;
		if (_sd.spline.Loop || stepSize == 1)
		{
			stepSize = 1f / stepSize;
		}else if(stepSize > _sd.spline.Length)
        {
			stepSize = 1f / (_sd.spline.Length - 1);
        }
		else
		{
			stepSize = 1f / (stepSize - 1);
		}
		for (int p = 0, f = 0; f < 1; f++)
		{
			for (int i = 0; i < items.Count; i++, p++)
			{
				items[i].SetActive(true);
				Vector3 position = _sd.spline.GetPoint(p * stepSize);
				items[i].transform.localPosition = position + Vector3.up;
				if (_sd.lookForward)
				{
					items[i].transform.up = _sd.spline.GetDirection(p * stepSize);
				}
				items[i].transform.parent = _sd.gameObject.transform;
			}
		}
	}

	public static void SetSpline(BezierSpline _bs) { _sd.spline = _bs; }
	public static void SetForwardLook(bool IsForward) { _sd.lookForward = IsForward; }
}
