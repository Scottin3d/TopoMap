//Used for help debug GameLift connection issues and other misc. GameLift potential problems. Uncomment to turn on
#define ASL_DEBUG
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Aws.GameLift.Realtime.Event;
using Aws.GameLift.Realtime;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Net.NetworkInformation;
using System.Linq;
using Aws.GameLift.Realtime.Command;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using UnityEditor;
using System.Net.Sockets;
using Aws.GameLift.Realtime.Types;

namespace ASL
{
    public partial class GameLiftManager
    {
        /// <summary>
        /// Internal class that is used to connect users to each other
        /// </summary>
        private class LobbyManager
        {
            #region Private Variables
            //Back button so user can return the previous area

            /// <summary>The back button used to go to the previous lobby screen</summary>
            private Button m_BackButton;

            //Informational text for user that appears in the bottom left of the screen

            /// <summary>The name of the user</summary>
            private Text m_UsernameText;

            /// <summary>The connection status</summary>
            private Text m_ConnectionStatusText;

            //Login screen area

            /// <summary>The login screen</summary>
            private GameObject m_LoginScreen;

            /// <summary>The username input field</summary>
            private InputField m_UsernameInputField;

            /// <summary>The login button </summary>
            private Button m_LoginButton;

            //Host/Find Session area

            /// <summary>The host or find session screen</summary>
            private GameObject m_HostOrFindSessionScreen;

            /// <summary>The host button </summary>
            private Button m_HostButton;

            /// <summary>The find button</summary>
            private Button m_FindButton;

            //Host Option Section area

            /// <summary>The Pre-host screen</summary>
            private GameObject m_PreHostScreen;

            /// <summary>The room name input field</summary>
            private InputField m_RoomNameInputField;

            /// <summary>The scenes available to host</summary>
            private Dropdown m_AvailableScenes;

            /// <summary>The start hosting button</summary>
            private Button m_StartHostingButton;

            //View Game Sessions area

            /// <summary>The available sessions screen</summary>
            private GameObject m_AvailableSessionsScreen;

            /// <summary>The available session holder</summary>
            private GameObject m_SessionsAvailableHolder;

            /// <summary>Refresh available sessions button</summary>
            private Button m_RefreshAvailableSessionsButton;

            /// <summary>The join match button</summary>
            private Button m_JoinMatchButton;

            //Lobby area

            /// <summary>The lobby screen</summary>
            private GameObject m_LobbyScreen;

            /// <summary>The match name </summary>
            private Text m_MatchNameText;

            /// <summary>The player count</summary>
            private Text m_PlayerCountText;

            /// <summary>The player list</summary>
            private Text m_PlayerListText;

            /// <summary>The chat history</summary>
            private Text m_ChatHistoryText;

            /// <summary>The chat input field</summary>
            private InputField m_ChatInputField;

            /// <summary>The send chat button</summary>
            private Button m_SendChatButton;

            /// <summary>The ready button</summary>
            private Button m_ReadyButton;

            //First scene setup screen

            /// <summary>The waiting for next scene screen</summary>
            private GameObject m_WaitingForNextSceneScreen;

            /// <summary>The default UDP port number to receive messages on</summary>
            private const int DEFAULT_UDP_PORT = 33400;

            /// <summary>The position of the found match buttons</summary>
            private Vector3 m_MatchFoundButtonPosition;

            /// <summary>The game session id used for connecting to that game session</summary>
            private string m_GameSessionId;

            /// <summary>
            /// Helps regulate when auto connect gets called - prevents multiple from happening at the same time
            /// </summary>
            private bool m_TryAutoConnectAgain = false;

            /// <summary>
            /// Used to prevent the auto connect coroutine from being activated more than once
            /// </summary>
            private bool m_StartedAutoConnect = false;

            /// <summary>The scene to load after every player readies up</summary>
            public string m_SceneName { get; private set; }

            /// <summary>The current login stage</summary>
            private enum CurrentLoginStage
            {
                /// <summary>The login screen</summary>
                Login,
                /// <summary>The host or find session screen</summary>
                HostOrFindSession,
                /// <summary>The host menu screen</summary>
                HostMenu,
                /// <summary>The available sessions screen</summary>
                AvailableSessions,
                /// <summary>The lobby screen </summary>
                LobbyScreen,
                /// <summary>The setup for the next scene screen</summary>
                Setup
            }
            /// <summary>The current UI screen being disabled</summary>
            private CurrentLoginStage m_CurrentUIScreen;

            /// <summary>
            /// Flag indicating whether or not we have initialized connecting to GameLift - used to stop the auto connect coroutine 
            /// </summary>
            private bool m_InitializingConnection = false;

            #endregion

            /// <summary>Error text displayed to the user when an error occurs</summary>
            public Text m_ErrorText;

            /// <summary>The UDP listening port number for Android devices</summary>
            private int m_AndroidOrOSXUDPListeningPort = 33400;

            /// <summary>
            /// This function starts the scene by assigning all UI elements and is manually called by GameLiftManager's Start function as 
            /// this class is not a MonoBehavior class and can't be.
            /// </summary>
            public void Start()
            {
                InitilizeUIElements();
                SetCorrectUIPanel(CurrentLoginStage.Login);
                if (QuickConnect.m_StaticQuickStart)
                {
                    m_SceneName = QuickConnect.m_StaticStartingScene;
                }
                else
                {
                    m_SceneName = "";
                }

#if UNITY_ANDROID || UNITY_STANDALONE_OSX
                CheckPorts();
#endif

            }

