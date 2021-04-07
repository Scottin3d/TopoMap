using UnityEngine;

namespace SimpleDemos
{
    /// <summary>
    /// Example of how to claim an object. Every object must be claimed if you want what you are attempting to do to be seen by
    /// other players
    /// </summary>
    public class ObjectToClaim_Example : MonoBehaviour
    {
        /// <summary>Provides an easy way to access the object we want to claim. </summary>
        public GameObject m_ObjectToClaim;

        /// <summary>Boolean value that the user triggers via the Unity Editor to send or not send a claim</summary>
        public bool SendClaim = false;

        /*
            Notice how if we already own the object, then we will call whatever is inside our claim function right away
            It is also important to note that if we select SendClaim to false, after a short period of time, m_Mine will
            become false again. This is because after 1 second, by default, an object will be released back to the server.
            Reclaiming the object before this timer runs out, by default, will reset this timer. See the documentation on
            SendAndSetClaim() for more information on these matters. In general it should be noted that you should not
            attempt to claim an object continuously inside the update function because you'll be wasting bandwidth since all
            users will be sending a packet every frame, which is very excessive. It is more efficient to implement a timer
            of some sort, or use a boolean flag system like the one here. Ideally, claims, and any server function for that
            matter, would be called rarely.

            For more examples go to https://uwb-arsandbox.github.io/ASL/ASLDocumentation/Help/html/36eb7dca-b9ae-3c4f-e5aa-31df6bc3e777.htm
        */
        /// <summary>
        /// When SendClaim is triggered to true via Unity Editor we will send a claim for this object.
        /// We will lose this claim after 1 second of setting SendClaim to false because SendAndSetClaim releases the object back to the server after 1 second
        /// </summary>
        void Update()
        {
            if (SendClaim)
            {
                m_ObjectToClaim.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    Debug.Log("Successfully claimed object!");
                });
            }
            if (m_ObjectToClaim.GetComponent<ASL.ASLObject>().m_Mine)
            {
                Debug.Log("We currently own this object!");
            }
            else
            {
                Debug.Log("We do NOT own this object!");
            }
        }
    }
}