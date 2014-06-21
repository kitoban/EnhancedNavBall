using UnityEngine;

namespace EnhancedNavBall
{
    public class SlidingNavballMonoBehaviour: MonoBehaviour
    {
        private GUIStyle _styleDefLabel;

        public SlidingNavballMonoBehaviour()
        {
            _styleDefLabel = new GUIStyle(GUI.skin.label);
            _styleDefLabel.fontSize = 13;
            _styleDefLabel.fontStyle = FontStyle.Normal;
            _styleDefLabel.normal.textColor = Color.white;
            _styleDefLabel.hover.textColor = Color.blue;
        }

        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Mouse is down");

                RaycastHit hitInfo = new RaycastHit();
                bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 20f, Constants.NavBallLayer);
                if (hit)
                {
                    Debug.Log("Hit " + hitInfo.transform.gameObject.name);
                    if (hitInfo.transform.gameObject.tag == "Construction")
                    {
                        Debug.Log("It's working!");
                    }
                    else
                    {
                        Debug.Log("nopz");
                    }
                }
                else
                {
                    Debug.Log("No hit");
                }
                Debug.Log("Mouse is down");
            }
        }

        public void Start()
        {

            Utilities.DebugLog(LogLevel.Minimal, "SlidingNavballMonoBehaviour start");
            RenderingManager.AddToPostDrawQueue(1, Draw);

        }

        public void OnGUI()
        {
            Draw();
        }

        public void OnDestroy()
        {
            RenderingManager.RemoveFromPostDrawQueue(1, Draw);
        }

        private void Draw()
        {
            if (ScreenSafeUI.referenceCam == null)
                return;

            if (this.transform == null)
                return;

            Vector3 worldToScreenPoint = ScreenSafeUI.referenceCam.WorldToScreenPoint(
                this.transform.position /*+ new Vector3()*/);
            Rect rect = new Rect(
                worldToScreenPoint.x,
                (float)Screen.height - worldToScreenPoint.y,
                1f,
                1f);
            GUI.Label(rect, "#######", _styleDefLabel);
        }
    }
}