            /// <summary>
            /// Initializes all of the UI elements that will be used in the lobby scene
            /// </summary>
            private void InitilizeUIElements()
            {
                #region AssignVariablesToUIElementsInScene

                m_ErrorText = GameObject.Find("ErrorText").GetComponent<Text>();
                m_BackButton = GameObject.Find("BackButton").GetComponent<Button>();
                m_UsernameText = GameObject.Find("UsernameText").GetComponent<Text>();
                m_ConnectionStatusText = GameObject.Find("ConnectionStatusText").GetComponent<Text>();
                m_LoginScreen = GameObject.Find("LoginPanel");
                m_UsernameInputField = GameObject.Find("UsernameInputField").GetComponent<InputField>();
                m_LoginButton = GameObject.Find("ConnectButton").GetComponent<Button>();
                m_HostOrFindSessionScreen = GameObject.Find("HostOrFindSessionPanel");
                m_HostButton = GameObject.Find("HostSessionButton").GetComponent<Button>();
                m_FindButton = GameObject.Find("FindSessionButton").GetComponent<Button>();
                m_PreHostScreen = GameObject.Find("HostMenuPanel");
                m_RoomNameInputField = GameObject.Find("RoomNameInputField").GetComponent<InputField>();
                m_AvailableScenes = GameObject.Find("AvailableScenes").GetComponent<Dropdown>();
                m_StartHostingButton = GameObject.Find("StartHostingButton").GetComponent<Button>();
                m_AvailableSessionsScreen = GameObject.Find("AvailableSessionsPanel");
                m_SessionsAvailableHolder = GameObject.Find("FindMatchesScrollWindow");
                m_RefreshAvailableSessionsButton = GameObject.Find("RefreshButton").GetComponent<Button>();
                m_JoinMatchButton = GameObject.Find("JoinMatchButton").GetComponent<Button>();
                m_LobbyScreen = GameObject.Find("LobbyScreenPanel");
                m_MatchNameText = GameObject.Find("MatchNameText").GetComponent<Text>();
                m_PlayerCountText = GameObject.Find("PlayerCountText").GetComponent<Text>();
                m_PlayerListText = GameObject.Find("PlayerListText").GetComponent<Text>();
                m_ChatHistoryText = GameObject.Find("ChatHistoryText").GetComponent<Text>();
                m_ChatInputField = GameObject.Find("ChatInput").GetComponent<InputField>();
                m_SendChatButton = GameObject.Find("Send").GetComponent<Button>();
                m_ReadyButton = GameObject.Find("ReadyButton").GetComponent<Button>();
                m_WaitingForNextSceneScreen = GameObject.Find("SetupPanel");

                #endregion

                #region AddOnListenersToUIVariables

                m_BackButton.onClick.AddListener(() =>
                {
                    GoBack();
                });

                m_LoginButton.onClick.AddListener(() =>
                {
                    m_LoginButton.interactable = false;
                    CheckUsernameAvailability();
                });

                m_HostButton.onClick.AddListener(() =>
                {
                    ChooseToHostSession();
                });

                m_FindButton.onClick.AddListener(() =>
                {
                    ChooseToFindSessions();
                });

                m_StartHostingButton.onClick.AddListener(() =>
                {
                    m_StartHostingButton.interactable = false;
                    HostSession();
                });

                m_RefreshAvailableSessionsButton.onClick.AddListener(() =>
                {
                    ChooseToFindSessions();
                });

                m_JoinMatchButton.onClick.AddListener(() =>
                {
                    JoinSelectedMatch();
                });

                m_SendChatButton.onClick.AddListener(() =>
                {
                    RTMessage message = GetInstance().CreateRTMessage(OpCode.LobbyTextMessage, Encoding.Default.GetBytes(GetInstance().m_Username + ":" + m_ChatInputField.text));
                    GetInstance().m_Client.SendMessage(message);
                    m_ChatInputField.text = string.Empty;
                });

                m_ReadyButton.onClick.AddListener(() =>
                {
                    m_ReadyButton.interactable = false;
                    ReadyUp();
                });

                //Disable buttons on start:
                m_ReadyButton.interactable = false;

                #endregion

            }

            /// <summary>
            /// Sets the UI panel to be displayed to the user
            /// </summary>
            /// <param name="_panel">The panel to be displayed to the user</param>
            private void SetCorrectUIPanel(CurrentLoginStage _panel)
            {
                m_LoginScreen.SetActive(false);
                m_HostOrFindSessionScreen.SetActive(false);
                m_PreHostScreen.SetActive(false);
                m_AvailableSessionsScreen.SetActive(false);
                m_LobbyScreen.SetActive(false);
                m_WaitingForNextSceneScreen.SetActive(false);

                switch (_panel)
                {
                    case CurrentLoginStage.Login:
                        m_LoginScreen.SetActive(true);
                        m_ReadyButton.interactable = false;
                        break;
                    case CurrentLoginStage.HostOrFindSession:
                        m_HostOrFindSessionScreen.SetActive(true);
                        m_ReadyButton.interactable = false;
                        break;
                    case CurrentLoginStage.HostMenu:
                        m_PreHostScreen.SetActive(true);
                        GetAvailableScenes();
                        m_ReadyButton.interactable = false;
                        m_StartHostingButton.interactable = true;
                        break;
                    case CurrentLoginStage.AvailableSessions:
                        m_AvailableSessionsScreen.SetActive(true);
                        m_ReadyButton.interactable = false;
                        break;
                    case CurrentLoginStage.LobbyScreen:
                        m_LobbyScreen.SetActive(true);
                        m_ReadyButton.interactable = true;
                        break;
                    case CurrentLoginStage.Setup:
                        m_BackButton.gameObject.SetActive(false); //Remove back button as you cannot go back from here
                        m_WaitingForNextSceneScreen.SetActive(true);
                        m_ReadyButton.interactable = false;
                        break;
                    default: break;
                }
                RemoveErrorText();
                m_CurrentUIScreen = _panel;

            }

            /// <summary>
            /// Gets the available scenes that the user can host. Due to limitations on Unity's end, this function is only useful if it is being called inside the editor. 
            /// In order to see your scene when running an EXE version, you must edit the AvailableScenes dropdown menu to include (or exclude) the scenes you want.
            /// </summary>
            private void GetAvailableScenes()
            {
#if UNITY_EDITOR
                m_AvailableScenes.options.Clear();
                int availableScenes = SceneManager.sceneCountInBuildSettings;
                for (int i = 0; i < availableScenes; i++)
                {
                    string lastPartOfPath = Regex.Match(EditorBuildSettings.scenes[i].path, "\\/\\w*.unity").Value;
                    lastPartOfPath = Regex.Replace(lastPartOfPath, ".unity", "");
                    lastPartOfPath = Regex.Replace(lastPartOfPath, "\\/", "");

                    ////Don't add the lobby scene and the scene loader scenes to the drop down menu
                    if (lastPartOfPath == "ASL_LobbyScene" || lastPartOfPath == "ASL_SceneLoader")
                    {
                        continue;
                    }
                    m_AvailableScenes.options.Add(new Dropdown.OptionData(lastPartOfPath));
                }

                m_AvailableScenes.value = 0; // optional
                m_AvailableScenes.Select(); // optional
                m_AvailableScenes.RefreshShownValue(); // this is the key
#else
#endif

            }

