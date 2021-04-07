using UnityEngine;
using UnityEngine.UI;

namespace SimpleDemos
{
    /// <summary>
    /// Used to demonstrate how a Texture2D can be sent to other users. While the Texture being sent technically already exists, 
    /// there is nothing preventing the texture to be sent from being dynamically created or modified and then sent. It was 
    /// just easier to sent up this way than to create something dynamically.
    /// </summary>
    public class Send2DTexture_Example : MonoBehaviour
    {
        /// <summary>The starting texture of our sprite</summary>
        public Texture2D _StartingTexture;
        /// <summary>The texture we will send to other users</summary>
        public Texture2D _TextureToSend;
        /// <summary>A handle to the ASL object we will be using to send the texture and change the texture on</summary>
        public GameObject _MyASLObject;
        /// <summary>A handle to the button used to initiate the texture transfer</summary>
        private Button _SendTextureButton;

        /// <summary>Start function is used to set up the button listener</summary>
        void Start()
        {
            _SendTextureButton = GameObject.Find("SendTextureButton").GetComponent<Button>();
            _SendTextureButton.onClick.AddListener(SendAndChangeTexture);
        }

        /// <summary>
        /// An example of how to send a Texture2D.
        /// Optional parameters include to send it as a PNG (larger file, but allows for transparent pixels, default is JPG),
        /// and to synchronize every user's callback function (in this case, "ChangeSPriteTexture"), the default is to not synchronize
        /// callback functions, so once a user downloads an image, by default they will execute their given function instead of
        /// waiting for every other user to download the image as well.
        /// </summary>
        public void SendAndChangeTexture()
        {
            _MyASLObject.GetComponent<ASL.ASLObject>().SendAndSetTexture2D(_TextureToSend, ChangeSpriteTexture, false);
        }

        /// <summary>
        /// The function is called after the Texture2D is sent by all users. Note: All users must have this function defined for them 
        /// (just like with other callback functions except claim). Just because other users do not have the Texture2D being sent 
        /// does not mean they do not need this function. This function can be used to do whatever you want to do after the Texture2D 
        /// has been transferred. In this case, it is simply swapping the Texture2D out on the current sprite, changing the image 
        /// the sprite displays. This function can be named anything, but must be a static public void function.
        /// </summary>
        /// <param name="_myGameObject">The GameObject that was used to send the Texture2D</param>
        /// <param name="_myTexture2D">The Texture2D that was sent</param>
        static public void ChangeSpriteTexture(GameObject _myGameObject, Texture2D _myTexture2D)
        {
            Sprite newSprite = Sprite.Create(_myTexture2D, _myGameObject.GetComponent<SpriteRenderer>().sprite.rect, 
                new Vector2(0.5f, 0.5f), _myGameObject.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit);
            _myGameObject.GetComponent<SpriteRenderer>().sprite = newSprite;
        }


    }
}