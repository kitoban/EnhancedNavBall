using UnityEngine;

namespace EnhancedNavBall
{
    public class CalculationStore
    {
        public Vector3d RadialPlus;
        public Vector3d ProgradeOrbit;
        public Vector3d ProgradeSurface;
        public Vector3d NormalPlus;
        public Vector3d ManeuverPlus = Vector3d.zero;
        public bool ManeuverPresent;
        public bool ManeuverApplied;

        public void RunCalculations(
            Vessel vessel,
            Quaternion gymbal)
        {
            // Calculations thanks to Mechjeb
            Vector3d CoM = vessel.findWorldCenterOfMass();
            Vector3d up = (CoM - vessel.mainBody.position).normalized;
            Vector3d velocityVesselOrbit = vessel.orbit.GetVel();
            Vector3d velocityVesselOrbitUnit = velocityVesselOrbit.normalized;
            Vector3d radialPlus = Vector3d.Exclude(velocityVesselOrbit, up).normalized;
            Vector3d velocityVesselSurface = velocityVesselOrbit - vessel.mainBody.getRFrmVel(CoM);
            Vector3d velocityVesselSurfaceUnit = velocityVesselSurface.normalized;

            RadialPlus = gymbal * radialPlus;
            NormalPlus = gymbal * -Vector3d.Cross(radialPlus, velocityVesselOrbitUnit);
            ProgradeOrbit = gymbal * velocityVesselOrbitUnit;
            ProgradeSurface = gymbal * velocityVesselSurfaceUnit;

            if (vessel.patchedConicSolver.maneuverNodes.Count > 0)
            {
                var manuever = vessel.patchedConicSolver.maneuverNodes[0];
                ManeuverApplied = manuever.DeltaV != Vector3d.zero;

                Vector3d burnVector = manuever.GetBurnVector(vessel.orbit);
                ManeuverPlus = gymbal * burnVector.normalized;
                ManeuverPresent = true;
            }
            else
            {
                ManeuverPresent = false;
            }
        }
    }
}