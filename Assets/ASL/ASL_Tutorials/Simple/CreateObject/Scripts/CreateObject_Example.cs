using System.Collections.Generic;
using UnityEngine;

namespace SimpleDemos
{
    /// <summary> A simple demo showcasing different ways you can spawn an ASL object</summary>
    public class CreateObject_Example : MonoBehaviour
    {
        /// <summary>
        /// Different objects you can create in this tutorial. Not all variations are listed here.
        /// See documentation for all variations
        /// </summary>
        public enum ObjectToCreate
        {
            /// <summary>Spawn a primitive with the basic spawn parameters</summary>
            SimplePrimitive,
            /// <summary>Spawn a prefab with the basic spawn parameters</summary>
            SimplePrefab,
            /// <summary>Spawn a primitive with all the spawn parameters possible</summary>
            FullPrimitive,
            /// <summary>Spawn a prefab with all the spawn parameters possible</summary>
            FullPrefab
        }

        /// <summary>The object type that will be created</summary>
        public ObjectToCreate m_CreateObject;

        /// <summary> Toggle for creating an object</summary>
        public bool m_SpawnObject = false;

        /// <summary> Handle to the latest Full Prefab object created</summary>
        private static List<GameObject> m_HandleToFreshObjects = new List<GameObject>();

        /// <summary>  Holds the rotation of our object so it gets updated properly - see Transform example for better explanation</summary>
        private Quaternion m_RotationHolder;

        /// <summary>Initialize values</summary>
        private void Start()
        {
            m_RotationHolder = Quaternion.identity;
        }

