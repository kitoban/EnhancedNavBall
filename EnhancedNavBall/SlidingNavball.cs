// Credit: xEvilReeperx for the sliding navball code, released unrestricted and implemented into the Enhanced navball with permission.
using UnityEngine;

namespace EnhancedNavBall
{
    public static class SlidingNavBall
    {
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

        public static float ResetNavballPostion()
        {
            ScreenSafeUI ui = References.Instance.ScreenSafeUi;
            if (ui == null)
                return 0.5f;

            Vector3 resetNavballPostion = (ui.leftAnchor.bottom.position + ui.rightAnchor.bottom.position) * 0.5f;
            ui.centerAnchor.bottom.position = resetNavballPostion;
            return GetNavballPostion();
        }

        public static float GetNavballPostion()
        {
            ScreenSafeUI ui = References.Instance.ScreenSafeUi;
            if (ui == null)
                return 0.5f;

            return ui.centerAnchor.bottom.position.x;
        }
    }
}