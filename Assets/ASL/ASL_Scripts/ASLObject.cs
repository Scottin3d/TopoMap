using Aws.GameLift.Realtime.Command;
#if UNITY_ANDROID || UNITY_IOS
using Google.XR.ARCoreExtensions;
#endif
using System.Text;
using UnityEngine;

namespace ASL
{
    /// /// <summary>
    /// ASLObject: ASLObject Partial Class containing all of the functions and variables relating to server actions - actions that affect all players instead of just a single player.
    /// Use this class to communicate object information to other players. If you are looking for where to create an object for all users at runtime, check out 
    /// <see cref="ASLHelper.InstantiateASLObject(string, Vector3, Quaternion)"/> and it's other variations.
    /// You must call the functions this class provides to synchronize the called actions amongst all players. See each function's documentation for a use case example.
    /// </summary>
    public partial class ASLObject : MonoBehaviour
    {
        /// <summary>Flag indicating whether or not this player currently owns this object or not</summary>
        public bool m_Mine { get; private set; }

        /// <summary>The anchor point for AR applications for this object</summary>
        public string m_AnchorID { get; private set; }

        /// <summary>Flag indicating if the cloud anchor that is attempting to be resolved has been resolved yet or not</summary>
        public bool m_ResolvedCloudAnchor { get; private set; }

        /// <summary>The unique id of this object. This id is synced with all other players</summary>
        public string m_Id { get; private set; }

        /// <summary>Callback to be executed once a player successfully claims this object</summary>
        public ClaimCallback m_ClaimCallback { get; private set; }

        /// <summary>Callback to be executed when a user passes a float value</summary>
        public FloatCallback m_FloatCallback { get; private set; }

        /// <summary>Callback to be executed when a claim is rejected</summary>
        public ClaimCancelledRecoveryCallback m_ClaimCancelledRecoveryCallback { get; private set; }

        /// <summary>Callback to be executed when an object is released. After the callback is executed, it will be set to null to prevent multiple callback's from stacking
        /// on this object after each release. To ensure you aren't attempting to execute this callback after its been set to null, make sure you always reset the callback
        /// when you obtain it if is important to have a release function of some sort</summary>
        public ReleaseFunction m_ReleaseFunction { get; private set; }

        /// <summary>Callback to be executed after an ASL Object is instantiated - this callback is only ever executed by the creator of the object</summary>
        public ASLGameObjectCreatedCallback m_ASLGameObjectCreatedCallback { get; private set; }
        
        /// <summary>Callback to be executed after a Texture2D download is successful</summary>
        public PostDownloadFunction m_PostDownloadFunction { get; private set; }

        /// <summary>Flag indicating whether or not there are any outstanding (not recognized by other users) claims on this object</summary>
        public bool m_OutStandingClaims { get; private set; }

        /// <summary>Delegate for the Claim callback function </summary>
        public delegate void ClaimCallback();

        /// <summary> Delegate for the Float callback function </summary>
        /// <param name="_id">The id of the object that called <see cref="ASLObject.SendFloatArray(float[])"/></param>
        /// <param name="_f">The float(s) to be passed into the user defined function</param>
        public delegate void FloatCallback(string _id, float[] _f);

        /// <summary>Delegate for the release callback function</summary>
        /// <param name="_go">The GameObject to be passed into the user defined function. </param>
        public delegate void ReleaseFunction(GameObject _go);

        /// <summary>Delegate for the claim canceled (rejected) callback function</summary>
        /// <param name="_id">The id of the object that got its claim rejected</param>
        /// <param name="cancelledCount">How many claims were rejected/canceled. Can be null</param>
        public delegate void ClaimCancelledRecoveryCallback(string _id, int cancelledCount);

        /// <summary>Delegate for the GameObject creation function which is executed when this object is instantiated</summary>
        /// <param name="_myGameObject"></param>
        public delegate void ASLGameObjectCreatedCallback(GameObject _myGameObject);

        /// <summary>
        /// Delegate for the Post Download function - used to send this function across the server to be called once a Texture2D download is successful
        /// </summary>
        /// <param name="_myGameObject">The GameObject associated with the Texture2D that was sent (the one used to send the Texture2D)</param>
        /// <param name="_myTexture2D">The Texture2D that was sent</param>
        public delegate void PostDownloadFunction(GameObject _myGameObject, Texture2D _myTexture2D);

        /// <summary>
        /// Delegate for the post create cloud anchor function to execute after creating a cloud anchor
        /// </summary>
        /// <param name="_anchorObjectPrefab">The ASL object this function is tied to</param>
        /// <param name="_hitResult">Information on where the cloud anchor was created</param>
        public delegate void PostCreateCloudAnchorFunction(GameObject _anchorObjectPrefab, Pose _hitResult = new Pose());

        /// <summary>The number of outstanding claims for this object. </summary>
        public int m_OutstandingClaimCallbackCount { get; set; }

