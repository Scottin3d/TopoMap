using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ASL
{
    /// <summary>
    /// An Slider with Echo for ASL. You should not need to modify this script. However, when using an ASLSliderWithEcho, you will want to modify ASLSliderController
    /// as this class maintains all of the sliders and tells each one what to do when their value changes. This class simply initializes these sliders, maintains their state,
    /// and passes the slider values to other users.
    /// and allow
    /// </summary>
    public class ASLSliderWithEcho : MonoBehaviour
    {
        /// <summary> The UI Slider</summary>
        public Slider TheSlider = null;
        /// <summary>The UI Text which echoes the slider's value</summary>
        public Text TheEcho = null;
        /// <summary>The UI Text which represents the label of the slider</summary>
        public Text TheLabel = null;

        /// <summary>The slider's previous value. Used to ensure multiple packets don't get sent for a value changed by other users</summary>
        private float m_OldSliderValue;
        /// <summary>The delegate to be called when the slider is changed</summary>
        /// <param name="v">The new value of the slider</param>
        private delegate void SliderCallbackDelegate(float v);
        /// <summary>The function that will be called when the slider is changed - sends the packet to all users</summary>
        private SliderCallbackDelegate mCallBack = null; 

        /// <summary>
        /// Ensures the Slider, Echo, and Label are properly initialized as well as adds a listener to the slider to monitor when its value changes
        /// </summary>
        void Start()
        {
            Debug.Assert(TheSlider != null);
            Debug.Assert(TheEcho != null);
            Debug.Assert(TheLabel != null);

            TheSlider.onValueChanged.AddListener(SliderValueChange);
        }

        /// <summary>Sets the slider's function to call when its value changes</summary>
        /// <param name="listener">The function to call when this slider's value changes</param>
        private void SetSliderListener(SliderCallbackDelegate listener)
        {
            mCallBack = listener;
        }

        /// <summary>
        /// When the slider's value changes, this function is executed, which executes the slider's callback function
        /// </summary>
        /// <param name="v">The new float value</param>
        void SliderValueChange(float v)
        {
            mCallBack?.Invoke(v);
        }

        /// <summary>Returns the current slider value</summary>
        /// <returns>The current slider value</returns>
        public float GetSliderValue() { return TheSlider.value; }

        /// <summary> Returns the Old slider value </summary>
        /// <returns>The old slider value</returns>
        public float GetOldSliderValue() { return m_OldSliderValue; }

        /// <summary>Sets the label of the slider</summary>
        /// <param name="l">The to be label of the slider</param>
        private void SetSliderLabel(string l) { TheLabel.text = l; }

        /// <summary>Sets the slider's value</summary>
        /// <param name="v">The to be value of the slider</param>
        private void SetSliderValue(float v) { TheSlider.value = v; SliderValueChange(v); }

        /// <summary>Sets the range of the slider</summary>
        /// <param name="min">The minimum value of the slider</param>
        /// <param name="max">The maximum value of the slider</param>
        /// <param name="v">The current value of the slider</param>
        private void InitSliderRange(float min, float max, float v)
        {
            TheSlider.minValue = min;
            TheSlider.maxValue = max;
            SetSliderValue(v);
        }

        /// <summary>Updates the slider value, old value, and echo </summary>
        /// <param name="_newFloat">The new value of the slider</param>
        public void UpdateSlider(float _newFloat)
        {
            m_OldSliderValue = _newFloat; //Set old value to prevent multiple sends of _newFloat value
            TheSlider.value = _newFloat; //Set Slider value
            TheEcho.text = _newFloat.ToString("0.0000"); //Set the Slider Echo
        }

        /// <summary>Initializes the slider</summary>
        /// <param name="_SliderLabel">The label of the slider</param>
        /// <param name="_startingValue">The starting value of the slider</param>
        /// <param name="_endingValue">The ending value of the slider</param>
        /// <param name="_initialValue">THe current value of the slider</param>
        /// <param name="_functionToCallAfterChangingSlider">The function to be called after the value of this slider is received from other players</param>
        public void InitilizeSlider(string _SliderLabel, float _startingValue, float _endingValue, float _initialValue, ASLObject.FloatCallback _functionToCallAfterChangingSlider)
        {
            SetSliderLabel(_SliderLabel);
            InitSliderRange(_startingValue, _endingValue, _initialValue);
            m_OldSliderValue = _initialValue;
            SetSliderListener(FunctionToCallWhenSliderIsChangedByAUser);
            gameObject.GetComponent<ASLObject>()._LocallySetFloatCallback(_functionToCallAfterChangingSlider);
        }

        /// <summary>
        /// This function is called whenever a slider's value is changed, sending a packet to all users to inform them of the change
        /// </summary>
        /// <param name="_newValue">The newest value of the slider</param>
        private void FunctionToCallWhenSliderIsChangedByAUser(float _newValue)
        {
            //By keeping track of the old value, we prevent this function from triggering when it gets updated from other players,
            //thus preventing sending multiple same value numbers
            if (!Mathf.Approximately(GetOldSliderValue(), _newValue))
            {
                gameObject.GetComponent<ASLObject>().SendAndSetClaim(() =>
                {
                    float[] myFloatArray = { _newValue };
                    GetComponent<ASLObject>().SendFloatArray(myFloatArray);
                });
            }
        }

    }
}