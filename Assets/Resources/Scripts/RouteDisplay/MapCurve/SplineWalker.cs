using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineWalker : MonoBehaviour
{
	//From https://catlikecoding.com/unity/tutorials/curves-and-splines/
	public BezierSpline spline;
	public SplineWalkerMode mode;

	public float duration;
	private float progress;

	public bool lookForward;
	private bool goingForward = true;

	public Vector3 walkOffset = Vector3.zero;

	private void Update()
	{
			if (goingForward)
			{
				progress += Time.deltaTime / duration;
				if (progress > 1f)
				{
					if (mode == SplineWalkerMode.Once)
					{
						progress = 1f;
					}
					else if (mode == SplineWalkerMode.Loop)
					{
						progress -= 1f;
					}
					else
					{
						progress = 2f - progress;
						goingForward = false;
					}
				}
			}
			else
			{
				progress -= Time.deltaTime / duration;
				if (progress < 0f)
				{
					progress = -progress;
					goingForward = true;
				}
			}

		if(spline != null)
        {
			gameObject.GetComponent<MeshRenderer>().enabled = true;
			Vector3 position = spline.GetPoint(progress);
			transform.localPosition = position + walkOffset;
			if (lookForward)
			{
				//transform.LookAt(position + spline.GetDirection(progress));
				transform.up = spline.GetDirection(progress);
			}
		} else
        {
			gameObject.GetComponent<MeshRenderer>().enabled = false;
		}
	}

	public void Reset()
    {
		progress = 0f;
		goingForward = true;
		gameObject.GetComponent<MeshRenderer>().enabled = false;
    }
}
