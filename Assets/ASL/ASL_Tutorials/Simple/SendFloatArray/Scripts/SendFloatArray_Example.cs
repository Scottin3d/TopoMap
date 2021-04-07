using UnityEngine;

namespace SimpleDemos
{
    /// <summary>
    /// An example of how to use SendFloatArray. While you can only send 4 values at a time, if you need your object to 5+ different
    /// values, at different times, you can do this via a switch/case, like this example shows. If you are using 4 of less values
    /// Then there is no need to use the switch example, but instead to just assign your values as you need them.
    /// </summary>
    public class SendFloatArray_Example : MonoBehaviour
    {
        /// <summary>Provides an easy way to access the object we want to send floats with/for. </summary>
        public GameObject m_ObjectToSendFloats;
        /// <summary>Bool that toggles when we send the floats, gets set to false after we send to save bandwidth</summary>
        public bool m_SendFloat = false;

        /// <summary>The floats that will be sent</summary>
        public float[] m_MyFloats = new float[4];

        /// <summary>Initialize values</summary>
        void Start()
        {
            //Send float function must be assigned in here, in Start, so all users get it, or when the object is created
            m_ObjectToSendFloats.GetComponent<ASL.ASLObject>()._LocallySetFloatCallback(MyFloatFunction);
        }

        /*For more examples go to:
        * https://uwb-arsandbox.github.io/ASL/ASLDocumentation/Help/html/5a174bc7-f1fe-c49a-b81d-4ad397ffb286.htm Position*/

        // Update is called once per frame
        void Update()
        {
            if (m_SendFloat)
            {
                m_ObjectToSendFloats.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    string floats = "Floats sent: ";
                    for (int i = 0; i < m_MyFloats.Length; i++)
                    {
                        floats += m_MyFloats[i].ToString();
                        if (m_MyFloats.Length - 1 != i)
                        {
                            floats += ", ";
                        }
                    }
                    Debug.Log(floats);
                    m_ObjectToSendFloats.GetComponent<ASL.ASLObject>().SendFloatArray(m_MyFloats);
                });
                m_SendFloat = false;
            }
        }

        /// <summary>What I want to do with the float values passed around for this object (_id). By using a case statement
        /// and reservering the last index for the switch value, I can execute more than 4 different things with this 
        /// 1 function depending on what floats I want to send.</summary>
        /// <param name="_myFloats">My float 4 array</param>
        /// <param name="_id">The id of the object that called <see cref="ASL.ASLObject.SendFloatArray_Example(float[])"/></param>
        public static void MyFloatFunction(string _id, float[] _myFloats)
        {
            string floats = "Floats received: ";
            for (int i = 0; i < _myFloats.Length; i++)
            {
                floats += _myFloats[i].ToString();
                if (_myFloats.Length - 1 != i)
                {
                    floats += ", ";
                }
            }
            Debug.Log(floats);
            //The following is a hypothetical switch that a user could create. While we are sending values in this example
            //We aren't actually sending what the comments in the case statement are saying (e.g., we aren't sending a player's
            //score.) We are simply giving an example of what could be sent using this switch method. If the user only has
            //a need for 4 or less values, then obviously a switch statement is not necessary.
            switch (_myFloats[3])
            {
                case 0:
                    Debug.Log("The values sent were: " + _myFloats[0] + ", " + _myFloats[1] 
                                                 + ", " + _myFloats[2] + ", " + _myFloats[3]);
                    break;
                case 1: //Sent Player's health
                    Debug.Log("Using case: " + _myFloats[3]);
                    Debug.Log("The Player's health is: " + _myFloats[0]);
                    break;
                case 2: //Sent Player's score
                    Debug.Log("Using case: " + _myFloats[3]);
                    Debug.Log("The Player's score is: " + _myFloats[0]);
                    break;
                case 3: //Sent Player's velocity and direction
                    Debug.Log("Using case: " + _myFloats[3]);
                    Debug.Log("The Player's velocity is: " + _myFloats[0]);
                    Debug.Log("The Player's direction is: " + _myFloats[1]);
                    break;
                case 4: //Sent random values
                    Debug.Log("Using case: " + _myFloats[3]);
                    Debug.Log("Random value 1: " + _myFloats[0]);
                    Debug.Log("Random value 2: " + _myFloats[1]);
                    Debug.Log("Random value 3: " + _myFloats[2]);
                    break;
                case 5: //Sent random values
                    if (ASL.ASLHelper.m_ASLObjects.TryGetValue(_id, out ASL.ASLObject myObject))
                    {
                        Debug.Log("The name of the object that sent these floats is: " + myObject.name);
                    }
                        break;
                default:
                    Debug.Log("This example does not do anything specific for m_MyFloats[3] above 5");
                    break;
            }
        }
    }
}