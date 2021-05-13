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
    public float velocity;

	public bool lookForward, propelSelf = true, constantVelocity = false;
	private bool goingForward = true;

	public Vector3 walkOffset = Vector3.zero;

	private void Update()
	{
        if (propelSelf)
        {
            Increment((duration > 0) ? Time.deltaTime / duration : 0);
        }

        if (spline != null)
        {
            Vector3 position = spline.GetPoint(progress);
            transform.localPosition = position + walkOffset;
            if (lookForward)
            {
                transform.LookAt(position + spline.GetDirection(progress));
            }
        }
	}

    public void Begin(float startProg)
    {
        goingForward = true;
        progress = startProg;
    }

    public void Increment(float incProg)
    {
        if (constantVelocity)
        {
            incProg = (duration > 0f) ? velocity / (4f * duration) : 0f;
            MakeProgress(incProg);
        }
        else
        {
            MakeProgress(incProg);
        }
    }

    private void MakeProgress(float _f)
    {
        if (goingForward)
        {
            progress += _f;
            if (progress > 1f)
            {
                if (mode == SplineWalkerMode.Once)
                {
                    progress = 1f;
                }
                else if (mode == SplineWalkerMode.Loop)
                {
                    progress -= 1f;
                    //loopedOnce = true;
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
            progress -= _f;
            if (progress < 0f)
            {
                progress = -progress;
                goingForward = true;
                //loopedOnce = true;
            }
        }
    }

    public void ToggleRender(bool toRender)
    {
        gameObject.GetComponent<MeshRenderer>().enabled = toRender;
        gameObject.GetComponent<MeshCollider>().enabled = toRender;
    }

    public float GetProgress()
    {
        return progress;
    }

	public void Reset()
    {
		progress = 0f;
		goingForward = true; //IsStarted = false; loopedOnce = false;
		gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<MeshCollider>().enabled = false;
    }

    public void SetVelocity(float _f)
    {
        velocity = _f;
    }

    public bool IsRendering {  get { return gameObject.GetComponent<MeshRenderer>().enabled; } }
}
