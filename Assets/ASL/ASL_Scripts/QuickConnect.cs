using UnityEngine;
using UnityEngine.SceneManagement;

namespace ASL
{
    /// <summary>
    /// Class used to quickly connect to a match for testing proposes 
    /// </summary>
    public class QuickConnect : MonoBehaviour
    {
        /// <summary>
        /// Flag used to determine if QuickStart is being used or not.
        /// </summary>
        public static bool m_StaticQuickStart = false;
        /// <summary>
        /// The name of the room that players should automatically join
        /// </summary>
        public string m_RoomName = string.Empty;
        /// <summary>
        /// The static room name - used to pass to other classes without having to have an instance to this class
        /// </summary>
        public static string m_StaticRoomName = string.Empty;
        /// <summary>
        /// The name of the scene to be loaded after everyone is connected and ready
        /// </summary>
        public string m_StartingScene = string.Empty;
        /// <summary>
        /// The static name of the scene to be loaded after everyone is connected and ready - used to pass to other classes without having to have an instance to this class
        /// </summary>
        public static string m_StaticStartingScene = string.Empty;

        /// <summary>
        /// Called upon script load, it switches our scene to the LobbyScene, which then, using this class's static variables, determines what to do next
        /// </summary>
        private void Awake()
        {           
            if (!m_StaticQuickStart && (FindObjectOfType<GameLiftManager>() == null || GameLiftManager.GetInstance() == null || !GameLiftManager.GetInstance().m_Client.ConnectedAndReady))
            {
                m_StaticQuickStart = true;
                m_StaticRoomName = m_RoomName;
                m_StaticStartingScene = m_StartingScene;
                SceneManager.LoadScene("ASL_LobbyScene");
            }
            
        }

    }
}