        /// <summary>
        /// Claims an object for the user until someone steals it or the passed in claim time is reached. Changing the time you hold onto an object and 
        /// deciding to reset or not the current amount of time you have held it is generally not recommended, but does have occasional applications
        /// </summary>
        /// <param name="callback">The callback to be called when the claim is approved by the server</param>
        /// <param name="claimTimeOut">The amount of time in milliseconds the user will own this object. If set to less than 0,
        /// then the user will own the object until it gets stolen.</param>
        /// /// <param name="resetClaimTimeout">Flag indicating whether or not a claim should reset the claim timer for this object</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///     //Claim the object
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(() =>
        ///     {
        ///         //Whatever code we want to execute AFTER we have successfully claimed this object, such as:
        ///         gameobject.GetComponent&lt;ASL.ASLObject&gt;().DeleteObject(); //Delete our object
        ///         //or
        ///         gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetLocalPosition(new Vector3(5, 0, -2)); //Move our object in its local space to 5, 0, -2
        ///     });
        /// }
        /// </code>
        /// <code>
        /// void SomeFunction()
        /// {
        ///     //Claim an object and then execute the ActionToTake function after a successful claim
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(ActionToTake);
        /// }
        /// //The function to call after a successful claim
        /// private void ActionToTake()
        /// {
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().DeleteObject(); //Delete our object
        /// }
        /// void SomeOtherFunction()
        /// {
        ///     //Same as before, but this time specify the time to hold onto our object before releasing back to the server - default time is 1000 (or 1 second)
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(ADifferentActionToTake, 2000); //Hold onto this object before releasing it to the server for 2 seconds
        /// }
        /// //The function to call after a successful claim
        /// private void ADifferentActionToTake()
        /// {
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetLocalPosition(new Vector3(5, 0, -2)); //Move our object in its local space to 5, 0, -2
        /// }
        /// </code>
        /// <code>
        /// void SomeFunction()
        /// {
        ///     //Claim an object and then (notice the 0 after the last curly brace) hang onto this object until someone steals it from us (do not auto-release to the server)
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(() =>
        ///     {
        ///         //Whatever code we want to execute after we have successfully claimed this object, such as:
        ///         gameobject.GetComponent&lt;ASL.ASLObject&gt;().DeleteObject(); //Delete our object
        ///         //or
        ///         gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetLocalPosition(new Vector3(5, 0, -2)); //Move our object in its local space to 5, 0, -2
        ///     }, 0); //Hold onto this object until someone steals it
        /// }
        /// </code>
        /// <code>
        /// void SomeFunction()
        /// {
        ///     //Claim an object for 1 second, but don't reset whatever our current time length of time we have already held on to it for.
        ///     //Normally if you claim an object, and then claim it again, by default, it will reset the release claim timer - in this case it does not
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(SomeActionToTake, 1000, false); 
        /// }
        /// private void SomeActionToTake()
        /// {
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().DeleteObject(); //Delete our object
        /// }
        /// </code>
        /// <code>
        /// void SomeFunction()
        /// {
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(() =>
        ///     {
        ///         //Whatever code we want to execute after we have successfully claimed this object, such as:
        ///         gameobject.GetComponent&lt;ASL.ASLObject&gt;().DeleteObject(); //Delete our object
        ///         //or
        ///         gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetLocalPosition(new Vector3(5, 0, -2)); //Move our object in its local space to 5, 0, -2
        ///     }, 0, false); //Hold on to this object until someone steals it, but don't reset whatever our current length of time we have already held on to it for
        /// }
        /// </code></example>
        public void SendAndSetClaim(ClaimCallback callback, int claimTimeOut = 1000, bool resetClaimTimeout = true)
        {
            if (Time.timeScale == 0) { return; } //Time scale is set to 0 when not all ASL objects have been assigned an id yet - once all assigned, time will resume
            if (!m_Mine) //If we already own the object, don't send anything and instead call our callback right away
            {
                if (!m_OutStandingClaims)
                {
                    m_OutStandingClaims = true;
                    RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.Claim, Encoding.ASCII.GetBytes(m_Id));
                    GameLiftManager.GetInstance().m_Client.SendMessage(message);
                }
                m_ClaimCallback += callback;
                m_OutstandingClaimCallbackCount++;
            }
            else
            {
                callback();
            }
            if (resetClaimTimeout)
            {
                m_ClaimReleaseTimer = 0; //Reset release timer
                m_ClaimTime = claimTimeOut; //Reset claim length
            }
        }

        /// <summary>
        /// Send and sets this objects color for the user who called this function and for all other players
        /// </summary>
        /// <param name="_myColor">The color to be set for the user who called this function</param>
        /// <param name="_opponentsColor">The color to be set for everyone who did not call this function (everyone else)</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///     //Claim our object first
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(() =>
        ///     {
        ///         //Once claimed, set the object color for the claimer and for everyone else
        ///         gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetObjectColor(new Color(0, 0, 0, 1),  new Color(1, 1, 1, 1));
        ///     });
        /// }
        /// </code></example>
        public void SendAndSetObjectColor(Color _myColor, Color _opponentsColor)
        {
            if (m_Mine)
            {
                byte[] id = Encoding.ASCII.GetBytes(m_Id);
                byte[] myColor = GameLiftManager.GetInstance().ConvertVector4ToByteArray(_myColor);
                byte[] opponentsColor = GameLiftManager.GetInstance().ConvertVector4ToByteArray(_opponentsColor);
                byte[] sender = GameLiftManager.GetInstance().ConvertIntToByteArray(GameLiftManager.GetInstance().m_PeerId);
                byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, myColor, opponentsColor, sender);

                RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.SetObjectColor, payload);
                GameLiftManager.GetInstance().m_Client.SendMessage(message);

            }
        }

        /// <summary>
        /// Deletes this object for all users
        /// </summary>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///     //Claim the object first
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(() =>
        ///     {
        ///         //Once we have it claimed, delete the object
        ///         gameobject.GetComponent&lt;ASL.ASLObject&gt;().DeleteObject();
        ///     });
        /// }
        /// </code></example>
        public void DeleteObject()
        {
            if (gameObject && m_Mine)
            {
                byte[] id = Encoding.ASCII.GetBytes(m_Id);
                RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.DeleteObject, id);
                GameLiftManager.GetInstance().m_Client.SendMessage(message);
            }
        }

        /// <summary>
        /// Sends and sets the local transform for this object for all users
        /// </summary>
        /// <param name="_localPosition">The new local position for this object</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///     //Claim the object first
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(() =>
        ///     {
        ///         //Send and then set (=) the new local position. Note that the object does not move to this position until
        ///         //it receives the packet from the server, even though it is the  caller of this function.
        ///         gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetLocalPosition(new Vector3(1, 2, 5));
        ///     });
        /// }
        /// </code></example>
        public void SendAndSetLocalPosition(Vector3? _localPosition)
        {
            if (m_Mine) //Can only send a transform if we own the object
            {
                if (_localPosition.HasValue)
                {
                    byte[] id = Encoding.ASCII.GetBytes(m_Id);
                    byte[] localPosition = GameLiftManager.GetInstance().ConvertVector3ToByteArray(new Vector3(_localPosition.Value.x, _localPosition.Value.y, _localPosition.Value.z));
                    byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, localPosition);

                    RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.SetLocalPosition, payload);
                    GameLiftManager.GetInstance().m_Client.SendMessage(message);

                }
                else //Send my position as is, not a new position as there was no new position passed in.
                {
                    byte[] id = Encoding.ASCII.GetBytes(m_Id);
                    byte[] localPosition = GameLiftManager.GetInstance().ConvertVector3ToByteArray(new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z));
                    byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, localPosition);

                    RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.SetLocalPosition, payload);
                    GameLiftManager.GetInstance().m_Client.SendMessage(message);
                }
            }
        }

        /// <summary>
        /// Sends and adds to the local transform of this object for all users
        /// </summary>
        /// <param name="_localPosition">The value to be added to the local position of this object</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///      //Claim the object first
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(() =>
        ///     {
        ///         //Send and then increment (+=) the local position. Note that the object does not move to this position until
        ///         //it receives the packet from the server, even though it is the  caller of this function.
        ///         gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndIncrementLocalPosition(new Vector3(1, 2, 5));
        ///     });
        /// }
        /// </code></example>
        public void SendAndIncrementLocalPosition(Vector3? _localPosition)
        {
            if (m_Mine) //Can only send a transform if we own the object
            {
                if (_localPosition.HasValue)
                {
                    byte[] id = Encoding.ASCII.GetBytes(m_Id);
                    byte[] localPosition = GameLiftManager.GetInstance().ConvertVector3ToByteArray(new Vector3(_localPosition.Value.x, _localPosition.Value.y, _localPosition.Value.z));
                    byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, localPosition);

                    RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.IncrementLocalPosition, payload);
                    GameLiftManager.GetInstance().m_Client.SendMessage(message);

                }
                else //Send my position as is, not a new position as there was no new position passed in.
                {
                    byte[] id = Encoding.ASCII.GetBytes(m_Id);
                    byte[] localPosition = GameLiftManager.GetInstance().ConvertVector3ToByteArray(new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z));
                    byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, localPosition);

                    RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.IncrementLocalPosition, payload);
                    GameLiftManager.GetInstance().m_Client.SendMessage(message);
                }
            }
        }

        /// <summary>
        /// Sends and sets the local rotation for this object for all users
        /// </summary>
        /// <param name="_localRotation">The new local rotation for this object.</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///     //Claim the object first
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(() =>
        ///     {
        ///         //Send and then set (=) the local rotation. Note that the object does not to this rotation until
        ///         //it receives the packet from the server, even though it is the  caller of this function.
        ///         gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetLocalRotation(new Quaternion(0, 80, 60, 1));
        ///     });
        /// }
        /// </code></example>
        public void SendAndSetLocalRotation(Quaternion? _localRotation)
        {
            if (_localRotation.HasValue)
            {
                byte[] id = Encoding.ASCII.GetBytes(m_Id);
                byte[] localRotation = GameLiftManager.GetInstance().ConvertVector4ToByteArray(new Vector4(_localRotation.Value.x, _localRotation.Value.y, 
                                                                                                                _localRotation.Value.z, _localRotation.Value.w));
                byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, localRotation);

                RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.SetLocalRotation, payload);
                GameLiftManager.GetInstance().m_Client.SendMessage(message);

            }
            else //Send my position as is, not a new position as there was no new position passed in.
            {
                byte[] id = Encoding.ASCII.GetBytes(m_Id);
                byte[] localRotation = GameLiftManager.GetInstance().ConvertVector4ToByteArray(new Vector4(transform.localRotation.x, transform.localRotation.y, transform.localRotation.z, transform.localRotation.w));
                byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, localRotation);

                RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.SetLocalRotation, payload);
                GameLiftManager.GetInstance().m_Client.SendMessage(message);
            }
        }

        /// <summary>
        /// Sends and adds to the local rotation of this object for all users
        /// </summary>
        /// <param name="_localRotation">The value that will be added to rotation of this object.</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///     //Claim the object first
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(() =>
        ///     {
        ///         //Send and then set (*=) the local rotation. Note that the object does not to this rotation until
        ///         //it receives the packet from the server, even though it is the  caller of this function.
        ///         gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndIncrementLocalRotation(new Quaternion(0, 80, 60, 1));
        ///     });
        /// }
        /// </code></example>
        public void SendAndIncrementLocalRotation(Quaternion? _localRotation)
        {
            if (_localRotation.HasValue)
            {
                byte[] id = Encoding.ASCII.GetBytes(m_Id);
                byte[] localRotation = GameLiftManager.GetInstance().ConvertVector4ToByteArray(new Vector4(_localRotation.Value.x, _localRotation.Value.y,
                                                                                                                _localRotation.Value.z, _localRotation.Value.w));
                byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, localRotation);

                RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.IncrementLocalRotation, payload);
                GameLiftManager.GetInstance().m_Client.SendMessage(message);

            }
            else //Send my position as is, not a new position as there was no new position passed in.
            {
                byte[] id = Encoding.ASCII.GetBytes(m_Id);
                byte[] localRotation = GameLiftManager.GetInstance().ConvertVector4ToByteArray(new Vector4(transform.localRotation.x, transform.localRotation.y, transform.localRotation.z, transform.localRotation.w));
                byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, localRotation);

                RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.IncrementLocalRotation, payload);
                GameLiftManager.GetInstance().m_Client.SendMessage(message);
            }
        }

        /// <summary>
        /// Send and set the local scale for this object for all users
        /// </summary>
        /// <param name="_localScale">The new local scale for this object</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///      //Claim the object first
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(() =>
        ///     {
        ///         //Send and then set (=) the local scale. Note that the object does not move to this scale until
        ///         //it receives the packet from the server, even though it is the  caller of this function.
        ///         gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetLocalScale(new Vector3(.5, 2, .5));
        ///     });
        /// }
        /// </code></example>
        public void SendAndSetLocalScale(Vector3? _localScale)
        {
            if (m_Mine) //Can only send a transform if we own the object
            {
                if (_localScale.HasValue)
                {
                    byte[] id = Encoding.ASCII.GetBytes(m_Id);
                    byte[] localScale = GameLiftManager.GetInstance().ConvertVector3ToByteArray(new Vector3(_localScale.Value.x, _localScale.Value.y, _localScale.Value.z));
                    byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, localScale);

                    RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.SetLocalScale, payload);
                    GameLiftManager.GetInstance().m_Client.SendMessage(message);

                }
                else //Send my position as is, not a new position as there was no new position passed in.
                {
                    byte[] id = Encoding.ASCII.GetBytes(m_Id);
                    byte[] localScale = GameLiftManager.GetInstance().ConvertVector3ToByteArray(new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z));
                    byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, localScale);

                    RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.SetLocalScale, payload);
                    GameLiftManager.GetInstance().m_Client.SendMessage(message);
                }
            }
        }

        /// <summary>
        /// Send and add to the local scale of this object for all users
        /// </summary>
        /// <param name="_localScale">The value that will be added to local scale of this object</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///      //Claim the object first
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(() =>
        ///     {
        ///         //Send and then increment (+=) the local scale. Note that the object does not move to this scale until
        ///         //it receives the packet from the server, even though it is the  caller of this function.
        ///         gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndIncrementLocalScale(new Vector3(.5, 2, .5));
        ///     });
        /// }
        /// </code></example>
        public void SendAndIncrementLocalScale(Vector3? _localScale)
        {
            if (_localScale.HasValue)
            {
                byte[] id = Encoding.ASCII.GetBytes(m_Id);
                byte[] localScale = GameLiftManager.GetInstance().ConvertVector3ToByteArray(new Vector3(_localScale.Value.x, _localScale.Value.y, _localScale.Value.z));
                byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, localScale);

                RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.IncrementLocalScale, payload);
                GameLiftManager.GetInstance().m_Client.SendMessage(message);

            }
            else //Send my position as is, not a new position as there was no new position passed in.
            {
                byte[] id = Encoding.ASCII.GetBytes(m_Id);
                byte[] localScale = GameLiftManager.GetInstance().ConvertVector3ToByteArray(new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z));
                byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, localScale);

                RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.IncrementLocalScale, payload);
                GameLiftManager.GetInstance().m_Client.SendMessage(message);
            }
        }

        /// <summary>
        /// Sends and sets the world position for this object for all users
        /// </summary>
        /// <param name="_position">The new world position for this object</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///     //Claim the object first
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(() =>
        ///     {
        ///         //Send and then set (=) the world position. Note that the object does not move to this position until
        ///         //it receives the packet from the server, even though it is the  caller of this function.
        ///         gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetWorldPosition(new Vector3(1, 2, 5));
        ///     });
        /// }
        /// </code></example>
        public void SendAndSetWorldPosition(Vector3? _position)
        {
            if (m_Mine) //Can only send a transform if we own the object
            {
                if (m_Mine) //Can only send a transform if we own the object
                {
                    if (_position.HasValue)
                    {
                        byte[] id = Encoding.ASCII.GetBytes(m_Id);
                        byte[] position = GameLiftManager.GetInstance().ConvertVector3ToByteArray(new Vector3(_position.Value.x, _position.Value.y, _position.Value.z));
                        byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, position);

                        RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.SetWorldPosition, payload);
                        GameLiftManager.GetInstance().m_Client.SendMessage(message);

                    }
                    else //Send my position as is, not a new position as there was no new position passed in.
                    {
                        byte[] id = Encoding.ASCII.GetBytes(m_Id);
                        byte[] position = GameLiftManager.GetInstance().ConvertVector3ToByteArray(new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z));
                        byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, position);

                        RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.SetWorldPosition, payload);
                        GameLiftManager.GetInstance().m_Client.SendMessage(message);
                    }
                }
            }
        }

        /// <summary>
        /// Sends and adds to the world transform of this object for all users
        /// </summary>
        /// <param name="_position">The value to be added to the world position of this object</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///     //Claim the object first
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(() =>
        ///     {
        ///         //Send and then increment (+=) the world position. Note that the object does not move to this position until
        ///         //it receives the packet from the server, even though it is the  caller of this function.
        ///         gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndIncrementWorldPosition(new Vector3(1, 2, 5));
        ///     });
        /// }
        /// </code></example>
        public void SendAndIncrementWorldPosition(Vector3? _position)
        {
            if (m_Mine) //Can only send a transform if we own the object
            {
                if (_position.HasValue)
                {
                    byte[] id = Encoding.ASCII.GetBytes(m_Id);
                    byte[] position = GameLiftManager.GetInstance().ConvertVector3ToByteArray(new Vector3(_position.Value.x, _position.Value.y, _position.Value.z));
                    byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, position);

                    RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.IncrementWorldPosition, payload);
                    GameLiftManager.GetInstance().m_Client.SendMessage(message);

                }
                else //Send my position as is, not a new position as there was no new position passed in.
                {
                    byte[] id = Encoding.ASCII.GetBytes(m_Id);
                    byte[] position = GameLiftManager.GetInstance().ConvertVector3ToByteArray(new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z));
                    byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, position);

                    RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.IncrementWorldPosition, payload);
                    GameLiftManager.GetInstance().m_Client.SendMessage(message);
                }
            }
        }

        /// <summary>
        /// Sends and sets the world rotation for this object for all users
        /// </summary>
        /// <param name="_rotation">The new world rotation for this object.</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///     //Claim the object first
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(() =>
        ///     {
        ///         //Send and then set (=) the world rotation. Note that the object does not move to this rotation until
        ///         //it receives the packet from the server, even though it is the  caller of this function.
        ///         gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetWorldRotation(new Quaternion(0, 80, 60, 1));
        ///     });
        /// }
        /// </code></example>
        public void SendAndSetWorldRotation(Quaternion? _rotation)
        {
            if (_rotation.HasValue)
            {
                byte[] id = Encoding.ASCII.GetBytes(m_Id);
                byte[] rotation = GameLiftManager.GetInstance().ConvertVector4ToByteArray(new Vector4(_rotation.Value.x, _rotation.Value.y,
                                                                                                                _rotation.Value.z, _rotation.Value.w));
                byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, rotation);

                RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.SetWorldRotation, payload);
                GameLiftManager.GetInstance().m_Client.SendMessage(message);

            }
            else //Send my position as is, not a new position as there was no new position passed in.
            {
                byte[] id = Encoding.ASCII.GetBytes(m_Id);
                byte[] rotation = GameLiftManager.GetInstance().ConvertVector4ToByteArray(new Vector4(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w));
                byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, rotation);

                RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.SetWorldRotation, payload);
                GameLiftManager.GetInstance().m_Client.SendMessage(message);
            }
        }

        /// <summary>
        /// Sends and adds to the world rotation of this object for all users
        /// </summary>
        /// <param name="_rotation">The value that will be added to world rotation of this object.</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///     //Claim the object first
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(() =>
        ///     {
        ///         //Send and then increment (*=) the world rotation. Note that the object does not move to this rotation until
        ///         //it receives the packet from the server, even though it is the  caller of this function.
        ///         gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndIncrementWorldRotation(new Quaternion(0, 80, 60, 1));
        ///     });
        /// }
        /// </code></example>
        public void SendAndIncrementWorldRotation(Quaternion? _rotation)
        {
            if (_rotation.HasValue)
            {
                byte[] id = Encoding.ASCII.GetBytes(m_Id);
                byte[] rotation = GameLiftManager.GetInstance().ConvertVector4ToByteArray(new Vector4(_rotation.Value.x, _rotation.Value.y,                                                                                                                _rotation.Value.z, _rotation.Value.w));
                byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, rotation);

                RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.IncrementWorldRotation, payload);
                GameLiftManager.GetInstance().m_Client.SendMessage(message);

            }
            else //Send my position as is, not a new position as there was no new position passed in.
            {
                byte[] id = Encoding.ASCII.GetBytes(m_Id);
                byte[] rotation = GameLiftManager.GetInstance().ConvertVector4ToByteArray(new Vector4(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w));
                byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, rotation);

                RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.IncrementWorldRotation, payload);
                GameLiftManager.GetInstance().m_Client.SendMessage(message);
            }
        }

        /// <summary>
        /// Send and set the world scale for this object for all users. To set world scale, the object must be de-parented
        /// and then have its scale set, then re-parented. It is not advised to use this function as its behavior, especially
        /// when the parent object is rotated, can cause strange, though correct, behavior.
        /// </summary>
        /// <param name="_scale">The new world scale for this object</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///     //Claim the object first
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(() =>
        ///     {
        ///         //Send and then set (=) the world scale. Note that the object does not move to this scale until
        ///         //it receives the packet from the server, even though it is the  caller of this function.
        ///         gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetWorldScale(new Vector3(.5, 2, .5));
        ///     });
        /// }
        /// </code></example>
        public void SendAndSetWorldScale(Vector3? _scale)
        {
            if (_scale.HasValue)
            {
                byte[] id = Encoding.ASCII.GetBytes(m_Id);
                byte[] scale = GameLiftManager.GetInstance().ConvertVector3ToByteArray(new Vector3(_scale.Value.x, _scale.Value.y, _scale.Value.z));
                byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, scale);

                RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.SetWorldScale, payload);
                GameLiftManager.GetInstance().m_Client.SendMessage(message);

            }
            else //Send my position as is, not a new position as there was no new position passed in.
            {
                byte[] id = Encoding.ASCII.GetBytes(m_Id);

                var parent = gameObject.transform.parent;
                gameObject.transform.parent = null;
                byte[] scale = GameLiftManager.GetInstance().ConvertVector3ToByteArray(new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z));
                gameObject.transform.parent = parent;
                byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, scale);

                RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.SetWorldScale, payload);
                GameLiftManager.GetInstance().m_Client.SendMessage(message);
            }
        }

        /// <summary>
        /// Send and add to the world scale of this object for all users. To set world scale, the object must be de-parented
        /// and then have its scale set, then re-parented. It is not advised to use this function as its behavior, especially
        /// when the parent object is rotated, can cause strange, though correct, behavior.
        /// </summary>
        /// <param name="_scale">The value that will be added to world scale of this object</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///     //Claim the object first
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(() =>
        ///     {
        ///         //Send and then increment (+=) the world scale. Note that the object does not set the anchor ID to this ID until
        ///         //it receives the packet from the server, even though it is the  caller of this function.
        ///         gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndIncrementWorldScale(new Vector3(.5, 2, .5));
        ///     });
        /// }
        /// </code></example>
        public void SendAndIncrementWorldScale(Vector3? _scale)
        {
            if (_scale.HasValue)
            {
                byte[] id = Encoding.ASCII.GetBytes(m_Id);
                byte[] scale = GameLiftManager.GetInstance().ConvertVector3ToByteArray(new Vector3(_scale.Value.x, _scale.Value.y, _scale.Value.z));
                byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, scale);

                RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.IncrementWorldScale, payload);
                GameLiftManager.GetInstance().m_Client.SendMessage(message);

            }
            else //Send my position as is, not a new position as there was no new position passed in.
            {
                byte[] id = Encoding.ASCII.GetBytes(m_Id);

                var parent = gameObject.transform.parent;
                gameObject.transform.parent = null;
                byte[] scale = GameLiftManager.GetInstance().ConvertVector3ToByteArray(new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z));
                gameObject.transform.parent = parent;
                byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, scale);

                RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.IncrementWorldScale, payload);
                GameLiftManager.GetInstance().m_Client.SendMessage(message);
            }
        }

        /// <summary>
        /// Send and set the AR anchor id for this object for all users. Normally, you will not need to call this function
        /// as creating an AR Anchor is done through <see cref="ASLHelper.CreateARCoreCloudAnchor(Pose?, ASLObject, PostCreateCloudAnchorFunction, bool, bool)"/>
        /// But if for some reason you want to change Anchors, you can do so via this function
        /// </summary>
        /// <param name="_anchorID">The anchor id for this object to reference</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///     //Claim the object first
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(() =>
        ///     {
        ///         //Send and then set (=) the id of the AR Anchor you want this object to have. Note that the object does not move to this scale until
        ///         //it receives the packet from the server, even though it is the  caller of this function.
        ///         gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetAnchorID("Your Anchor id Here");
        ///     });
        /// }
        /// </code></example>
        public void SendAndSetAnchorID(string _anchorID)
        {
            byte[] id = Encoding.ASCII.GetBytes(m_Id);
            byte[] anchorId = Encoding.ASCII.GetBytes(_anchorID);

            byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, anchorId);

            RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.AnchorIDUpdate, payload);
            GameLiftManager.GetInstance().m_Client.SendMessage(message);
        }

        /// <summary>
        /// Send and set the tag for this object for all users. As Unity does not all users to create tags at runtime, all players must have this tag defined
        /// </summary>
        /// <param name="_tag">The tag for this object to get</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///     //Claim your object first
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(() =>
        ///     {
        ///         //Send and then set (=) the tag this object should have. Note that the object does not set the tag of this object until
        ///         //it receives the packet from the server, even though it is the  caller of this function.
        ///         gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetTag("Your Tag Here");
        ///     });
        /// }
        /// </code></example>
        public void SendAndSetTag(string _tag)
        {
            byte[] id = Encoding.ASCII.GetBytes(m_Id);
            byte[] anchorId = Encoding.ASCII.GetBytes(_tag);

            byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, anchorId);

            RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.TagUpdate, payload);
            GameLiftManager.GetInstance().m_Client.SendMessage(message);
        }

        /// <summary>
        /// Send and set up to a maximum of 1000 float value(s). The float value(s) will then be processed by a user defined function.
        /// If you need to send more than 1000 floats with this object, or you need this object to perform more than one action with this function,
        /// then a switch statement is a good way to implement what you need.
        /// </summary>
        /// <param name="_f">The float value to be passed to all users</param>
        /// <example>
        /// <code>
        /// void SomeFunction()
        /// {
        ///     //Claim the object first
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(() =>
        ///     {
        ///         float[] myValue = new float[1];
        ///         myValue[0] = 3.5;
        ///         //In this example, playerHealth would be updated to 3.5 for all users
        ///         gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendFloatArray(myValue); 
        ///     });
        /// }
        /// </code><code>
        /// The same gameobject would then also have these qualities: 
        /// 
        /// void Start()
        /// {
        ///     //Or if this object was created dynamically, then to have this function assigned on creation instead of in start like this one is
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;()._LocallySetFloatCallback(UserDefinedFunction) 
        /// }
        /// //Assuming it is properly linked via the float callback function (see void Start() in this example)
        /// //This function executes upon receiving the SendFloatArray() packet from the server, even if you are the player that called that function
        /// public void UserDefinedFunction(string _id, float[] _f) 
        /// {
        ///     //Update some value for all users based on _f. 
        ///     //Example:
        ///     playerHealth = _f[0]; //Where playerHealth is shown to kept track/shown to all users
        /// }
        ///</code>
        /// It is possible to use this function to send more than 1000 variables if the user sets up the function to execute upon receiving SendFloatArray to include a switch statement
        /// with the final value (really, any value you want) in the float array being what determines how the other values are handled. See below for an example
        /// <code>
        /// //For example, use this function to update player stats using the same SendFloatArray UserDefinedFunction that can also update velocity and score
        /// void SomeFunction()
        /// {
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(() =>
        ///     {
        ///         float[] myValue = new float[1];
        ///         myValue[0] = 3.5f;
        ///         myValue[1] = 0;
        ///         myValue[2] = 1.2f;
        ///         myValue[3] = 2;
        ///         //In this example, playerHealth would be updated to 3.5 for all users, playerArmor to 0, playerSpeedBuff to 1.2, and the switch case to properly assign these values, 2
        ///         gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendFloatArray(myValue); 
        ///     });
        /// }
        /// //For example, use this function to update velocity using the same SendFloatArray UserDefinedFunction that can also update player stats and score
        /// void SomeOtherFunction()
        /// {
        ///     //Claim the object
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(() =>
        ///     {
        ///         float[] myValue = new float[4];
        ///         myValue[0] = 17.8f;
        ///         myValue[1] = 180.2f;
        ///         myValue[2] = 1.2f;
        ///         myValue[3] = 1;
        ///         //In this example, velocity would be set to 17.8 and direction to 180.2
        ///         gameobject.GetComponent&lt;ASL.ASLObject&gt;().SendFloatArray(myValue); //Send the floats
        ///     });
        /// }
        /// 
        /// void Start()
        /// {
        ///     //Or if this object was created dynamically, then to have this function assigned on creation instead of in start like this one is
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;()._LocallySetFloatCallback(UserDefinedFunction) 
        /// }
        /// //Assuming it is properly linked via the float callback function (see void Start() in this example)
        /// //This function executes upon receiving the SendFloatArray() packet from the server, even if you are the player that called that function
        /// public void UserDefinedFunction(string _id, float[] _f)
        /// {
        ///     //Update some value for all users based on _f and update 1 object specifically. 
        ///     //Example:
        ///     //If we find the object that called this operation
        ///    if (ASL.ASLHelper.m_ASLObjects.TryGetValue(_id, out ASL.ASLObject myObject))
        ///    {
        ///         switch (_f[3])
        ///         {
        ///             case 0:
        ///                 //Update score based on _f[0] for example
        ///                 break;
        ///             case 1:
        ///                 //Update player velocity and direction with f[0] and f[1] for example
        ///                 playerVelocity = _f[0]; //Velocity gets set to 17.8
        ///                 playerDirection = _f[1]; //Player Direction gets set to 180.2
        ///                 break;
        ///             case 2:
        ///                 playerHealth = _f[0]; //Health gets assigned to 3.5
        ///                 playerArmor = _f[1]; //Armor gets assigned to 0
        ///                 playerSpeedBuff = _f[2]; SpeedBuff gets assigned to 1.2
        ///                 break;
        ///             case 3:
        ///                 myObject.GetComponent&lt;RigidBody&gt;().velocity = _f[0];
        ///                 myObject.GetComponent&lt;MyScript&gt;().MyVariable = _f[1];
        ///                 break;
        ///         }
        ///     }
        /// }
        /// </code>
        ///</example>
        public void SendFloatArray(float[] _f)
        {
            if (m_Mine) //Can only send a transform if we own the object
            {
                byte[] id = Encoding.ASCII.GetBytes(m_Id);
                byte[] floats = GameLiftManager.GetInstance().ConvertFloatArrayToByteArray(_f);
                byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, floats);

                if (payload.Length > 4096)
                {
                    Debug.LogError("Your float array is too large. Max float array length: 1000");
                    return;
                }

                RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.SendFloats, payload);
                GameLiftManager.GetInstance().m_Client.SendMessage(message);

            }
            else
            {
                Debug.Log("Cannot send floats - do not have ownership of object");
            }
        }

        /// <summary>Sends a Texture2D to other users and then calls the sent function once it is successfully recreated</summary>
        /// <param name="_myTexture2D">The Texture2D to be uploaded and sent to others</param>
        /// <param name="_myPostDownloadFunction">The function to be called after the Texture2D is downloaded</param>
        /// <param name="_uploadAsPNG">Optional parameter allowing the user to upload the image as a PNG. The default is JPG.</param>
        /// <example><code>
        /// An example of how to send a Texture2D.
        /// Optional parameters include to send it as a PNG (larger file, but allows for transparent pixels, default is JPG),
        /// and to synchronize every user's callback function (in this case, "ChangeSpriteTexture"), the default is to not synchronize
        /// callback functions, so once a user downloads an image, by default they will execute their given function instead of
        /// waiting for every other user to download the image as well.
        /// public void SendAndChangeTexture()
        /// {
        ///    _MyASLObject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetTexture2D(_TextureToSend, ChangeSpriteTexture, false);
        /// }
        /// The function is called after the Texture2D is sent by all users. Note: All users must have this function defined for them 
        /// (just like with other callback functions except claim). Just because other users do not have the Texture2D being sent 
        /// does not mean they do not need this function. This function can be used to do whatever you want to do after the Texture2D 
        /// has been transferred. In this case, it is simply swapping the Texture2D out on the current sprite, changing the image 
        /// the sprite displays. This function can be named anything, but must be a static public void function.
        /// static public void ChangeSpriteTexture(GameObject _myGameObject, Texture2D _myTexture2D)
        /// {
        ///    Sprite newSprite = Sprite.Create(_myTexture2D, _myGameObject.GetComponent&lt;SpriteRenderer&gt;().sprite.rect,
        ///        new Vector2(0.5f, 0.5f), _myGameObject.GetComponent&lt;SpriteRenderer&gt;().sprite.pixelsPerUnit);
        ///    _myGameObject.GetComponent&lt;SpriteRenderer&gt;().sprite = newSprite;
        /// }
        /// </code></example>
        public void SendAndSetTexture2D(Texture2D _myTexture2D, PostDownloadFunction _myPostDownloadFunction, bool _uploadAsPNG = false)
        {
            byte[] id = Encoding.ASCII.GetBytes(m_Id);
            byte[] imageAsBytes;
            //Change Texture2D into png 
            if (_uploadAsPNG)
            {
                imageAsBytes = _myTexture2D.EncodeToPNG(); //Can also encode to jpg, just make sure to change the file extensions down below
            }
            else //change Texture2D into JPG
            {
                imageAsBytes = _myTexture2D.EncodeToJPG();
            }
            int imageLength = imageAsBytes.Length; //Get length of image so we know when we no longer need to send packets
            byte[] textureName = Encoding.ASCII.GetBytes(_myTexture2D.name); //Send name to help distinguish this texture when received and so receivers know what it is called
            byte[] postDownloadFunction = Encoding.ASCII.GetBytes(_myPostDownloadFunction.Method.ReflectedType + " " + _myPostDownloadFunction.Method.Name);
            byte[] firstPositionFlag = GameLiftManager.GetInstance().ConvertIntToByteArray(1);

            int maxPacketSize = 4076; //4096 - 20 (20 is the size of our meta data)
            //First packet:
            int imagePacketsSent = maxPacketSize - id.Length - firstPositionFlag.Length - textureName.Length;
            byte[] firstImagePacket;
            if (imagePacketsSent < imageLength) //if we need to split, use the maximum packet size we can
            {
                firstImagePacket = GameLiftManager.GetInstance().SpiltByteArray(imageAsBytes, 0, imagePacketsSent);
            }
            else //if we don't need to split, use actual image size - will still send last packet flag packet regardless if it could've been sent in 1 packet or not
            {
                firstImagePacket = GameLiftManager.GetInstance().SpiltByteArray(imageAsBytes, 0, imageLength);
            }

            byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, firstPositionFlag, firstImagePacket, textureName);
            RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.SendTexture2D, payload);
            GameLiftManager.GetInstance().m_Client.SendMessage(message);

            //Middle packets:
            byte[] middlePositionFlag = GameLiftManager.GetInstance().ConvertIntToByteArray(2);
            while (imagePacketsSent < imageLength)
            {               
                int currentImagePacketLength = maxPacketSize - id.Length - middlePositionFlag.Length - textureName.Length;
                byte[] nextImagePacket;
                //if current packet size + what we've sent is less than the image length and the post download function we need to send, then we still need to break up the image into more packets
                if (currentImagePacketLength + imagePacketsSent < imageLength + postDownloadFunction.Length) 
                {
                    if (currentImagePacketLength + imagePacketsSent < imageLength) //if still more image to send 
                    {
                        nextImagePacket = GameLiftManager.GetInstance().SpiltByteArray(imageAsBytes, imagePacketsSent, currentImagePacketLength);
                        imagePacketsSent += currentImagePacketLength;
                    }
                    else //This is the last image packet, but no room for postDownload function, so still count as a middle packet
                    {
                        nextImagePacket = GameLiftManager.GetInstance().SpiltByteArray(imageAsBytes, imagePacketsSent, imageLength - imagePacketsSent);
                        imagePacketsSent = imageLength;
                    }
                    byte[] middlePayload = GameLiftManager.GetInstance().CombineByteArrays(id, middlePositionFlag, nextImagePacket, textureName);
                    RTMessage middleMessage = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.SendTexture2D, middlePayload);
                    GameLiftManager.GetInstance().m_Client.SendMessage(middleMessage);
                }
                else
                {                    
                    break;
                }
            }

            //Send last packets of image (if any) and the post download function
            byte[] lastImagePacket = GameLiftManager.GetInstance().SpiltByteArray(imageAsBytes, imagePacketsSent, imageLength - imagePacketsSent);
            byte[] lastPositionFlag = GameLiftManager.GetInstance().ConvertIntToByteArray(3);
            byte[] lastPayload = GameLiftManager.GetInstance().CombineByteArrays(id, lastPositionFlag, lastImagePacket, textureName, postDownloadFunction);

            RTMessage lastMessage = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.SendTexture2D, lastPayload);
            GameLiftManager.GetInstance().m_Client.SendMessage(lastMessage);
        }

        /// <summary>
        /// Used by ASLHelper to send information about the Cloud Anchor that was just created to all users so that they can resolve it and handle it accordingly on their end
        /// </summary>
        /// <param name="_setWorldOrigin">Whether or not this cloud anchor should be used as a world origin</param>
        /// <param name="_waitForAllUsersToResolve">Whether or not this cloud anchor should wait to perform any action until every user has resolved it</param>
        public void SendCloudAnchorToResolve(bool _setWorldOrigin, bool _waitForAllUsersToResolve)
        {
            if (!m_HaventSetACloudAnchor)
            {
                m_HaventSetACloudAnchor = true;

                byte[] id = Encoding.ASCII.GetBytes(m_Id);
                byte[] anchorId = Encoding.ASCII.GetBytes(m_AnchorID);
                byte[] waitForAllUsersToResolve = GameLiftManager.GetInstance().ConvertBoolToByteArray(_waitForAllUsersToResolve);
                byte[] setWorldOrigin = GameLiftManager.GetInstance().ConvertBoolToByteArray(_setWorldOrigin);
                byte[] sender = GameLiftManager.GetInstance().ConvertIntToByteArray(GameLiftManager.GetInstance().m_PeerId);
                byte[] payload = GameLiftManager.GetInstance().CombineByteArrays(id, anchorId, waitForAllUsersToResolve, setWorldOrigin, sender);
                
                RTMessage message = GameLiftManager.GetInstance().CreateRTMessage(GameLiftManager.OpCode.ResolveAnchorId, payload);
                GameLiftManager.GetInstance().m_Client.SendMessage(message);
            }
        }

        #if UNITY_ANDROID || UNITY_IOS
        /// <summary>
        /// Used to trigger a coroutine that doesn't execute until all users have set their cloud anchor
        /// </summary>
        /// <param name="_cloudAnchor">The cloud anchor that was resolved</param>
        /// <param name="_setWorldOrigin">Whether or not to set the cloud anchor</param>
        /// <param name="_postResolveCloudAnchorFunction">The function to execute after resolving the cloud anchor - only called by the creator of the cloud anchor</param>
        /// <param name="_hitResult">Information on where the cloud anchor was created</param>
        public void StartWaitForAllUsersToResolveCloudAnchor(ARCloudAnchor _cloudAnchor,
            bool _setWorldOrigin, PostCreateCloudAnchorFunction _postResolveCloudAnchorFunction = null, Pose _hitResult = new Pose())
        {
            StartCoroutine(WaitForAllUsersToResolveCloudAnchor(_cloudAnchor, _setWorldOrigin, _postResolveCloudAnchorFunction, _hitResult));
        }

        /// <summary>
        /// Waits for m_ResolvedCloudAnchor to be true before doing anything with the passed in cloud anchor
        /// </summary>
        /// <param name="_cloudAnchor">The cloud anchor that was just created/resolved</param>
        /// <param name="_setWorldOrigin">Flag indicating if this cloud anchor should become the world origin</param>
        /// <param name="_postResolveCloudAnchorFunction">The function to call after creating a cloud anchor (will be null for those clients resolving)</param>
        /// <param name="_hitResult">Information on where the cloud anchor was created</param>
        /// <returns>Waits for the end of the frame before trying again</returns>
        private IEnumerator WaitForAllUsersToResolveCloudAnchor(ARCloudAnchor _cloudAnchor,
            bool _setWorldOrigin, PostCreateCloudAnchorFunction _postResolveCloudAnchorFunction = null, Pose _hitResult = new Pose())
        {
            while (!m_ResolvedCloudAnchor)
            {
                yield return new WaitForSeconds(1.0f);
            }

            if (_setWorldOrigin)
            {
                //Set our anchor object prefab to always follow our cloud anchor by setting it as a child of that cloud anchor
                //All users will do this, thus no need to use a SendAndSet function
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.SetParent(_cloudAnchor.transform, false);

                ARWorldOriginHelper.GetInstance().SetWorldOrigin(_cloudAnchor.transform);
                _cloudAnchor.name = "World Origin Anchor";
            }
            else
            {
                //Set our anchor object prefab to always follow our cloud anchor by setting it as a child of that cloud anchor
                //All users will do this, thus no need to use a SendAndSet function
                transform.SetParent(_cloudAnchor.transform, false);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }


            ASLHelper.m_CloudAnchors.Add(_cloudAnchor.cloudAnchorId, new ASLHelper.CloudAnchor(_cloudAnchor, _setWorldOrigin));
            _postResolveCloudAnchorFunction?.Invoke(gameObject, _hitResult);
        }
#endif
        /// <summary>
        /// Gets called right before an object is destroyed. Used to remove this object from the dictionary
        /// </summary>
        private void OnDestroy()
        {
            if (ASLHelper.m_ASLObjects.ContainsKey(m_Id))
            {
                ASLHelper.m_ASLObjects.Remove(m_Id);
            }
            if (m_AnchorID != null && m_AnchorID != string.Empty && ASLHelper.m_CloudAnchors.ContainsKey(m_AnchorID))
            {
                ASLHelper.m_CloudAnchors.Remove(m_AnchorID);
            }

        }
    }
}
