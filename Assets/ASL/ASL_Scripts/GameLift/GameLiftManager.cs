//Used for help debug GameLift packet issues and other misc. GameLift potential problems.
#define ASL_DEBUG
using Aws.GameLift.Realtime.Event;
using Aws.GameLift.Realtime;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Aws.GameLift.Realtime.Command;
using Aws.GameLift.Realtime.Types;
using System.Collections;

namespace ASL
{
    /// <summary>
    /// The class that makes all multiplayer possible
    /// </summary>
    public partial class GameLiftManager : MonoBehaviour
    {
        /// <summary>
        /// An internal class that is used to create and contain information about the Player Session that this user creates when joining a Game Session
        /// </summary>
        [System.Serializable]
        private class PlayerSessionObject
        {
            public string PlayerSessionId = null;
            public string PlayerId = null;
            public string GameSessionId = null;
            public string FleetId = null;
            public string CreationTime = null;
            public string Status = null;
            public string IpAddress = null;
            public string DnsName = null;
            public string Port = null;
            public string GameName = null;
            public string SceneName = null;
        }

        /// <summary>
        /// An internal class that contains all of the GameSessionObjects found by this user
        /// </summary>
        [System.Serializable]
        private class GameSessionObjectCollection
        {
            public List<GameSessionObject> GameSessions = null;
        }

        /// <summary>
        /// An internal class containing information about the GameSession this user has found or joined
        /// </summary>
        [System.Serializable]
        private class GameSessionObject
        {
            public string GameSessionId = null;
            public string Name = null;
            public string CurrentPlayerSessionCount = null;
            public string MaximumPlayerSessionCount = null;
            public string IpAddress = null;
            public string Port = null;
        }

        /// <summary>
        /// The singleton instance for this class
        /// </summary>
        private static GameLiftManager m_Instance;
        
        /// <summary>
        /// Internal class used to setup and connect users to each other
        /// </summary>
        private LobbyManager m_LobbyManager;
        
        /// <summary>
        /// Internal class used to load scenes for all users
        /// </summary>
        private SceneLoader m_SceneLoader;
        
        /// <summary>
        /// Internal class used to decoded packets received from the AWS
        /// </summary>
        private GameController m_GameController;

        /// <summary>
        /// This current user's username
        /// </summary>
        public string m_Username { get; private set; }

        /// <summary>
        /// The name of the game session the user is currently in
        /// </summary>
        public string m_GameSessionName { get; private set; }

        /// <summary>
        /// This current user's peerId
        /// </summary>
        public int m_PeerId { get; private set; }

        /// <summary>
        /// Dictionary containing the all users that are connected peerIds and usernames
        /// </summary>
        public Dictionary<int, string> m_Players = new Dictionary<int, string>();

        /// <summary>
        /// The id of the server,  used to send messages to the server
        /// </summary>
        public int m_ServerId { get; private set; }

        /// <summary>
        /// The group ID used to communicate with all users
        /// </summary>
        public int m_GroupId { get; private set; }

        /// <summary>
        /// The AWS client variable that allows a connection to and the ability to communicate with GameLift
        /// </summary>
        public Client m_Client { get; private set; }

        /// <summary>
        /// Can be any positive number, but must be matched with the OpCodes in the RealTime script.
        /// </summary>
        public enum OpCode
        {
            /// <summary>Packet identifier that indicates a player has logged in</summary>
            PlayerLoggedIn,
            /// <summary>Packet identifier that indicates a player has joined the match</summary>
            PlayerJoinedMatch,
            /// <summary>Packet identifier that indicates a packet that contains the information to add a player to a lobby</summary>
            AddPlayerToLobbyUI,
            /// <summary>Packet identifier that indicates a packet that contains the information on which player disconnected</summary>
            PlayerDisconnected,
            /// <summary>Packet identifier that indicates a packet that contains the information that all players are ready to launch the first scene</summary>
            AllPlayersReady,
            /// <summary>Packet identifier that indicates a packet that contains the information that a player has disconnected before the match began</summary>
            PlayerDisconnectedBeforeMatchStart,
            /// <summary>Packet identifier that indicates a packet that contains the information that a player is ready</summary>
            PlayerReady,
            /// <summary>Packet identifier that indicates a packet that contains the information to launch a scene</summary>
            LaunchScene,
            /// <summary>Packet code for changing the scene</summary>
            LoadScene,
            /// <summary>Packet code for creating an id for an object on the server</summary>
            ServerSetId,
            /// <summary>Packet code to release a claim back to the server</summary>
            ReleaseClaimToServer,
            /// <summary>Packet code representing a claim</summary>
            Claim,
            /// <summary>Packet code informing a player their claim was rejected</summary>
            RejectClaim,
            /// <summary>Packet code informing player who has a claim on an object to release to another player</summary>
            ReleaseClaimToPlayer,
            /// <summary>Packet code informing a player who claimed an object from another player that they have it now</summary>
            ClaimFromPlayer,
            /// <summary>Packet code for setting an object's color</summary>
            SetObjectColor,
            /// <summary>Packet code for deleting an object</summary>
            DeleteObject,
            /// <summary>Packet code representing data that will set the local position of an ASL object</summary>
            SetLocalPosition,
            /// <summary>Packet code representing data that will add to the local position of an ASL object</summary>
            IncrementLocalPosition,
            /// <summary>Packet code representing data that will set the local rotation of an ASL object</summary>
            SetLocalRotation,
            /// <summary>Packet code representing data that will add to the local rotation of an ASL object</summary>
            IncrementLocalRotation,
            /// <summary>Packet code representing data that will set the local scale of an ASL object</summary>
            SetLocalScale,
            /// <summary>Packet code representing data that will add to the local scale of an ASL object</summary>
            IncrementLocalScale,
            /// <summary>Packet code representing data that will set the world position of an ASL object</summary>
            SetWorldPosition,
            /// <summary>Packet code representing data that will add to the world position of an ASL object</summary>
            IncrementWorldPosition,
            /// <summary>Packet code representing data that will set the world rotation of an ASL object</summary>
            SetWorldRotation,
            /// <summary>Packet code representing data that will add to the world rotation of an ASL object</summary>
            IncrementWorldRotation,
            /// <summary>Packet code representing data that will set the world scale of an ASL object</summary>
            SetWorldScale,
            /// <summary>Packet code representing data that will add to the world scale of an ASL object</summary>
            IncrementWorldScale,
            /// <summary>Packet code for spawning a prefab</summary>
            SpawnPrefab,
            /// <summary>Packet code for spawning a primitive object</summary>
            SpawnPrimitive,
            /// <summary>Packet code informing a player float(s) were sent</summary>
            SendFloats,
            /// <summary>Packet code representing data that will be used to recreate a Texture2D as pieces of it come in</summary>
            SendTexture2D,
            /// <summary>Packet code representing data that will be used to resolve a cloud anchor</summary>
            ResolveAnchorId,
            /// <summary>Packet code representing data that will be used to inform the relay server that this user has completed
            /// resolving a cloud anchor and will be received as a flag indicating that all users have resolved this cloud anchor </summary>
            ResolvedCloudAnchor,
            /// <summary>Packet code for updating the AR anchor point</summary>
            AnchorIDUpdate,
            /// <summary>Packet code for sending text messages to other users</summary>
            LobbyTextMessage,
            /// <summary>Used to help keep the Android socket connection alive</summary>
            AndroidKeepConnectionAlive,
            /// <summary>Packet code for sending object tags</summary>
            TagUpdate

        }

