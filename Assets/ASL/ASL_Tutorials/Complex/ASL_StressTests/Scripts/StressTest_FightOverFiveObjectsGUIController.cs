using UnityEngine;
using UnityEngine.UI; // for GUI elements: Button, Toggle

namespace StressTesting
{
    /// <summary>GUI Controller for StressTest_FightOverFiveObjects - Controls displaying object positions</summary>
    public partial class StressTest_FightOverFiveObjectsGUIController : MonoBehaviour
    {
        // reference to all UI elements in the Canvas
        /// <summary>Text holding the 1st object's world position</summary>
        public Text Object1Pos;
        /// <summary>Text holding the 2nd object's world position</summary>
        public Text Object2Pos;
        /// <summary>Text holding the 3rd object's world position</summary>
        public Text Object3Pos;
        /// <summary>Text holding the 4th object's world position</summary>
        public Text Object4Pos;
        /// <summary>Text holding the 5th object's world position</summary>
        public Text Object5Pos;

        /// <summary>The 1st object in the scene</summary>
        public GameObject Grandparent = null;
        /// <summary>The 2nd object in the scene</summary>
        public GameObject Parent = null;
        /// <summary>The 3rd object in the scene</summary>
        public GameObject Child = null;
        /// <summary>The 4th object in the scene</summary>
        public GameObject Capsule = null;
        /// <summary>The 5th object in the scene</summary>
        public GameObject Cube = null;

        // Use this for initialization
        void Start()
        {
            Debug.Assert(Object1Pos != null);
            Debug.Assert(Object2Pos != null);
            Debug.Assert(Object3Pos != null);
            Debug.Assert(Object4Pos != null);
            Debug.Assert(Object5Pos != null);

            Object1Pos.text = "Object 1 World Position: " + Grandparent.transform.position;
            Object2Pos.text = "Object 2 World Position: " + Parent.transform.position;
            Object3Pos.text = "Object 3 World Position: " + Child.transform.position;
            Object4Pos.text = "Object 4 World Position: " + Capsule.transform.position;
            Object5Pos.text = "Object 5 World Position: " + Cube.transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            UpdatePositionText();
        }

        //Update the GUI positions based on the world positions of the corresponding objects
        private void UpdatePositionText()
        {
            if (Grandparent != null)
            {
                Object1Pos.text = "Object 1 World Position: " + Grandparent.transform.position;
            }
            if (Parent != null)
            {
                Object2Pos.text = "Object 2 World Position: " + Parent.transform.position;
            }
            if (Child != null)
            {
                Object3Pos.text = "Object 3 World Position: " + Child.transform.position;
            }
            if (Capsule != null)
            {
                Object4Pos.text = "Object 4 World Position: " + Capsule.transform.position;
            }
            if (Cube != null)
            {
                Object5Pos.text = "Object 5 World Position: " + Cube.transform.position;
            }
        }
    }
}