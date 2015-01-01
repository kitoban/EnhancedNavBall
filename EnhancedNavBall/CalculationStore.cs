using UnityEngine;

namespace EnhancedNavBall
{
    public class CalculationStore
    {
        public Vector3d ProgradeOrbit;
        public Vector3d ProgradeSurface;
        public Vector3d ManeuverPlus = Vector3d.zero;
        public bool ManeuverPresent;
        public bool ManeuverApplied;

        public void RunCalculations(
            Vessel vessel,
            Quaternion gymbal)
        {
            ManeuverPresent = false;

            // Calculations thanks to Mechjeb
            Vector3d CoM = vessel.findWorldCenterOfMass();
            Vector3d velocityVesselOrbit = vessel.orbit.GetVel();
            Vector3d velocityVesselOrbitUnit = velocityVesselOrbit.normalized;
            Vector3d velocityVesselSurface = velocityVesselOrbit - vessel.mainBody.getRFrmVel(CoM);
            Vector3d velocityVesselSurfaceUnit = velocityVesselSurface.normalized;

            ProgradeOrbit = gymbal * velocityVesselOrbitUnit;
            ProgradeSurface = gymbal * velocityVesselSurfaceUnit;

            PatchedConicSolver patchedConicSolver = vessel.patchedConicSolver;
            if (patchedConicSolver == null)
                return;

            if (patchedConicSolver.maneuverNodes == null)
                return;

            if (patchedConicSolver.maneuverNodes.Count > 0)
            {
                var manuever = patchedConicSolver.maneuverNodes[0];
                ManeuverApplied = manuever.DeltaV != Vector3d.zero;

                Vector3d burnVector = manuever.GetBurnVector(vessel.orbit);
                ManeuverPlus = gymbal * burnVector.normalized;
                ManeuverPresent = true;
            }
        }
    }
}