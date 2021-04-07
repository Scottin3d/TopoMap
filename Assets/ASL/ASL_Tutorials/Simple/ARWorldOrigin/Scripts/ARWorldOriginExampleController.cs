using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SimpleDemos
{
    /// <summary>
    /// Demonstrates how you can use the SetWorldOrigin parameter when creating a cloud anchor to synchronize AR origins.
    /// </summary>
    public class ARWorldOriginExampleController : MonoBehaviour
    {
        /// <summary>Singleton for this class so that functions can be called after objects and cloud anchors are created using the same parameter they were created with</summary>
        private static ARWorldOriginExampleController m_Controller;

        /// <summary>Determines which object to spawn: Cloud Anchor or normal sphere</summary>
        public Dropdown m_ObjectToSpawnDropDown;

        /// <summary>Text that displays scene information to the user</summary>
        public Text m_DisplayInformation;

        /// <summary>GameObject that represents the world origin cloud anchor (will be placed on where the world origin is) </summary>
        public GameObject m_WorldOriginCloudAnchorObject;

        /// <summary> GameObject that represents a cloud anchor object (will be placed where the cloud anchor is)</summary>
        public GameObject m_NormalCloudAnchorObject;

        /// <summary>Flag indicating if the world origin has been initialized or not - it should only be set once</summary>
        private bool m_WorldOriginInitilized = false;

        /// <summary>Flag indicating if we are currently spawning a cloud anchor - 1 user should only spawn 1 cloud anchor at a time</summary>
        private bool m_CurrentlySpawningCloudAnchor = false;

        /// <summary> Gets the hit position where the user touched the screen to help record where the object is verses where the user tapped</summary>
        private Pose? m_LastValidPose;

        /// <summary>Called before start - sets up the singleton object for this class</summary>
        private void Awake()
        {
            m_Controller = this;
        }

        /// <summary>Called on start up - sets the initial text for the user</summary>
        void Start()
        {
            m_DisplayInformation.text = "The first location you touch will spawn the World Origin Cloud Anchor. " +
                "Only 1 player can spawn this cloud anchor and it should always be the first cloud anchor created if you plan on utilizing cloud anchors.";
        }

        /// <summary> The logic of this example - listens for screen touches and spawns whichever object is currently active on the drop down menu</summary>
        void Update()
        {
            Pose? touchPose = GetTouch();
            if (touchPose == null) //If we didn't hit anything - return
            {
                return;
            }
            //If we haven't set the world origin yet and we are player 1. By checking if we are playing 1 it 
            //helps ensure that two people don't attempt to set the World Origin at the same time
            if (!m_WorldOriginInitilized && ASL.GameLiftManager.GetInstance().AmLowestPeer())
            {
                m_WorldOriginInitilized = true;
                m_DisplayInformation.text = "Creating World Origin Visualization object now.";

                m_LastValidPose = touchPose;
                m_CurrentlySpawningCloudAnchor = true;
                //It doesn't matter what we set the position to be to when creating this object because it will be reset to zero before it gets parented to its cloud anchor. 
                //Setting it to 100 initially just helps prevent confusion as we shouldn't see the world origin cloud anchor object until the world origin cloud anchor is set
                ASL.ASLHelper.InstantiateASLObject("SimpleDemoPrefabs/WorldOriginCloudAnchorObject", new Vector3(100, 100, 100), Quaternion.identity, string.Empty, string.Empty, SpawnWorldOrigin);
          
            }
            if (!m_CurrentlySpawningCloudAnchor)
            {
                //If spawn cloud anchor is selected
                if (m_ObjectToSpawnDropDown.value == 0)
                {
                    m_DisplayInformation.text = "Creating a Normal Cloud Anchor Visualization object now.";
                    m_LastValidPose = touchPose;
                    m_CurrentlySpawningCloudAnchor = true;
                    //It doesn't matter what we set the position to be to when creating this object because it will be reset to zero before it gets parented to its cloud anchor. 
                    //Setting it to 100 initially just helps prevent confusion as we shouldn't see the cloud anchor object until the cloud anchor is set
                    ASL.ASLHelper.InstantiateASLObject("SimpleDemoPrefabs/NormalCloudAnchorObject", new Vector3(100, 100, 100), Quaternion.identity, string.Empty, string.Empty, SpawnNormalCloudAnchor);
                }
                else if (m_ObjectToSpawnDropDown.value == 1)
                {
                    //If spawn cube is selected
                    m_DisplayInformation.text = "Creating a sphere at: " + touchPose?.position.ToString();
                    ASL.ASLHelper.InstantiateASLObject(PrimitiveType.Sphere, (Vector3)touchPose?.position, Quaternion.identity, string.Empty, string.Empty, SpawnSphere);
                }
            }
        }

        /// <summary>
        /// Gets the location of the user's touch
        /// </summary>
        /// <returns>Returns null if UI or nothing touched</returns>
        private Pose? GetTouch()
        {
            Touch touch;
            // If the player has not touched the screen then the update is complete.
            if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            {
                return null;
            }

            // Ignore the touch if it's pointing on UI objects.
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return null;
            }

            return ASL.ARWorldOriginHelper.GetInstance().Raycast(Input.GetTouch(0).position);
        }

        /// <summary>
        /// Spawns the world origin cloud anchor after the world origin object visualizer has been created (blue cube)
        /// </summary>
        /// <param name="_worldOriginVisualizationObject">The game object that represents the world origin</param>
        public static void SpawnWorldOrigin(GameObject _worldOriginVisualizationObject)
        {
            m_Controller.m_DisplayInformation.text = "Creating World Origin Cloud Anchor now.";
            _worldOriginVisualizationObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                //_worldOriginVisualizationObject will be parented to the cloud anchor which is the world origin, thus showing where the world origin is
                ASL.ASLHelper.CreateARCoreCloudAnchor(m_Controller.m_LastValidPose, _worldOriginVisualizationObject.GetComponent<ASL.ASLObject>(), WorldOriginTextUpdate, true, true);
            });
        }

        /// <summary>
        /// The cloud anchor callback that informs the user the world origin is finished being created
        /// </summary>
        /// <param name="_worldOriginVisualizationObject">The game object that represents the world origin</param>
        /// <param name="_spawnLocation">The pose of the world origin</param>
        public static void WorldOriginTextUpdate(GameObject _worldOriginVisualizationObject, Pose _spawnLocation)
        {
            m_Controller.m_DisplayInformation.text = "Finished creating World Origin Cloud Anchor. You are now free to create more cloud anchors or objects. " +
                "The position you touched on the screen was: " + _spawnLocation.position + " and the anchor's world position is: " + _worldOriginVisualizationObject.transform.position
                + " with a local position of: " + _worldOriginVisualizationObject.transform.localPosition; ;
            m_Controller.m_CurrentlySpawningCloudAnchor = false;
        }

        /// <summary>
        /// Spawns a normal cloud anchor now that the cloud anchor visualization object has been created (red cylinder)
        /// </summary>
        /// <param name="_normalCloudAnchorVisualizationObject">The game object that will represent a normal cloud anchor</param>
        public static void SpawnNormalCloudAnchor(GameObject _normalCloudAnchorVisualizationObject)
        {
            m_Controller.m_DisplayInformation.text = "Creating a Normal Cloud Anchor now.";
            _normalCloudAnchorVisualizationObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                //_worldOriginVisualizationObject will be parented to the cloud anchor which is the world origin, thus showing where the world origin is
                ASL.ASLHelper.CreateARCoreCloudAnchor(m_Controller.m_LastValidPose, _normalCloudAnchorVisualizationObject.GetComponent<ASL.ASLObject>(), NormalCloudAnchorTextUpdate, true, false);
            });
        }

        /// <summary>
        /// THe cloud anchor callback that is used to inform the user that a normal cloud anchor was created
        /// </summary>
        /// <param name="_normalCloudAnchorVisualizationObject">The gameobject that is tied to this cloud anchor (the cloud anchor visualization object)</param>
        /// <param name="_spawnLocation">The location of the cloud anchor</param>
        public static void NormalCloudAnchorTextUpdate(GameObject _normalCloudAnchorVisualizationObject, Pose _spawnLocation)
        {
            m_Controller.m_DisplayInformation.text = "Finished creating a Normal Cloud Anchor. You are now free to create more cloud anchors or objects. " +
                "The position you touched on the screen was: " + _spawnLocation.position + " and the anchor's world position is: " + _normalCloudAnchorVisualizationObject.transform.position
                + " with a local position of: " + _normalCloudAnchorVisualizationObject.transform.localPosition;
            m_Controller.m_CurrentlySpawningCloudAnchor = false;
        }

        /// <summary>
        /// The create object call back for the normal spheres - used to inform the user the sphere was created and to scale it down so it matches the size of the other objects
        /// </summary>
        /// <param name="_sphere">The game object that was just created</param>
        public static void SpawnSphere(GameObject _sphere)
        {
            m_Controller.m_DisplayInformation.text = "Finished creating a normal sphere with a local position of: " + _sphere.transform.localPosition 
                + " and a world position of: " + _sphere.transform.position;

            //Scale the sphere down to 5cm (the same size as the other objects)
            _sphere.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                _sphere.GetComponent<ASL.ASLObject>().SendAndSetLocalScale(new Vector3(.05f, .05f, .05f)); 
            });

        }

    }
}