using Aws.GameLift.Realtime.Command;
using Aws.GameLift.Realtime.Event;
#if UNITY_ANDROID || UNITY_IOS
using Google.XR.ARCoreExtensions;
using UnityEngine.XR.ARFoundation;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ASL
{
    public partial class GameLiftManager
    {
        /// <summary>
        /// An internal class used to handle data packets incoming from the GameLift Realtime Server. It should be noted that the order in which a packet is created matters.
        /// The order should be as follows so that it can be decoded here properly. Amount of data pieces (e.g., 2), the lengths in bytes of these data pieces (e.g., 4,4), 
        /// and the data themselves converted to byte format (e.g., 4,4). If you follow how ASLObject functions already create these data packets then you will be following 
        /// the correct formatting. The important thing to remember is that however you encode a packet, you must remember to decode in the same manner.
        /// </summary>
        private class GameController
        {
            /// <summary>Used to count how many objects have been assigned an ID yet</summary>
            private int m_ObjectIDAssignedCount;
            
            /// <summary>The pause canvas that is shown while waiting for objects to be synced</summary>
            private GameObject m_PauseCanvas;
            
            /// <summary>The pause text that is shown while waiting for objects to be synced</summary>
            private GameObject m_PauseText;
            
            /// <summary>
            /// Dictionary containing the received Texture2Ds that are being rebuilt as new packets come in.
            /// </summary>
            private readonly Dictionary<string, byte[]> ReceivedTexture2Ds = new Dictionary<string, byte[]>();

            /// <summary>
            /// Start function that states any scene loaded will call the SyncronizeId function, ensuring all ASL objects are synced upon scene loads
            /// </summary>
            public void Start()
            {
                SceneManager.sceneLoaded += SyncronizeID;
            }

            /// <summary>
            /// Find all ASL Objects in the scene and have the relay server create a unique ID for them as well as add this object to our ASLObject dictionary
            /// This function is triggered when this script is first loaded. This function will keep the timeScale at 0 until all objects have been ID.
            /// All objects are ID in the SetObjectID(RTPacket _packet) function.
            /// </summary>
            private void SyncronizeID(Scene _scene, LoadSceneMode _mode)
            {
                Debug.Log("Sending Synchronizing Ids...: " + _scene);
                m_ObjectIDAssignedCount = 0;
                ASLObject[] m_StartingASLObjects = FindObjectsOfType(typeof(ASLObject)) as ASLObject[];

                //If there are ASL objects to initialize - pause until done
                if (m_StartingASLObjects.Length > 0)
                {
                    SetupSynchronizingASLObjectScreen();
                    Time.timeScale = 0; //"Pause" the game until all object's have their id's set

                    //Sort by name + sqrt(transform components + localPosition * Dot(rotation, scale))
                    m_StartingASLObjects = m_StartingASLObjects.OrderBy(aslObj => aslObj.name +
                        ((aslObj.transform.position + new Vector3(aslObj.transform.position.x, aslObj.transform.rotation.y, aslObj.transform.rotation.z) +
                         aslObj.transform.localScale) + (aslObj.transform.position + Vector3.Dot(new Vector3(aslObj.transform.rotation.x, aslObj.transform.rotation.y, aslObj.transform.rotation.z),
                         aslObj.transform.localScale) * Vector3.one)).sqrMagnitude)
                        .ToArray();
                }
                

                int startingObjectCount = 0;
                foreach (ASLObject _aslObject in m_StartingASLObjects)
                {
                    if (_aslObject.m_Id == string.Empty || _aslObject.m_Id == null) //If object does not have an ID
                    {
                        m_ObjectIDAssignedCount++;
                        RTMessage message = GetInstance().CreateRTMessage(OpCode.ServerSetId, Encoding.ASCII.GetBytes(""));
                        GetInstance().m_Client.SendMessage(message);
                        ASLHelper.m_ASLObjects.Add(startingObjectCount.ToString(), _aslObject);
                        _aslObject._LocallySetID(startingObjectCount.ToString());
                        startingObjectCount++;
                    }
                }
            }

            /// <summary>
            /// Used to setup the canvas screen that informs users that ASL objects are syncing
            /// </summary>
            private void SetupSynchronizingASLObjectScreen()
            {
                if (m_PauseCanvas == null)
                {
                    m_PauseCanvas = new GameObject("FinalLoadingCanvas");
                    m_PauseCanvas.transform.SetParent(GetInstance().transform);
                    m_PauseCanvas.AddComponent<Canvas>();
                    m_PauseCanvas.AddComponent<UnityEngine.UI.CanvasScaler>().uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    m_PauseCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                    m_PauseText = new GameObject("FinalizingLoadingText");
                    m_PauseText.transform.SetParent(m_PauseCanvas.transform);
                    m_PauseText.AddComponent<UnityEngine.UI.Text>().text = "Finalizing loading... Game will begin automatically once all ASL Objects are synced.";
                    UnityEngine.UI.Text pauseText = m_PauseText.GetComponent<UnityEngine.UI.Text>();
                    pauseText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                    pauseText.fontSize = 20;
                    pauseText.color = Color.black;
                    m_PauseText.transform.localPosition = new Vector3(0, 150, 0);
                    m_PauseText.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 100);
                    m_PauseCanvas.SetActive(true);
                }
                else
                {
                    m_PauseCanvas.SetActive(true);
                }
            }

            /// <summary>
            /// Looks for and assigns any ASLObjects that do not have a unique ID yet. This ID is given through the relay server. 
            /// This function is triggered by a packet received from the relay server. This function will keep the time scale at 0 until all ASL objects have a proper ID
            /// </summary>
            /// <param name="_packet">The packet containing the unique ID of an ASL Object</param>
            public void SetObjectID(DataReceivedEventArgs _packet)
            {
                string id = Encoding.Default.GetString(_packet.Data);
                //Cycle through all items in the dictionary looking for the items with invalid keys 
                //For objects that start in the scene, their keys are originally set to be invalid so we set them properly through the RT Server
                //Ensuring all clients have the same key for each object
                foreach (KeyValuePair<string, ASLObject> _object in ASLHelper.m_ASLObjects)
                {
                    //Since GUIDs aren't numbers, if we find a number, then we know it's a fake key value and it should be updated to match all other clients
                    if (int.TryParse(_object.Key, out int result))
                    {
                        ASLHelper.m_ASLObjects.Add(id, _object.Value);
                        ASLHelper.m_ASLObjects.Remove(_object.Key);
                        InitializeStartObject(id, _object.Value);
                        break;
                    }

                }

                m_ObjectIDAssignedCount--;
                if (m_ObjectIDAssignedCount <= 0)
                {
                    m_PauseCanvas.SetActive(false);
                    Time.timeScale = 1; //Restart time
                }
            }

            /// <summary>
            /// Upon game start, any ASL Objects in the scene do not have synchronized IDs. This function changes their ID to be synced with other clients
            /// </summary>
            /// <param name="_id">The new id for _object</param>
            /// <param name="_object">The ASL object that will be getting a new id</param>
            /// <returns></returns>
            private bool InitializeStartObject(string _id, ASLObject _object)
            {
                if (ASLHelper.m_ASLObjects.ContainsKey(_id))
                {
                    _object._LocallySetID(_id);
                    _object._LocallyRemoveReleaseCallback();
                    return true;
                }
                else
                {
                    Debug.LogError("Attempted to set the id of an object to a value that does not exist in our dictionary. If this was intended, then" +
                        " add this object to the m_ASLObjects dictionary first.");
                    return false;
                }
            }

            /// <summary>
            /// Finds and claims a specified object and updates everybody's permission for that object. This function is triggered by a packet received from the relay server.
            /// </summary>
            /// <param name="_packet">The packet containing the id of the object to claim</param>
            public void SetObjectClaim(DataReceivedEventArgs _packet)
            {
                string data = Encoding.Default.GetString(_packet.Data);
                string[] dataParts = data.Split(':');

                if (ASLHelper.m_ASLObjects.TryGetValue(dataParts[0] ?? string.Empty, out ASLObject myObject))
                {
                    if (int.TryParse(dataParts[1], out int sentPeerId))
                    {
                        if (sentPeerId == GetInstance().m_PeerId) //If this is the player who sent the claim
                        {
                            myObject._LocallySetClaim(true);
                            myObject.m_ClaimCallback?.Invoke();
                            myObject.m_OutstandingClaimCallbackCount = 0;
                            myObject._LocallyRemoveClaimCallbacks();
                        }
                        else //This is not the player who sent the claim - remove any claims this player may have (shouldn't be any)
                        {
                            myObject._LocallySetClaim(false);
                            myObject._LocallyRemoveClaimCallbacks();
                        }
                    }
                }
            }

            /// <summary>
            /// Releases an object so another user can claim it. This function will also call this object's release function if it exists.
            /// This function is triggered by a packet received from the relay server.
            /// </summary>
            /// <param name="_packet">The packet containing the id of the object that another player wants to claim</param>
            public void ReleaseClaimedObject(DataReceivedEventArgs _packet)
            {
                string data = Encoding.Default.GetString(_packet.Data);
                string[] dataParts = data.Split(':');

                if (ASLHelper.m_ASLObjects.TryGetValue(dataParts[0] ?? string.Empty, out ASLObject myObject))
                {
                    if (int.TryParse(dataParts[1], out int sentPeerId))
                    {
                        if (sentPeerId == GetInstance().m_PeerId) //If this is the current owner
                        {
                            //Send a packet to new owner informing them that the previous owner (this client) no longer owns the object
                            myObject.m_ReleaseFunction?.Invoke(myObject.gameObject); //If user wants to do something before object is released - let them do so
                            myObject._LocallyRemoveReleaseCallback();
                            myObject._LocallySetClaim(false);
                            myObject._LocallyRemoveClaimCallbacks();


                            string newData = dataParts[0] + ":" + dataParts[2];
                            RTMessage message = GetInstance().CreateRTMessage(OpCode.ClaimFromPlayer, Encoding.ASCII.GetBytes(newData));
                            GetInstance().m_Client.SendMessage(message);

                        }
                    }
                }
            }

            /// <summary>
            /// Get the claim to an object that was previously owned by another player. This function is triggered by a packet received from the relay server.
            /// </summary>
            /// <param name="_packet">The packet sent by the previous owner of this object, 
            /// it contains the id of the object to be claimed by the receiver of this packet.</param>
            public void ObjectClaimReceived(DataReceivedEventArgs _packet)
            {
                string data = Encoding.Default.GetString(_packet.Data);
                string[] dataParts = data.Split(':');
                if (ASLHelper.m_ASLObjects.TryGetValue(dataParts[0] ?? string.Empty, out ASLObject myObject))
                {
                    myObject._LocallySetClaim(true);
                    //Call the function the user passed into original claim as they now have "complete control" over the object
                    myObject.m_ClaimCallback?.Invoke();
                    myObject.m_OutstandingClaimCallbackCount = 0;
                    myObject._LocallyRemoveClaimCallbacks();
                }
            }

            /// <summary>
            /// Reject a player's claim request on an ASL Object. This function is triggered by a packet received from the relay server.
            /// </summary>
            /// <param name="_packet">The packet containing the id of the object that a player wanted to claim</param>
            public void RejectClaim(DataReceivedEventArgs _packet)
            {
                string data = Encoding.Default.GetString(_packet.Data);
                Debug.LogWarning("Claim Rejected id: " + data);
                if (ASLHelper.m_ASLObjects.TryGetValue(data ?? string.Empty, out ASLObject myObject))
                {
                    Debug.LogWarning("Claim Rejected");
                    //Remove all callbacks created as our claim was rejected
                    if (myObject.m_ClaimCallback?.GetInvocationList().Length == null)
                    {
                        myObject.m_ClaimCancelledRecoveryCallback?.Invoke(myObject.m_Id, 0);
                    }
                    else
                    {
                        myObject.m_ClaimCancelledRecoveryCallback?.Invoke(myObject.m_Id, myObject.m_ClaimCallback.GetInvocationList().Length);
                    }
                    myObject._LocallyRemoveClaimCallbacks();
                }
            }

            /// <summary>
            /// Sets the object specified by the id contained in _packet to the color specified in _packet. This function is triggered by a packet received from the relay server.
            /// </summary>
            /// <param name="_packet">The packet containing the id of the object to change the color of, the color for the owner of the object,
            /// and the color for those who don't own the object</param>
            public void SetObjectColor(DataReceivedEventArgs _packet)
            {
                (int[] startLocation, int[] dataLength) = DataLengthsAndStartLocations(_packet.Data);
                
                string id = ConvertByteArrayIntoString(_packet.Data, startLocation[0], dataLength[0]);
                int sender = ConvertByteArrayIntoInt(_packet.Data, startLocation[3], dataLength[3]);

                if (ASLHelper.m_ASLObjects.TryGetValue(id ?? string.Empty, out ASLObject myObject))
                {
                    if (GetInstance().m_PeerId == sender)
                    {
                        myObject.GetComponent<Renderer>().material.color = ConvertByteArrayIntoVector(_packet.Data, startLocation[1], dataLength[1]);
                    }
                    else //Everyone else
                    {
                        myObject.GetComponent<Renderer>().material.color = ConvertByteArrayIntoVector(_packet.Data, startLocation[2], dataLength[2]);
                    }
                }
                
            }

            /// <summary>
            /// Destroys an ASL Object based upon its ID. This function is triggered by a packet received from the relay server.
            /// </summary>
            /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to delete</param>
            public void DeleteObject(DataReceivedEventArgs _packet)
            {                
                string id = Encoding.Default.GetString(_packet.Data);
                if (ASLHelper.m_ASLObjects.TryGetValue(id ?? string.Empty, out ASLObject myObject))
                {
                    ASLHelper.m_ASLObjects.Remove(id);
                    Destroy(myObject.gameObject);
                }
            }

            /// <summary>
            /// Updates the local transform of an ASL Object based upon its ID. This function is triggered by a packet received from the relay server.
            /// </summary>
            /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
            /// and the Vector3 of the object's new position</param>
            public void SetLocalPosition(DataReceivedEventArgs _packet)
            {
                (int[] startLocation, int[] dataLength) = DataLengthsAndStartLocations(_packet.Data);
                string id = ConvertByteArrayIntoString(_packet.Data, startLocation[0], dataLength[0]);
                if (ASLHelper.m_ASLObjects.TryGetValue(id ?? string.Empty, out ASLObject myObject))
                {
                    myObject.transform.localPosition = ConvertByteArrayIntoVector(_packet.Data, startLocation[1], dataLength[1]);
                }
            }

            /// <summary>
            /// Updates the local transform of an ASL Object based upon its ID by taking the value passed and adding it to the current localPosition value.
            /// This function is triggered by a packet received from the relay server.
            /// </summary>
            /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
            /// and the Vector3 of the object's new position</param>
            public void IncrementLocalPosition(DataReceivedEventArgs _packet)
            {
                (int[] startLocation, int[] dataLength) = DataLengthsAndStartLocations(_packet.Data);
                string id = ConvertByteArrayIntoString(_packet.Data, startLocation[0], dataLength[0]);
                if (ASLHelper.m_ASLObjects.TryGetValue(id ?? string.Empty, out ASLObject myObject))
                {
                    myObject.transform.localPosition += (Vector3)ConvertByteArrayIntoVector(_packet.Data, startLocation[1], dataLength[1]);
                }
            }

            /// <summary>
            /// Updates the local rotation of an ASL Object based upon its ID. This function is triggered by a packet received from the relay server.
            /// </summary>
            /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
            /// and the Vector4 of the object's new rotation</param>
            public void SetLocalRotation(DataReceivedEventArgs _packet)
            {
                (int[] startLocation, int[] dataLength) = DataLengthsAndStartLocations(_packet.Data);
                string id = ConvertByteArrayIntoString(_packet.Data, startLocation[0], dataLength[0]);
                if (ASLHelper.m_ASLObjects.TryGetValue(id ?? string.Empty, out ASLObject myObject))
                {
                    Vector4 quaternion = ConvertByteArrayIntoVector(_packet.Data, startLocation[1], dataLength[1]);
                    myObject.transform.localRotation = new Quaternion(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
                }
            }

            /// <summary>
            /// Updates the local rotation of an ASL Object based upon its ID. This function is triggered by a packet received from the relay server.
            /// </summary>
            /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
            /// and the Vector4 of the object's new rotation</param>
            public void IncrementLocalRotation(DataReceivedEventArgs _packet)
            {
                (int[] startLocation, int[] dataLength) = DataLengthsAndStartLocations(_packet.Data);
                string id = ConvertByteArrayIntoString(_packet.Data, startLocation[0], dataLength[0]);
                if (ASLHelper.m_ASLObjects.TryGetValue(id ?? string.Empty, out ASLObject myObject))
                {
                    Vector4 quaternion = ConvertByteArrayIntoVector(_packet.Data, startLocation[1], dataLength[1]);
                    myObject.transform.localRotation *= new Quaternion(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
                }
            }

            /// <summary>
            /// Updates the local scale of an ASL Object based upon its ID. This function is triggered by a packet received from the relay server.
            /// </summary>
            /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
            /// and the Vector3 of the object's new scale</param>
            public void SetLocalScale(DataReceivedEventArgs _packet)
            {
                (int[] startLocation, int[] dataLength) = DataLengthsAndStartLocations(_packet.Data);
                string id = ConvertByteArrayIntoString(_packet.Data, startLocation[0], dataLength[0]);
                if (ASLHelper.m_ASLObjects.TryGetValue(id ?? string.Empty, out ASLObject myObject))
                {
                    myObject.transform.localScale = ConvertByteArrayIntoVector(_packet.Data, startLocation[1], dataLength[1]);
                }
            }

            /// <summary>
            /// Updates the local scale of an ASL Object based upon its ID by taking the value passed in and adding it to the current localScale value. 
            /// This function is triggered by a packet received from the relay server.
            /// </summary>
            /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
            /// and the Vector3 of the object's new scale</param>
            public void IncrementLocalScale(DataReceivedEventArgs _packet)
            {
                (int[] startLocation, int[] dataLength) = DataLengthsAndStartLocations(_packet.Data);
                string id = ConvertByteArrayIntoString(_packet.Data, startLocation[0], dataLength[0]);
                if (ASLHelper.m_ASLObjects.TryGetValue(id ?? string.Empty, out ASLObject myObject))
                {
                    myObject.transform.localScale += (Vector3)ConvertByteArrayIntoVector(_packet.Data, startLocation[1], dataLength[1]);
                }
            }

            /// <summary>
            /// Updates the world position of an ASL Object based upon its ID. This function is triggered by a packet received from the relay server.
            /// </summary>
            /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
            /// and the Vector3 of the object's new position</param>
            public void SetWorldPosition(DataReceivedEventArgs _packet)
            {
                (int[] startLocation, int[] dataLength) = DataLengthsAndStartLocations(_packet.Data);
                string id = ConvertByteArrayIntoString(_packet.Data, startLocation[0], dataLength[0]);
                if (ASLHelper.m_ASLObjects.TryGetValue(id ?? string.Empty, out ASLObject myObject))
                {
                    myObject.transform.position = ConvertByteArrayIntoVector(_packet.Data, startLocation[1], dataLength[1]);
                }
            }

            /// <summary>
            /// Updates the world transform of an ASL Object based upon its ID by taking the value passed and adding it to the current localPosition value.
            /// This function is triggered by a packet received from the relay server.
            /// </summary>
            /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
            /// and the Vector3 of the object's new position</param>
            public void IncrementWorldPosition(DataReceivedEventArgs _packet)
            {
                (int[] startLocation, int[] dataLength) = DataLengthsAndStartLocations(_packet.Data);
                string id = ConvertByteArrayIntoString(_packet.Data, startLocation[0], dataLength[0]);
                if (ASLHelper.m_ASLObjects.TryGetValue(id ?? string.Empty, out ASLObject myObject))
                {
                    myObject.transform.position += (Vector3)ConvertByteArrayIntoVector(_packet.Data, startLocation[1], dataLength[1]);
                }
            }

            /// <summary>
            /// Updates the world rotation of an ASL Object based upon its ID. This function is triggered by a packet received from the relay server.
            /// </summary>
            /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
            /// and the Vector4 of the object's new rotation</param>
            public void SetWorldRotation(DataReceivedEventArgs _packet)
            {
                (int[] startLocation, int[] dataLength) = DataLengthsAndStartLocations(_packet.Data);
                string id = ConvertByteArrayIntoString(_packet.Data, startLocation[0], dataLength[0]);
                if (ASLHelper.m_ASLObjects.TryGetValue(id ?? string.Empty, out ASLObject myObject))
                {
                    Vector4 quaternion = ConvertByteArrayIntoVector(_packet.Data, startLocation[1], dataLength[1]);
                    myObject.transform.rotation = new Quaternion(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
                }
            }

            /// <summary>
            /// Updates the world rotation of an ASL Object based upon its ID. This function is triggered by a packet received from the relay server.
            /// </summary>
            /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
            /// and the Vector4 of the object's new rotation</param>
            public void IncrementWorldRotation(DataReceivedEventArgs _packet)
            {
                (int[] startLocation, int[] dataLength) = DataLengthsAndStartLocations(_packet.Data);
                string id = ConvertByteArrayIntoString(_packet.Data, startLocation[0], dataLength[0]);
                if (ASLHelper.m_ASLObjects.TryGetValue(id ?? string.Empty, out ASLObject myObject))
                {
                    Vector4 quaternion = ConvertByteArrayIntoVector(_packet.Data, startLocation[1], dataLength[1]);
                    myObject.transform.rotation *= new Quaternion(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
                }
            }

            /// <summary>
            /// Updates the world scale of an ASL Object based upon its ID by setting its parent to null and then 
            /// reassigning its parent after setting the scale you want it to have. This function is triggered by a 
            /// packet received from the relay server.
            /// </summary>
            /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
            /// and the Vector3 of the object's new scale</param>
            public void SetWorldScale(DataReceivedEventArgs _packet)
            {
                (int[] startLocation, int[] dataLength) = DataLengthsAndStartLocations(_packet.Data);
                string id = ConvertByteArrayIntoString(_packet.Data, startLocation[0], dataLength[0]);
                if (ASLHelper.m_ASLObjects.TryGetValue(id ?? string.Empty, out ASLObject myObject))
                {
                    var parent = myObject.transform.parent;
                    myObject.transform.parent = null;
                    myObject.transform.localScale = ConvertByteArrayIntoVector(_packet.Data, startLocation[1], dataLength[1]);
                    myObject.transform.parent = parent;
                }
            }

            /// <summary>
            /// Updates the world scale of an ASL Object based upon its ID by taking the value passed in and adding it to the current scale value
            /// by setting its parent to null and then reassigning its parent after setting the scale you want it to have. This function 
            /// is triggered by a packet received from the relay server.
            /// </summary>
            /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
            /// and the Vector3 of the object's new scale</param>
            public void IncrementWorldScale(DataReceivedEventArgs _packet)
            {
                (int[] startLocation, int[] dataLength) = DataLengthsAndStartLocations(_packet.Data);
                string id = ConvertByteArrayIntoString(_packet.Data, startLocation[0], dataLength[0]);
                if (ASLHelper.m_ASLObjects.TryGetValue(id ?? string.Empty, out ASLObject myObject))
                {
                    var parent = myObject.transform.parent;
                    myObject.transform.parent = null;
                    myObject.transform.localScale += (Vector3)ConvertByteArrayIntoVector(_packet.Data, startLocation[1], dataLength[1]);
                    myObject.transform.parent = parent;
                }
            }

            /// <summary>
            /// Pass in the float value(s) from the relay server to a function of the user's choice (delegate function). The function that uses these float(s) is determined 
            /// by the user by setting the ASL Object of choosing's m_FloatCallback function to their own personal function. This function is triggered by a packet received from the relay server.
            /// Remember, though the user can pass in a float array, the max size of this array is 4 because we send it via a Vector4 due to GameSpark constraints
            /// </summary>
            /// <param name="_packet">The packet containing the id of the ASL Object and the float value to be passed into the user defined m_FloatCallback function</param>
            public void SentFloats(DataReceivedEventArgs _packet)
            {
                (int[] startLocation, int[] dataLength) = DataLengthsAndStartLocations(_packet.Data);
                string id = ConvertByteArrayIntoString(_packet.Data, startLocation[0], dataLength[0]);
                if (ASLHelper.m_ASLObjects.TryGetValue(id ?? string.Empty, out ASLObject myObject))
                {
                    float[] myFloats = ConvertByteArrayIntoFloatArray(_packet.Data, startLocation[1], dataLength[1]);

                    //Sliders are  updated through SendFloat, so check here if this is a slider.
                    //By doing this on the ASL side, users don't have to worry about forgetting to update the slider themselves
                    if (myObject.GetComponent<ASLSliderWithEcho>())
                    {
                        myObject.GetComponent<ASLSliderWithEcho>().UpdateSlider(myFloats[0]);
                    }


                    myObject.m_FloatCallback?.Invoke(id, myFloats);
                }
            }

            /// <summary>
            /// Is called when someone sends a Texture2D. This transforms the byte[] of the Texture2D into a Texture2D and calls the function associated with
            /// this sent Texture2D if one exists and async start is enabled. If sync start is enabled instead, then it informs the relay server that is has successfully recreated
            /// the image and is ready to execute its function.
            /// </summary>
            /// <param name="_packet"></param>
            public void RecieveTexture2D(DataReceivedEventArgs _packet)
            {
                (int[] startLocation, int[] dataLength) = DataLengthsAndStartLocations(_packet.Data);
                string id = ConvertByteArrayIntoString(_packet.Data, startLocation[0], dataLength[0]);
                int positionFlag = ConvertByteArrayIntoInt(_packet.Data, startLocation[1], dataLength[1]);

                if (positionFlag == 1) //if first texture packet - create dictionary value that other packets will add to
                {
                    byte[] texture = GetPartOfByteArray(_packet.Data, startLocation[2], dataLength[2]);
                    ReceivedTexture2Ds.Add(id + ConvertByteArrayIntoString(_packet.Data, startLocation[3], dataLength[3]), texture);
                }
                else if (positionFlag == 2) //if packet just contains just needed texture info - add to texture with 
                {
                    string key = id + ConvertByteArrayIntoString(_packet.Data, startLocation[3], dataLength[3]);
                    byte[] texture = GetPartOfByteArray(_packet.Data, startLocation[2], dataLength[2]);
                    if (ReceivedTexture2Ds.TryGetValue(key ?? string.Empty, out byte[] textureSoFar))
                    {
                        ReceivedTexture2Ds[key] = GetInstance().CombineByteArrayWithoutLengths(textureSoFar, texture);
                    }
                }
                else //if we have the last texture packet - finalize the image and then transform byte[] into the image and call the function attached to it
                {
                    string key = id + ConvertByteArrayIntoString(_packet.Data, startLocation[3], dataLength[3]);
                    byte[] texture = GetPartOfByteArray(_packet.Data, startLocation[2], dataLength[2]);
                    if (ReceivedTexture2Ds.TryGetValue(key ?? string.Empty, out byte[] textureSoFar))
                    {
                        ReceivedTexture2Ds[key] = GetInstance().CombineByteArrayWithoutLengths(textureSoFar, texture);
                    }
                    else
                    {
                        Debug.LogError("Unable to create texture - could not find texture.");
                    }

                    if (ASLHelper.m_ASLObjects.TryGetValue(id ?? string.Empty, out ASLObject myObject))
                    {
                        //Get Texture2D
                        Texture2D dummyTexture = new Texture2D(1, 1); //Size doesn't matter
                        dummyTexture.LoadImage(ReceivedTexture2Ds[key]);
                        dummyTexture.name = ConvertByteArrayIntoString(_packet.Data, startLocation[3], dataLength[3]);
                        string postDownloadFunction = ConvertByteArrayIntoString(_packet.Data, startLocation[4], dataLength[4]);
                        //Call PostDownloadFunction
                        var functionName = Regex.Match(postDownloadFunction, @"(\w+)$");
                        functionName.Value.Replace(" ", "");
                        var className = Regex.Replace(postDownloadFunction, @"\s(\w+)$", "");
                        Type callerClass = Type.GetType(className);

                        myObject._LocallySetPostDownloadFunction(
                            (ASLObject.PostDownloadFunction)Delegate.CreateDelegate(typeof(ASLObject.PostDownloadFunction), callerClass, functionName.Value));

                        myObject.m_PostDownloadFunction.Invoke(myObject.gameObject, dummyTexture);
                        ReceivedTexture2Ds.Remove(key); //remove texture from dictionary as we have successfully built it and called the function attached to it
                    }
                }

            }

            /// <summary>
            /// Packet informing user to start trying to resolve a cloud anchor
            /// </summary>
            /// <param name="_packet">Contains cloud anchor id to resolve, ASLObjects to attach to the cloud anchor, 
            /// whether or not to set the world origin, and if to wait for others or not</param>
            public void ResolveAnchorId(DataReceivedEventArgs _packet)
            {
#if UNITY_ANDROID || UNITY_IOS
                (int[] startLocation, int[] dataLength) = DataLengthsAndStartLocations(_packet.Data);
                int sender = ConvertByteArrayIntoInt(_packet.Data, startLocation[4], dataLength[4]);
                //Creator of anchor ID already has this anchor resolved, thus no need to do it
                if (sender != GetInstance().m_PeerId)
                {
                    Debug.Log("Resolving received Anchor Id");
                    string id = ConvertByteArrayIntoString(_packet.Data, startLocation[0], dataLength[0]);
                    string anchorId = ConvertByteArrayIntoString(_packet.Data, startLocation[1], dataLength[1]);
                    bool waitForAllUsersToResolve = ConvertByteArrayIntoBool(_packet.Data, startLocation[2], dataLength[2]);
                    bool setWorldOrigin = ConvertByteArrayIntoBool(_packet.Data, startLocation[3], dataLength[3]);

                    GetInstance().StartCoroutine(ResolveCloudAnchor(id, anchorId, waitForAllUsersToResolve, setWorldOrigin));
                }
#else
                Debug.LogError("Can only resolve cloud anchors on mobile devices.");
#endif
            }
#if UNITY_ANDROID || UNITY_IOS
            /// <summary>
            /// CoRoutine that resolves the cloud anchor
            /// </summary>
            /// <param name="_objectId">The ASLObject tied to this cloud anchor</param>
            /// <param name="anchorID">The cloud anchor to resolve</param>
            /// <param name="_setWorldOrigin">Whether or not to set the world origin</param>
            /// <param name="_waitForAllUsersToResolve">Whether or not to wait for all users before creating the cloud anchor</param>
            /// <returns>yield wait for - until tracking</returns>
            private IEnumerator ResolveCloudAnchor(string _objectId, string anchorID, bool _waitForAllUsersToResolve, bool _setWorldOrigin)
            {
                //If not the device is currently not tracking, wait to resolve the anchor
                while (ARSession.state != ARSessionState.SessionTracking)
                {
                    yield return new WaitForEndOfFrame();
                }

                ARCloudAnchor cloudAnchor = ARWorldOriginHelper.GetInstance().m_ARAnchorManager.ResolveCloudAnchorId(anchorID);

                if (cloudAnchor == null)
                {
                    Debug.LogError("Failed to resolve cloud anchor: " + anchorID);
                }

                //While we are resolving - wait
                while (cloudAnchor.cloudAnchorState == CloudAnchorState.TaskInProgress)
                {
                    yield return new WaitForEndOfFrame();
                }

                if (cloudAnchor.cloudAnchorState == CloudAnchorState.Success)
                {
                    Debug.Log("Successfully Resolved cloud anchor: " + anchorID + " for object: " + _objectId);
                    //Now have at least one cloud anchor in the scene
                    ASLObject anchorObjectPrefab;
                    if (ASLHelper.m_ASLObjects.TryGetValue(_objectId ?? string.Empty, out ASLObject myObject)) //if user has ASL object -> ASL Object was created before linking to cloud anchor
                    {
                        anchorObjectPrefab = myObject;
                        anchorObjectPrefab._LocallySetAnchorID(cloudAnchor.cloudAnchorId);
                        anchorObjectPrefab.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f); //Set scale to be 5 cm
                    }
                    else //ASL Object was created to link to cloud anchor - do the same here
                    {
                        //Uncomment the line below to aid in visual debugging (helps display the cloud anchor)
                        //anchorObjectPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube).AddComponent<ASLObject>(); //if null, then create empty game object   
                        anchorObjectPrefab = new GameObject().AddComponent<ASLObject>();
                        anchorObjectPrefab._LocallySetAnchorID(cloudAnchor.cloudAnchorId); //Add ASLObject component to this anchor and set its anchor id variable
                        anchorObjectPrefab._LocallySetID(_objectId); //Locally set the id of this object to be that of the anchor id (which is unique)

                        //Add this anchor object to our ASL dictionary using the anchor id as its key. All users will do this once they resolve this cloud anchor to ensure they still in sync.
                        ASLHelper.m_ASLObjects.Add(_objectId, anchorObjectPrefab.GetComponent<ASLObject>());
                        //anchorObjectPrefab.GetComponent<Material>().color = Color.magenta;
                        anchorObjectPrefab.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f); //Set scale to be 5 cm
                    }

                    if (_waitForAllUsersToResolve)
                    {
                        //Send packet to relay server letting it know this user is ready
                        byte[] id = Encoding.ASCII.GetBytes(anchorObjectPrefab.m_Id);
                        RTMessage message = GetInstance().CreateRTMessage(OpCode.ResolvedCloudAnchor, id);
                        GetInstance().m_Client.SendMessage(message);

                        //Wait for others
                        anchorObjectPrefab.StartWaitForAllUsersToResolveCloudAnchor(cloudAnchor, _setWorldOrigin, null);
                    }
                    else
                    {
                        anchorObjectPrefab._LocallySetCloudAnchorResolved(true);
                        anchorObjectPrefab.StartWaitForAllUsersToResolveCloudAnchor(cloudAnchor, _setWorldOrigin, null);
                    }
                }
                else
                {
                    Debug.LogError("Failed to host Cloud Anchor " + cloudAnchor.name + " " + cloudAnchor.cloudAnchorState.ToString());
                }

            }
#endif

            /// <summary>
            /// Is called when all clients have finished resolving a cloud anchor
            /// </summary>
            /// <param name="_packet">The packet containing any information about the cloud anchor that is needed</param>
            public void AllClientsFinishedResolvingCloudAnchor(DataReceivedEventArgs _packet)
            {
                string id = Encoding.Default.GetString(_packet.Data);
                if (ASLHelper.m_ASLObjects.TryGetValue(id ?? string.Empty, out ASLObject myObject))
                {
                    myObject._LocallySetCloudAnchorResolved(true);
                }
            }

            /// <summary>
            /// Updates the Anchor Point of an ASL Object based upon its ID. The anchor point is used for AR applications.
            /// This function is triggered by a packet received from the relay server. 
            /// </summary>
            /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
            /// and the object's new Anchor Point information</param>
            public void SetAnchorID(DataReceivedEventArgs _packet)
            {
                (int[] startLocation, int[] dataLength) = DataLengthsAndStartLocations(_packet.Data);
                string id = ConvertByteArrayIntoString(_packet.Data, startLocation[0], dataLength[0]);
                string anchorId = ConvertByteArrayIntoString(_packet.Data, startLocation[1], dataLength[1]);
                if (ASLHelper.m_ASLObjects.TryGetValue(id ?? string.Empty, out ASLObject myObject))
                {
                    myObject._LocallySetAnchorID(anchorId);
                }
            }

            /// <summary>
            /// Updates the tag of an ASL Object based upon its ID. Remember that this tag must be defined by all players.
            /// This function is triggered by a packet received from the relay server. 
            /// </summary>
            /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
            /// and the object's new tag</param>
            public void SetObjectTag(DataReceivedEventArgs _packet)
            {
                (int[] startLocation, int[] dataLength) = DataLengthsAndStartLocations(_packet.Data);
                string id = ConvertByteArrayIntoString(_packet.Data, startLocation[0], dataLength[0]);
                string tag = ConvertByteArrayIntoString(_packet.Data, startLocation[1], dataLength[1]);
                if (ASLHelper.m_ASLObjects.TryGetValue(id ?? string.Empty, out ASLObject myObject))
                {
                    myObject.tag = tag;
                }
            }

            /// <summary>
            /// This function spawns a prefab object with ASL attached as a component. This function is triggered by a packet received from the relay server.
            /// </summary>
            /// <param name="_packet">The packet containing the id of the object to create, what prefab to create it with, where to create it, and depending on what the user inputted, may
            /// contain its parent's id, a callback function's class name and function name that is called upon creation, and a callback function's class name and function name
            /// that will get called whenever a claim for that object is rejected.</param>
            public void SpawnPrefab(DataReceivedEventArgs _packet)
            {
                (int[] startLocation, int[] dataLength) = DataLengthsAndStartLocations(_packet.Data);
                //[0] = id 
                //[1] = position
                //[2] = rotation
                //[3] = prefabName
                //[4] = parentId
                //[5] = component name/type
                //[6] = class of function to call upon instantiation
                //[7] = function to call upon instantiation
                //[8] = claim recovery class 
                //[9] = claim recovery function
                //[10] = send float class
                //[11] = send float function
                //[12] = creator peerId
                string id = ConvertByteArrayIntoString(_packet.Data, startLocation[0], dataLength[0]); 
                GameObject newASLObject = Instantiate(Resources.Load(@"MyPrefabs\" + ConvertByteArrayIntoString(_packet.Data, startLocation[3], dataLength[3]))) as GameObject;
                //Do we need to set the parent?
                string parent = ConvertByteArrayIntoString(_packet.Data, startLocation[4], dataLength[4]);
                if (parent != string.Empty || parent != null)
                {
                    SetParent(newASLObject, parent);
                }

                newASLObject.transform.localPosition = ConvertByteArrayIntoVector(_packet.Data, startLocation[1], dataLength[1]);
                Vector4 rotation = ConvertByteArrayIntoVector(_packet.Data, startLocation[2], dataLength[2]);
                newASLObject.transform.localRotation = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);

                //Set ID
                newASLObject.AddComponent<ASLObject>()._LocallySetID(id);

                //Add any components if needed
                string componentName = ConvertByteArrayIntoString(_packet.Data, startLocation[5], dataLength[5]);
                if (componentName != string.Empty && componentName != null)
                {
                    Type component = Type.GetType(componentName);
                    newASLObject.AddComponent(component);
                }

                //If we have the means to set up the recovery callback function - then do it
                string claimRecoveryClass = ConvertByteArrayIntoString(_packet.Data, startLocation[8], dataLength[8]);
                string claimRecoveryFunction = ConvertByteArrayIntoString(_packet.Data, startLocation[9], dataLength[9]);
                if (claimRecoveryClass != string.Empty && claimRecoveryClass != null &&
                    claimRecoveryFunction != string.Empty && claimRecoveryFunction != null)
                {
                    Type callerClass = Type.GetType(claimRecoveryClass);
                    newASLObject.GetComponent<ASLObject>()._LocallySetClaimCancelledRecoveryCallback(
                        (ASLObject.ClaimCancelledRecoveryCallback)Delegate.CreateDelegate(typeof(ASLObject.ClaimCancelledRecoveryCallback),
                        callerClass, claimRecoveryFunction));
                }

                //If we have the means to set up the SendFloat callback function - then do it
                string floatClass = ConvertByteArrayIntoString(_packet.Data, startLocation[10], dataLength[10]);
                string floatFunction = ConvertByteArrayIntoString(_packet.Data, startLocation[11], dataLength[11]);
                if (floatClass != string.Empty && floatClass != null &&
                    floatFunction != string.Empty && floatFunction != null)
                {
                    Type callerClass = Type.GetType(floatClass);
                    newASLObject.GetComponent<ASLObject>()._LocallySetFloatCallback(
                        (ASLObject.FloatCallback)Delegate.CreateDelegate(typeof(ASLObject.FloatCallback),
                        callerClass, floatFunction));
                }

                //Add object to our dictionary
                ASLHelper.m_ASLObjects.Add(id, newASLObject.GetComponent<ASLObject>());

                //If this client is the creator of this object, then call the ASLGameObjectCreatedCallback if it exists for this object
                if (ConvertByteArrayIntoString(_packet.Data, startLocation[12], dataLength[12]) == GetInstance().m_PeerId.ToString())
                {
                    string instantiationClass = ConvertByteArrayIntoString(_packet.Data, startLocation[6], dataLength[6]);
                    string instantiationFunction = ConvertByteArrayIntoString(_packet.Data, startLocation[7], dataLength[7]);
                    if (instantiationClass != string.Empty && instantiationClass != null && instantiationFunction != string.Empty && instantiationFunction != null)
                    {
                        //Find Callback function
                        Type callerClass = Type.GetType(instantiationClass);
                        newASLObject.GetComponent<ASLObject>()._LocallySetGameObjectCreatedCallback(
                            (ASLObject.ASLGameObjectCreatedCallback)Delegate.CreateDelegate(typeof(ASLObject.ASLGameObjectCreatedCallback), callerClass,
                            instantiationFunction));
                        //Call function
                        newASLObject.GetComponent<ASLObject>().m_ASLGameObjectCreatedCallback.Invoke(newASLObject);
                    }
                }
            }

            /// <summary>
            /// This function spawns a primitive object with ASL attached as a component. This function is triggered by a packet received from the relay server.
            /// </summary>
            /// <param name="_packet">The packet containing the id of the object to create, what type of primitive to create, where to create it, and depending on what the user inputted, may
            /// contain its parent's id, a callback function's class name and function name that is called upon creation, and a callback function's class name and function name
            /// that will get called whenever a claim for that object is rejected.</param>
            public void SpawnPrimitive(DataReceivedEventArgs _packet)
            {
                (int[] startLocation, int[] dataLength) = DataLengthsAndStartLocations(_packet.Data);
                //[0] = id 
                //[1] = position
                //[2] = rotation
                //[3] = primitive type
                //[4] = parentId
                //[5] = component name/type
                //[6] = class of function to call upon instantiation
                //[7] = function to call upon instantiation
                //[8] = claim recovery class 
                //[9] = claim recovery function
                //[10] = send float class
                //[11] = send float function
                //[12] = creator peerId
                string id = ConvertByteArrayIntoString(_packet.Data, startLocation[0], dataLength[0]);
                GameObject newASLObject;
                object primitiveType = (PrimitiveType)ConvertByteArrayIntoInt(_packet.Data, startLocation[3], dataLength[3]);
                if (Enum.IsDefined(typeof(PrimitiveType), primitiveType))
                {
                    newASLObject = GameObject.CreatePrimitive((PrimitiveType)primitiveType);
                }
                else
                {
                    Debug.LogError("Could not parse primitive type when spawning primitive object. Primitive Type given: " + primitiveType.ToString());
                    return;
                }
                //Do we need to set the parent?
                string parent = ConvertByteArrayIntoString(_packet.Data, startLocation[4], dataLength[4]);
                if (parent != string.Empty || parent != null)
                {
                    SetParent(newASLObject, parent);
                }

                newASLObject.transform.localPosition = ConvertByteArrayIntoVector(_packet.Data, startLocation[1], dataLength[1]);
                Vector4 rotation = ConvertByteArrayIntoVector(_packet.Data, startLocation[2], dataLength[2]);
                newASLObject.transform.localRotation = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);

                //Set ID
                newASLObject.AddComponent<ASLObject>()._LocallySetID(id);

                //Add any components if needed
                string componentName = ConvertByteArrayIntoString(_packet.Data, startLocation[5], dataLength[5]);
                if (componentName != string.Empty && componentName != null)
                {
                    Type component = Type.GetType(componentName);
                    newASLObject.AddComponent(component);
                }

                //If we have the means to set up the recovery callback function - then do it
                string claimRecoveryClass = ConvertByteArrayIntoString(_packet.Data, startLocation[8], dataLength[8]);
                string claimRecoveryFunction = ConvertByteArrayIntoString(_packet.Data, startLocation[9], dataLength[9]);
                if (claimRecoveryClass != string.Empty && claimRecoveryClass != null &&
                    claimRecoveryFunction != string.Empty && claimRecoveryFunction != null)
                {
                    Type callerClass = Type.GetType(claimRecoveryClass);
                    newASLObject.GetComponent<ASLObject>()._LocallySetClaimCancelledRecoveryCallback(
                        (ASLObject.ClaimCancelledRecoveryCallback)Delegate.CreateDelegate(typeof(ASLObject.ClaimCancelledRecoveryCallback),
                        callerClass, claimRecoveryFunction));
                }

                //If we have the means to set up the SendFloat callback function - then do it
                string floatClass = ConvertByteArrayIntoString(_packet.Data, startLocation[10], dataLength[10]);
                string floatFunction = ConvertByteArrayIntoString(_packet.Data, startLocation[11], dataLength[11]);
                if (floatClass != string.Empty && floatClass != null &&
                    floatFunction != string.Empty && floatFunction != null)
                {
                    Type callerClass = Type.GetType(floatClass);
                    newASLObject.GetComponent<ASLObject>()._LocallySetFloatCallback(
                        (ASLObject.FloatCallback)Delegate.CreateDelegate(typeof(ASLObject.FloatCallback),
                        callerClass, floatFunction));
                }

                //Add object to our dictionary
                ASLHelper.m_ASLObjects.Add(id, newASLObject.GetComponent<ASLObject>());

                //If this client is the creator of this object, then call the ASLGameObjectCreatedCallback if it exists for this object
                if (ConvertByteArrayIntoString(_packet.Data, startLocation[12], dataLength[12]) == GetInstance().m_PeerId.ToString())
                {
                    string instantiationClass = ConvertByteArrayIntoString(_packet.Data, startLocation[6], dataLength[6]);
                    string instantiationFunction = ConvertByteArrayIntoString(_packet.Data, startLocation[7], dataLength[7]);
                    if (instantiationClass != string.Empty && instantiationClass != null && instantiationFunction != string.Empty && instantiationFunction != null)
                    {
                        //Find Callback function
                        Type callerClass = Type.GetType(instantiationClass);
                        newASLObject.GetComponent<ASLObject>()._LocallySetGameObjectCreatedCallback(
                            (ASLObject.ASLGameObjectCreatedCallback)Delegate.CreateDelegate(typeof(ASLObject.ASLGameObjectCreatedCallback), callerClass,
                            instantiationFunction));
                        //Call function
                        newASLObject.GetComponent<ASLObject>().m_ASLGameObjectCreatedCallback.Invoke(newASLObject);
                    }
                }
            }
            
            /// <summary>
            /// Sets an object's parent based upon that object's ID or name. Preferably ID as it's a lot faster
            /// </summary>
            /// <param name="_myObject">The object that needs a parent assigned to it</param>
            /// <param name="_parentID">The id of the parent to be assigned, or the name of the parent if the parent is not an ASL object</param>
            private void SetParent(GameObject _myObject, string _parentID)
            {
                bool matchFound = false;
                //Search for the parent in ASLObjects            
                if (ASLHelper.m_ASLObjects.TryGetValue(_parentID ?? string.Empty, out ASLObject myParent))
                {
                    _myObject.transform.SetParent(myParent.transform);
                    matchFound = true;
                }

                //Search for parent in regular GameObjects - this is slow compared to our first attempt. But it should only be ran if the user
                //attempts to set the parent object by passing in the parent's name and not their id.
                if (!matchFound)
                {
                    GameObject[] allGameObjects = FindObjectsOfType(typeof(GameObject)) as GameObject[];
                    foreach (GameObject _gameObject in allGameObjects)
                    {
                        if (_gameObject.name == _parentID)
                        {
                            _myObject.transform.SetParent(_gameObject.transform);
                            break;
                        }
                    }
                }
            }



#region Data Transformations

            /// <summary>
            /// Converts a byte array into a vector3 or 4 depending on the vector size sent
            /// </summary>
            /// <param name="_payload">The byte array to be converted</param>
            /// <param name="_vectorStartLocation">The start location in the byte array for the vector</param>
            /// <param name="_vectorSize">the size of the vector to be found in bytes (3*sizeof(float) for vector3)</param>
            /// <returns>A vector2, 3, or 4, depending on the _vectorSize variable</returns>
            private Vector4 ConvertByteArrayIntoVector(byte[] _payload, int _vectorStartLocation, int _vectorSize)
            {
                float[] vectors = new float[_vectorSize / sizeof(float)];
                Buffer.BlockCopy(_payload, _vectorStartLocation, vectors, 0, _vectorSize);

                if (vectors.Length == 2)
                {
                    return new Vector2(vectors[0], vectors[1]);
                }
                else if (vectors.Length == 3)
                {
                    return new Vector3(vectors[0], vectors[1], vectors[2]);
                }
                else
                {
                    return new Vector4(vectors[0], vectors[1], vectors[2], vectors[3]);
                }
            }

            /// <summary>
            /// Converts a byte array into a float array
            /// </summary>
            /// <param name="_payload">the byte array to be converted</param>
            /// <param name="_floatStartLocation">The start location of the floats in the byte array</param>
            /// <param name="_floatArraySize">The size of the float array</param>
            /// <returns>A float array</returns>
            private float[] ConvertByteArrayIntoFloatArray(byte[] _payload, int _floatStartLocation, int _floatArraySize)
            {
                float[] floats = new float[_floatArraySize / sizeof(float)];
                Buffer.BlockCopy(_payload, _floatStartLocation, floats, 0, _floatArraySize);

                return floats;
            }

            /// <summary>
            /// Converts a byte array into a string
            /// </summary>
            /// <param name="_payload">The byte array to be converted</param>
            /// <param name="_stringStartLocation">The start location of the string in the byte array</param>
            /// <param name="_stringLength">The length of the string </param>
            /// <returns>A string</returns>
            private string ConvertByteArrayIntoString(byte[] _payload, int _stringStartLocation, int _stringLength)
            {
                byte[] stringPortion = new byte[_stringLength];
                Buffer.BlockCopy(_payload, _stringStartLocation, stringPortion, 0, _stringLength);
                return Encoding.Default.GetString(stringPortion);
            }

            /// <summary>
            /// Converts a byte array into a boolean variable
            /// </summary>
            /// <param name="_payload">The byte array to be converted</param>
            /// <param name="_boolStartLocation">The start location of the boolean in the byte array</param>
            /// <param name="_boolLength">How length of the bool variable</param>
            /// <returns>A single bool</returns>
            private bool ConvertByteArrayIntoBool(byte[] _payload, int _boolStartLocation, int _boolLength)
            {
                bool[] newBool = new bool[_boolLength];
                Buffer.BlockCopy(_payload, _boolStartLocation, newBool, 0, _boolLength);
                return newBool[0];
            }

            /// <summary>
            /// Returns the specified part of a byte array without converting it into anything else
            /// </summary>
            /// <param name="_payload">The byte array to be sliced</param>
            /// <param name="_startLocation">The start location of the new byte array</param>
            /// <param name="_length">The length of the new byte array</param>
            /// <returns>A new byte array</returns>
            private byte[] GetPartOfByteArray(byte[] _payload, int _startLocation, int _length)
            {
                byte[] newByteArray = new byte[_length];
                Buffer.BlockCopy(_payload, _startLocation, newByteArray, 0, _length);
                return newByteArray;
            }

            /// <summary>
            /// Converts a byte array into an int
            /// </summary>
            /// <param name="_payload">The byte array to be converted</param>
            /// <param name="_startLocation">The start location of the int in the byte array</param>
            /// <param name="_length">The length of the int in the byte array</param>
            /// <returns>A single int</returns>
            private int ConvertByteArrayIntoInt(byte[] _payload, int _startLocation, int _length)
            {
                int[] newInt = new int[1];
                Buffer.BlockCopy(_payload, _startLocation, newInt, 0, _length);
                return newInt[0];
            }

            /// <summary>
            /// Gathers the length and start location of the data inside a byte array
            /// </summary>
            /// <param name="_payload">The byte array</param>
            /// <returns>The start location of the different data pieces inside the byte array and the length of each data piece</returns>
            private (int[] startLocation, int[] dataLength) DataLengthsAndStartLocations(byte[] _payload)
            {
                int dataCount = GetDataCount(_payload);
                int[] dataLengths = GetDataLengths(_payload, dataCount);
                return (GetStartLocations(dataLengths), dataLengths);
            }

            /// <summary>
            /// Gets the start locations of the data pieces in the byte array
            /// </summary>
            /// <param name="_dataLengths">The lengths of each data piece in the original byte array</param>
            /// <returns>An array of ints storing the start location of each data piece in the byte array</returns>
            private int[] GetStartLocations(int[] _dataLengths)
            {
                int metaDataLength = (1 + _dataLengths.Length) * sizeof(int);
                int[] startLocations = new int[_dataLengths.Length];
                startLocations[0] = metaDataLength;
                for (int i = 1; i < startLocations.Length; i++)
                {
                    startLocations[i] = startLocations[i - 1] + _dataLengths[i - 1];
                }
                return startLocations;
            }

            /// <summary>
            /// The data lengths of each data piece in the byte array
            /// </summary>
            /// <param name="_payload">The byte array to be used</param>
            /// <param name="_dataCount">How many pieces of data there are</param>
            /// <returns></returns>
            private int[] GetDataLengths(byte[] _payload, int _dataCount)
            {
                int[] count = new int[_dataCount];
                Buffer.BlockCopy(_payload, sizeof(int), count, 0, sizeof(int) * _dataCount);
                return count;
            }

            /// <summary>
            /// Gets the amount of data pieces in a byte array by looking at the first number in the byte array
            /// </summary>
            /// <param name="_payload">The byte array to be used</param>
            /// <returns>An int showcasing how many data pieces are contained in the byte array</returns>
            private int GetDataCount(byte[] _payload)
            {
                int[] count = new int[1];
                Buffer.BlockCopy(_payload, 0, count, 0, sizeof(int));
                return count[0];
            }

#endregion
        }
    }
}
