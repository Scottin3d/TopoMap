using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PositionPanel : MonoBehaviour
{
    private static PositionPanel panel;
    private static bool IsVisible = false;
    private GameObject curNode = null;
    public Text myText = null;
    public Button myButton = null;

    void Awake()
    {
        if (panel == null) panel = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(myButton != null);
        Debug.Assert(myText != null);
    }

    // Update is called once per frame
    void Update()
    {
        if(curNode != null)
        {
            //position text
            //turn coordinates into real world, if possible
            myText.text = string.Format("Position:\n({0:f4},{1:f4}", curNode.transform.position.x, curNode.transform.position.z);
        } else
        {
            myText.text = "No node selected";
        }
    }

    public static void SelectNode(GameObject _g)
    {
        panel.curNode = _g;
    }

    public static void ToggleVisibility()
    {
        IsVisible = !IsVisible;
        panel.StartCoroutine(HidePanel(IsVisible));
    }

    private static IEnumerator HidePanel(bool toggle)
    {
        bool NotHide = IsVisible;
        RectTransform theRect = panel.gameObject.GetComponent<RectTransform>();
        if(theRect != null)
        {
            Debug.Log(theRect.anchoredPosition);
            int width = (int)(theRect.rect.width - panel.myButton.gameObject.GetComponent<RectTransform>().rect.height);
           
            while(width > 0)
            {
                if (NotHide)
                {
                    theRect.anchoredPosition += 10f * Vector2.left;
                } else
                {
                    theRect.anchoredPosition -= 10f * Vector2.left;
                }
                width -= 10;
                yield return new WaitForSeconds(0.001f);
            }
        }
        yield return new WaitForSeconds(0.01f);
    }
}
