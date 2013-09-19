using EnhancedNavBall;
using System;
using UnityEngine;

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

    private Vector3 _radialPlusNavPosition;
    private Vector3 _normalPlusNavPosition;
    private Vector3 _radialMinusNavPosition;
    private Vector3 _normalMinusNavPosition;

    private const int navBallLayer = 12;

    public void LateUpdate()
    {
        if (FlightGlobals.ready == false)
            return;

        if (NavBallGameObject == null)
            return;
        
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

        BuildEnhancedNavBall();
        PerformCalculations();
        HideBehindVectors();
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
        bool visable = o.transform.localPosition.z > 0.0d;
        o.SetActive(visable);
    }

    private void PerformCalculations()
    {
        // Calculations thanks to Mechjeb
        Vessel vessel = FlightGlobals.ActiveVessel;
        Vector3d CoM = vessel.findWorldCenterOfMass();
        Vector3d up = (CoM - vessel.mainBody.position).normalized;
        Vector3d velocityVesselOrbit = vessel.orbit.GetVel();
        Vector3d velocityVesselOrbitUnit = velocityVesselOrbit.normalized;
        Vector3d radialPlus = Vector3d.Exclude(velocityVesselOrbit, up).normalized;
        Vector3d normalPlus = -Vector3d.Cross(radialPlus, velocityVesselOrbitUnit);

        if (_navBallProgradeMagnatude == 0f)
            _navBallProgradeMagnatude = _navBallBehaviour.progradeVector.localPosition.magnitude;

        // Apply to nav ball
        _radialPlus.transform.localPosition = _navBallBehaviour.attitudeGymbal * radialPlus * _navBallProgradeMagnatude;
        _normalPlus.transform.localPosition = _navBallBehaviour.attitudeGymbal * normalPlus * _navBallProgradeMagnatude;

        _radialMinus.transform.localPosition = _navBallBehaviour.attitudeGymbal * -radialPlus * _navBallProgradeMagnatude;
        _normalMinus.transform.localPosition = _navBallBehaviour.attitudeGymbal * -normalPlus * _navBallProgradeMagnatude;


        Utilities.DebugLog(LogLevel.Diagnostic,
            string.Format("MechJeb Calc: {0}\n{1}\n{2}\n{3}\n{4}\n{5}",
                BuildOutput(CoM, "CoM"),
                BuildOutput(up, "up"),
                BuildOutput(velocityVesselOrbit, "velocityVesselOrbit"),
                BuildOutput(velocityVesselOrbitUnit, "velocityVesselOrbitUnit"),
                BuildOutput(radialPlus, "radialPlus"),
                BuildOutput(normalPlus, "normalPlus")));

        Utilities.DebugLog(LogLevel.Diagnostic,
            string.Format("NavBall Calc: {0}\n{1}\n{2}\n{3}\n{4}\n{5}",
                BuildOutput(_navBallBehaviour.attitudeGymbal, "attitudeGymbal"),
                BuildOutput(_navBallProgradeMagnatude, "_navBallProgradeMagnatude"),
                BuildOutput(_radialPlus.transform.localPosition, "_radialPlus"),
                BuildOutput(_normalPlus.transform.localPosition, "_normalPlus"),
                BuildOutput(_radialMinus.transform.localPosition, "_radialMinus"),
                BuildOutput(_normalMinus.transform.localPosition, "_normalMinus")));
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

    private static string FloatFormat(float f)
    {
        return f.ToString("0.0000");
    }


    private void BuildEnhancedNavBall()
    {
        if (_normalPlus != null)
            return;

        Utilities.DebugLog(LogLevel.Minimal, "BuildEnhancedNavBall");

        _normalPlus = GameObject.CreatePrimitive(PrimitiveType.Plane);
        _normalMinus = GameObject.CreatePrimitive(PrimitiveType.Plane);
        _radialPlus = GameObject.CreatePrimitive(PrimitiveType.Plane);
        _radialMinus = GameObject.CreatePrimitive(PrimitiveType.Plane);

        Color radialColour = new Color(0, 1, 0.958f);
        Color normalColour = new Color(0.930f, 0, 1);

        SetupObject(
            _normalPlus,
            new Vector2(0.0f, 0.0f),
            normalColour);

        SetupObject(
            _normalMinus,
            new Vector2(0.3f, 0.0f),
            normalColour);

        SetupObject(
            _radialPlus,
            new Vector2(0.3f, 0.3f),
            radialColour);

        SetupObject(
            _radialMinus,
            new Vector2(0.0f, 0.3f),
            radialColour);
    }

    private void SetupObject(
        GameObject planeObject,
        Vector2 textureOffset,
        Color color)
    {
        LoadTexture();
        
        planeObject.layer = navBallLayer;// NavBallInstance.layer;
        planeObject.transform.parent = _vectorsPivot;
        planeObject.transform.localPosition = Vector3.zero;
        planeObject.transform.position = Vector3.zero;
        planeObject.transform.localRotation = Quaternion.Euler(90f, 180f, 0);
        planeObject.transform.localScale = planeObject.transform.localScale * 0.005f;

        planeObject.renderer.sharedMaterial = new Material(_maneuverGizmoTexture);
        planeObject.renderer.sharedMaterial.mainTextureScale = Vector2.one / 3;
        planeObject.renderer.sharedMaterial.mainTextureOffset = textureOffset;
        planeObject.renderer.sharedMaterial.color = color;
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