using UnityEngine;

namespace EnhancedNavBall
{
    internal static class ScalingNavBall
    {
        private static float? _maneuverVectorScaleDefault;
        private static float? _navballTextSizeDefault;

        public static void UpdateNavballScale(float navballScale)
        {
            References.Instance.ScreenSafeUi.centerAnchor.bottom.localScale = new Vector3(
                navballScale,
                navballScale,
                navballScale);

            var navBallBurnVector = References.Instance.NavBallBurnVector;

            if (_maneuverVectorScaleDefault.HasValue == false)
                _maneuverVectorScaleDefault = navBallBurnVector.readoutSpacing;

            var scale = navballScale * _maneuverVectorScaleDefault.Value;
            navBallBurnVector.readoutSpacing = scale;

            var navBall = References.Instance.Navball;

            if (_navballTextSizeDefault.HasValue == false)
                _navballTextSizeDefault = navBall.headingText.textSize;

            var flightUiController = References.Instance.FlightUIController;

            scale = navballScale * _navballTextSizeDefault.Value;
            navBall.headingText.textSize = scale;
            navBallBurnVector.ebtText.textSize = scale;
            navBallBurnVector.TdnText.textSize = scale;
            flightUiController.speed.textSize = scale;
            flightUiController.spdCaption.textSize = scale;
        }

        public static float ResetNavballScale()
        {
            const float resetNavballScale = 1f;
            UpdateNavballScale(resetNavballScale);
            return resetNavballScale;
        }
        
        public static float GetScale()
        {
            var ui = References.Instance.ScreenSafeUi;
            
            if (ui != null)
            {
                return ui.centerAnchor.bottom.localScale.x;
            }
            else
            {
                return 1f;
            }
        }
    }
}