using System.IO;
using System.Linq;
using Soulpace.Dialogues.Data.Config;
using Soulpace.Dialogues.Data.Save;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Soulpace.Dialogues.Windows
{
    using Utilities;
    
    public class DSEditorWindow : EditorWindow
    {
        private const string DefaultFileName = "DialoguesFileName";
        
        private static TextField s_fileNameTextField;

        private DSGraphConfig _config;
        private VisualElement _mainContainer;
        private VisualElement _contentContainer;
        private DSGraphView _graphView;
        private Button _saveButton;
        private Button _miniMapButton;
        
        
        [MenuItem("Dialogue System/Dialogue Graph")]
        public static void OpenWindow()
        {
            GetWindow<DSEditorWindow>("Dialogue Graph");
        }

        private void OnEnable()
        {
            GetGraphConfig();
            AddMainContainer();
        }

        private void AddMainContainer()
        {
            _mainContainer = new VisualElement();
            _mainContainer.style.flexDirection = FlexDirection.Row;
            _mainContainer.style.flexGrow = 1;
            _mainContainer.StretchToParentSize();
            
            _contentContainer = new VisualElement();
            _contentContainer.style.flexDirection = FlexDirection.Column;
            _contentContainer.style.flexGrow = 1;
            
            AddToolbar();
            AddSideBar();
            AddGraphView();
            
            var leftPanel = new VisualElement
            {
                style =
                {
                    width = 200,
                    backgroundColor = new Color(0.25f, 0.25f, 0.25f),
                    flexShrink = 0
                }
            };
            leftPanel.Add(new Label("Sidebar"));
            
            _mainContainer.Add(_sideToolbar);
            _mainContainer.Add(_contentContainer);
            rootVisualElement.Add(_mainContainer);
            
            AddStyles();
        }

        private void GetGraphConfig()
        {
            _config = DSIOUtility.GetOrCreateGraphConfigAsset();
        }

        #region Elements Addition

        private void AddGraphView()
        {
            _graphView = new DSGraphView(this);
            _graphView.style.flexGrow = 1;
            //_graphView.StretchToParentSize();
            
            _contentContainer.Add(_graphView);
        }

        private void AddToolbar()
        {
            Toolbar toolbar = new Toolbar();
            
            s_fileNameTextField = DSElementUtilities.CreateTextField(DefaultFileName, "File Name:",
                callback =>
                {
                    s_fileNameTextField.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
                });
            
            _saveButton = DSElementUtilities.CreateButton("Save", () => Save());
            Button loadButton = DSElementUtilities.CreateButton("Load", () => Load());

            Button clearButton = DSElementUtilities.CreateButton("Clear", () => Clear());
            Button resetButton = DSElementUtilities.CreateButton("Reset", () => ResetGraph());
            _miniMapButton = DSElementUtilities.CreateButton("MiniMap", () => ToggleMiniMap());
            
            toolbar.Add(s_fileNameTextField);
            toolbar.Add(_saveButton);
            toolbar.Add(loadButton);
            toolbar.Add(clearButton);
            toolbar.Add(resetButton);
            toolbar.Add(_miniMapButton);
            
            toolbar.AddStyleSheets("DialogueSystem/DSToolbarStyles.uss");
            
            _contentContainer.Add(toolbar);
        }

        private DSSideToolbar _sideToolbar;
        private void AddSideBar()
        {
            _sideToolbar = new DSSideToolbar(_mainContainer, _config);

            //_sideToolbar.style.flexShrink = 0;
        }

        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets("DialogueSystem/DSVariables.uss");
        }
        
        #endregion

        #region ToolbarActions

        private void Save()
        {
            if (string.IsNullOrEmpty(s_fileNameTextField.value))
            {
                EditorUtility.DisplayDialog(
                    "Invalid file name.",
                    "Please ensure the file name you've typed is valid.",
                    "Aye sir!");

                return;
            }
            
            DSIOUtility.Initialize(_graphView, s_fileNameTextField.value);
            DSIOUtility.Save();
        }

        private void Load()
        {
            string filePath =
                EditorUtility.OpenFilePanel("Dialogue Graph", "Assets/Editor/DialogueSystem/Graphs", "asset");

            if (string.IsNullOrEmpty(filePath))
                return;

            Clear();
            
            DSIOUtility.Initialize(_graphView, Path.GetFileNameWithoutExtension(filePath));
            DSIOUtility.Load();
        }

        private bool Clear()
        {
            if (!EditorUtility.DisplayDialog(
                    "Clear graph",
                    "Are you sure you want to clear the graph of all its elements?",
                    "Aye sir!",
                    "Cancel"))
                return false;
            
            _graphView.ClearGraph();
            
            return true;
        }

        private void ResetGraph()
        {
            if(!Clear())
                return;
            
            UpdateFileName(DefaultFileName);
        }

        private void ToggleMiniMap()
        {
            _graphView.ToggleMiniMap();
            
            _miniMapButton.ToggleInClassList("ds-toolbar__button__selected");
        }

        #endregion

        #region Utility Methods

        public static void UpdateFileName(string fileName)
        {
            s_fileNameTextField.value = fileName;
        }
        
        public void EnableSaving()
        {
            _saveButton.SetEnabled(true);
        }

        public void DisableSaving()
        {
            _saveButton.SetEnabled(false);
        }

        #endregion
    }
}