            /// <summary>
            /// Checks if the username the user entered is currently in any game sessions and if so, does not allow that username to be used again.
            /// By default, this lambda function will also set the desired active instances of the fleet being used to 1. It will do this regardless if
            /// the instance is already at 1 or if its at 0. This is because after no users after 30 minutes, the instance auto scales down to 0.
            /// </summary>
            /// <param name="_client">The AWS client variable</param>
            /// <param name="_request">The request parameters and permissions</param>
            private async void InvokeUsernameLambda(AmazonLambdaClient _client, InvokeRequest _request)
            {
                InvokeResponse invokeResponse = null;
                try
                {
                    invokeResponse = await _client.InvokeAsync(_request);
                }
                catch (Exception _exception)
                {
                    Debug.LogError(invokeResponse.FunctionError + _exception);
                    GetInstance().QForMainThread(AddErrorText, invokeResponse.FunctionError + _exception.ToString());
                }
                if (invokeResponse != null)
                {
                    if (invokeResponse.StatusCode == 200)
                    {
                        var payload = Encoding.ASCII.GetString(invokeResponse.Payload.ToArray()) + "\n";
                        //if we found player session details then this user must be either in an active match or queued up for one. 
                        if (!payload.Contains("PlayerId")) //check if payload is empty by checking if contains PlayerId which it always will if not empty
                        {
#if ASL_DEBUG
                            Debug.Log("Username Available.");
#endif
                            GetInstance().QForMainThread(RemoveErrorText);
                            GetInstance().QForMainThread(UpdateUsername, m_UsernameInputField.text);
                            GetInstance().QForMainThread(UpdateConnectionStatusText, "Logged in.");
                            if (!QuickConnect.m_StaticQuickStart)
                            {
                                GetInstance().QForMainThread(SetCorrectUIPanel, CurrentLoginStage.HostOrFindSession);
                            }
                            else
                            {
                                GetInstance().QForMainThread(ChangeInteractablility, m_LoginButton, false);
                                GetInstance().QForMainThread(QuickConnectMatch);
                            }

                        }
                        else
                        {
#if ASL_DEBUG
                            Debug.Log("Username already in use. Try another.");
#endif
                            GetInstance().QForMainThread(AddErrorText, "Username already in use. Try another.");
                            GetInstance().QForMainThread(UpdateConnectionStatusText, "Error - Invalid username.");
                            GetInstance().QForMainThread(ChangeInteractablility, m_LoginButton, true);
                        }
                    }
                }
            }

            /// <summary>
            /// Checks if the username the user entered is currently being used by another user. If it is - then they are prompted to enter a different username.
            /// If not, they can then host or find a session to join.
            /// </summary>
            private void CheckUsernameAvailability()
            {
#if ASL_DEBUG
                Debug.Log("Checking username availability...");
#endif

                if (string.IsNullOrEmpty(m_UsernameInputField.text))
                {
                    GetInstance().QForMainThread(AddErrorText, "Username cannot be empty.");
                    GetInstance().QForMainThread(ChangeInteractablility, m_LoginButton, true);
                    return;
                }

                /***///AWSConfigs.AWSRegion = "us-west-2"; // Your region here
                     /***///AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
                          // paste this in from the Amazon Cognito Identity Pool console
                CognitoAWSCredentials credentials = new CognitoAWSCredentials(
                    "us-west-2:548832a2-6238-46c2-b6a1-6e8870549a81", // Identity pool ID
                    RegionEndpoint.USWest2 // Region
                );

                AmazonLambdaClient client = new AmazonLambdaClient(credentials, RegionEndpoint.USWest2);

                InvokeRequest request = new InvokeRequest
                {
                    FunctionName = "CheckUsernameAvailability",
                    Payload = "{\"username\" : \"" + m_UsernameInputField.text + "\"}",
                    InvocationType = InvocationType.RequestResponse
                };

                InvokeUsernameLambda(client, request);

            }

            /// <summary>
            /// Sets the UI panel to the HostMenu screen
            /// </summary>
            private void ChooseToHostSession()
            {
                SetCorrectUIPanel(CurrentLoginStage.HostMenu);
            }

            /// <summary>
            /// Sets the UI panel to the available sessions screen, destroys any old matches found, and searches for new matches
            /// </summary>
            private void ChooseToFindSessions()
            {
                DestroyMatchOptions();
                SetCorrectUIPanel(CurrentLoginStage.AvailableSessions);
                SearchForSessions();
            }

            /// <summary>
            /// Invokes the QuickConnect lambda function which looks for a game, if none found, then creates a game for the user. Allowing them to bypass the host/search for game method
            /// </summary>
            /// <param name="_client">The AWS client variable</param>
            /// <param name="_request">The request parameters and permissions</param>
            private async void InvokeQuickConnectLambda(AmazonLambdaClient _client, InvokeRequest _request)
            {
                InvokeResponse invokeResponse = null;
                try
                {
                    invokeResponse = await _client.InvokeAsync(_request);
                }
                catch (Exception _exception)
                {

#if (ASL_DEBUG)
                    Debug.LogError(invokeResponse?.FunctionError + _exception);
#endif
                    GetInstance().QForMainThread(AddErrorText, invokeResponse?.FunctionError + _exception.ToString());
                    GetInstance().QForMainThread(SetQuickConnectFlag, false); //connect old way if quick connect fails

                }
                if (invokeResponse != null)
                {
                    if (invokeResponse.StatusCode == 200)
                    {
                        var payload = Encoding.ASCII.GetString(invokeResponse.Payload.ToArray()) + "\n";
                        var playerSessionObj = JsonUtility.FromJson<PlayerSessionObject>(payload);

                        if (playerSessionObj.FleetId == null)
                        {
#if (ASL_DEBUG)
                            Debug.Log($"Error in Lambda: {payload}");
#endif

                            if (Regex.IsMatch(payload.ToString(), "FleetCapacityExceededException"))
                            {
                                GetInstance().QForMainThread(AddErrorText, "ASL currently does not have the capacity for another Game Session. Scaling up fleet to make room. " +
                                    "You will automatically be connected once the fleet is scaled up. This may take up to 5 minutes.");
                                GetInstance().QForMainThread(StartAutoConnect);
                            }
                            else
                            {
                                GetInstance().QForMainThread(AddErrorText, $"Error in Lambda: {payload}");
                                GetInstance().QForMainThread(SetQuickConnectFlag, false); //connect old way if quick connect fails                               
                            }
                            GetInstance().QForMainThread(ChangeInteractablility, m_LoginButton, true);
                        }
                        else
                        {
                            GetInstance().QForMainThread(ActionConnectToServer, playerSessionObj.DnsName, Int32.Parse(playerSessionObj.Port),
                                playerSessionObj.PlayerSessionId, playerSessionObj.GameName, playerSessionObj.GameSessionId, m_SceneName);
                        }
                    }
                }
            }

