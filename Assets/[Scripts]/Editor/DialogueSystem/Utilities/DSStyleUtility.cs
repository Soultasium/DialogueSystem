using UnityEditor;
using UnityEngine.UIElements;

namespace Soulpace.Dialogues.Utilities
{
    public static class DSStyleUtility
    {
        public static VisualElement AddClasses(this VisualElement element, params string[] classNames)
        {
            foreach (string className in classNames)
            {
                element.AddToClassList(className);
            }
            
            return element;
        }
        
        public static void AddStyleSheets(this VisualElement element, params string[] styleSheetNames)
        {
            foreach (string styleSheetName in styleSheetNames)
            {
                StyleSheet styleSheet = (StyleSheet)EditorGUIUtility.Load(styleSheetName);
                
                element.styleSheets.Add(styleSheet);
            }
        }
    }
}
