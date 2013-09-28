using EnhancedNavBall;
using System;
using UnityEngine;

[KSPAddon(KSPAddon.Startup.Flight, false)]
public class EnhancedNavBallBehaviour : MonoBehaviour
{
    public GameObject NavBallGameObject { get; set; }

    private NavBall _navBallBehaviour;
    private float _navBallProgradeMagnatude;
    private Material _maneuverGizmoTexture;
    private Transform _vectorsPivotTransform;
    private GameObject _ghostPivot;
    private Transform _ghostPivotTransform;
    private readonly Vector2 _mainTextureScale = Vector2.one / 3;

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

    private const float _manueverAlpha = 0.6f;
    private const float _ghostingAlpha = 0.8f;
    private const float _ghostingHideZ = 0.68f;
    private const float _ghostingHighestIntensity = 0.4f;
    private const float _graphicOffset = 1f / 3f;
    private const int _navBallLayer = 12;
    private const float _vectorSize = 0.025f;
    private const float _ghostingPositionOffset = 0.08f;

    #region Setup

    public void Awake() { }
    public void Start() 
    {
        _calculationStore = new CalculationStore();

        NavBallGameObject = GameObject.Find("NavBall");
        
        if (_vectorsPivotTransform == null)
        {
            _vectorsPivotTransform = NavBallGameObject.transform.FindChild("vectorsPivot");
        }

        if (_vectorsPivotTransform == null)
            return;

        if (_navBallBehaviour == null)
        {
            _navBallBehaviour = NavBallGameObject.GetComponent<NavBall>();
        }

        LoadTexture();
        BuildEnhancedNavBall();
        CreateManueverPlane();
        BuildGhostingLayer();


        //TestPlane();
    }

    private void TestPlane()
    {
        GameObject simplePlane = Utilities.CreateSimplePlane("Test Plane", 0.5f);

        SetupObjectPosition(simplePlane,
            _vectorsPivotTransform);
        simplePlane.transform.localPosition = new Vector3(0, 0.25f, 0);

        simplePlane.renderer.sharedMaterial = new Material(_maneuverGizmoTexture);
        simplePlane.renderer.sharedMaterial.mainTextureScale = Vector2.one;
        simplePlane.renderer.sharedMaterial.mainTextureOffset = Vector2.zero;
        simplePlane.renderer.sharedMaterial.color = _radialColour;
    }

    private void LoadTexture()
    {
        if (_maneuverGizmoTexture == null)
        {
            if (MapView.ManeuverNodePrefab == null)
                throw new ArgumentNullException("MapView.ManeuverNodePrefab");

            ManeuverGizmo maneuverGizmo = MapView.ManeuverNodePrefab.GetComponent<ManeuverGizmo>();
            if (maneuverGizmo == null)
                throw new ArgumentNullException("maneuverGizmo");

            ManeuverGizmoHandle maneuverGizmoHandle = maneuverGizmo.handleNormal;

            if (maneuverGizmoHandle == null)
                throw new ArgumentNullException("maneuverGizmoHandle");

            Transform transform = maneuverGizmoHandle.flag;

            if (transform == null)
                throw new ArgumentNullException("transform");

            Renderer renderer1 = transform.renderer;
            if (renderer1 == null)
                throw new ArgumentNullException("renderer1");

            _maneuverGizmoTexture = renderer1.sharedMaterial;
        }
    }

    private void BuildGhostingLayer()
    {
        _ghostPivot = new GameObject("ghostPivot");
        _ghostPivotTransform = _ghostPivot.transform;
        _ghostPivotTransform.parent = _vectorsPivotTransform;
        _ghostPivotTransform.localPosition = new Vector3(0, 0, 0.01f);

        _ghostManeuver = Utilities.CreateSimplePlane(
            "ghostManeuver",
            _vectorSize);
        
        SetupObject(
            _ghostManeuver,
            new Vector2(_graphicOffset * 2, 0f),
            _maneuverColour,
            _ghostPivotTransform);
        
        _ghostPrograde = Utilities.CreateSimplePlane(
            "ghostPrograde",
            _vectorSize);

        SetupObject(
            _ghostPrograde,
            new Vector2(0f, _graphicOffset * 2),
            _progradeColour,
            _ghostPivotTransform);

        _ghostRetrograde = Utilities.CreateSimplePlane(
            "ghostRetrograde",
            _vectorSize);

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
            _vectorSize);
        
