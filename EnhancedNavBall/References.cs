
using UnityEngine;

namespace EnhancedNavBall
{
    internal class References
    {
        private static References _instance;

        public static References Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new References();

                return _instance;
            }
        }

        private ScreenSafeUI _screenSafeUi;
        public ScreenSafeUI ScreenSafeUi 
        { 
            get { return _screenSafeUi; } 
        }

        private NavBall _navball;
        public NavBall Navball
        {
            get { return _navball; } 
        }

        private Transform _vectorsPivotTransform;
        public Transform VectorsPivotTransform
        {
            get { return _vectorsPivotTransform; }
        }

        private Material _navBallTexture;
        public Material NavballTexture
        {
            get { return _navBallTexture; }
        }

        private Material _maneuverTexture;
        public Material ManeuverTexture
        {
            get { return _maneuverTexture; }
        }

        private NavBallBurnVector _navBallBurnVector;
        public NavBallBurnVector NavBallBurnVector
        {
            get { return _navBallBurnVector; }
        }

        private FlightUIController _flightUiController;
        public FlightUIController FlightUIController
        {
            get { return _flightUiController; }
        }

        public References()
        {
            _screenSafeUi = ScreenSafeUI.fetch;
            _flightUiController = _screenSafeUi.GetComponent<FlightUIController>();

            GameObject navballGameObject = GameObject.Find("NavBall");
            _navball = navballGameObject.GetComponent<NavBall>();
            _navBallTexture = _navball.navBall.renderer.sharedMaterial;
            _vectorsPivotTransform = navballGameObject.transform.FindChild("vectorsPivot");
            
            ManeuverGizmo maneuverGizmo = MapView.ManeuverNodePrefab.GetComponent<ManeuverGizmo>();
            _maneuverTexture = maneuverGizmo.handleNormal.flag.renderer.sharedMaterial;

            var maneuverVector = GameObject.Find("maneuverVector");
            _navBallBurnVector = maneuverVector.GetComponent<NavBallBurnVector>();
        }

        public static void Destroy()
        {
            _instance._screenSafeUi = null;
            _instance._navball = null;
            _instance._vectorsPivotTransform = null;
            _instance._navBallTexture = null;
            _instance._maneuverTexture = null;
            _instance._navBallBurnVector = null;
            _instance._flightUiController = null;
            _instance = null;
        }
    }
}