            /// <summary>
            /// Reduces the number of screens a user has to go through in order to connect to a match
            /// </summary>
            private void QuickConnectMatch()
            {
#if ASL_DEBUG
                Debug.Log("Attempting to QuickConnect a game");
#endif
                m_TryAutoConnectAgain = false;
                // paste this in from the Amazon Cognito Identity Pool console
                CognitoAWSCredentials credentials = new CognitoAWSCredentials(
                    "us-west-2:766a7439-be4a-404e-b1a9-192fc429eee2", // Identity pool ID
                    RegionEndpoint.USWest2 // Region
                );

                AmazonLambdaClient client = new AmazonLambdaClient(credentials, RegionEndpoint.USWest2);

                InvokeRequest request = new InvokeRequest
                {
                    FunctionName = "QuickConnect",
                    Payload = "{\"MatchName\" : \"" + QuickConnect.m_StaticRoomName + "\"," + "\"Username\" : \"" + GetInstance().m_Username
                            + "\"," + "\"scene\" : \"" + QuickConnect.m_StaticStartingScene + "\"}",
                    InvocationType = InvocationType.RequestResponse
                };

                InvokeQuickConnectLambda(client, request);

            }

            /// <summary>
            /// Hosts a game via a lambda function for the user
            /// </summary>
            /// <param name="_client">The AWS client variable</param>
            /// <param name="_request">The request parameters and permissions</param>
            private async void InvokeHostLambda(AmazonLambdaClient _client, InvokeRequest _request)
            {
                InvokeResponse invokeResponse = null;
                try
                {
                    invokeResponse = await _client.InvokeAsync(_request);
                }
                catch (Exception _exception)
                {
                    Debug.LogError(invokeResponse?.FunctionError + _exception);
                    GetInstance().QForMainThread(AddErrorText, invokeResponse?.FunctionError + _exception.ToString());
                    GetInstance().QForMainThread(ChangeInteractablility, m_StartHostingButton, true);
                }
                if (invokeResponse != null)
                {
                    if (invokeResponse.StatusCode == 200)
                    {
                        var payload = Encoding.ASCII.GetString(invokeResponse.Payload.ToArray()) + "\n";
                        var playerSessionObj = JsonUtility.FromJson<PlayerSessionObject>(payload);

                        if (playerSessionObj.FleetId == null)
                        {
#if (ASL_DEBUG)
                            Debug.Log($"Error in Lambda: {payload}");
#endif
                            if (Regex.IsMatch(payload.ToString(), "FleetCapacityExceededException"))
                            {
                                GetInstance().QForMainThread(AddErrorText, "ASL currently does not have the capacity for another Game Session. Scaling up fleet to make room. " +
                                    "You will automatically be connected once the fleet is scaled up. This may take up to 5 minutes.");
                                GetInstance().QForMainThread(StartAutoConnect);
                            }
                            else
                            {
                                GetInstance().QForMainThread(AddErrorText, $"Error in Lambda: {payload}");                              
                            }
                            GetInstance().QForMainThread(ChangeInteractablility, m_StartHostingButton, true);
                        }
                        else
                        {
                            GetInstance().QForMainThread(ActionConnectToServer, playerSessionObj.DnsName, Int32.Parse(playerSessionObj.Port),
                                playerSessionObj.PlayerSessionId, playerSessionObj.GameName, playerSessionObj.GameSessionId, m_SceneName);
                        }
                    }
                }
            }

            /// <summary>
            /// Triggers the HostLambda function, allowing a user to host a game session
            /// </summary>
            private void HostSession()
            {
#if ASL_DEBUG
                Debug.Log("Attempting to host game");
#endif
                m_TryAutoConnectAgain = false;
                if (string.IsNullOrEmpty(m_RoomNameInputField.text))
                {
                    GetInstance().QForMainThread(AddErrorText, "Room name cannot be empty.");
                    GetInstance().QForMainThread(ChangeInteractablility, m_StartHostingButton, true);
                    return;
                }

                m_SceneName = m_AvailableScenes.options[m_AvailableScenes.value].text;

                // paste this in from the Amazon Cognito Identity Pool console
                CognitoAWSCredentials credentials = new CognitoAWSCredentials(
                    "us-west-2:9dc2d6b8-58a0-4f0a-9369-b83c5c5e796a", // Identity pool ID
                    RegionEndpoint.USWest2 // Region
                );

                AmazonLambdaClient client = new AmazonLambdaClient(credentials, RegionEndpoint.USWest2);

                InvokeRequest request = new InvokeRequest
                {
                    FunctionName = "HostGameSession",
                    Payload = "{" + "\"username\" : \"" + GetInstance().m_Username + "\","
                                + "\"name\" : \"" + m_RoomNameInputField.text + "-" + m_SceneName + "\"," + "\"scene\" : \"" + m_SceneName + "\"}",
                    InvocationType = InvocationType.RequestResponse
                };
                InvokeHostLambda(client, request);
            }

