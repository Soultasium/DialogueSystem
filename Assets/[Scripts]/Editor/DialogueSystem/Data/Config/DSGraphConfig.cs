using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Soulpace.Dialogues.Data.Config
{
    [CreateAssetMenu(fileName = "DialogueGraphConfig", menuName = "Dialogue System/Create Graph Config" , order = 1)]
    public class DSGraphConfig : ScriptableObject
    {
        private const string AssetsPath = "Assets";
        private const string DialoguesFolderName = "Dialogues";
        
        [SerializeField] private string _defaultGraphsPath = "DialogueSystem/Graphs";

        [SerializeField, HideInInspector] private string _saveMainPath;
        [SerializeField, HideInInspector] private string _saveDialoguesPath;
        
        public string SaveMainPath => _saveMainPath;
        public string SaveDialoguesPath => _saveDialoguesPath;

        private void OnValidate()
        {
            GenerateInternalPaths();
        }

        private void GenerateInternalPaths()
        {
            _saveMainPath = $"{AssetsPath}/{_defaultGraphsPath}";
            _saveDialoguesPath = $"{SaveMainPath}/{DialoguesFolderName}";
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif  
        }
    }
}
