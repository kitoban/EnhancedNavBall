/******************************************************************************
 *                   Sliding NavBall for Kerbal Space Program                 *
 *                                                                            *
 * Version 1.0 (first release)                                                *
 * Created: 10/27/2013                                                        *
 * ************************************************************************** *
 
This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
 * ***************************************************************************/

// xEvilReeperx  

using UnityEngine;

namespace EnhancedNavBall
{
    //[KSPAddon(KSPAddon.Startup.Flight, false)]
    public class SlidingNavBall : MonoBehaviour
    {
        ScreenSafeUIButton.ButtonPressCallback originalCallback;
        float buttonOffset = 0.0f;
        private ScreenSafeUI _screenSafeUi;


        public void Start()
        {
            Debug.Log("SlidingNavBall start ");

            GameObject ce = GameObject.Find("collapseExpandButton");

            if (ce == null || ce.GetComponent<ScreenSafeUIButton>() == null)
            {
                Debug.LogWarning("SlidingNavBall: failed to find \"collapseExpandButton\"; you will not be able to move the NavBall.");
            }
            else
            {
                _screenSafeUi = ScreenSafeUI.fetch;

                ScreenSafeUIButton collapseButton = ce.GetComponent<ScreenSafeUIButton>();

                if (collapseButton != null)
                {
                    
                    originalCallback = collapseButton.OnRightPress;
                    collapseButton.OnRightPress = OnRightPress;
                    collapseButton.enableRightClick = true;

                    float textureWidthScreen = 
                        (float)collapseButton.renderer.material.mainTexture.width 
                        / collapseButton.mapXFrames * collapseButton.renderer.transform.localScale.x / (float)Screen.width;

                    buttonOffset = 
                        (
                            ScreenSafeUI.referenceCam.WorldToScreenPoint(collapseButton.transform.position).x
                            - ScreenSafeUI.referenceCam.WorldToScreenPoint(_screenSafeUi.centerAnchor.bottom.transform.position).x
                        ) 
                        / ScreenSafeUI.referenceCam.WorldToScreenPoint(_screenSafeUi.rightAnchor.bottom.transform.position).x
                        + textureWidthScreen 
                        * 0.5f;
                }
            }
        }



        public void OnRightPress()
        {
            if (_screenSafeUi == null)
                return;

            ScreenSafeUI ui = _screenSafeUi;

            float newX = Mathf.Clamp(ui.leftAnchor.bottom.position.x + (ui.rightAnchor.bottom.position.x - ui.leftAnchor.bottom.position.x) * ((Input.mousePosition.x / Screen.width) - buttonOffset), ui.leftAnchor.bottom.position.x, ui.rightAnchor.bottom.position.x - buttonOffset);

            //Debug.Log("rightClamp = " + ui.rightAnchor.bottom.position.x);
            //Debug.Log("Moving NavBall to newx: " + newX);

            // the -1.0 puts the NavBall in front of the controls on the bottom-left; otherwise
            // some of them will appear on top of the NavBall due to zordering
            UpdateNavballPostion(
                _screenSafeUi,
                newX);

            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                Debug.Log("Restoring NavBall to original centered position");
                ui.centerAnchor.bottom.position = (ui.leftAnchor.bottom.position + ui.rightAnchor.bottom.position) * 0.5f;
            }

            if (originalCallback != null)
                originalCallback();
        }

        public static void UpdateNavballPostion(
            ScreenSafeUI ui,
            float newX)
        {
            if (ui == null)
                return;

            ui.centerAnchor.bottom.position = new Vector3(
                newX,
                ui.centerAnchor.bottom.position.y,
                -0.01f);
        }

        public static float ResetNavballPostion(ScreenSafeUI ui)
        {
            if (ui == null)
                return 0.5f;

            Vector3 resetNavballPostion = (ui.leftAnchor.bottom.position + ui.rightAnchor.bottom.position) * 0.5f;
            ui.centerAnchor.bottom.position = resetNavballPostion;
            return GetNavballPostion(ui);
        }

        public static float GetNavballPostion(ScreenSafeUI ui)
        {
            if (ui == null)
                return 0.5f;

            return ui.centerAnchor.bottom.position.x;
        }
    }
}