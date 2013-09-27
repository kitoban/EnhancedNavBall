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

    private static readonly Color _radialColour = new Color(0, 1, 0.958f);
    private static readonly Color _maneuverColour = new Color(0, 0.1137f, 1, 0.5f);
    private static readonly Color _normalColour = new Color(0.930f, 0, 1);

    private CalculationStore _calculationStore;

    private const float _graphicOffset = 1f / 3f;
    private const int navBallLayer = 12;
    private const float _vectorSize = 0.025f;

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
            "maneuverGhost",
            _vectorSize);
        
        SetupObject(
            _ghostManeuver,
            new Vector2(_graphicOffset * 2, 0f),
            _maneuverColour,
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
        planeObject.layer = navBallLayer;
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

        PerformCalculations(vessel);
        CalculateManeuver(vessel);
        HideBehindVectors();
        UpdateGhostingVectors(vessel);
    }

    private void UpdateGhostingVectors(Vessel vessel)
    {
        if (vessel.patchedConicSolver.maneuverNodes.Count > 0)
        {
            _ghostManeuver.transform.localPosition = ProcessVectorForGhosting(_calculationStore.ManeuverPlus);
            _ghostManeuver.SetActive(_calculationStore.ManeuverPlus.z <= 0.7d);
        }
        else
        {
            _ghostManeuver.SetActive(false);
        }
    }

    private Vector3 ProcessVectorForGhosting(Vector3d vector)
    {
        Vector3 navballPoint = vector;
        Vector3 ghostPoint = new Vector3(
            navballPoint.x,
            navballPoint.y,
            0);

        Quaternion ghostDirection = Quaternion.LookRotation(ghostPoint);

        return ghostDirection * Vector3.forward * 0.08f;
    }

    private void CalculateManeuver(Vessel vessel)
    {
        Quaternion gymbal = _navBallBehaviour.attitudeGymbal;
        _calculationStore.RunManeuverCalculations(vessel, gymbal);

        if (vessel.patchedConicSolver.maneuverNodes.Count > 0)
        {
            _antiManeuverNode.transform.localPosition = -_calculationStore.ManeuverPlus * _navBallProgradeMagnatude;
        }
        else
        {
            _antiManeuverNode.transform.localPosition = _calculationStore.ManeuverPlus;
        }
    }

    private void HideBehindVectors()
    {
        TestVisibility(_radialPlus);
        TestVisibility(_radialMinus);
        TestVisibility(_normalPlus);
        TestVisibility(_normalMinus);
        TestVisibility(_antiManeuverNode);
    }

    private void TestVisibility(GameObject o)
    {
        if (o == null)
            return;

        bool visable;
            
        if (FlightUIController.speedDisplayMode == FlightUIController.SpeedDisplayModes.Surface
            || FlightUIController.speedDisplayMode == FlightUIController.SpeedDisplayModes.Target)
            visable = false;
        else
            visable = o.transform.localPosition.z > 0.0d;

        o.SetActive(visable);
    }

    private void PerformCalculations(Vessel vessel)
    {
        Quaternion gymbal = _navBallBehaviour.attitudeGymbal;
        _calculationStore.RunOrbitCalculations(vessel, gymbal);
        
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