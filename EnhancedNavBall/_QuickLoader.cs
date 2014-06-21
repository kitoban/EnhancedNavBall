
namespace EnhancedNavBall
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class Debug_AutoLoadQuicksaveOnStartup : UnityEngine.MonoBehaviour
    {
        private static bool _first = true;
        public void Start()
        {
            if (_first)
            {
                _first = false;
                HighLogic.SaveFolder = "default";
                var game = GamePersistence.LoadGame("persistent", HighLogic.SaveFolder, true, false);
                if (game != null && game.flightState != null && game.compatible)
                {
                    FlightDriver.StartAndFocusVessel(game, game.flightState.activeVesselIdx);
                }
                CheatOptions.InfiniteFuel = true;
            }
        }
    }
}
