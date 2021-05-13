using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PositionPanel : MonoBehaviour
{
    private bool IsVisible = false;
    public PanelHideDirection direction;
    public Button myButton = null;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(myButton != null);
    }

    /// <summary>
    /// Toggle whether this panel should be visible, and hide accordingly
    /// </summary>
    public void ToggleVisibility()
    {
        IsVisible = !IsVisible;
        StartCoroutine(HidePanel(IsVisible));
    }

    /// <summary>
    /// Handles panel movement
    /// </summary>
    /// <param name="toggle">Whether the panel should be visible</param>
    /// <returns></returns>
    private IEnumerator HidePanel(bool toggle)
    {
        bool NotHide = IsVisible;
        RectTransform theRect = gameObject.GetComponent<RectTransform>();
        if(theRect != null)
        {
            //Debug.Log(theRect.anchoredPosition);
            int delta = -1;
            //Get offset of panel
            switch(direction){
                case PanelHideDirection.Left:
                case PanelHideDirection.Right:
                    delta = (int)(theRect.rect.width - myButton.gameObject.GetComponent<RectTransform>().rect.height);
                    break;
                case PanelHideDirection.Up:
                case PanelHideDirection.Down:
                    delta = (int)(theRect.rect.height - myButton.gameObject.GetComponent<RectTransform>().rect.height);
                    break;
            }
            while(delta > 0)
            {
                switch (direction)
                {
                    case PanelHideDirection.Left:
                        if(NotHide) theRect.anchoredPosition += 10f * Vector2.right;
                        else theRect.anchoredPosition -= 10f * Vector2.right;
                        break;
                    case PanelHideDirection.Right:
                        if (NotHide) theRect.anchoredPosition += 10f * Vector2.left;
                        else theRect.anchoredPosition -= 10f * Vector2.left;
                        break;
                    case PanelHideDirection.Up:
                        if (NotHide) theRect.anchoredPosition += 10f * Vector2.down;
                        else theRect.anchoredPosition -= 10f * Vector2.down;
                        break;
                    case PanelHideDirection.Down:
                        if (NotHide) theRect.anchoredPosition += 10f * Vector2.up;
                        else theRect.anchoredPosition -= 10f * Vector2.up;
                        break;
                }
                delta -= 10;
                yield return new WaitForSeconds(0.001f);
            }
        }
        yield return new WaitForSeconds(0.01f);
    }
}

/// <summary>
/// Denotes the side of the screen this panel hides on
/// </summary>
public enum PanelHideDirection
{
    Left,
    Right,
    Up,
    Down
}
