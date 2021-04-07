using UnityEngine;

namespace StressTesting
{
    /// <summary>
    /// Used to hold the starting color of the object this script is attached to - allowing switch back to this color when no one is selecting it
    /// </summary>
    public class StressTest_150_ASLObjects_Support : MonoBehaviour
    {
        /// <summary>The original color of the object this class gets assigned to</summary>
        public Color m_MyObjectOriginalColor;
        
        /// <summary>
        /// Start function - called right away
        /// </summary>
        void Start()
        {
            m_MyObjectOriginalColor = transform.GetComponent<Renderer>().material.color;
        }

        /// <summary>
        /// Move object based on button inputs
        /// </summary>
        private void Update()
        {
            Move();
        }

        /// <summary>
        /// Move object based on button input - as this code is called from the update loop, it will send the maximum amount of packets as possible 
        /// (about 2100 packets for a single quick click) Generally, this is a bad thing, but the point is to stress the system.
        /// Bottom line - know that if you want a lot of dynamically moving ASL objects in your scene. you should not do set it up 
        /// like this stress test does, there are better ways to do it (look up delegates)
        /// </summary>
        private void Move()
        {
            if (StressTest_ButtonManager.m_MoveRight) //Move right
            {
                gameObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    gameObject.GetComponent<ASL.ASLObject>().SendAndIncrementLocalPosition(new Vector3(.01f, 0, 0));
                }, 0); //By setting timeout to 0 we are keeping this object until someone steals it from us - thus forcing the OnRelease function to only occur when stolen
            }
            else if (StressTest_ButtonManager.m_MoveLeft) //Move left
            {
                gameObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    gameObject.GetComponent<ASL.ASLObject>().SendAndIncrementLocalPosition(new Vector3(-.01f, 0, 0));
                }, 0); //By setting timeout to 0 we are keeping this object until someone steals it from us - thus forcing the OnRelease function to only occur when stolen
            }
            if (StressTest_ButtonManager.m_MoveUp) //Move up
            {
                gameObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    gameObject.GetComponent<ASL.ASLObject>().SendAndIncrementLocalPosition(new Vector3(0, .01f, 0));
                }, 0); //By setting timeout to 0 we are keeping this object until someone steals it from us - thus forcing the OnRelease function to only occur when stolen
            }
            else if (StressTest_ButtonManager.m_MoveDown) //Move down
            {
                gameObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    gameObject.GetComponent<ASL.ASLObject>().SendAndIncrementLocalPosition(new Vector3(0, -.01f, 0));
                }, 0); //By setting timeout to 0 we are keeping this object until someone steals it from us - thus forcing the OnRelease function to only occur when stolen
            }
        }

    }
}