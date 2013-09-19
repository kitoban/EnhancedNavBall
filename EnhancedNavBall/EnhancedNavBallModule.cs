using UnityEngine;

public class EnhancedNavBallModule : PartModule
{
    public override void OnAwake()
    {
        if (BaseEnhancedNavBall.GameObjectInstance == null)
        {
            BaseEnhancedNavBall.GameObjectInstance = GameObject.Find("BaseEnhancedNavBall") ?? new GameObject("BaseEnhancedNavBall", typeof(BaseEnhancedNavBall));
        }
    }
}
