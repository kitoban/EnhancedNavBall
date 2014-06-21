using UnityEngine;

namespace EnhancedNavBall
{
    internal static class ScalingNavBall
    {
        private static float? _maneuverVectorScaleDefault;
        private static float? _navballTextSizeDefault;

        private static ScreenSafeUI _screenSafeUi;
        private static NavBall _navball;
        private static NavBallBurnVector _navBallBurnVector;
        private static FlightUIController _flightUiController;

        public static void UpdateNavballScale(float navballScale)
        {
            if (_navBallBurnVector == null)
            {
                var maneuverVector = GameObject.Find("maneuverVector");
                _navBallBurnVector = maneuverVector.GetComponent<NavBallBurnVector>();
            }

            if (_screenSafeUi == null)
            {
                _screenSafeUi = ScreenSafeUI.fetch;
                _flightUiController = _screenSafeUi.GetComponent<FlightUIController>();
            }

            if (_navball == null)
            {
                var navballGameObject = GameObject.Find("NavBall");
                _navball = navballGameObject.GetComponent<NavBall>();
            }

            if ((_navBallBurnVector == null) || (_screenSafeUi == null) || (_navball == null) || _flightUiController == null)
                return;

            _screenSafeUi.centerAnchor.bottom.localScale = new Vector3(
                navballScale,
                navballScale,
                navballScale);

            if (_maneuverVectorScaleDefault.HasValue == false)
                _maneuverVectorScaleDefault = _navBallBurnVector.readoutSpacing;

            var scale = navballScale * _maneuverVectorScaleDefault.Value;
            _navBallBurnVector.readoutSpacing = scale;

            if (_navballTextSizeDefault.HasValue == false)
                _navballTextSizeDefault = _navball.headingText.textSize;

            scale = navballScale * _navballTextSizeDefault.Value;
            _navball.headingText.textSize = scale;
            _navBallBurnVector.ebtText.textSize = scale;
            _navBallBurnVector.TdnText.textSize = scale;
            _flightUiController.speed.textSize = scale;
            _flightUiController.spdCaption.textSize = scale;
        }

        public static float ResetNavballScale()
        {
            const float resetNavballScale = 1f;
            UpdateNavballScale(resetNavballScale);
            return resetNavballScale;
        }
        
        public static float GetScale()
        {
            var ui = ScreenSafeUI.fetch;
            
            if (ui != null)
            {
                return ui.centerAnchor.bottom.localScale.x;
            }
            else
            {
                return 1f;
            }
        }

        //private static float? _referenceRotation = null;
        //private static Vector3? _position;

        //public static void UpdateNavballRotation(float navballRotation)
        //{
        //    GameObject gameObject = GameObject.Find("maneuverVector");

        //    if (gameObject != null)
        //    {
        //        if (_referenceRotation.HasValue == false)
        //            _referenceRotation = gameObject.transform.localScale.x;

        //        gameObject.transform.localRotation = Quaternion.Euler(0, navballRotation, 0);
        //    }
        //}

        //////////////////////////////////////////////////////////////////////////////

        //public static float ResetNavballDistance()
        //{
        //    ScreenSafeUI ui = ScreenSafeUI.fetch;

        //    ui.centerAnchor.bottom.position = _position.Value;

        //    return ui.centerAnchor.bottom.position.y;
        //}

        //public static void UpdateNavballPostion(
        //    ScreenSafeUI ui,
        //    float newX)
        //{
        //    if (ui == null)
        //        return;

        //    ui.centerAnchor.bottom.position = new Vector3(
        //        newX,
        //        ui.centerAnchor.bottom.position.y,
        //        -0.1f);
        //}

        //public static float GetNavballPostion(ScreenSafeUI ui)
        //{
        //    if (ui == null)
        //        return 0.5f;

        //    return ui.centerAnchor.bottom.position.y;
        //}
    }
}