        SetupObject(
            _antiManeuverNode,
            new Vector2(_graphicOffset, _graphicOffset * 2),
            _radialColour,
            _vectorsPivotTransform);
    }

    private void BuildEnhancedNavBall()
    {
        if (_normalPlus != null)
            return;

        Utilities.DebugLog(LogLevel.Minimal, "BuildEnhancedNavBall");

        _normalPlus = Utilities.CreateSimplePlane(
            "normalPlus",
            _vectorSize);

        _normalMinus = Utilities.CreateSimplePlane(
            "normalMinus",
            _vectorSize);

        _radialPlus = Utilities.CreateSimplePlane(
            "radialPlus",
            _vectorSize);

        _radialMinus = Utilities.CreateSimplePlane(
            "radialMinus",
            _vectorSize);

        SetupObject(
            _normalPlus,
            new Vector2(0.0f, 0.0f),
            _normalColour,
            _vectorsPivotTransform);

        SetupObject(
            _normalMinus,
            new Vector2(_graphicOffset, 0.0f),
            _normalColour,
            _vectorsPivotTransform);

        SetupObject(
            _radialPlus,
            new Vector2(_graphicOffset, _graphicOffset),
            _radialColour,
            _vectorsPivotTransform);

        SetupObject(
            _radialMinus,
            new Vector2(0.0f, _graphicOffset),
            _radialColour,
            _vectorsPivotTransform);
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

        planeObject.renderer.sharedMaterial = new Material(_maneuverGizmoTexture);
        planeObject.renderer.sharedMaterial.mainTextureScale = _mainTextureScale;
        planeObject.renderer.sharedMaterial.mainTextureOffset = textureOffset;
        planeObject.renderer.sharedMaterial.color = color;
    }

    private void SetupObjectPosition(
        GameObject planeObject,
        Transform parentTransform)
    {
        planeObject.layer = _navBallLayer;
        planeObject.transform.parent = parentTransform;
        planeObject.transform.localPosition = Vector3.zero;
        planeObject.transform.localRotation = Quaternion.Euler(90f, 180f, 0);
    }

    #endregion

    #region Update

    public void LateUpdate()
    {
        if (FlightGlobals.ready == false)
            return;

        Vessel vessel = FlightGlobals.ActiveVessel;

        Quaternion gymbal = _navBallBehaviour.attitudeGymbal;
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
            _ghostManeuver.transform.localPosition = ProcessVectorForGhosting(_calculationStore.ManeuverPlus);
            _ghostManeuver.SetActive(_calculationStore.ManeuverPlus.z <= _ghostingHideZ);

            _ghostPrograde.SetActive(false);
            _ghostRetrograde.SetActive(false);
        }
        else
        {
            _ghostManeuver.SetActive(false);

            switch (FlightUIController.speedDisplayMode)
            {
                case FlightUIController.SpeedDisplayModes.Surface:
                    UpdateGhostingSurface(vessel.Landed);
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
        UpdateGhostingAlpha(_ghostPrograde, _calculationStore.ProgradeOrbit.z);
        
        _ghostRetrograde.transform.localPosition = ProcessVectorForGhosting(-_calculationStore.ProgradeOrbit);
        _ghostRetrograde.SetActive(-_calculationStore.ProgradeOrbit.z <= _ghostingHideZ);
        UpdateGhostingAlpha(_ghostRetrograde, -_calculationStore.ProgradeOrbit.z);
    }

    private void UpdateGhostingSurface(bool landed)
    {
        _ghostPrograde.transform.localPosition = ProcessVectorForGhosting(_calculationStore.ProgradeSurface);
        _ghostPrograde.SetActive(
            _calculationStore.ProgradeSurface.z <= _ghostingHideZ
            && landed == false);
        UpdateGhostingAlpha(_ghostPrograde, _calculationStore.ProgradeSurface.z);

        _ghostRetrograde.transform.localPosition = ProcessVectorForGhosting(-_calculationStore.ProgradeSurface);
        _ghostRetrograde.SetActive(
            -_calculationStore.ProgradeSurface.z <= _ghostingHideZ
            && landed == false);
        UpdateGhostingAlpha(_ghostRetrograde, -_calculationStore.ProgradeSurface.z);
    }

    private void UpdateGhostingAlpha(
        GameObject ghostVector,
        double inputZ)
    {
        float alpha;
        const float shiftFactor = 2.5f;

        if (inputZ < _ghostingHighestIntensity)
        {
            alpha = 1 / ((shiftFactor + 1) * _ghostingHighestIntensity) * ((float)inputZ + (_ghostingHighestIntensity * shiftFactor)) * _ghostingAlpha;
        }
        else
        {
            double scalePoint = inputZ - _ghostingHighestIntensity;
            const float scaleFactor = _ghostingHideZ - _ghostingHighestIntensity;

            alpha = -(((1 / scaleFactor) * (float)scalePoint) - 1) * _ghostingAlpha;
            
            Utilities.DebugLog(LogLevel.Diagnostic,
                string.Format(
@"UpdateGhostingAlpha
    scalePoint: {0}
    scaleFactor: {1}
    alpha: {2}",
                    Utilities.FloatFormat((float)scalePoint),
                    Utilities.FloatFormat(scaleFactor),
                    Utilities.FloatFormat(alpha)));

        }

        ghostVector.renderer.sharedMaterial.color = ApplyGhostingAlpha(ghostVector.renderer.sharedMaterial.color, alpha);
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

        return ghostDirection * Vector3.forward * _ghostingPositionOffset;
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
    }

    private void UpdateRadialNormalVectors()
    {
        if (_navBallProgradeMagnatude == 0f)
            _navBallProgradeMagnatude = _navBallBehaviour.progradeVector.localPosition.magnitude;
        

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
        
        //Utilities.DebugLog(LogLevel.Diagnostic,
        //    string.Format("MechJeb Calc: \n{0}\n{1}\n{2}\n{3}\n{4}\n{5}",
        //        BuildOutput(CoM, "CoM"),
        //        BuildOutput(up, "up"),
        //        BuildOutput(velocityVesselOrbit, "velocityVesselOrbit"),
        //        BuildOutput(velocityVesselOrbitUnit, "velocityVesselOrbitUnit"),
        //        BuildOutput(radialPlus, "radialPlus"),
        //        BuildOutput(normalPlus, "normalPlus")));

        //Utilities.DebugLog(LogLevel.Diagnostic,
        //    string.Format("NavBall Calc: \n{0}\n{1}\n{2}\n{3}\n{4}\n{5}",
        //        BuildOutput(gymbal, "attitudeGymbal"),
        //        BuildOutput(_navBallProgradeMagnatude, "_navBallProgradeMagnatude"),
        //        BuildOutput(_radialPlus.transform.localPosition, "_radialPlus"),
        //        BuildOutput(_normalPlus.transform.localPosition, "_normalPlus"),
        //        BuildOutput(_radialMinus.transform.localPosition, "_radialMinus"),
        //        BuildOutput(_normalMinus.transform.localPosition, "_normalMinus")));
    }

    #endregion
}