            /// <summary>
            /// Connects this user to a GameLift sever
            /// </summary>
            /// <param name="_dnsName">The DNS name to connect to</param>
            /// <param name="_port">The port to connect to</param>
            /// <param name="_tokenUID">The unique token, used on GameLift's end to create matches</param>
            /// <param name="_gameName">The name of the game the user created or joined</param>
            /// <param name="_gameSessionId">The game session id the user created or joined</param>
            /// <param name="_sceneName">The name of the next scene to load after all players are connected and ready</param>
            private void ActionConnectToServer(string _dnsName, int _port, string _tokenUID, string _gameName, string _gameSessionId, string _sceneName)
            {
                m_InitializingConnection = true;
                m_GameSessionId = _gameSessionId;
                m_SceneName = _sceneName;
                GetInstance().StartCoroutine(ConnectToServer(_dnsName, _port, _tokenUID, _gameName));
            }

            /// <summary>
            /// Connects to a GameLift server
            /// </summary>
            /// <param name="_dnsName">The name of the DNS server to connect to</param>
            /// <param name="_port">The port number to connect to</param>
            /// <param name="_tokenUID">The token ID to be used in the connection process</param>
            /// <param name="_gameName">The name of the game the user is connecting to</param>
            /// <returns></returns>
            private IEnumerator ConnectToServer(string _dnsName, int _port, string _tokenUID, string _gameName)
            {
#if (ASL_DEBUG)
                ClientLogger.LogHandler = (x) => Debug.Log(x);
#endif
                //The following do not help resolve 
                //"System.Security.Authentication.AuthenticationException: A call to SSPI failed, see inner exception. 
                //---> Mono.Security.Interface.TlsException: Handshake failed - error code: 
                //UNITYTLS_INTERNAL_ERROR, verify result: UNITYTLS_X509VERIFY_NOT_DONE"   

                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //ServicePointManager.ServerCertificateValidationCallback = delegate {return true;};

                ConnectionToken token = new ConnectionToken(_tokenUID, null);
                ClientConfiguration clientConfiguration = new ClientConfiguration();

                //https://docs.aws.amazon.com/gamelift/latest/developerguide/realtime-sdk-csharp-ref-datatypes.html#realtime-sdk-csharp-ref-datatypes-clientconfiguration
                //clientConfiguration.ConnectionType = ConnectionType.RT_OVER_WSS_DTLS_TLS12; //Still working on getting this to work 



                GetInstance().m_Client = new Client(clientConfiguration);
                GetInstance().m_Client.ConnectionOpen += new EventHandler(GetInstance().OnOpenEvent);
                GetInstance().m_Client.ConnectionClose += new EventHandler(GetInstance().OnCloseEvent);
                GetInstance().m_Client.DataReceived += new EventHandler<DataReceivedEventArgs>(GetInstance().OnDataReceived);
                GetInstance().m_Client.ConnectionError += new EventHandler<Aws.GameLift.Realtime.Event.ErrorEventArgs>(GetInstance().OnConnectionErrorEvent);


                //Run one test at a time
                //Try stress tests to see packets

#if UNITY_ANDROID || UNITY_STANDALONE_OSX
                int UDPListenPort = m_AndroidOrOSXUDPListeningPort;                
#else
                int UDPListenPort = FindAvailableUDPPort(DEFAULT_UDP_PORT, DEFAULT_UDP_PORT + 100); //33400 - 33500 - Function does not work on Android.
#endif

                if (UDPListenPort == -1)
                {
#if ASL_DEBUG
                    Debug.Log("Unable to find an open UDP listen port");
#endif
                    GetInstance().QForMainThread(AddErrorText, "Unable to find an open UDP listen port");
                    yield break;
                }
                else
                {
#if ASL_DEBUG
                    Debug.Log($"UDP listening on port: {UDPListenPort}");
#endif
                }
#if ASL_DEBUG
                Debug.Log($"[client] Attempting to connect to server DNS: {_dnsName} TCP port: {_port} Player Session ID: {_tokenUID}");
#endif
                GetInstance().m_Client.Connect(_dnsName, _port, UDPListenPort, token);


                while (true)
                {
                    if (GetInstance().m_Client.ConnectedAndReady)
                    {
#if ASL_DEBUG
                        Debug.Log("[client] Connected to server");
#endif
                        GetInstance().QForMainThread(SetGameName, _gameName);
                        GetInstance().QForMainThread(SetCorrectUIPanel, CurrentLoginStage.LobbyScreen);
                        GetInstance().QForMainThread(UpdateOtherUsers);
                        break;
                    }
                    yield return null;
                }
            }

            /// <summary>
            /// Finds an available UDP port on the user's end to receive UDP messages on. It should be noted that this function cannot be called on Android, and 
            /// therefore, only 1 port is available on Android, the default UDP port of 33400. If this port is not available, then that Android device will not be able
            /// to receive UDP messages, which ASL does not currently send anyways.
            /// </summary>
            /// <param name="firstPort">The first port to try to use</param>
            /// <param name="lastPort">The last port to try to use</param>
            /// <returns>An available port to use for UDP traffic</returns>
            private int FindAvailableUDPPort(int firstPort, int lastPort)
            {
                var UDPEndPoints = IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners();

                List<int> usedPorts = new List<int>();
                usedPorts.AddRange(from n in UDPEndPoints where n.Port >= firstPort && n.Port <= lastPort select n.Port);
                usedPorts.Sort();
                for (int testPort = firstPort; testPort <= lastPort; ++testPort)
                {
                    if (!usedPorts.Contains(testPort))
                    {
                        return testPort;
                    }
                }
                return -1;
            }

