using EnhancedNavBall.Config;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedNavBall
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class EnhancedNavBall : MonoBehaviour
    {
        private float _navBallProgradeMagnatude;
        private GameObject _ghostPivot;
        private Transform _ghostPivotTransform;
        private readonly Vector2 _mainTextureScale = Vector2.one / 3;
        private bool _isInPostDrawQueue = false;
        private bool _shouldBeInPostDrawQueue = false;

        private GameObject _normalPlus;
        private GameObject _normalMinus;
        private GameObject _radialMinus;
        private GameObject _radialPlus;
        private GameObject _antiManeuverNode;
        private GameObject _ghostManeuver;
        private GameObject _ghostPrograde;
        private GameObject _ghostRetrograde;

        private static readonly Color _radialColour = new Color(0, 1, 0.958f);
        private static readonly Color _maneuverColour = new Color(0, 0.1137f, 1, _manueverAlpha);
        private static readonly Color _normalColour = new Color(0.930f, 0, 1);
        private static readonly Color _progradeColour = new Color(0.84f, 0.98f, 0);

        private CalculationStore _calculationStore;
        private ConfigurationWindow _configWindow;
        public List<GameScenes> DrawScenes = new List<GameScenes> { GameScenes.FLIGHT };
        private NavballSettings _navballSettings;
        public float GhostOffset = 0.0236692f;

        private const float _manueverAlpha = 0.6f;
        private const float _ghostingAlpha = 0.8f;
        private const float _ghostingHideZ = 0.68f;
        private const float _ghostingHighestIntensity = 0.4f;
        private const float _graphicOffset = 1f / 3f;
        private const float _vectorSize = 0.025f;
        private const float _scaleFactorGhostCloseActual = _ghostingHideZ - _ghostingHighestIntensity;

        private float ScaledVectorSize
        {
            get { return _vectorSize; }
        }

        public void OnGUI()
        {
            //Do the GUI Stuff - basically get the workers draw stuff into the postrendering queue
            //If the two flags are different are we going in or out of the queue
            if (_shouldBeInPostDrawQueue != _isInPostDrawQueue)
            {
                if (_shouldBeInPostDrawQueue && !_isInPostDrawQueue)
                {
                    Utilities.DebugLogFormatted(LogLevel.Diagnostic, "Adding DrawGUI to PostRender Queue");

                    //reset any existing pane display
                    //WorkerObjectInstance.ResetPanes();

                    //Add to the queue
                    RenderingManager.AddToPostDrawQueue(5, DrawGUI);
                    _isInPostDrawQueue = true;

                    //if we are adding the renderer and we are in flight then do the daily version check if required
                    //if (HighLogic.LoadedScene == GameScenes.FLIGHT && Settings.DailyVersionCheck)
                    //    Settings.VersionCheck(false);

                }
                else
                {
                    Utilities.DebugLogFormatted(LogLevel.Diagnostic, "Removing DrawGUI from PostRender Queue");
                    RenderingManager.RemoveFromPostDrawQueue(5, DrawGUI);
                    _isInPostDrawQueue = false;
                }
            }
        }

        public void DrawGUI()
        {
            _configWindow.DrawUi();
        }

        #region Setup

        public void Awake()
        {
        }

        private void LoadFromSettings()
        {
            Utilities.DebugLogFormatted(LogLevel.Diagnostic, "NavballPosition: {0}", _navballSettings.NavballPosition);
            SlidingNavBall.UpdateNavballPostion(
                References.Instance.ScreenSafeUi,
                _navballSettings.NavballPosition);

            Utilities.DebugLogFormatted(LogLevel.Diagnostic, "NavballScale: {0}", _navballSettings.NavballScale);
            ScalingNavBall.UpdateNavballScale(_navballSettings.NavballScale);
        }


        public void Start()
        {
            _navballSettings = new NavballSettings();
            _navballSettings.Load();

            _calculationStore = new CalculationStore();

            BuildEnhancedNavBall();
            CreateManueverPlane();
            BuildGhostingLayer();

            _configWindow = new ConfigurationWindow(_navballSettings);
            LoadFromSettings();

            _configWindow.BuildIcon();

            //TestPlane();
        }

        internal void OnDestroy()
        {
            _configWindow.DestroyToolbarButton();
            References.Destroy();
        }

        private void TestPlane()
        {
            GameObject simplePlane = Utilities.CreateSimplePlane("Test Plane", 0.5f);

            SetupObjectPosition(
                simplePlane,
                References.Instance.VectorsPivotTransform);
            simplePlane.transform.localPosition = new Vector3(0, 0.25f, 0);

            simplePlane.renderer.sharedMaterial = new Material(References.Instance.ManeuverTexture);
            simplePlane.renderer.sharedMaterial.mainTextureScale = Vector2.one;
            simplePlane.renderer.sharedMaterial.mainTextureOffset = Vector2.zero;
            simplePlane.renderer.sharedMaterial.color = _radialColour;
        }

        private void BuildGhostingLayer()
        {
            _ghostPivot = new GameObject("ghostPivot");
            _ghostPivotTransform = _ghostPivot.transform;
            _ghostPivotTransform.parent = References.Instance.VectorsPivotTransform;
            _ghostPivotTransform.localPosition = new Vector3(0, 0, 0.01f);

            _ghostManeuver = Utilities.CreateSimplePlane(
                "ghostManeuver",
                ScaledVectorSize);

            SetupObject(
                _ghostManeuver,
                new Vector2(_graphicOffset * 2, 0f),
                _maneuverColour,
                _ghostPivotTransform);

            _ghostPrograde = Utilities.CreateSimplePlane(
                "ghostPrograde",
                ScaledVectorSize);

            SetupObject(
                _ghostPrograde,
                new Vector2(0f, _graphicOffset * 2),
                _progradeColour,
                _ghostPivotTransform);

            _ghostRetrograde = Utilities.CreateSimplePlane(
                "ghostRetrograde",
                ScaledVectorSize);

            SetupObject(
                _ghostRetrograde,
                new Vector2(_graphicOffset, _graphicOffset * 2),
                _progradeColour,
                _ghostPivotTransform);
        }

        private void CreateManueverPlane()
        {
            _antiManeuverNode = Utilities.CreateSimplePlane(
                "antiManeuver",
                ScaledVectorSize);

            SetupObject(
                _antiManeuverNode,
                new Vector2(_graphicOffset, _graphicOffset * 2),
                _radialColour,
                References.Instance.VectorsPivotTransform);
        }

        private void BuildEnhancedNavBall()
        {
            if (_normalPlus != null)
                return;

            Utilities.DebugLog(LogLevel.Minimal, "BuildEnhancedNavBall");

            _normalPlus = Utilities.CreateSimplePlane(
                "normalPlus",
                ScaledVectorSize);

            _normalMinus = Utilities.CreateSimplePlane(
                "normalMinus",
                ScaledVectorSize);

            _radialPlus = Utilities.CreateSimplePlane(
                "radialPlus",
                ScaledVectorSize);

            _radialMinus = Utilities.CreateSimplePlane(
                "radialMinus",
                ScaledVectorSize);

            SetupObject(
                _normalPlus,
                new Vector2(0.0f, 0.0f),
                _normalColour,
                References.Instance.VectorsPivotTransform);

            SetupObject(
                _normalMinus,
                new Vector2(_graphicOffset, 0.0f),
                _normalColour,
                References.Instance.VectorsPivotTransform);

            SetupObject(
                _radialPlus,
                new Vector2(_graphicOffset, _graphicOffset),
                _radialColour,
                References.Instance.VectorsPivotTransform);

            SetupObject(
                _radialMinus,
                new Vector2(0.0f, _graphicOffset),
                _radialColour,
                References.Instance.VectorsPivotTransform);
        }

        private void SetupObject(
            GameObject planeObject,
            Vector2 textureOffset,
            Color color,
            Transform parentTransform)
        {
            SetupObjectPosition(
                planeObject,
                parentTransform);

            planeObject.renderer.sharedMaterial = new Material(References.Instance.ManeuverTexture);
            planeObject.renderer.sharedMaterial.mainTextureScale = _mainTextureScale;
            planeObject.renderer.sharedMaterial.mainTextureOffset = textureOffset;
            planeObject.renderer.sharedMaterial.color = color;
        }

        private void SetupObjectPosition(
            GameObject planeObject,
            Transform parentTransform)
        {
            planeObject.layer = Constants.NavBallLayer;
            planeObject.transform.parent = parentTransform;
            planeObject.transform.localPosition = Vector3.zero;
            planeObject.transform.localRotation = Quaternion.Euler(90f, 180f, 0);
        }

        #endregion

        #region Update
        public void Update()
        {

            //Work out if we should be in the gui queue
            _shouldBeInPostDrawQueue = DrawScenes.Contains(HighLogic.LoadedScene);
        }

        public void LateUpdate()
        {
            Utilities.DebugLog(LogLevel.Diagnostic, "LateUpdate called");

            if (FlightGlobals.ready == false)
                return;

            Vessel vessel = FlightGlobals.ActiveVessel;

            Quaternion gymbal = References.Instance.Navball.attitudeGymbal;
            _calculationStore.RunCalculations(vessel, gymbal);

            UpdateRadialNormalVectors();
            CalculateManeuver();
            HideBehindVectors();
            UpdateGhostingVectors(vessel);
        }

        private void UpdateGhostingVectors(Vessel vessel)
        {
            if (_calculationStore.ManeuverPresent)
            {
                if (_calculationStore.ManeuverApplied)
                {
                    _ghostManeuver.transform.localPosition = ProcessVectorForGhosting(_calculationStore.ManeuverPlus);
                    _ghostManeuver.SetActive(_calculationStore.ManeuverPlus.z <= _ghostingHideZ);

                    float alpha;

                    if (_calculationStore.ManeuverPlus.z < _ghostingHighestIntensity)
                    {
                        alpha = _manueverAlpha;
                    }
                    else
                    {
                        alpha = GetAlphaForCloseActual(
                            _calculationStore.ManeuverPlus.z,
                            _manueverAlpha);
                    }


                    UpdateGhostingAlpha(
                        _ghostManeuver,
                        alpha);
                }
                else
                {
                    _ghostManeuver.SetActive(false);
                }

                _ghostPrograde.SetActive(false);
                _ghostRetrograde.SetActive(false);
            }
            else
            {
                _ghostManeuver.SetActive(false);

                switch (FlightUIController.speedDisplayMode)
                {
                    case FlightUIController.SpeedDisplayModes.Surface:
                        var ghostingDisabled = vessel.Landed || vessel.Splashed;
                        UpdateGhostingSurface(ghostingDisabled == false);
                        break;

                    case FlightUIController.SpeedDisplayModes.Orbit:
                        UpdateGhostingOrbit();
                        break;

                    case FlightUIController.SpeedDisplayModes.Target:
                        UpdateGhostingTarget();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            Utilities.DebugLog(LogLevel.Diagnostic, Utilities.BuildOutput(_ghostManeuver.transform.localPosition, "_ghostManeuver"));
            Utilities.DebugLog(LogLevel.Diagnostic, Utilities.BuildOutput(_ghostPrograde.transform.localPosition, "_ghostPrograde"));
            Utilities.DebugLog(LogLevel.Diagnostic, Utilities.BuildOutput(_ghostRetrograde.transform.localPosition, "_ghostRetrograde"));
            Utilities.DebugLogFormatted(LogLevel.Diagnostic, "_ghostManeuver active: {0}", _ghostManeuver.activeSelf);
            Utilities.DebugLogFormatted(LogLevel.Diagnostic, "_ghostPrograde active: {0}", _ghostPrograde.activeSelf);
            Utilities.DebugLogFormatted(LogLevel.Diagnostic, "_ghostRetrograde active: {0}", _ghostRetrograde.activeSelf);
        }

        private void UpdateGhostingTarget()
        {
            _ghostPrograde.SetActive(false);
            _ghostRetrograde.SetActive(false);
        }

        private void UpdateGhostingOrbit()
        {
            _ghostPrograde.transform.localPosition = ProcessVectorForGhosting(_calculationStore.ProgradeOrbit);
            _ghostPrograde.SetActive(_calculationStore.ProgradeOrbit.z <= _ghostingHideZ);
            CalculateAndUpdateGhostingAlpha(
                _ghostPrograde,
                _calculationStore.ProgradeOrbit.z,
                _ghostingAlpha);

            _ghostRetrograde.transform.localPosition = ProcessVectorForGhosting(-_calculationStore.ProgradeOrbit);
            _ghostRetrograde.SetActive(-_calculationStore.ProgradeOrbit.z <= _ghostingHideZ);
            CalculateAndUpdateGhostingAlpha(
                _ghostRetrograde,
                -_calculationStore.ProgradeOrbit.z,
                _ghostingAlpha);
        }

        private void UpdateGhostingSurface(bool ghostEnabled)
        {
            _ghostPrograde.transform.localPosition = ProcessVectorForGhosting(_calculationStore.ProgradeSurface);
            _ghostPrograde.SetActive(
                _calculationStore.ProgradeSurface.z <= _ghostingHideZ
                && ghostEnabled);
            CalculateAndUpdateGhostingAlpha(
                _ghostPrograde,
                _calculationStore.ProgradeSurface.z,
                _ghostingAlpha);

            _ghostRetrograde.transform.localPosition = ProcessVectorForGhosting(-_calculationStore.ProgradeSurface);
            _ghostRetrograde.SetActive(
                -_calculationStore.ProgradeSurface.z <= _ghostingHideZ
                && ghostEnabled);
            CalculateAndUpdateGhostingAlpha(
                _ghostRetrograde,
                -_calculationStore.ProgradeSurface.z,
                _ghostingAlpha);
        }

        private void CalculateAndUpdateGhostingAlpha(
            GameObject ghostVector,
            double inputZ,
            float ghostingAlpha)
        {
            float alpha;
            const float shiftFactor = 2.5f;

            if (inputZ < _ghostingHighestIntensity)
            {
                alpha = 1 / ((shiftFactor + 1) * _ghostingHighestIntensity) * ((float)inputZ + (_ghostingHighestIntensity * shiftFactor)) * ghostingAlpha;
            }
            else
            {
                alpha = GetAlphaForCloseActual(inputZ, ghostingAlpha);
            }

            UpdateGhostingAlpha(
                ghostVector,
                alpha);
        }

        private static void UpdateGhostingAlpha(
            GameObject ghostVector,
            float alpha)
        {
            ghostVector.renderer.sharedMaterial.color = ApplyGhostingAlpha(
                ghostVector.renderer.sharedMaterial.color,
                alpha);
        }

        private static float GetAlphaForCloseActual(
            double inputZ,
            float ghostingAlpha)
        {
            double scalePoint = inputZ - _ghostingHighestIntensity;
            float alpha = -(((1 / _scaleFactorGhostCloseActual) * (float)scalePoint) - 1) * ghostingAlpha;
            return alpha;
        }

        private static Color ApplyGhostingAlpha(
            Color colour,
            float ghostingAlpha)
        {
            return new Color(
                colour.r,
                colour.g,
                colour.b,
                ghostingAlpha);
        }

        private Vector3 ProcessVectorForGhosting(Vector3d vector)
        {
            Vector3 navballPoint = vector;
            Vector3 ghostPoint = new Vector3(
                navballPoint.x,
                navballPoint.y,
                0);

            Quaternion ghostDirection = Quaternion.LookRotation(ghostPoint);

            return ghostDirection * (Vector3.forward * (_navBallProgradeMagnatude + (GhostOffset)));
        }

        private void CalculateManeuver()
        {
            if (_calculationStore.ManeuverPresent)
            {
                _antiManeuverNode.transform.localPosition = -_calculationStore.ManeuverPlus * _navBallProgradeMagnatude;
                _antiManeuverNode.SetActive(_antiManeuverNode.transform.localPosition.z > 0.0d);
            }
            else
            {
                _antiManeuverNode.transform.localPosition = _calculationStore.ManeuverPlus;
                _antiManeuverNode.SetActive(false);
            }

            Utilities.DebugLog(LogLevel.Diagnostic, Utilities.BuildOutput(_antiManeuverNode.transform.localPosition, "_antiManeuverNode"));
            Utilities.DebugLogFormatted(LogLevel.Diagnostic, "_antiManeuverNode active: {0}", _antiManeuverNode.activeSelf);
        }

        private void HideBehindVectors()
        {
            TestVisibility(_radialPlus);
            TestVisibility(_radialMinus);
            TestVisibility(_normalPlus);
            TestVisibility(_normalMinus);
        }

        private void TestVisibility(GameObject o)
        {
            if (o == null)
                return;

            bool visable;

            if (FlightUIController.speedDisplayMode == FlightUIController.SpeedDisplayModes.Surface
                || FlightUIController.speedDisplayMode == FlightUIController.SpeedDisplayModes.Target)
            {
                visable = false;
            }
            else
            {

                visable = o.transform.localPosition.z > 0.0d
                    && _calculationStore.ManeuverPresent == false;
            }

            o.SetActive(visable);
            Utilities.DebugLogFormatted(LogLevel.Diagnostic, "{0} set active: {1}", o.name, visable);
        }

        private void UpdateRadialNormalVectors()
        {

            if (_navBallProgradeMagnatude == 0f)
                _navBallProgradeMagnatude = References.Instance.Navball.progradeVector.localPosition.magnitude;

            Utilities.DebugLogFormatted(LogLevel.Diagnostic, "Magnatude: {0}", References.Instance.Navball.progradeVector.localPosition.magnitude);

            //switch (FlightUIController.speedDisplayMode)
            //{
            //    case FlightUIController.SpeedDisplayModes.Surface:
            //    case FlightUIController.SpeedDisplayModes.Orbit:
            //        gymbal = _navBallBehaviour.attitudeGymbal;
            //        break;

            //    case FlightUIController.SpeedDisplayModes.Target:
            //        gymbal = _navBallBehaviour.relativeGymbal;
            //        break;

            //    default:
            //        throw new ArgumentOutOfRangeException();
            //}

            // Apply to nav ball
            _radialPlus.transform.localPosition = _calculationStore.RadialPlus * _navBallProgradeMagnatude;
            _normalPlus.transform.localPosition = _calculationStore.NormalPlus * _navBallProgradeMagnatude;

            _radialMinus.transform.localPosition = -_calculationStore.RadialPlus * _navBallProgradeMagnatude;
            _normalMinus.transform.localPosition = -_calculationStore.NormalPlus * _navBallProgradeMagnatude;

            Utilities.DebugLog(LogLevel.Diagnostic, Utilities.BuildOutput(_radialPlus.transform.localPosition, "_radialPlus"));
            Utilities.DebugLog(LogLevel.Diagnostic, Utilities.BuildOutput(_normalPlus.transform.localPosition, "_normalPlus"));
            Utilities.DebugLog(LogLevel.Diagnostic, Utilities.BuildOutput(_radialMinus.transform.localPosition, "_radialMinus"));
            Utilities.DebugLog(LogLevel.Diagnostic, Utilities.BuildOutput(_normalMinus.transform.localPosition, "_normalMinus"));
        }

        #endregion
    }
}