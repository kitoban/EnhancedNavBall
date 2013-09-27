public class CalculationStore
{
    public Vector3d RadialPlus;
    public Vector3d NormalPlus;
    public Vector3d ManeuverPlus = Vector3d.zero;

    public void RunOrbitCalculations(Vessel vessel)
    {
        // Calculations thanks to Mechjeb
        Vector3d CoM = vessel.findWorldCenterOfMass();
        Vector3d up = (CoM - vessel.mainBody.position).normalized;
        Vector3d velocityVesselOrbit = vessel.orbit.GetVel();
        Vector3d velocityVesselOrbitUnit = velocityVesselOrbit.normalized;
        RadialPlus = Vector3d.Exclude(velocityVesselOrbit, up).normalized;
        NormalPlus = -Vector3d.Cross(RadialPlus, velocityVesselOrbitUnit);
    }

    public void RunManeuverCalculations(Vessel vessel)
    {
        if (vessel.patchedConicSolver.maneuverNodes.Count > 0)
        {
            Vector3d burnVector = vessel.patchedConicSolver.maneuverNodes[0].GetBurnVector(vessel.orbit);
            ManeuverPlus = burnVector.normalized;
        }
    }
}