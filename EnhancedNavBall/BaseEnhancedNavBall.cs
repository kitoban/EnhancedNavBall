using EnhancedNavBall;
using UnityEngine;

public class BaseEnhancedNavBall : MonoBehaviour
{
    private GameObject _navBallInstance;
    private EnhancedNavBallBehaviour _enhancedNavBallBehaviour;
    public static GameObject GameObjectInstance { get; set; }

    public void Awake()
    {
        Utilities.DebugLog(LogLevel.Minimal, "BaseEnhancedNavBall loaded");
        DontDestroyOnLoad(this);
        CancelInvoke();
    }

    private bool SceneIsValid
    {
        get { return HighLogic.LoadedScene == GameScenes.FLIGHT; }
    }

    public void Update()
    {
        if (SceneIsValid == false)
            return;
        
        if (_navBallInstance == null)
        {
            _navBallInstance = GameObject.Find("NavBall");
        }

        if (_navBallInstance != null)
        {
            PerformEnhancedNavBallCheck();
        }
    }

    private void PerformEnhancedNavBallCheck()
    {
        if (_enhancedNavBallBehaviour == null)
        {
            _enhancedNavBallBehaviour = _navBallInstance.GetComponent<EnhancedNavBallBehaviour>();

            if (_enhancedNavBallBehaviour == null)
            {
                _enhancedNavBallBehaviour = _navBallInstance.AddComponent<EnhancedNavBallBehaviour>();
                _enhancedNavBallBehaviour.NavBallGameObject = _navBallInstance;
                Utilities.DebugLog(LogLevel.Minimal, "EnhancedNavBallBehaviour loaded");
            }
        }
    }
}