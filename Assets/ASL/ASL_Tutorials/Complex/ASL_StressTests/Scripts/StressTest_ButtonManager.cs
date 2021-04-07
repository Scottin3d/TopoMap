using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StressTesting
{
    /// <summary>Button Manager for Stress Tests</summary>
    public class StressTest_ButtonManager : MonoBehaviour
    {
        /// <summary>A handle to the Delete Random button</summary>
        public Button mDeleteButton = null;
        /// <summary>A handle to the Stop button</summary>
        public Button mStopAll = null;
        /// <summary> Up button for 150 objects stress test</summary>
        public Button m_Up = null;
        /// <summary> Down button for 150 objects stress test</summary>
        public Button m_Down = null;
        /// <summary> Left button for 150 objects stress test</summary>
        public Button m_Left = null;
        /// <summary> Right button for 150 objects stress test</summary>
        public Button m_Right = null;

        public static bool m_MoveUp;
        public static bool m_MoveDown;
        public static bool m_MoveLeft;
        public static bool m_MoveRight;

        /// <summary>Delete a random ASL object in the scene</summary>
        public void DeleteObject()
        {
            Debug.Log("Randomly selecting and deleting an object...");
            //Since we don't keep track of the amount of ASL objects in the scene, we need to find all of them so we know which ones we can move
            int objectNumber = -1;
            var aslObjects = FindObjectsOfType<ASL.ASLObject>(); //Warning: Getting objects this way is slow
            if (aslObjects.Length > 0) //If there is an ASL object to move
            {
                objectNumber = Random.Range(0, aslObjects.Length); //Randomly grab an ASL object
                aslObjects[objectNumber].SendAndSetClaim(() =>
                {
                    aslObjects[objectNumber].DeleteObject(); //Once claimed, delete this object
                });
            }
        }

        /// <summary>
        /// Find a random object and use it to stop all other objects in the scene so you can examine their positions to see if they're still in sync
        /// </summary>
        public void StopAllClients()
        {
            var randomObject = FindObjectOfType<ASL.ASLObject>();
            randomObject.GetComponent<ASL.ASLObject>()?.SendAndSetClaim(() =>
            {
                float[] myValue = new float[4];
                myValue[0] = 0;
                myValue[1] = 1;
                myValue[2] = 2;
                myValue[3] = 3;
                randomObject.GetComponent<ASL.ASLObject>()?.SendFloatArray(myValue);
            });

        }

        /// <summary> For 150_Objects stress test: Move all objects up </summary>
        public void MoveUpSelect()
        {
            m_MoveUp = true;
        }

        /// <summary> For 150_Objects stress test: Stop all objects from moving up </summary>
        public void MoveUpDeselect()
        {
            m_MoveUp = false;
        }

        /// <summary> For 150_Objects stress test: Move all objects down </summary>
        public void MoveDownSelect()
        {
            m_MoveDown = true;
        }

        /// <summary> For 150_Objects stress test: Stop all objects from moving down </summary>
        public void MoveDownDeselect()
        {
            m_MoveDown = false;
        }

        /// <summary> For 150_Objects stress test: Move all objects left </summary>
        public void MoveLeftSelect()
        {
            m_MoveLeft = true;
        }

        /// <summary> For 150_Objects stress test: Stop all objects from moving left </summary>
        public void MoveLeftDeselect()
        {
            m_MoveLeft = false;
        }

        /// <summary> For 150_Objects stress test: Move all objects right </summary>
        public void MoveRightSelect()
        {
            m_MoveRight = true;
        }

        /// <summary> For 150_Objects stress test: Stop all objects from moving right </summary>
        public void MoveRightDeselect()
        {
            m_MoveRight = false;
        }
    }
}