        /// <summary>
        /// The queue that will hold all of the functions to be triggered by AWS events
        /// </summary>
        private readonly Queue<Action> m_MainThreadQueue = new Queue<Action>();

        /// <summary>
        /// Flag indicating whether or not we have activated the KeepConnectionAlive() coroutine so it doesn't get activated more than once
        /// </summary>
        private bool m_StreamActive = false;

        /// <summary>
        /// Used to get the Singleton instance of the GameLiftManager class
        /// </summary>
        /// <returns>The singleton instance of this class</returns>
        public static GameLiftManager GetInstance()
        {
            if (m_Instance != null)
            {
                return m_Instance;
            }
            else
            {
#if (ASL_DEBUG)
                Debug.LogError("GameLift not initialized.");
#endif
            }
            return null;
        }

        /// <summary>
        /// The Awake function, called right after object creation. Used to communicate that this object should not be destroyed between scene loads
        /// </summary>
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);           
        }

        /// <summary>
        /// The start function called upon object creation but after Awake, used to initialize internal classes and setup the singleton for this class
        /// </summary>
        private void Start()
        {
            m_Instance = this;
            m_LobbyManager = new LobbyManager();
            m_LobbyManager.Start();
            m_SceneLoader = new SceneLoader();
            m_GameController = new GameController();
            m_GameController.Start();
#if UNITY_ANDROID
            Screen.sleepTimeout = SleepTimeout.NeverSleep; //Prevents Android app from turning the screen off, thus closing the application
#endif
        }

        /// <summary>
        /// Update is called once per frame and in this case is used to ensure any function triggered by AWS is ran by the main Unity thread
        /// </summary>
        private void Update()
        {
            RunMainThreadQueueActions();
        }

        /// <summary>
        /// An AWS listener function that is called when a connection is opened
        /// </summary>
        /// <param name="sender">The entity that called this function</param>
        /// <param name="error">Any error associated with this function</param>
        private void OnOpenEvent(object sender, EventArgs error)
        {
#if (ASL_DEBUG)
            Debug.Log("[server-sent] OnOpenEvent");
#endif
        }

        /// <summary>
        /// An AWS listener function that is called when a connection is ended with GameLift
        /// </summary>
        /// <param name="sender">The entity that called this error</param>
        /// <param name="error">Any error associated with this connection termination</param>
        private void OnCloseEvent(object sender, EventArgs error)
        {
#if (ASL_DEBUG)
            Debug.Log("[server-sent] OnCloseEvent: " + error);
#endif
            DisconnectFromServer();
            if (m_LobbyManager != null)
            {
                QForMainThread(m_LobbyManager.Reset);
            }
        }

        /// <summary>
        /// An AWS listener function that is called when there is a connection error event
        /// </summary>
        /// <param name="sender">The entity that called this error</param>
        /// <param name="error">The error that was received</param>
        private void OnConnectionErrorEvent(object sender, Aws.GameLift.Realtime.Event.ErrorEventArgs error)
        {
#if (ASL_DEBUG)
            Debug.Log($"[client] Connection Error! : ");
            if (error != null)
            {
                Debug.Log("Exception: \n" + error.Exception);
            }
#endif
            QForMainThread(DisconnectFromServer);
        }

        /// <summary>
        /// An AWS listener function that gets called every time a packet is received.
        /// </summary>
        /// <param name="sender">The packet sender</param>
        /// <param name="_packet">The packet that was received</param>
        private void OnDataReceived(object sender, DataReceivedEventArgs _packet)
        {
            #if (ASL_DEBUG)
            string data = System.Text.Encoding.Default.GetString(_packet.Data);
            Debug.Log($"[server-sent] OnDataReceived - Sender: {_packet.Sender} OpCode: {_packet.OpCode} data: {data.ToString()}");
            #endif
            switch (_packet.OpCode)
            {
                case (int)OpCode.PlayerLoggedIn: //Auto packet sent by GameLift
                    break;
                case (int)OpCode.PlayerJoinedMatch:
#if UNITY_ANDROID
                    QForMainThread(StartPacketStream);
#endif
                    QForMainThread(m_LobbyManager.PlayerJoinedMatch, _packet);
                    break;
                case (int)OpCode.AddPlayerToLobbyUI:
                    QForMainThread(m_LobbyManager.AddPlayerToMatch, _packet);
                    break;
                case (int)OpCode.PlayerDisconnected:
                    QForMainThread(RemovePlayerFromList, _packet);
                    break;
                case (int)OpCode.PlayerDisconnectedBeforeMatchStart:
                    QForMainThread(m_LobbyManager.AllowReadyUp);
                    break;
                case (int)OpCode.AllPlayersReady:
                    QForMainThread(m_LobbyManager.LockSession);
                    QForMainThread(DestroyLobbyManager);
                    QForMainThread(m_SceneLoader.LoadScene, _packet);
                    break;
                case (int)OpCode.LaunchScene:
                    QForMainThread(m_SceneLoader.LaunchScene);
                    break;
                case (int)OpCode.LoadScene:
                    QForMainThread(m_SceneLoader.LoadScene, _packet);
                    break;
                case (int)OpCode.ServerSetId:
                    QForMainThread(m_GameController.SetObjectID, _packet);
                    break;
                case (int)OpCode.Claim:
                    QForMainThread(m_GameController.SetObjectClaim, _packet);
                    break;
                case (int)OpCode.ReleaseClaimToPlayer:
                    QForMainThread(m_GameController.ReleaseClaimedObject, _packet);
                    break;
                case (int)OpCode.ClaimFromPlayer:
                    QForMainThread(m_GameController.ObjectClaimReceived, _packet);
                    break;
                case (int)OpCode.RejectClaim:
                    QForMainThread(m_GameController.RejectClaim, _packet);
                    break;
                case (int)OpCode.SetObjectColor:
                    QForMainThread(m_GameController.SetObjectColor, _packet);
                    break;
                case (int)OpCode.DeleteObject:
                    QForMainThread(m_GameController.DeleteObject, _packet);
                    break;
                case (int)OpCode.SetLocalPosition:
                    QForMainThread(m_GameController.SetLocalPosition, _packet);
                    break;
                case (int)OpCode.IncrementLocalPosition:
                    QForMainThread(m_GameController.IncrementLocalPosition, _packet);
                    break;
                case (int)OpCode.SetLocalRotation:
                    QForMainThread(m_GameController.SetLocalRotation, _packet);
                    break;
                case (int)OpCode.IncrementLocalRotation:
                    QForMainThread(m_GameController.IncrementLocalRotation, _packet);
                    break;
                case (int)OpCode.SetLocalScale:
                    QForMainThread(m_GameController.SetLocalScale, _packet);
                    break;
                case (int)OpCode.IncrementLocalScale:
                    QForMainThread(m_GameController.IncrementLocalScale, _packet);
                    break;
                case (int)OpCode.SetWorldPosition:
                    QForMainThread(m_GameController.SetWorldPosition, _packet);
                    break;
                case (int)OpCode.IncrementWorldPosition:
                    QForMainThread(m_GameController.IncrementWorldPosition, _packet);
                    break;
                case (int)OpCode.SetWorldRotation:
                    QForMainThread(m_GameController.SetWorldRotation, _packet);
                    break;
                case (int)OpCode.IncrementWorldRotation:
                    QForMainThread(m_GameController.IncrementWorldRotation, _packet);
                    break;
                case (int)OpCode.SetWorldScale:
                    QForMainThread(m_GameController.SetWorldScale, _packet);
                    break;
                case (int)OpCode.IncrementWorldScale:
                    QForMainThread(m_GameController.IncrementWorldScale, _packet);
                    break;
                case (int)OpCode.SpawnPrefab:
                    QForMainThread(m_GameController.SpawnPrefab, _packet);
                    break;
                case (int)OpCode.SpawnPrimitive:
                    QForMainThread(m_GameController.SpawnPrimitive, _packet);
                    break;
                case (int)OpCode.SendFloats:
                    QForMainThread(m_GameController.SentFloats, _packet);
                    break;
                case (int)OpCode.SendTexture2D:
                    QForMainThread(m_GameController.RecieveTexture2D, _packet);
                        break;
                case (int)OpCode.ResolveAnchorId:
                    QForMainThread(m_GameController.ResolveAnchorId, _packet);
                    break;
                case (int)OpCode.ResolvedCloudAnchor:
                    QForMainThread(m_GameController.AllClientsFinishedResolvingCloudAnchor, _packet);
                    break;
                case (int)OpCode.AnchorIDUpdate:
                    QForMainThread(m_GameController.SetAnchorID, _packet);
                    break;
                case (int)OpCode.TagUpdate:
                    QForMainThread(m_GameController.SetObjectTag, _packet);
                    break;
                case (int)OpCode.LobbyTextMessage:
                    QForMainThread(m_LobbyManager.UpdateChatLog, _packet);
                    break;
                default:
                    Debug.LogError("Unassigned OpCode received: " + _packet.OpCode);
                    break;
            }
        }

        /// <summary>
        /// Destroys the LobbyManager internal class to free up space after a game is started
        /// </summary>
        private void DestroyLobbyManager()
        {
            m_LobbyManager = null;
        }

        /// <summary>
        /// Adds a player to the player list for this session
        /// </summary>
        /// <param name="_peerId">The peerId of the player to be added. Is a unique number that originally started at 1.</param>
        /// <param name="_username">The username of the player to be added</param>
        private void AddPlayerToList(int _peerId, string _username)
        {
            m_Players.Add(_peerId, _username);
        }

        /// <summary>
        /// Removes a player from the player list for this session. Happens when a player disconnects
        /// </summary>
        /// <param name="_packet">The packet that was received from the server</param>
        private void RemovePlayerFromList(DataReceivedEventArgs _packet)
        {
            string data = Encoding.Default.GetString(_packet.Data);
            m_Players.Remove(int.Parse(data));
            m_LobbyManager?.UpdateLobbyScreen();
        }

        /// <summary>
        /// Creates an RealTime message to be sent to other users
        /// </summary>
        /// <param name="_opCode">The OpCode to be used to communicate what packet this is</param>
        /// <param name="_payload">The byte array containing the information to be transmitted in this message</param>
        /// <param name="_deliveryIntent">How this message should be sent. The default is reliable (TCP)</param>
        /// <param name="_targetGroup">The target group to send this message to. The default is the group for all users</param>
        /// <param name="_targetPlayer">The target player to send this message to. The default is the server where it is then intercepted and sent to all players reliably. </param>
        /// <returns></returns>
        public RTMessage CreateRTMessage(OpCode _opCode, byte[] _payload, DeliveryIntent _deliveryIntent = DeliveryIntent.Reliable, int _targetGroup = -1, int _targetPlayer = -1)
        {
            RTMessage message = m_Client.NewMessage((int)_opCode);
            message.WithPayload(_payload);
            message.WithDeliveryIntent(_deliveryIntent);
            if (_targetGroup == -1) { _targetGroup = m_GroupId; } //Default group is all
            message.WithTargetGroup(_targetGroup); //Default group is every user
            if (_targetPlayer == -1) { _targetPlayer = m_ServerId; } //Default player is server -> thus, don't specify player if not changed (if -1)
            message.WithTargetPlayer(_targetPlayer); //Default player is the server
            return message;
        }

        //Functions dealing with converting variable types to byte arrays and combining byte arrays
        #region Byte[] Conversions and Combinations

        // It should be noted that the order in which a packet is created matters.
        // The order should be as follows so that it can be decoded properly. Amount of data pieces (e.g., 2), the lengths in bytes of these data pieces (e.g., 4,4), 
        // and the data themselves converted to byte format (e.g., 4,4). The Combing functions found here automatically add this meta data for you.
        // If you follow how ASLObject functions already create these data packets then you will be following 
        // the correct formatting. The important thing to remember is that however you encode a packet, you must remember to decode in the same manner.

        /// <summary>
        /// Converts a Vector4 variable into a byte array
        /// </summary>
        /// <param name="_vector4">The Vector4 to convert</param>
        /// <returns>A byte array representing a Vector4 variable</returns>
        public byte[] ConvertVector4ToByteArray(Vector4 _vector4)
        {
            float[] vectorInFloatFormat = new float[4] { _vector4.x, _vector4.y, _vector4.z, _vector4.w };
            byte[] bytes = new byte[vectorInFloatFormat.Length * sizeof(float)];

            Buffer.BlockCopy(BitConverter.GetBytes(_vector4.x), 0, bytes, 0 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(_vector4.y), 0, bytes, 1 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(_vector4.z), 0, bytes, 2 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(_vector4.w), 0, bytes, 3 * sizeof(float), sizeof(float));

            return bytes;
        }

        /// <summary>
        /// Converts an int into a byte array
        /// </summary>
        /// <param name="_int">The int to be converted into a byte array</param>
        /// <returns>A byte array representing an int</returns>
        public byte[] ConvertIntToByteArray(int _int)
        {
            byte[] bytes = BitConverter.GetBytes(_int);
            return bytes;
        }

        /// <summary>
        /// Converts a bool into a byte array
        /// </summary>
        /// <param name="_bool">The bool to convert into a byte array</param>
        /// <returns>A byte array representing a boolean value</returns>
        public byte[] ConvertBoolToByteArray(bool _bool)
        {
            return BitConverter.GetBytes(_bool);
        }

        /// <summary>
        /// Converts a vector3 variable into a byte array
        /// </summary>
        /// <param name="_vector3">The vector 3 to convert</param>
        /// <returns>A byte array representing a vector3</returns>
        public byte[] ConvertVector3ToByteArray(Vector3 _vector3)
        {
            float[] vectorInFloatFormat = new float[3] { _vector3.x, _vector3.y, _vector3.z };
            byte[] bytes = new byte[vectorInFloatFormat.Length * sizeof(float)];

            Buffer.BlockCopy(BitConverter.GetBytes(_vector3.x), 0, bytes, 0 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(_vector3.y), 0, bytes, 1 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(_vector3.z), 0, bytes, 2 * sizeof(float), sizeof(float));

            return bytes;
        }

        /// <summary>
        /// Converts a float array into a byte array
        /// </summary>
        /// <param name="_floats">The float array to convert</param>
        /// <returns>A byte array representing the floats passed in</returns>
        public byte[] ConvertFloatArrayToByteArray(float[] _floats)
        {
            byte[] bytes = new byte[_floats.Length * sizeof(float)];
            Buffer.BlockCopy(_floats, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        /// <summary>
        /// Splits up a single byte array, returning only the section requested
        /// </summary>
        /// <param name="_byteArray">The byte array to be sliced</param>
        /// <param name="_startLocation">The location on the byte array in which to start from</param>
        /// <param name="_length">How long of a slice you want from the byte array</param>
        /// <returns>A section of the original byte array</returns>
        public byte[] SpiltByteArray(byte[] _byteArray, int _startLocation, int _length)
        {
            byte[] newByteArray = new byte[_length];
            Buffer.BlockCopy(_byteArray, _startLocation, newByteArray, 0, _length);
            return newByteArray;
        }

        /// <summary>
        /// Combines byte arrays so they can be sent as one byte array to other users
        /// </summary>
        /// <param name="_first">The first byte array</param>
        /// <param name="_second">The second byte array</param>
        /// <returns>A single byte array containing just the original byte arrays</returns>
        public byte[] CombineByteArrayWithoutLengths(byte[] _first, byte[] _second)
        {

            byte[] combinedResults = new byte[_first.Length + _second.Length];

            Buffer.BlockCopy(_first, 0, combinedResults, 0, _first.Length);
            Buffer.BlockCopy(_second, 0, combinedResults, _first.Length, _second.Length);

            return combinedResults;
        }

        /// <summary>
        /// Combines byte arrays so they can be sent as one byte array to other users
        /// </summary>
        /// <param name="_first">The first byte array</param>
        /// <param name="_second">The second byte array</param>
        /// <returns>A single byte array containing the original byte arrays and length information about them so that they can be properly decoded when received by users</returns>
        public byte[] CombineByteArrays(byte[] _first, byte[] _second)
        {
            byte[] count = BitConverter.GetBytes(2);
            byte[] firstLength = BitConverter.GetBytes(_first.Length);
            byte[] secondLength = BitConverter.GetBytes(_second.Length);

            byte[] combinedResults = new byte[count.Length + firstLength.Length + secondLength.Length + _first.Length + _second.Length];

            Buffer.BlockCopy(count, 0, combinedResults, 0, count.Length);
            Buffer.BlockCopy(firstLength, 0, combinedResults, count.Length, firstLength.Length);
            Buffer.BlockCopy(secondLength, 0, combinedResults, count.Length + firstLength.Length, secondLength.Length);
            Buffer.BlockCopy(_first, 0, combinedResults, count.Length + firstLength.Length + secondLength.Length, _first.Length);
            Buffer.BlockCopy(_second, 0, combinedResults, count.Length + firstLength.Length + secondLength.Length + _first.Length, _second.Length);

            return combinedResults;
        }

        /// <summary>
        /// Combines byte arrays so they can be sent as one byte array to other users
        /// </summary>
        /// <param name="_first">The first byte array</param>
        /// <param name="_second">The second byte array</param>
        /// <param name="_third">The third byte array</param>
        /// <returns>A single byte array containing the original byte arrays and length information about them so that they can be properly decoded when received by users</returns>
        public byte[] CombineByteArrays(byte[] _first, byte[] _second, byte[] _third)
        {
            byte[] count = BitConverter.GetBytes(3);
            byte[] firstLength = BitConverter.GetBytes(_first.Length);
            byte[] secondLength = BitConverter.GetBytes(_second.Length);
            byte[] thirdLength = BitConverter.GetBytes(_third.Length);

            byte[] combinedResults = new byte[count.Length + firstLength.Length + secondLength.Length + thirdLength.Length + _first.Length + _second.Length + _third.Length];

            Buffer.BlockCopy(count, 0, combinedResults, 0, count.Length);
            Buffer.BlockCopy(firstLength, 0, combinedResults, count.Length, firstLength.Length);
            Buffer.BlockCopy(secondLength, 0, combinedResults, count.Length + firstLength.Length, secondLength.Length);
            Buffer.BlockCopy(thirdLength, 0, combinedResults, count.Length + firstLength.Length + secondLength.Length, thirdLength.Length);
            Buffer.BlockCopy(_first, 0, combinedResults, count.Length + firstLength.Length + secondLength.Length + thirdLength.Length, _first.Length);
            Buffer.BlockCopy(_second, 0, combinedResults, count.Length + firstLength.Length + secondLength.Length + thirdLength.Length + _first.Length, _second.Length);
            Buffer.BlockCopy(_third, 0, combinedResults, count.Length + firstLength.Length + secondLength.Length + thirdLength.Length + _first.Length + _second.Length, _third.Length);

            return combinedResults;
        }

        /// <summary>
        /// Combines byte arrays so they can be sent as one byte array to other users
        /// </summary>
        /// <param name="_first">The first byte array</param>
        /// <param name="_second">The second byte array</param>
        /// <param name="_third">The third byte array</param>
        /// <param name="_fourth">The fourth byte array</param>
        /// <returns>A single byte array containing the original byte arrays and length information about them so that they can be properly decoded when received by users</returns>
        public byte[] CombineByteArrays(byte[] _first, byte[] _second, byte[] _third, byte[] _fourth)
        {
            byte[] count = BitConverter.GetBytes(4);
            byte[] firstLength = BitConverter.GetBytes(_first.Length);
            byte[] secondLength = BitConverter.GetBytes(_second.Length);
            byte[] thirdLength = BitConverter.GetBytes(_third.Length);
            byte[] fourthLength = BitConverter.GetBytes(_fourth.Length);

            byte[] combinedResults = new byte[count.Length + firstLength.Length + secondLength.Length + thirdLength.Length + fourthLength.Length
                                               + _first.Length + _second.Length + _third.Length + _fourth.Length];

            Buffer.BlockCopy(count, 0, combinedResults, 0, count.Length);
            Buffer.BlockCopy(firstLength, 0, combinedResults, count.Length, firstLength.Length);
            Buffer.BlockCopy(secondLength, 0, combinedResults, count.Length + firstLength.Length, secondLength.Length);
            Buffer.BlockCopy(thirdLength, 0, combinedResults, count.Length + firstLength.Length + secondLength.Length, thirdLength.Length);
            Buffer.BlockCopy(fourthLength, 0, combinedResults, count.Length + firstLength.Length + secondLength.Length + thirdLength.Length, fourthLength.Length);
            Buffer.BlockCopy(_first, 0, combinedResults, count.Length + firstLength.Length + secondLength.Length + thirdLength.Length + fourthLength.Length, _first.Length);
            Buffer.BlockCopy(_second, 0, combinedResults, count.Length + firstLength.Length + secondLength.Length + thirdLength.Length + fourthLength.Length + _first.Length, _second.Length);
            Buffer.BlockCopy(_third, 0, combinedResults, count.Length + firstLength.Length + secondLength.Length + thirdLength.Length + fourthLength.Length 
                                + _first.Length + _second.Length, _third.Length);
            Buffer.BlockCopy(_fourth, 0, combinedResults, count.Length + firstLength.Length + secondLength.Length + thirdLength.Length + fourthLength.Length 
                                + _first.Length + _second.Length + _third.Length, _fourth.Length);

            return combinedResults;
        }

        /// <summary>
        /// Combines byte arrays so they can be sent as one byte array to other users
        /// </summary>
        /// <param name="_first">The first byte array</param>
        /// <param name="_second">The second byte array</param>
        /// <param name="_third">The third byte array</param>
        /// <param name="_fourth">The fourth byte array</param>
        /// <param name="_fifth">The fifth byte array</param>
        /// <returns>A single byte array containing the original byte arrays and length information about them so that they can be properly decoded when received by users</returns>
        public byte[] CombineByteArrays(byte[] _first, byte[] _second, byte[] _third, byte[] _fourth, byte[] _fifth)
        {
            byte[] count = BitConverter.GetBytes(5);
            byte[] firstLength = BitConverter.GetBytes(_first.Length);
            byte[] secondLength = BitConverter.GetBytes(_second.Length);
            byte[] thirdLength = BitConverter.GetBytes(_third.Length);
            byte[] fourthLength = BitConverter.GetBytes(_fourth.Length);
            byte[] fifthLength = BitConverter.GetBytes(_fifth.Length);

            byte[] combinedResults = new byte[count.Length + firstLength.Length + secondLength.Length + thirdLength.Length + fourthLength.Length + fifthLength.Length
                                               + _first.Length + _second.Length + _third.Length + _fourth.Length + _fifth.Length];

            Buffer.BlockCopy(count, 0, combinedResults, 0, count.Length);
            Buffer.BlockCopy(firstLength, 0, combinedResults, count.Length, firstLength.Length);
            Buffer.BlockCopy(secondLength, 0, combinedResults, count.Length + firstLength.Length, secondLength.Length);
            Buffer.BlockCopy(thirdLength, 0, combinedResults, count.Length + firstLength.Length + secondLength.Length, thirdLength.Length);
            Buffer.BlockCopy(fourthLength, 0, combinedResults, count.Length + firstLength.Length + secondLength.Length + thirdLength.Length, fourthLength.Length);
            Buffer.BlockCopy(fifthLength, 0, combinedResults, count.Length + firstLength.Length + secondLength.Length + thirdLength.Length + fourthLength.Length, fifthLength.Length);
            Buffer.BlockCopy(_first, 0, combinedResults, count.Length + firstLength.Length + secondLength.Length + thirdLength.Length + fourthLength.Length + fifthLength.Length, _first.Length);
            Buffer.BlockCopy(_second, 0, combinedResults, count.Length + firstLength.Length + secondLength.Length + 
                                thirdLength.Length + fourthLength.Length + fifthLength.Length + _first.Length, _second.Length);
            Buffer.BlockCopy(_third, 0, combinedResults, count.Length + firstLength.Length + secondLength.Length +
                                thirdLength.Length + fourthLength.Length + fifthLength.Length + _first.Length + _second.Length, _third.Length);
            Buffer.BlockCopy(_fourth, 0, combinedResults, count.Length + firstLength.Length + secondLength.Length +
                                thirdLength.Length + fourthLength.Length + fifthLength.Length + _first.Length + _second.Length + _third.Length, _fourth.Length);
            Buffer.BlockCopy(_fifth, 0, combinedResults, count.Length + firstLength.Length + secondLength.Length +
                                thirdLength.Length + fourthLength.Length + fifthLength.Length + _first.Length + _second.Length + _third.Length + _fourth.Length, _fifth.Length);

            return combinedResults;
        }

        /// <summary>
        /// Combines byte arrays so they can be sent as one byte array to other users
        /// </summary>
        /// <param name="_first">The first byte array</param>
        /// <param name="_second">The second byte array</param>
        /// <param name="_third">The third byte array</param>
        /// <param name="_fourth">The fourth byte array</param>
        /// <param name="_fifth">The fifth byte array</param>
        /// <param name="_sixth">The sixth byte array</param>
        /// <param name="_seventh">The seventh byte array</param>
        /// <param name="_eighth">The eighth byte array</param>
        /// <param name="_ninth">The ninth byte array</param>
        /// <param name="_tenth">The tenth byte array</param>
        /// <param name="_eleventh">The eleventh byte array</param>
        /// <param name="_twelfth">The twelfth byte array</param>
        /// <param name="_thirteenth">The thirteenth byte array</param>
        /// <returns>A single byte array containing the original byte arrays and length information about them so that they can be properly decoded when received by users</returns>
        public byte[] CombineByteArrays(byte[] _first, byte[] _second, byte[] _third, byte[] _fourth, byte[] _fifth, byte[] _sixth, byte[] _seventh, byte[] _eighth, byte[] _ninth, byte[] _tenth,
            byte[] _eleventh, byte[] _twelfth, byte[] _thirteenth)
        {
            byte[] count = BitConverter.GetBytes(13);
            byte[] firstLength = BitConverter.GetBytes(_first.Length);
            byte[] secondLength = BitConverter.GetBytes(_second.Length);
            byte[] thirdLength = BitConverter.GetBytes(_third.Length);
            byte[] fourthLength = BitConverter.GetBytes(_fourth.Length);
            byte[] fifthLength = BitConverter.GetBytes(_fifth.Length);
            byte[] sixthLength = BitConverter.GetBytes(_sixth.Length);
            byte[] seventhLength = BitConverter.GetBytes(_seventh.Length);
            byte[] eighthLength = BitConverter.GetBytes(_eighth.Length);
            byte[] ninthLength = BitConverter.GetBytes(_ninth.Length);
            byte[] tenthLength = BitConverter.GetBytes(_tenth.Length);
            byte[] evelethLength = BitConverter.GetBytes(_eleventh.Length);
            byte[] twelfthLength = BitConverter.GetBytes(_twelfth.Length);
            byte[] thirteenthLength = BitConverter.GetBytes(_thirteenth.Length);

            byte[] combinedResults = new byte[count.Length + firstLength.Length + secondLength.Length + thirdLength.Length +
                                        fourthLength.Length + fifthLength.Length + sixthLength.Length + seventhLength.Length +
                                        eighthLength.Length + ninthLength.Length + tenthLength.Length + evelethLength.Length + 
                                        twelfthLength.Length + thirteenthLength.Length + _first.Length + _second.Length + _third.Length + 
                                        _fourth.Length + _fifth.Length + _sixth.Length + _seventh.Length + _eighth.Length + _ninth.Length +
                                         _tenth.Length + _eleventh.Length + _twelfth.Length + _thirteenth.Length];

            int offset = 0;

            Buffer.BlockCopy(count, 0, combinedResults, 0, count.Length);
            offset += count.Length;
            Buffer.BlockCopy(firstLength, 0, combinedResults, offset, firstLength.Length);
            offset += firstLength.Length;
            Buffer.BlockCopy(secondLength, 0, combinedResults, offset, secondLength.Length);
            offset += secondLength.Length;
            Buffer.BlockCopy(thirdLength, 0, combinedResults, offset, thirdLength.Length);
            offset += thirdLength.Length;
            Buffer.BlockCopy(fourthLength, 0, combinedResults, offset, fourthLength.Length);
            offset += fourthLength.Length;
            Buffer.BlockCopy(fifthLength, 0, combinedResults, offset, fifthLength.Length);
            offset += fifthLength.Length;
            Buffer.BlockCopy(sixthLength, 0, combinedResults, offset, sixthLength.Length);
            offset += sixthLength.Length;
            Buffer.BlockCopy(seventhLength, 0, combinedResults, offset, seventhLength.Length);
            offset += seventhLength.Length;
            Buffer.BlockCopy(eighthLength, 0, combinedResults, offset, eighthLength.Length);
            offset += eighthLength.Length;
            Buffer.BlockCopy(ninthLength, 0, combinedResults, offset, ninthLength.Length);
            offset += ninthLength.Length;
            Buffer.BlockCopy(tenthLength, 0, combinedResults, offset, tenthLength.Length);
            offset += tenthLength.Length;
            Buffer.BlockCopy(evelethLength, 0, combinedResults, offset, evelethLength.Length);
            offset += evelethLength.Length;
            Buffer.BlockCopy(twelfthLength, 0, combinedResults, offset, twelfthLength.Length);
            offset += twelfthLength.Length;
            Buffer.BlockCopy(thirteenthLength, 0, combinedResults, offset, thirteenthLength.Length);
            offset += thirteenthLength.Length;
            Buffer.BlockCopy(_first, 0, combinedResults, offset, _first.Length);
            offset += _first.Length;
            Buffer.BlockCopy(_second, 0, combinedResults, offset, _second.Length);
            offset += _second.Length;
            Buffer.BlockCopy(_third, 0, combinedResults, offset, _third.Length);
            offset += _third.Length;
            Buffer.BlockCopy(_fourth, 0, combinedResults, offset, _fourth.Length);
            offset += _fourth.Length;
            Buffer.BlockCopy(_fifth, 0, combinedResults, offset, _fifth.Length);
            offset += _fifth.Length;
            Buffer.BlockCopy(_sixth, 0, combinedResults, offset, _sixth.Length);
            offset += _sixth.Length;
            Buffer.BlockCopy(_seventh, 0, combinedResults, offset, _seventh.Length);
            offset += _seventh.Length;
            Buffer.BlockCopy(_eighth, 0, combinedResults, offset, _eighth.Length);
            offset += _eighth.Length;
            Buffer.BlockCopy(_ninth, 0, combinedResults, offset, _ninth.Length);
            offset += _ninth.Length;
            Buffer.BlockCopy(_tenth, 0, combinedResults, offset, _tenth.Length);
            offset += _tenth.Length;
            Buffer.BlockCopy(_eleventh, 0, combinedResults, offset, _eleventh.Length);
            offset += _eleventh.Length;
            Buffer.BlockCopy(_twelfth, 0, combinedResults, offset, _twelfth.Length);
            offset += _twelfth.Length;
            Buffer.BlockCopy(_thirteenth, 0, combinedResults, offset, _thirteenth.Length);

            return combinedResults;
        }

        #endregion

        //As AWS runs on a different thread, but Unity is single threaded, this is how we ensure that any information on the AWS thread is communicated to Unity thread
        #region QForMainThreadSection

        /// <summary>
        /// Queue a function to be called on the Unity thread that contains no parameters
        /// </summary>
        /// <param name="fn">The name of the function to be called</param>
        private void QForMainThread(Action fn)
        {
            lock (m_MainThreadQueue)
            {
                m_MainThreadQueue.Enqueue(() => { fn(); });
            }
        }

        /// <summary>
        /// Queue a function to be called on the Unity thread that contains 1 parameter
        /// </summary>
        /// <typeparam name="T1">The type of function to be called</typeparam>
        /// <param name="fn">The name of the function to be called</param>
        /// <param name="p1">The first parameter of the function to be called</param>
        private void QForMainThread<T1>(Action<T1> fn, T1 p1)
        {
            lock (m_MainThreadQueue)
            {
                m_MainThreadQueue.Enqueue(() => { fn(p1); });
            }
        }

        /// <summary>
        /// Queue a function to be called on the Unity thread that contains 2 parameters
        /// </summary>
        /// <typeparam name="T1">The type of function to be called</typeparam>
        /// <typeparam name="T2">The type of function to be called</typeparam>
        /// <param name="fn">The name of the function to be called</param>
        /// <param name="p1">The first parameter of the function to be called</param>
        /// <param name="p2">The second parameter of the function to be called</param>
        private void QForMainThread<T1, T2>(Action<T1, T2> fn, T1 p1, T2 p2)
        {
            lock (m_MainThreadQueue)
            {
                m_MainThreadQueue.Enqueue(() => { fn(p1, p2); });
            }
        }

        /// <summary>
        /// Queue a function to be called on the Unity thread that contains 3 parameters
        /// </summary>
        /// <typeparam name="T1">The type of function to be called</typeparam>
        /// <typeparam name="T2">The type of function to be called</typeparam>
        /// <typeparam name="T3">The type of function to be called</typeparam>
        /// <param name="fn">The name of the function to be called</param>
        /// <param name="p1">The first parameter of the function to be called</param>
        /// <param name="p2">The second parameter of the function to be called</param>
        /// <param name="p3">The third parameter of the function to be called</param>
        private void QForMainThread<T1, T2, T3>(Action<T1, T2, T3> fn, T1 p1, T2 p2, T3 p3)
        {
            lock (m_MainThreadQueue)
            {
                m_MainThreadQueue.Enqueue(() => { fn(p1, p2, p3); });
            }
        }

        /// <summary>
        /// Queue a function to be called on the Unity thread that contains 4 parameters
        /// </summary>
        /// <typeparam name="T1">The type of function to be called</typeparam>
        /// <typeparam name="T2">The type of function to be called</typeparam>
        /// <typeparam name="T3">The type of function to be called</typeparam>
        /// <typeparam name="T4">The type of function to be called</typeparam>
        /// <param name="fn">The name of the function to be called</param>
        /// <param name="p1">The first parameter of the function to be called</param>
        /// <param name="p2">The second parameter of the function to be called</param>
        /// <param name="p3">The third parameter of the function to be called</param>
        /// <param name="p4">The fourth parameter of the function to be called</param>
        private void QForMainThread<T1, T2, T3, T4>(Action<T1, T2, T3, T4> fn, T1 p1, T2 p2, T3 p3, T4 p4)
        {
            lock (m_MainThreadQueue)
            {
                m_MainThreadQueue.Enqueue(() => { fn(p1, p2, p3, p4); });
            }
        }

        /// <summary>
        /// Queue a function to be called on the Unity thread that contains 5 parameters
        /// </summary>
        /// <typeparam name="T1">The type of function to be called</typeparam>
        /// <typeparam name="T2">The type of function to be called</typeparam>
        /// <typeparam name="T3">The type of function to be called</typeparam>
        /// <typeparam name="T4">The type of function to be called</typeparam>
        /// <typeparam name="T5">The type of function to be called</typeparam>
        /// <param name="fn">The name of the function to be called</param>
        /// <param name="p1">The first parameter of the function to be called</param>
        /// <param name="p2">The second parameter of the function to be called</param>
        /// <param name="p3">The third parameter of the function to be called</param>
        /// <param name="p4">The fourth parameter of the function to be called</param>
        /// <param name="p5">The fifth parameter of the function to be called</param>
        private void QForMainThread<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> fn, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
        {
            lock (m_MainThreadQueue)
            {
                m_MainThreadQueue.Enqueue(() => { fn(p1, p2, p3, p4, p5); });
            }
        }

        /// <summary>
        /// Queue a function to be called on the Unity thread that contains 6 parameters
        /// </summary>
        /// <typeparam name="T1">The type of function to be called</typeparam>
        /// <typeparam name="T2">The type of function to be called</typeparam>
        /// <typeparam name="T3">The type of function to be called</typeparam>
        /// <typeparam name="T4">The type of function to be called</typeparam>
        /// <typeparam name="T5">The type of function to be called</typeparam>
        /// <typeparam name="T6">The type of function to be called</typeparam>
        /// <param name="fn">The name of the function to be called</param>
        /// <param name="p1">The first parameter of the function to be called</param>
        /// <param name="p2">The second parameter of the function to be called</param>
        /// <param name="p3">The third parameter of the function to be called</param>
        /// <param name="p4">The fourth parameter of the function to be called</param>
        /// <param name="p5">The fifth parameter of the function to be called</param>
        /// <param name="p6">The sixth parameter of the function to be called</param>
        private void QForMainThread<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> fn, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6)
        {
            lock (m_MainThreadQueue)
            {
                m_MainThreadQueue.Enqueue(() => { fn(p1, p2, p3, p4, p5, p6); });
            }
        }

        /// <summary>
        /// Locks and executes any functions that have been added to the QForMainThread queue. Is continuously called from this class's Update function
        /// </summary>
        private void RunMainThreadQueueActions()
        {
            // as our server messages come in on their own thread
            // we need to queue them up and run them on the main thread
            // when the methods need to interact with Unity
            lock (m_MainThreadQueue)
            {
                while (m_MainThreadQueue.Count > 0)
                {
                    m_MainThreadQueue.Dequeue().Invoke();
                }
            }
        }

        #endregion

        /// <summary>
        /// Disconnects the user from the GameLift servers if they are connected
        /// </summary>
        public void DisconnectFromServer()
        {
            if (m_Client != null && m_Client.Connected)
            {
                m_Client.Disconnect();
            }
        }

        /// <summary>
        /// Called when an application quits, ensuring the user cleanly exits the GameLift server
        /// </summary>
        private void OnApplicationQuit()
        {
            DisconnectFromServer();            
        }

        /// <summary>
        /// Used to make sure Android devices can disconnect from the GameLift servers. This function will quit the application when it loses focus
        /// e.g., when you exit the app to the home screen. Doing so is a good thing and prevents hanging connections.
        /// </summary>
        /// <param name="_isPaused">flag representing if the app is paused or not</param>
        private void OnApplicationPause(bool _isPaused)
        {
#if UNITY_ANDROID || UNITY_WSA
            if (_isPaused) //If we exit the app but don't force quit - e.g., go to the home screen
            {
                Application.Quit(); //Then quit the application, which calls our disconnect function
            }
#endif
        }

        /// <summary>
        /// Starts the coroutine that will send an empty packet to the relay server every second to help maintain the Android connection
        /// </summary>
        private void StartPacketStream()
        {
            if (!m_StreamActive)
            {
                m_StreamActive = true;
                StartCoroutine(KeepConnectionAlive());
            }
        }

        /// <summary>
        /// Sends an empty packet to the server to help maintain the android socket
        /// </summary>
        /// <returns>Waits for 1 second before sending another packet</returns>
        private IEnumerator KeepConnectionAlive()
        {
            while (m_Client.ConnectedAndReady)
            {
                yield return new WaitForSeconds(1);
                RTMessage message = CreateRTMessage(OpCode.AndroidKeepConnectionAlive, null, DeliveryIntent.Fast); 
                m_Client.SendMessage(message);
            }
            while (!m_Client.ConnectedAndReady)
            {
                yield return new WaitForSeconds(1);
            }
            StartCoroutine(KeepConnectionAlive());
        }

        /// <summary>Returns the current lowest peerID value out of all the currently connected players</summary>
        /// <returns>The lowest peer id of all the users in this match</returns>
        public int GetLowestPeerId()
        {
            int lowestPeerId = int.MaxValue;
            foreach (KeyValuePair<int, string> _aPlayer in m_Players)
            {
                if (lowestPeerId > _aPlayer.Key)
                {
                    lowestPeerId = _aPlayer.Key;
                }
            }
            return lowestPeerId;
        }

        /// <summary>
        /// Returns true if the caller is the lowest peer id user in the match. This is a good way to assign a "Host" player if desired.
        /// Though do keep in mind that ASL is a P2P network.
        /// </summary>
        /// <returns>True if caller has the lowest peer id</returns>
        public bool AmLowestPeer()
        {
            int currentLowest = GetLowestPeerId();
            if (currentLowest == m_PeerId)
            {
                return true;
            }
            return false;
        }

    }
}
