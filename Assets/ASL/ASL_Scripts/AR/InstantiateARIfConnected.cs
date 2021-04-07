using UnityEngine;

namespace ASL
{
    /// <summary>
    /// Simply spawns the ARHolder object once the scene is connected - this prevents errors from occurring when using QuickConnect.
    /// </summary>
    public class InstantiateARIfConnected : MonoBehaviour
    {
        /// <summary>
        /// Called on start
        /// </summary>
        private void Awake()
        {
            //If the user is connected, then spawn AR camera objects
            if (FindObjectOfType<GameLiftManager>() != null && ASL.GameLiftManager.GetInstance() != null && ASL.GameLiftManager.GetInstance().m_Client.ConnectedAndReady)
            {
                Instantiate(Resources.Load("ASL_Prefabs/ARFoundationPrefabs/ARHolder"), Vector3.zero, Quaternion.identity);
                Destroy(gameObject); //No longer need - delete to clean up resources
            }

        }
    }
}
