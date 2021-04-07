using UnityEngine;

namespace StressTesting
{
    /// <summary>Controller for StressTest_FightOverFightObjects - Randomly moves 5 objects to compare positions at a later time</summary>
    public class StressTest_FightOverFiveObjectsController : MonoBehaviour
    {
        /// <summary>All of the ASL objects in this scene. Since these are ASL objects and not GameObjects,
        /// we do not need to get the ASL component before calling ASL scripts like previous examples </summary>
        public ASL.ASLObject[] TestObjects = new ASL.ASLObject[5];
        /// <summary>Flag indicating whether or not all objects in the scene should be stopped so position comparisons can be done</summary>
        public static bool StopTest;
        /// <summary>The amount of time that has gone by</summary>
        private float timer = 0;
        /// <summary>The random amount of time that needs to go by before an object can be moved</summary>
        private float randomTime = 0;
        /// <summary>The object that was selected to be moved</summary>
        int objectNumber = 0;

        // Use this for initialization
        void Start()
        {
            TestObjects = FindObjectsOfType(typeof(ASL.ASLObject)) as ASL.ASLObject[];
            StopTest = false;
            //Set float callbacks - its okay to do so locally because these objects start in the scene and 
            //this code will set the float callback for all users since its in the Start() function
            TestObjects[0]._LocallySetFloatCallback(StopClients);
            TestObjects[1]._LocallySetFloatCallback(StopClients);
            TestObjects[2]._LocallySetFloatCallback(StopClients);
            TestObjects[3]._LocallySetFloatCallback(StopClients);
            TestObjects[4]._LocallySetFloatCallback(StopClients);
        }

        private void Update()
        { 
            
            if (!StopTest)
            {
                if (timer > randomTime)
                {
                    RandomlyMoveAnObject();
                }
            }
            timer += Time.deltaTime * 1000; //Timer in milliseconds           
        }

        /// <summary>Randomly move a random object a random amount</summary>
        private void RandomlyMoveAnObject()
        {
            // Since we send commands out via the TestObjects array, but we can delete objects in this app, we need a way to ensure the array is properly set after deletion
            TestObjects = FindObjectsOfType(typeof(ASL.ASLObject)) as ASL.ASLObject[];
            objectNumber = Random.Range(0, TestObjects.Length); //Randomly select an object to move

            //Only move if not all deleted
            if (TestObjects.Length > 0)
            {
                TestObjects[objectNumber].SendAndSetClaim(() =>
                {
                    TestObjects[objectNumber].SendAndIncrementLocalPosition(GetRandomVector());
                });
            }
            
            randomTime = Random.Range(0, 2000); //Chose a new random time to wait before you move another object
            timer = 0; //Reset timer
        }

        /// <summary>Using ASL SendFloat callback, stop all movement in the scene to compare positions</summary>
        /// <param name="_id">The id of the object that sent these floats</param>
        /// <param name="f">The 4 floats that were sent</param>
        public void StopClients(string _id, float[] f)
        {
            if (f[0] == 0)
            {
                StopTest = true;
                Debug.Log("Stop");
            }
            if (f[1] == 1)
            {
                Debug.Log("1");
            }
            if (f[2] == 2)
            {
                Debug.Log("2");
            }
            if (f[3] == 3)
            {
                Debug.Log("3");
            }
        }

        /// <summary>Randomly generate a vector with values between -1 and 1</summary>
        /// <returns>A random vector between -1 and 1</returns>
        private Vector3 GetRandomVector()
        {
            return new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
        }
    }
}