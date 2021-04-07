using Microsoft.MixedReality.Toolkit;
using UnityEngine;

namespace ASL
{
    /// <summary>
    /// Simply spawns the MixedRealityToolkit and MixedRealityPlayspace objects after a connection is made.
    /// It also sets the MixedRealityToolkit profile to be DefaultXRSDKConfigurationProfile
    /// </summary>
    public class InstantiateMRTKIfConnected : MonoBehaviour
    {
        [SerializeField]
        private MixedRealityToolkitConfigurationProfile m_DefaultXRSDKConfigurationProfile = null; 

        /// <summary>
        /// Called on start
        /// </summary>
        private void Awake()
        {
            //If the user is connected, then spawn AR camera objects
            if (FindObjectOfType<GameLiftManager>() != null && ASL.GameLiftManager.GetInstance() != null && ASL.GameLiftManager.GetInstance().m_Client.ConnectedAndReady)
            {
                //Spawn camera
                Instantiate(Resources.Load("ASL_Prefabs/MixedRealityToolkitPrefabs/MixedRealityPlayspace"), Vector3.zero, Quaternion.identity);
                GameObject mixedRealityToolkit = new GameObject();
                mixedRealityToolkit.AddComponent<MixedRealityToolkit>().ActiveProfile = m_DefaultXRSDKConfigurationProfile;
                Destroy(gameObject); //No longer need - delete to clean up resources
            }

        }
    }
}