            /// <summary>
            /// Invokes the lambda function that searches for available game sessions
            /// </summary>
            /// <param name="_client">The AWS client variable</param>
            /// <param name="_request">The request parameters and permissions</param>
            private async void InvokeSearchForSessionsLambda(AmazonLambdaClient _client, InvokeRequest _request)
            {
                InvokeResponse invokeResponse = null;
                try
                {
                    invokeResponse = await _client.InvokeAsync(_request);
                }
                catch (Exception _exception)
                {
                    Debug.LogError(invokeResponse.FunctionError + _exception);
                    GetInstance().QForMainThread(AddErrorText, invokeResponse.FunctionError + _exception.ToString());
                }
                if (invokeResponse != null)
                {
                    if (invokeResponse.StatusCode == 200)
                    {
                        var payload = Encoding.ASCII.GetString(invokeResponse.Payload.ToArray()) + "\n";
                        var gameSessionObj = JsonUtility.FromJson<GameSessionObjectCollection>(payload);
                        GetInstance().QForMainThread(DestroyMatchOptions);
                        foreach (var _gameSession in gameSessionObj.GameSessions)
                        {
                            if (_gameSession.Name == null)
                            {
#if (ASL_DEBUG)
                                Debug.Log($"Error in Lambda: {payload}");
#endif
                                GetInstance().QForMainThread(AddErrorText, $"Error in Lambda: {payload}");
                            }
                            else
                            {
                                GetInstance().QForMainThread(AddPotentialSession, _gameSession);
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Triggers the search for available game sessions lambda function
            /// </summary>
            private void SearchForSessions()
            {
#if ASL_DEBUG
                Debug.Log("Attempting to find a session to join");
#endif
                // paste this in from the Amazon Cognito Identity Pool console
                CognitoAWSCredentials credentials = new CognitoAWSCredentials(
                    "us-west-2:856d1d38-01de-4d9d-9d2b-c5f520018b7a", // Identity pool ID
                    RegionEndpoint.USWest2 // Region
                );

                AmazonLambdaClient client = new AmazonLambdaClient(credentials, RegionEndpoint.USWest2);
                InvokeRequest request = new InvokeRequest
                {
                    FunctionName = "SearchForSessions",
                    InvocationType = InvocationType.RequestResponse
                };
                InvokeSearchForSessionsLambda(client, request);
            }

            /// <summary>
            /// Adds a potential game session to the available session screen as a button to select and thus join with
            /// </summary>
            /// <param name="_foundSession">A found session</param>
            private void AddPotentialSession(GameSessionObject _foundSession)
            {
                Button aMatch = Instantiate(Resources.Load("ASL_Prefabs/AvailableMatchButton") as GameObject, m_SessionsAvailableHolder.transform).GetComponent<Button>();
                aMatch.transform.localPosition = m_MatchFoundButtonPosition;
                m_MatchFoundButtonPosition.y -= 60.0f; //Update for next button
                var buttonText = aMatch.GetComponentsInChildren<Text>();
                buttonText[0].text = "ASL";
                buttonText[1].text = _foundSession.Name;
                buttonText[2].text = _foundSession.CurrentPlayerSessionCount + "/" + _foundSession.MaximumPlayerSessionCount;
                aMatch.gameObject.tag = "AvailableMatches";
                aMatch.onClick.AddListener(() =>
                {
                    SetGameName(_foundSession.Name);
                    m_GameSessionId = _foundSession.GameSessionId;
                    m_JoinMatchButton.interactable = true;
                });
            }

            /// <summary>
            /// Invokes the join session lambda function that allows this user to a join a selected game session
            /// </summary>
            /// <param name="_client">The AWS client variable</param>
            /// <param name="_request">The request parameters and permissions</param>
            private async void InvokeJoinSessionLambda(AmazonLambdaClient _client, InvokeRequest _request)
            {
                InvokeResponse invokeResponse = null;
                try
                {
                    invokeResponse = await _client.InvokeAsync(_request);
                }
                catch (Exception _exception)
                {
                    GetInstance().QForMainThread(AddErrorText, invokeResponse.FunctionError + _exception.ToString());
                }
                if (invokeResponse != null)
                {
                    if (invokeResponse.StatusCode == 200)
                    {
                        var payload = Encoding.ASCII.GetString(invokeResponse.Payload.ToArray()) + "\n";
                        var playerSessionObj = JsonUtility.FromJson<PlayerSessionObject>(payload);

                        if (playerSessionObj.FleetId == null)
                        {
#if (ASL_DEBUG)
                            Debug.Log($"Error in Lambda: {payload}");
#endif
                            GetInstance().QForMainThread(AddErrorText, $"Error in Lambda: {payload}" + "\nMatch was started or destroyed before you could join. Please refresh.");
                        }
                        else
                        {
                            GetInstance().QForMainThread(ActionConnectToServer, playerSessionObj.DnsName, Int32.Parse(playerSessionObj.Port),
                                playerSessionObj.PlayerSessionId, playerSessionObj.GameName, playerSessionObj.GameSessionId, playerSessionObj.SceneName);
                        }
                    }
                }
            }

            /// <summary>
            /// Joins a game session
            /// </summary>
            private void JoinSession()
            {
#if ASL_DEBUG
                Debug.Log("Attempting to join a session");
#endif
                // paste this in from the Amazon Cognito Identity Pool console
                CognitoAWSCredentials credentials = new CognitoAWSCredentials(
                    "us-west-2:e22325d8-789c-486c-884f-09fc36dbcb80", // Identity pool ID
                    RegionEndpoint.USWest2 // Region
                );

                AmazonLambdaClient client = new AmazonLambdaClient(credentials, RegionEndpoint.USWest2);
                InvokeRequest request = new InvokeRequest
                {
                    FunctionName = "JoinSession",
                    Payload = "{\"GameSessionId\" : \"" + m_GameSessionId + "\"," + "\"PlayerId\" : \"" + GetInstance().m_Username + "\"}",
                    InvocationType = InvocationType.RequestResponse
                };
                InvokeJoinSessionLambda(client, request);
            }

            /// <summary>
            /// Updates other users of this user's presence in the lobby
            /// </summary>
            private void UpdateOtherUsers()
            {
#if ASL_DEBUG
                Debug.Log("Updating other users of presence in lobby");
#endif

                RTMessage message = GetInstance().CreateRTMessage(OpCode.AddPlayerToLobbyUI, Encoding.Default.GetBytes(GetInstance().m_PeerId.ToString() + ":" + GetInstance().m_Username));
                GetInstance().m_Client.SendMessage(message);
                UpdateLobbyScreen(); //Update screen for local user
            }

            /// <summary>
            /// Adds text to the error text, allowing the user to know if an error occurred during connection
            /// </summary>
            /// <param name="_errorText">The error text to display</param>
            private void AddErrorText(string _errorText)
            {
                m_ErrorText.text = _errorText;
            }

            /// <summary>
            /// Clears any error text
            /// </summary>
            private void RemoveErrorText()
            {
                m_ErrorText.text = string.Empty;
            }

            /// <summary>
            /// Updates the connection status text based on the current connection status
            /// </summary>
            /// <param name="_connectionStatus"></param>
            private void UpdateConnectionStatusText(string _connectionStatus)
            {
                m_ConnectionStatusText.text = "Connection Status: " + _connectionStatus;
            }

            /// <summary>
            /// Updates the username of the user based on the username they selected
            /// </summary>
            /// <param name="_username"></param>
            private void UpdateUsername(string _username)
            {
                m_UsernameText.text = "Username: " + _username;
                GetInstance().m_Username = _username;
            }

            /// <summary>
            /// Sets the game name to be displayed to users
            /// </summary>
            /// <param name="_gameName">the name of the game</param>
            private void SetGameName(string _gameName)
            {
                GetInstance().m_GameSessionName = _gameName;
            }

            /// <summary>
            /// Used to start the auto connect coroutine
            /// </summary>
            private void StartAutoConnect()
            {
                m_TryAutoConnectAgain = true;
                if (!m_StartedAutoConnect)
                {
                    m_StartedAutoConnect = true;
                    GetInstance().StartCoroutine(AutoConnect());
                }
            }

            /// <summary>
            /// When the servers are down, this function will ping them every 30 seconds until a connection can occur.
            /// </summary>
            /// <returns>Waits for 30 seconds before attempting to connect again</returns>
            private IEnumerator AutoConnect()
            {
                while (!m_InitializingConnection && m_TryAutoConnectAgain)
                {                   
                    if (!QuickConnect.m_StaticQuickStart)
                    {
                        HostSession();
                    }
                    else
                    {
                        QuickConnectMatch();
                    }
                    yield return new WaitForSeconds(30);
                }
            }

            /// <summary>
            /// Goes back to the last UI screen and cleans anything up that may have happened on the current UI screen
            /// </summary>
            public void GoBack()
            {
                m_JoinMatchButton.interactable = false; //Prevent user from joining a match with outdated details
                GetInstance().m_Players.Clear();
                DestroyMatchOptions();
                GetInstance().DisconnectFromServer();
                SeePreviousUIScreen(m_CurrentUIScreen);
                AddErrorText(string.Empty);
            }

            /// <summary>
            /// Join the currently selected match 
            /// </summary>
            public void JoinSelectedMatch()
            {
                DestroyMatchOptions();
                JoinSession();
            }

            /// <summary>
            /// Returns to the previous UI screen, triggered by hitting the back button
            /// </summary>
            /// <param name="_currentScreen">The current UI screen the user is on</param>
            private void SeePreviousUIScreen(CurrentLoginStage _currentScreen)
            {
                switch (_currentScreen)
                {
                    case CurrentLoginStage.Login:
                        SetCorrectUIPanel(_currentScreen);
                        break;
                    case CurrentLoginStage.HostOrFindSession:
                        m_LoginButton.interactable = true;
                        SetCorrectUIPanel(CurrentLoginStage.Login);
                        break;
                    case CurrentLoginStage.HostMenu:
                        SetCorrectUIPanel(CurrentLoginStage.HostOrFindSession);
                        break;
                    case CurrentLoginStage.AvailableSessions:
                        SetCorrectUIPanel(CurrentLoginStage.HostOrFindSession);
                        break;
                    case CurrentLoginStage.LobbyScreen:
                        SetCorrectUIPanel(CurrentLoginStage.HostOrFindSession);
                        break;
                    case CurrentLoginStage.Setup:

                        break;
                    default: break;
                }
            }

            /// <summary>
            /// Indicates that the user is ready for the first scene to being loading
            /// </summary>
            private void ReadyUp()
            {
                RTMessage message = GetInstance().CreateRTMessage(OpCode.PlayerReady, Encoding.ASCII.GetBytes(m_SceneName));
                GetInstance().m_Client.SendMessage(message);
            }

            /// <summary>
            /// Destroys any match options locally so that they are not repeated to the user
            /// </summary>
            private void DestroyMatchOptions()
            {
                foreach (var matchOption in GameObject.FindGameObjectsWithTag("AvailableMatches"))
                {
                    Destroy(matchOption);
                }
                m_MatchFoundButtonPosition.y = 0; //reset position
            }

            /// <summary>
            /// Allows a button's interactability to be changed on the main Unity thread
            /// </summary>
            /// <param name="_buttonToChange">The button to change</param>
            /// <param name="_value">The value determining the button's interactability</param>
            private void ChangeInteractablility(Button _buttonToChange, bool _value)
            {
                _buttonToChange.interactable = _value;
            }

            /// <summary>
            /// Changes the value of the QuickConnect flag
            /// </summary>
            /// <param name="_value">the new value of that boolean</param>
            private void SetQuickConnectFlag( bool _value)
            {
                QuickConnect.m_StaticQuickStart = _value;
                m_RoomNameInputField.text = QuickConnect.m_StaticRoomName;
            }

            /// <summary>
            /// Is called to update any new comers of who this user is
            /// </summary>
            private void UpdateNewComer()
            {
#if ASL_DEBUG
                Debug.Log("Updating joiner of presence in lobby");
#endif
                RTMessage message = GetInstance().CreateRTMessage(OpCode.AddPlayerToLobbyUI, Encoding.Default.GetBytes(GetInstance().m_PeerId.ToString() + ":" + GetInstance().m_Username));
                GetInstance().m_Client.SendMessage(message);

            }

            /// <summary>
            /// Looks for an available port for the android device to use by attempting a connection to it
            /// </summary>
            private void CheckPorts()
            {
                try
                {
                    UdpClient testClient = new UdpClient(m_AndroidOrOSXUDPListeningPort);
                    testClient.Connect("www.contoso.com", m_AndroidOrOSXUDPListeningPort);
                    if (testClient.Client.Connected)
                    {
#if ASL_DEBUG
                        Debug.Log("Connection available on port: " + m_AndroidOrOSXUDPListeningPort);
#endif
                        testClient.Close();
                        m_LoginButton.interactable = true;
                    }
                    else
                    {
                        m_LoginButton.interactable = false;
#if ASL_DEBUG
                        Debug.Log("Failed to connect on port: " + m_AndroidOrOSXUDPListeningPort);
#endif
                        m_AndroidOrOSXUDPListeningPort++;
                        if (m_AndroidOrOSXUDPListeningPort > 33500)
                        {
                            AddErrorText("Could not find an available port to use. Please restart device.");
                            return;
                        }
                        CheckPorts();
                    }
                }
                catch (Exception _error)
                {
#if ASL_DEBUG
                    Debug.LogError(_error);
#endif
                    m_AndroidOrOSXUDPListeningPort++;
                    AddErrorText("Tried initializing the next connection with a bad port. Port is now: " + m_AndroidOrOSXUDPListeningPort + " Try again.");                  
                    if (m_AndroidOrOSXUDPListeningPort > 33500)
                    {
#if ASL_DEBUG
                        AddErrorText("Could not find an available port to use. Please restart device. " + _error.ToString());
#endif
                        return;
                    }
                    CheckPorts();
                }
            }

            /// <summary>
            /// Is called when a player joins the match
            /// </summary>
            /// <param name="_packet">The packet containing information about the server</param>
            public void PlayerJoinedMatch(DataReceivedEventArgs _packet)
            {
                string data = Encoding.Default.GetString(_packet.Data);
                string[] parts = data.Split(':');
                GetInstance().m_PeerId = int.Parse(parts[2]);
                if (!int.TryParse(parts[0], out int tmpId))
                {
                    GetInstance().m_ServerId = -1; //The typical ServerId
#if (ASL_DEBUG)
                    Debug.LogWarning("ServerId could not be set - using standard server id instead.");
#endif
                }
                else { GetInstance().m_ServerId = tmpId; }
                if (!int.TryParse(parts[1], out tmpId))
                {
                    GetInstance().m_GroupId = -1; //The typical GroupId
#if (ASL_DEBUG)
                    Debug.LogWarning("All player group Id could not be set - using standard all player group id instead.");
#endif
                }
                else { GetInstance().m_GroupId = tmpId; }

                GetInstance().m_Players.Add(GetInstance().m_PeerId, GetInstance().m_Username);

            }

            /// <summary>
            /// Updates the lobby screen to show any new players
            /// </summary>
            public void UpdateLobbyScreen()
            {
#if ASL_DEBUG
                Debug.Log("Updating Lobby UI");
#endif
                m_PlayerListText.text = "Player list:\n";
                m_MatchNameText.text = GetInstance().m_GameSessionName;
                m_PlayerCountText.text = GetInstance().m_Players.Count + "/20";

                var playersInOrderOfJoining = GetInstance().m_Players.OrderBy(x => x.Key);

                foreach (var player in playersInOrderOfJoining)
                {
                    m_PlayerListText.text += player.Value + "\n";
                }
            }

            /// <summary>
            /// Adds a player to the match
            /// </summary>
            /// <param name="_packet">The packet containing new player info</param>
            public void AddPlayerToMatch(DataReceivedEventArgs _packet)
            {
                string data = Encoding.Default.GetString(_packet.Data);
                string[] parts = data.Split(':');
                if (!GetInstance().m_Players.ContainsKey(int.Parse(parts[0]))) //if we haven't talked to this person yet or are not the original sender, then do stuff
                {
                    UpdateNewComer();
                    GetInstance().AddPlayerToList(int.Parse(parts[0]), parts[1]);
                    UpdateLobbyScreen();
                }
            }

            /// <summary>
            /// Allows users to ready up
            /// </summary>
            public void AllowReadyUp()
            {
                m_ReadyButton.interactable = true;
            }

            /// <summary>
            /// Invokes the lambda function that locks the game session
            /// </summary>
            /// <param name="_client">The AWS client variable</param>
            /// <param name="_request">The request parameters and permissions</param>
            private async void InvokeLockSessionLambda(AmazonLambdaClient _client, InvokeRequest _request)
            {
                InvokeResponse invokeResponse = null;
                try
                {
                    invokeResponse = await _client.InvokeAsync(_request);
                }
                catch (Exception _exception)
                {
                    GetInstance().QForMainThread(AddErrorText, invokeResponse.FunctionError + _exception.ToString());
                }
                if (invokeResponse != null)
                {
                    if (invokeResponse.StatusCode != 200)
                    {
                        GetInstance().QForMainThread(AddErrorText, "Error when updating session via Lambda. Code: " + invokeResponse.StatusCode);
                    }
                }
            }

            /// <summary>
            /// Locks the game session, preventing any other users from joining it
            /// </summary>
            public void LockSession()
            {
#if ASL_DEBUG
                Debug.Log("Locking session to prevent late joiners");
#endif
                // paste this in from the Amazon Cognito Identity Pool console
                CognitoAWSCredentials credentials = new CognitoAWSCredentials(
                    "us-west-2:19f43248-f4e9-435d-891c-d695ffcf8149", // Identity pool ID
                    RegionEndpoint.USWest2 // Region
                );

                AmazonLambdaClient client = new AmazonLambdaClient(credentials, RegionEndpoint.USWest2);
                InvokeRequest request = new InvokeRequest
                {
                    FunctionName = "UpdateSession",
                    Payload = "{\"GameSessionId\" : \"" + m_GameSessionId + "\"}",
                    InvocationType = InvocationType.RequestResponse
                };
                InvokeLockSessionLambda(client, request);

            }

            /// <summary>
            /// Updates the chat log based on the message received 
            /// </summary>
            /// <param name="_packet">The packet containing the username and message</param>
            public void UpdateChatLog(DataReceivedEventArgs _packet)
            {
                string data = Encoding.Default.GetString(_packet.Data);
                string[] parts = data.Split(':');

                m_ChatHistoryText.text += "\n" + parts[0] + ":" + parts[1];

            }

            //Resets the lobby to the original login screen in case something happened
            public void Reset()
            {
                SetCorrectUIPanel(CurrentLoginStage.Login);
                AddErrorText("Connection Lost.");
                m_ChatHistoryText.text = "Chat Log:\n";
                m_LoginButton.interactable = true;
                m_PlayerListText.text = string.Empty;
                m_PlayerCountText.text = string.Empty;
                GetInstance().m_Players.Clear();
#if UNITY_ANDROID || UNITY_STANDALONE_OSX
                CheckPorts();
#endif
            }

        }

    }
}
