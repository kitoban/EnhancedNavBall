using UnityEngine;

namespace EnhancedNavBall.Config
{
    public static class BasicResources
    {
        public static GUIStyle styleWindow;
        public static GUIStyle styleAddFieldAreas;
        public static GUIStyle styleAddField;
        public static GUIStyle styleAddHeading;

        public static GUIStyle styleCheckbox;
        public static GUIStyle styleCheckboxLabel;
        
        public static GUIStyle styleIconStyle;

        //////////////////////////////////////////////////////
        public static GUIStyle StyleSliderThumb;
        public static GUIStyle StyleSlider;
        public static GUIStyle StyleSectionHeading;
        public static GUIStyle StyleButton;

        public static void SetStyles()
        {
            Color32 colLabelText = new Color32(220, 220, 220, 255);
            int intFontSizeDefault = 13;


            styleIconStyle = new GUIStyle();

            GUIStyle styleDefLabel = new GUIStyle(GUI.skin.label);
            styleDefLabel.fontSize = intFontSizeDefault;
            styleDefLabel.fontStyle = FontStyle.Normal;
            styleDefLabel.normal.textColor = colLabelText;
            styleDefLabel.hover.textColor = Color.blue;

            GUIStyle styleDefTextArea = new GUIStyle(GUI.skin.textArea);
            styleDefTextArea.fontSize = intFontSizeDefault;
            styleDefTextArea.fontStyle = FontStyle.Normal;

            GUIStyle styleDefTextField = new GUIStyle(GUI.skin.textField);
            styleDefTextField.fontSize = intFontSizeDefault;
            styleDefTextField.fontStyle = FontStyle.Normal;

            GUIStyle styleDefButton = new GUIStyle(GUI.skin.button);
            styleDefTextField.fontSize = intFontSizeDefault;
            styleDefTextField.fontStyle = FontStyle.Normal;

            //GUIStyle styleDefToggle = new GUIStyle(GUI.skin.toggle);
            //styleDefToggle.fontSize = intFontSizeDefault;
            //styleDefToggle.fontStyle = FontStyle.Normal;

            GUIStyle styleDefSlider = new GUIStyle(GUI.skin.horizontalSlider);
            styleDefSlider.fontSize = intFontSizeDefault;
            styleDefSlider.fontStyle = FontStyle.Normal;

            GUIStyle styleDefSliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb);
            styleDefSliderThumb.fontSize = intFontSizeDefault;
            styleDefSliderThumb.fontStyle = FontStyle.Normal;
            
            styleWindow = new GUIStyle(GUI.skin.window);
            styleWindow.padding = SetRectOffset(styleWindow.padding, 4);

            StyleSlider = new GUIStyle(styleDefSlider);
            StyleSliderThumb = new GUIStyle(styleDefSliderThumb);
            
            StyleSectionHeading = new GUIStyle(styleDefLabel);
            StyleSectionHeading.normal.textColor = Color.white;
            StyleSectionHeading.fontStyle = FontStyle.Bold;
            StyleSectionHeading.padding.bottom = 0;
            StyleSectionHeading.margin.bottom = 0;

            StyleButton = new GUIStyle(styleDefButton);




            styleAddFieldAreas = new GUIStyle(styleDefTextArea);
            styleAddFieldAreas.padding = SetRectOffset(styleAddFieldAreas.padding, 4);
            styleAddFieldAreas.margin.left = 0;
            styleAddFieldAreas.margin.right = 0;

            styleAddField = new GUIStyle(styleDefTextField);
            styleAddField.stretchWidth = true;
            styleAddField.alignment = TextAnchor.UpperLeft;
            styleAddField.normal.textColor = Color.yellow;

            //styleCheckbox = new GUIStyle(styleDefToggle);

            styleCheckboxLabel = new GUIStyle(styleDefLabel);

        }

        public static RectOffset SetWindowRectOffset(RectOffset tmpRectOffset, int intValue)
        {
            tmpRectOffset.left = intValue;
            //tmpRectOffset.top = Top;
            tmpRectOffset.right = intValue;
            tmpRectOffset.bottom = intValue;
            return tmpRectOffset;
        }

        public static RectOffset SetRectOffset(RectOffset tmpRectOffset, int intValue)
        {
            return SetRectOffset(tmpRectOffset, intValue, intValue, intValue, intValue);
        }

        public static RectOffset SetRectOffset(RectOffset tmpRectOffset, int Left, int Right, int Top, int Bottom)
        {
            tmpRectOffset.left = Left;
            tmpRectOffset.top = Top;
            tmpRectOffset.right = Right;
            tmpRectOffset.bottom = Bottom;
            return tmpRectOffset;
        }
    }
}