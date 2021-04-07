using UnityEngine;

namespace SimpleDemos
{
    /// <summary>Example of how to change scenes for all users</summary>
    public class LoadScene_Example : MonoBehaviour
    {
        /// <summary>The name of the scene the user wants to load</summary>
        public string m_SceneToLoad;
        /// <summary>Resets the scene so the user can delete the object again</summary>
        public bool m_LoadScene;

        /// <summary> Initialize scene to load string</summary>
        private void Start()
        {
            if (string.IsNullOrEmpty(m_SceneToLoad))
            {
                Debug.Log("No scene specified. Choosing one for you.");
                m_SceneToLoad = "TransformObjectViaLocalSpace_Example";
            }
        }

        /* For more examples go to https://uwb-arsandbox.github.io/ASL/ */

        /// <summary> Game Logic</summary>
        void Update()
        {
            if (m_LoadScene)
            {
                ASL.ASLHelper.SendAndSetNewScene(m_SceneToLoad);
                m_LoadScene = false;
            }
        }
    }
}