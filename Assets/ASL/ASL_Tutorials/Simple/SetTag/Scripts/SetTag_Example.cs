using UnityEngine;

namespace SimpleDemos
{
    /// <summary> Example of how to set an ASL Object's tag for all users</summary>
    public class SetTag_Example : MonoBehaviour
    {
        /// <summary> The ASL object we will change the tag of for all players</summary>
        public GameObject m_MyObjectToTag;

        /// <summary> The tag you want to send and set for this object - note this tag must be predefined</summary>
        public string m_TagToSendAndSet;

        /// <summary>Flag indicating to send the tag</summary>
        public bool m_SendTag;

        /// <summary>
        /// Our game logic
        /// </summary>
        void Update()
        {
            if (m_SendTag)
            {
                //Claim the object
                m_MyObjectToTag.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                //Send and then set (once received - NOT here) the tag
                m_MyObjectToTag.GetComponent<ASL.ASLObject>().SendAndSetTag(m_TagToSendAndSet);
                });
                m_SendTag = false;
            }

            Debug.Log("MyObjectToTag current tag: " + m_MyObjectToTag.tag);
        }
    }
}
