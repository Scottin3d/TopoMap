//-----------------------------------------------------------------------
// <copyright file="ARCoreWorldOriginHelper.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>

//Copied and Edited by Greg Smith, UW Bothell 2019

//-----------------------------------------------------------------------

namespace ASL
{
    #if UNITY_ANDROID || UNITY_IOS
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;
    using Google.XR.ARCoreExtensions;
    #endif
    using UnityEngine;

    /// <summary>
    /// A helper script to set the apparent world origin of ARCore through applying an offset to the
    /// ARCoreDevice (and therefore also it's FirstPersonCamera child); this also provides
    /// mechanisms to handle resulting changes to ARCore plane positioning and raycasting.
    /// This class SHOULD be used when doing any ray casting - especially if cloud anchors and world origin is being used
    /// in your app.
    /// </summary>
    public class ARWorldOriginHelper : MonoBehaviour
    {
        private static ARWorldOriginHelper m_Instance;

        /// <summary>
        /// SIngleton for this class
        /// </summary>
        /// <returns>Singleton to be used anywhere</returns>
        public static ARWorldOriginHelper GetInstance()
        {
            if (m_Instance != null)
            {
                return m_Instance;
            }
            else
            {
                Debug.Log("ARWorldOriginHelper is null.");
                return m_Instance ?? null;
            }
        }

        /// <summary>
        /// Called when class first initialized
        /// </summary>
        private void Awake()
        {
            m_Instance = this;
        }

        /// <summary>
        /// Assigns each script manually
        /// </summary>
        private void Start()
        {
            ARCoreDeviceTransform = GameObject.Find("AR Session Origin").transform;
#if UNITY_ANDROID || UNITY_IOS
            m_ARSession = GameObject.Find("AR Session").GetComponent<ARSession>();
            m_RaycastManager = GameObject.Find("AR Session Origin").GetComponent<ARRaycastManager>();
            m_ARAnchorManager = GameObject.Find("AR Session Origin").GetComponent<ARAnchorManager>();
            m_ARPlaneManager = GameObject.Find("AR Session Origin").GetComponent<ARPlaneManager>();
#endif
        }

        /// <summary>
        /// The transform of the ARCore Device.
        /// </summary>
        public Transform ARCoreDeviceTransform;

        /// <summary>
        /// Indicates whether the Origin of the new World Coordinate System, i.e. the Cloud Anchor,
        /// was placed.
        /// </summary>
        private bool m_IsOriginPlaced = false;

        /// <summary>
        /// The Transform of the Anchor object representing the World Origin.
        /// </summary>
        private Transform m_AnchorTransform;

        #if UNITY_ANDROID || UNITY_IOS
        /// <summary>
        /// The AR Session - is a part of the ARHolder object
        /// </summary>
        public ARSession m_ARSession;

        /// <summary>
        /// The AR Raycast Manager - is a part of the ARHolder object
        /// </summary>
        public ARRaycastManager m_RaycastManager;

        /// <summary>
        /// The AR Anchor Manager - is a part of the ARHolder object
        /// </summary>
        public ARAnchorManager m_ARAnchorManager;

        /// <summary>
        /// The AR Plane Manager - is a part of the ARHolder object
        /// </summary>
        public ARPlaneManager m_ARPlaneManager;
#endif

        /// <summary>
        /// Sets the apparent world origin of ARCore through applying an offset to the ARCoreDevice
        /// (and therefore also it's FirstPersonCamera child), so that the Origin of Unity's World
        /// Coordinate System coincides with the Anchor. This function needs to be called once the
        /// Cloud Anchor is either hosted or resolved.
        /// </summary>
        /// <param name="anchorTransform">Transform of the Cloud Anchor.</param>
        public void SetWorldOrigin(Transform anchorTransform)
        {
            // Each client will store the anchorTransform, and will have to move the ARCoreDevice
            // (and therefore also it's FirstPersonCamera child) and update other tracked poses
            // (planes, anchors, etc.) so that they appear in the same position in the real world.
            if (m_IsOriginPlaced)
            {
                Debug.LogWarning("The World Origin can be set only once.");
                return;
            }

            m_IsOriginPlaced = true;

            m_AnchorTransform = anchorTransform;

            Pose worldPose = _WorldToAnchorPose(new Pose(ARCoreDeviceTransform.position,
                                                         ARCoreDeviceTransform.rotation));
            ARCoreDeviceTransform.SetPositionAndRotation(worldPose.position, worldPose.rotation);
        }


#if UNITY_ANDROID || UNITY_IOS
        /// <summary>
        /// Helper function to perform an AR Raycast for the user
        /// </summary>
        /// <param name="_touchPosition">Where the user touched</param>
        /// <param name="_trackableType">The type of trackable to look for</param>
        /// <returns>True if they hit a trackable that fits the specified trackable type</returns>
        public Pose? Raycast(Vector2 _touchPosition, TrackableType _trackableType)
        {
            List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
            m_RaycastManager.Raycast(_touchPosition, hitResults, _trackableType);

            bool foundHit = hitResults.Count > 0;

            if (foundHit)
            {
                Pose worldPose = _WorldToAnchorPose(hitResults[0].pose);
                return worldPose;
            }

            return null;
            
        }
#endif
        /// <summary>
        /// Helper function to perform an AR Raycast for the user - by default uses TrackableType.PlaneWithinPolygon which is the most common
        /// </summary>
        /// <param name="_touchPosition">Where the user touched</param>
        /// <returns>True if they hit a trackable that fits the specified trackable type</returns>
        public Pose? Raycast(Vector2 _touchPosition)
        {
#if UNITY_ANDROID || UNITY_IOS
            TrackableType trackableType = TrackableType.PlaneWithinPolygon;
            List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
            m_RaycastManager.Raycast(_touchPosition, hitResults, trackableType);
            bool foundHit = hitResults.Count > 0;
            //hitResult = new ARRaycastHit();
            if (foundHit)
            {
                return _WorldToAnchorPose(hitResults[0].pose);
            }
            return null;
#else
            Debug.LogError("Can only AR Raycast on mobile devices");
            return null;
#endif

        }

