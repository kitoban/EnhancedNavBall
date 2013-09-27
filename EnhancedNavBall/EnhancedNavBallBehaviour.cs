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
    private Transform _vectorsPivot;

    private GameObject _normalPlus;
    private GameObject _normalMinus;
    private GameObject _radialMinus;
    private GameObject _radialPlus;
    private GameObject _antiManeuverNode;
    
    private static readonly Color _radialColour = new Color(0, 1, 0.958f);
    private static readonly Color _normalColour = new Color(0.930f, 0, 1);

    private CalculationStore _calculationStore;

    private readonly Vector2 _mainTextureScale = Vector2.one / 3;
    private const float _graphicOffset = 1f / 3f;
    private const int navBallLayer = 12;
    private const float _vectorSize = 0.025f;

    public void Awake() { }
    public void Start() 
    {
        _calculationStore = new CalculationStore();

        NavBallGameObject = GameObject.Find("NavBall");
        
        if (_vectorsPivot == null)
        {
            _vectorsPivot = NavBallGameObject.transform.FindChild("vectorsPivot");
        }

        if (_vectorsPivot == null)
            return;

        if (_navBallBehaviour == null)
        {
            _navBallBehaviour = NavBallGameObject.GetComponent<NavBall>();
        }

        LoadTexture();
        BuildEnhancedNavBall();
        CreateManueverPlane();

        //TestPlane();
    }

    private void TestPlane()
    {
        GameObject simplePlane = Utilities.CreateSimplePlane("Test Plane", 0.5f);

        SetupObjectPosition(simplePlane);
        simplePlane.transform.localPosition = new Vector3(0, 0.25f, 0);

        simplePlane.renderer.sharedMaterial = new Material(_maneuverGizmoTexture);
        simplePlane.renderer.sharedMaterial.mainTextureScale = Vector2.one;
        simplePlane.renderer.sharedMaterial.mainTextureOffset = Vector2.zero;
        simplePlane.renderer.sharedMaterial.color = _radialColour;
    }

    private void CreateManueverPlane()
    {
        _antiManeuverNode = Utilities.CreateSimplePlane(
            "antiManeuver",
            _vectorSize);
        
        SetupObject(
            _antiManeuverNode,
            new Vector2(_graphicOffset, _graphicOffset * 2),
            _radialColour);
    }

    public void LateUpdate()
    {
        if (FlightGlobals.ready == false)
            return;

        Vessel vessel = FlightGlobals.ActiveVessel;

        PerformCalculations(vessel);
        CalculateManeuver(vessel);
        HideBehindVectors();
    }

    private void CalculateManeuver(Vessel vessel)
    {
        _calculationStore.RunManeuverCalculations(vessel);

        if (vessel.patchedConicSolver.maneuverNodes.Count > 0)
        {
            _antiManeuverNode.transform.localPosition = _navBallBehaviour.attitudeGymbal * -_calculationStore.ManeuverPlus * _navBallProgradeMagnatude;
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
        _calculationStore.RunOrbitCalculations(vessel);


        if (_navBallProgradeMagnatude == 0f)
            _navBallProgradeMagnatude = _navBallBehaviour.progradeVector.localPosition.magnitude;
        
        Quaternion gymbal = _navBallBehaviour.attitudeGymbal;

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
        _radialPlus.transform.localPosition = gymbal * _calculationStore.RadialPlus * _navBallProgradeMagnatude;
        _normalPlus.transform.localPosition = gymbal * _calculationStore.NormalPlus * _navBallProgradeMagnatude;

        _radialMinus.transform.localPosition = gymbal * -_calculationStore.RadialPlus * _navBallProgradeMagnatude;
        _normalMinus.transform.localPosition = gymbal * -_calculationStore.NormalPlus * _navBallProgradeMagnatude;
        
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

    private string BuildOutput(
        Quaternion quart,
        string paramName)
    {
        return string.Format(
            "{0} Euler - x:{1} y:{2} z:{3}",
            paramName,
            FloatFormat(quart.eulerAngles.x),
            FloatFormat(quart.eulerAngles.y),
            FloatFormat(quart.eulerAngles.z));
    }

    private string BuildOutput(
        float f,
        string paramName)
    {
        return string.Format(
            "{0} {1}",
            paramName,
            FloatFormat(f));
    }

    private string BuildOutput(
        Vector3 vector3,
        string paramName)
    {
        return string.Format(
            "{0} Vector - x:{1} y:{2} z:{3}",
            paramName,
            FloatFormat(vector3.x),
            FloatFormat(vector3.y),
            FloatFormat(vector3.z));
    }

    private string BuildOutput(
        Vector2 vector2,
        string paramName)
    {
        return string.Format(
            "{0} Vector - x:{1} y:{2}",
            paramName,
            FloatFormat(vector2.x),
            FloatFormat(vector2.y));
    }

    private static string FloatFormat(float f)
    {
        return f.ToString("0.0000");
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
            _normalColour);

        SetupObject(
            _normalMinus,
            new Vector2(_graphicOffset, 0.0f),
            _normalColour);

        SetupObject(
            _radialPlus,
            new Vector2(_graphicOffset, _graphicOffset),
            _radialColour);

        SetupObject(
            _radialMinus,
            new Vector2(0.0f, _graphicOffset),
            _radialColour);
    }

    private void SetupObject(
        GameObject planeObject,
        Vector2 textureOffset,
        Color color)
    {
        SetupObjectPosition(planeObject);

        planeObject.renderer.sharedMaterial = new Material(_maneuverGizmoTexture);
        planeObject.renderer.sharedMaterial.mainTextureScale = _mainTextureScale;
        planeObject.renderer.sharedMaterial.mainTextureOffset = textureOffset;
        planeObject.renderer.sharedMaterial.color = color;
    }

    private void SetupObjectPosition(GameObject planeObject)
    {
        planeObject.layer = navBallLayer;
        planeObject.transform.parent = _vectorsPivot;
        planeObject.transform.localPosition = Vector3.zero;
        planeObject.transform.localRotation = Quaternion.Euler(90f, 180f, 0);
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
}