using UnityEngine;

namespace SimpleDemos
{
    /// <summary>Example of how to delete an object</summary>
    public class DeleteObject_Example : MonoBehaviour
    {
        /// <summary>Provides an easy way to access the object we want to delete. </summary>
        public GameObject m_MyObjectToDelete;
        /// <summary>Bool triggering the object's deletion</summary>
        public bool m_Delete;
        /// <summary>Resets the scene so the user can delete the object again</summary>
        public bool m_Reset;

        /* For more examples go to https://uwb-arsandbox.github.io/ASL/ASLDocumentation/Help/html/913377d0-20b0-d547-891d-671b0d6f69dd.htm */

        /// <summary>Game Logic</summary>
        void Update()
        {
            if (m_Delete)
            {
                m_MyObjectToDelete.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    m_MyObjectToDelete.GetComponent<ASL.ASLObject>().DeleteObject();
                });
                m_Delete = false;
            }
            //Reset the scene for all players
            if (m_Reset)
            {
                ASL.ASLHelper.SendAndSetNewScene("DeleteObject_Example");
                m_Reset = false;
            }
        }
    }
}