        /// <summary>
        /// Converts a pose from Unity world space to Anchor-relative space.
        /// </summary>
        /// <returns>A pose in Unity world space.</returns>
        /// <param name="pose">A pose in Anchor-relative space.</param>
        private Pose _WorldToAnchorPose(Pose pose)
        {
            if (!m_IsOriginPlaced)
            {
                return pose;
            }

            Matrix4x4 anchorTWorld = Matrix4x4.TRS(
                m_AnchorTransform.position, m_AnchorTransform.rotation, Vector3.one).inverse;

            Vector3 position = anchorTWorld.MultiplyPoint(pose.position);
            Quaternion rotation = pose.rotation * Quaternion.LookRotation(
                anchorTWorld.GetColumn(2), anchorTWorld.GetColumn(1));

            return new Pose(position, rotation);
        }

#if UNITY_ANDROID || UNITY_IOS
        /// <summary>
        /// Waits for the cloud anchor to be created so it can then send it's information to other users
        /// </summary>
        /// <param name="_cloudAnchor">The cloud anchor that is being created</param>
        /// <param name="_hitResult">The location where it was created</param>
        /// <param name="_anchorObjectPrefab">The object that will be attached to the cloud anchor</param>
        /// <param name="_myPostCreateCloudAnchorFunction">The function to call after creating the cloud anchor</param>
        /// <param name="_waitForAllUsersToResolve">Flag indicating to wait or not for all users to resolve before calling the post create function</param>
        /// <param name="_setWorldOrigin">Flag indicating to set this cloud anchor as the world origin or not</param>
        /// <returns>Nothing - just waits until the cloud anchor succeeds or fails</returns>
        public IEnumerator WaitForCloudAnchorToBeCreated(ARCloudAnchor _cloudAnchor, Pose _hitResult, ASLObject _anchorObjectPrefab = null, 
            ASLObject.PostCreateCloudAnchorFunction _myPostCreateCloudAnchorFunction = null,
            bool _waitForAllUsersToResolve = true, bool _setWorldOrigin = true)
        {
            while (_cloudAnchor.cloudAnchorState == CloudAnchorState.TaskInProgress)
            {
                yield return new WaitForEndOfFrame();
            }
            
            if (_cloudAnchor.cloudAnchorState == CloudAnchorState.Success)
            {
                //Successful:
                Debug.Log("Successfully Resolved cloud anchor: " + _cloudAnchor.cloudAnchorId + " for object: " + _anchorObjectPrefab?.m_Id);
                if (_anchorObjectPrefab == null)
                {
                    //Uncomment the line below to aid in visual debugging (helps display the cloud anchor)
                    //_anchorObjectPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube).AddComponent<ASLObject>(); //if null, then create empty game object               
                    _anchorObjectPrefab = new GameObject().AddComponent<ASLObject>();
                    _anchorObjectPrefab._LocallySetAnchorID(_cloudAnchor.cloudAnchorId); //Add ASLObject component to this anchor and set its anchor id variable

                    _anchorObjectPrefab._LocallySetID(Guid.NewGuid().ToString()); //Locally set the id of this object to a new id

                    //Add this anchor object to our ASL dictionary using the anchor id as its key. All users will do this once they resolve this cloud anchor to ensure they still in sync.
                    ASLHelper.m_ASLObjects.Add(_anchorObjectPrefab.m_Id, _anchorObjectPrefab.GetComponent<ASLObject>());
                    //_anchorObjectPrefab.GetComponent<Material>().color = Color.magenta;
                    _anchorObjectPrefab.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f); //Set scale to be 5 cm
                }
                else
                {
                    _anchorObjectPrefab.GetComponent<ASLObject>()._LocallySetAnchorID(_cloudAnchor.cloudAnchorId); //Set anchor id variable
                    _anchorObjectPrefab.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f); //Set scale to be 5 cm
                }
                //Send Resolve packet using _anchorObjectPrefab 
                _anchorObjectPrefab.GetComponent<ASLObject>().SendCloudAnchorToResolve(_setWorldOrigin, _waitForAllUsersToResolve);

                if (_waitForAllUsersToResolve)
                {
                    byte[] id = Encoding.ASCII.GetBytes(_anchorObjectPrefab.m_Id);
                    RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.ResolvedCloudAnchor, id);
                    GameLiftManager.GetInstance().m_Client.SendMessage(message);

                    _anchorObjectPrefab.StartWaitForAllUsersToResolveCloudAnchor(_cloudAnchor, _setWorldOrigin, _myPostCreateCloudAnchorFunction, _hitResult);
                }
                else //Don't wait for users to know about this cloud anchor
                {
                    _anchorObjectPrefab.GetComponent<ASLObject>()._LocallySetCloudAnchorResolved(true);
                    _anchorObjectPrefab.StartWaitForAllUsersToResolveCloudAnchor(_cloudAnchor, _setWorldOrigin, _myPostCreateCloudAnchorFunction, _hitResult);
                }
            }
            else
            {
                Debug.LogError("Failed to host Cloud Anchor " + _cloudAnchor.name + " " + _cloudAnchor.cloudAnchorState.ToString());
            }        
        }
#endif
    }
}