        /// <summary> Scene Logic</summary>
        void Update()
        {
            if (m_SpawnObject)
            {
                if (m_CreateObject == ObjectToCreate.SimplePrimitive)
                {
                    //Creates a cube at a random location with a normal rotation orientation
                    ASL.ASLHelper.InstantiateASLObject(PrimitiveType.Cube,
                        new Vector3(Random.Range(-2f, 2f), Random.Range(0f, 2f), Random.Range(-2f, 2f)),
                        Quaternion.identity);
                }
                else if (m_CreateObject == ObjectToCreate.SimplePrefab)
                {
                    //Creates a prefab that is located in Resources/MyPrefabs at a random location with
                    //a normal rotation orientation. Note that this prefab must be the specified (above)
                    //folder location in order to be found and spawned.
                    ASL.ASLHelper.InstantiateASLObject("_ASL_ExamplePrefab",
                        new Vector3(Random.Range(-2f, 2f), Random.Range(0f, 2f), Random.Range(-2f, 2f)),
                        Quaternion.identity);
                }
                else if (m_CreateObject == ObjectToCreate.FullPrimitive)
                {
                    //Creates a sphere primitive at a random location with a normal rotation orientation, giving it no parent, adding the Rigidbody
                    //component using the Rigidbody's namespace (the first part) and assembly name (the part after the comma) and then setting the function
                    //to be called upon object creation, if a claim gets rejected, and when floats are sent with this object. Note that
                    //GetType().Namespace + "." + GetType().Name gets the namespace and class name of the function that this object should reference
                    //The values can be typed in manually if desired, and these functions can also exist in different classes (can exist in a class
                    //that the object is not being instantiated in) however, if done this way, then the namespace and class name must be manually entered
                    //As GetType grabs 'this' (where this code was written) namespace and class name. Also note that the assembly part for adding a component (the part after the comma)
                    //does not need to be included when the component you are adding is your own script - but the namespace is still needed
                    ASL.ASLHelper.InstantiateASLObject(PrimitiveType.Sphere,
                        new Vector3(Random.Range(-2f, 2f), Random.Range(0f, 2f), Random.Range(-2f, 2f)), Quaternion.identity, "", "UnityEngine.Rigidbody,UnityEngine",
                        WhatToDoWithMyGameObjectNowThatItIsCreated,
                        ClaimRecoveryFunction,
                        MyFloatsFunction);

                }
                else if (m_CreateObject == ObjectToCreate.FullPrefab)
                {
                    //Creates a prefab that is located in Resources/MyPrefabs at a random location with a normal rotation orientation, 
                    //giving it no parent,  adding the Rigidbody component using the Rigidbody's namespace (the first part) and assembly 
                    //name (the part after the comma) and then setting the function to be called upon object creation, if a claim gets rejected, 
                    //and when floats are sent with this object. Note that GetType().Namespace + "." + GetType().Name gets the namespace 
                    //and class name of the function that this object should reference. The values can be typed in manually if desired, 
                    //and these functions can also exist in different classes (can exist in a class that the object is not being instantiated in) 
                    //however, if done this way, then the namespace and class name must be manually entered As GetType grabs 'this' 
                    //(where this code was written) namespace and class name. Also note that the assembly part for adding a component (the part after the comma)
                    //does not need to be included when the component you are adding is your own script - but the namespace is still needed
                    ASL.ASLHelper.InstantiateASLObject("_ASL_ExamplePrefab",
                        new Vector3(Random.Range(-2f, 2f), Random.Range(0f, 2f), Random.Range(-2f, 2f)), Quaternion.identity, "", "UnityEngine.Rigidbody,UnityEngine",
                        WhatToDoWithMyOtherGameObjectNowThatItIsCreated,
                        ClaimRecoveryFunction,
                        MyFloatsFunction);
                }

                m_SpawnObject = false; //Reset to false to prevent multiple unwanted spawns
            }

            //For each object we created with full prefab - spin.
            //This is also a good way to stress test your system - to see how many objects you can have concurrently sending commands
            //A better way to continually rotate an object would be to set it in motion and then let local handle the actual rotation, with
            //ASL only updating it when it changes. This example does not do that - it sends the rotation continually over the network for all
            //objects.
            foreach (var _object in m_HandleToFreshObjects)
            {
                _object.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    Quaternion.AngleAxis(45f, Vector3.up);
                    m_RotationHolder = Quaternion.AngleAxis(1f, Vector3.up);
                    _object.GetComponent<ASL.ASLObject>().SendAndIncrementLocalRotation(m_RotationHolder);
                });
            }
            
        }

        /// <summary>
        /// This function is how you get a handle to the object you just created
        /// </summary>
        /// <param name="_myGameObject">A handle to the gameobject that was just created</param>
        public static void WhatToDoWithMyGameObjectNowThatItIsCreated(GameObject _myGameObject)
        {
            //Change the color
            _myGameObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                _myGameObject.GetComponent<ASL.ASLObject>().SendAndSetObjectColor(new Color(0.7830189f, 0.3792925f, 03324135f, 1), new Color(0, 0, 0));
            });

            //Send floats to show MyFloatsFunction got set properly
            _myGameObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                float[] myFloats = { 1, 2, 3, 4 };
                _myGameObject.GetComponent<ASL.ASLObject>().SendFloatArray(myFloats);
            });
        }

        /// <summary>
        /// A function that is called right after an ASL game object is created if that object was passed in the class name and function name of this function.
        /// This is called immediately upon creation, allowing the user a way to access their newly created object after the server has spawned it
        /// </summary>
        /// <param name="_gameObject">The gameobject that was created</param>
        public static void WhatToDoWithMyOtherGameObjectNowThatItIsCreated(GameObject _gameObject)
        {
            //An example of how we can get a handle to our object that we just created but want to use later
            m_HandleToFreshObjects.Add(_gameObject);
        }

        /// <summary>
        /// A function that is called when an ASL object's claim is rejected. This function can be set to be called upon object creation.
        /// </summary>
        /// <param name="_id">The id of the object who's claim was rejected</param>
        /// <param name="_cancelledCallbacks">The amount of claim callbacks that were cancelled</param>
        public static void ClaimRecoveryFunction(string _id, int _cancelledCallbacks)
        {
            Debug.Log("Aw man. My claim got rejected for my object with id: " + _id + " it had " + _cancelledCallbacks + " claim callbacks to execute.");
            //If I can't have this object, no one can. (An example of how to get the object we were unable to claim based on its ID and then perform an action). Obviously,
            //deleting the object wouldn't be very nice to whoever prevented your claim
            if (ASL.ASLHelper.m_ASLObjects.TryGetValue(_id, out ASL.ASLObject _myObject))
            {
                _myObject.GetComponent<ASL.ASLObject>().DeleteObject();
            }

        }

        /// <summary>
        /// A function that is called whenever an ASL object calls <see cref="ASL.ASLObject.SendFloatArray(float[])"/>.
        /// This function can be assigned to an ASL object upon creation.
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_myFloats"></param>
        public static void MyFloatsFunction(string _id, float[] _myFloats)
        {
            Debug.Log("The floats that were sent are:\n");
            for (int i = 0; i < 4; i++)
            {
                Debug.Log(_myFloats[i] + "\n");
            }
        }

    }
}