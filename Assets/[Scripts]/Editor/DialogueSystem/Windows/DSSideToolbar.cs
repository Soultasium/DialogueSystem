using System.Collections.Generic;
using Soulpace.Dialogues.Data.Config;
using Soulpace.Dialogues.Utilities;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Soulpace.Dialogues.Windows
{
    public class DSSideToolbar : VisualElement
    {
        private class GraphAssetData
        {
            public Button Button { get; set; }
            public string AssetName { get; set; }
            public string DisplayName { get; set; }

            public GraphAssetData(string assetName, string displayName)
            {
                AssetName = assetName;
                DisplayName = displayName;
            }
        }
        
        private const string DefaultFileName = "NewDialogueGraph";
        
        private const float CollapsedWidth = 40f;
        private const float FileTextFieldButtonCollapseWidth = 50;
        private const float SaveButtonCollapseWidth = 100;
        
        
        private DSGraphConfig Config { get; set; }

        private TextField _fileNameTextField;
        private ScrollView _scrollView;
        private Button _saveButton;
        private Button _toggleButton;

        private float _defaultExpandedWidth = 400f;
        private bool _isExpanded = true;
        
        private VisualElement _gridContent;
        private bool _isResizing = false;
        private Vector2 _lastMousePosition;
        private List<GraphAssetData> _savedGraphs = new List<GraphAssetData>();

        public DSSideToolbar(VisualElement rootVisualElement, DSGraphConfig config)
        {
            Config = config;
            
            this.AddStyleSheets("DialogueSystem/DSSidePanelStyles.uss");
            AddToClassList("ds-toolbar__main-container");

            //rootVisualElement.Add(this);

            AddToolbar();
            AddResizeHandle();
            AddGridScrollView();

            RefreshButtons();
        }

        private void AddGridScrollView()
        {
            _gridContent = new VisualElement();
            _gridContent.AddToClassList("ds-toolbar__grid-container");

            _scrollView = new ScrollView(ScrollViewMode.Vertical);
            _scrollView.AddToClassList("ds-toolbar__scroll-view");

            var content = _scrollView.contentContainer;
            content.AddToClassList("ds-toolbar__grid");

            _gridContent.Add(_scrollView);
            Add(_gridContent);

            Button addButton = new Button(AddNewAsset)
            {
                text = "+"
            };
            addButton.AddToClassList("ds-toolbar__button-add");
            
            _scrollView.Add(addButton);
        }

        private void AddToolbar()
        {
            Toolbar toolbar = new Toolbar();
            
            _toggleButton = new Button(ToggleOverlay)
            {
                text = "←"
            };
            _toggleButton.AddToClassList("ds_toolbar__toggle-button");
            
            _fileNameTextField = DSElementUtilities.CreateTextField(DefaultFileName, "File Name:",
                callback =>
                {
                    _fileNameTextField.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
                });
            
            _saveButton = DSElementUtilities.CreateButton("Save", () => Save());
            
            toolbar.Add(_fileNameTextField);
            toolbar.Add(_saveButton);
            toolbar.Add(_toggleButton);
            
            toolbar.AddStyleSheets("DialogueSystem/DSSidePanelToolbarStyles.uss");
            
            Add(toolbar);
        }

        private void AddResizeHandle()
        {
            var resizeHandle = new VisualElement();
            resizeHandle.AddToClassList("ds-toolbar__resize-handle");

            var cursor = UnityDefaultCursor.DefaultCursor(UnityDefaultCursor.CursorType.ResizeHorizontal);
            resizeHandle.style.cursor = new StyleCursor(cursor);
            
            resizeHandle.RegisterCallback<MouseDownEvent>(evt =>
            {
                _isResizing = true;
                _lastMousePosition = evt.mousePosition;
                resizeHandle.CaptureMouse();
                resizeHandle.AddToClassList("ds-toolbar__resize-handle--dragging");
            });
            
            resizeHandle.RegisterCallback<MouseMoveEvent>(evt =>
            {
                if (!_isResizing) 
                    return;

                float delta = evt.mousePosition.x - _lastMousePosition.x;
                float newWidth = resolvedStyle.width + delta;
                _defaultExpandedWidth = Mathf.Clamp(newWidth, CollapsedWidth, 800);

                style.width = _defaultExpandedWidth;
                _lastMousePosition = evt.mousePosition;

                // Auto-expand if dragged past threshold
                if (!_isExpanded && newWidth > CollapsedWidth + 20f)
                {
                    _isExpanded = true;
                    _gridContent.style.display = DisplayStyle.Flex;
                    _toggleButton.text = "←";
                    
                    _saveButton.style.display = DisplayStyle.Flex;
                    _fileNameTextField.style.display = DisplayStyle.Flex;
                }
                else if (_isExpanded && newWidth <= CollapsedWidth + 20f)
                {
                    _isExpanded = false;
                    _gridContent.style.display = DisplayStyle.None;
                    _toggleButton.text = "→";
            
                    // Hide save button if not enough space
                    _saveButton.style.display = DisplayStyle.None;
                    _fileNameTextField.style.display = DisplayStyle.None;
                }
                
                SetVisibilityBasedOnWidth(_saveButton, newWidth, SaveButtonCollapseWidth);
                SetVisibilityBasedOnWidth(_fileNameTextField, newWidth, FileTextFieldButtonCollapseWidth);
            });

            resizeHandle.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (!_isResizing) 
                    return;
                
                _isResizing = false;
                resizeHandle.ReleaseMouse();
                resizeHandle.RemoveFromClassList("ds-toolbar__resize-handle--dragging");
            });

            Add(resizeHandle);
        }

        private void SetVisibilityBasedOnWidth(VisualElement visualElement, float currentWidth, float maxWidth)
        {
            visualElement.style.display = currentWidth > maxWidth ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void Save()
        {
            
        }

        private void AddNewAsset()
        {
            
        }

        private void ToggleOverlay()
        {
            _isExpanded = !_isExpanded;

            style.width = _isExpanded ? _defaultExpandedWidth : CollapsedWidth;
            _gridContent.style.display = _isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
            _toggleButton.text = _isExpanded ? "←" : "→"; // ⮞ points to expand
        }

        private void RefreshButtons()
        {
            for (int i = 0; i < _savedGraphs.Count; i++)
            {
                _gridContent.Remove(_savedGraphs[i].Button);
            }
            
            _savedGraphs.Clear();

            var graphs = DSIOUtility.GetAllSavedGraphSO();

            foreach (var graph in graphs)
            {
                GraphAssetData graphData = new GraphAssetData(graph.name, graph.FileName);
                Button button = new Button(() =>
                {
                    OnButtonClicked(graphData);
                });
                
                button.text = graphData.AssetName;
                button.AddToClassList("ds-toolbar__button");
                
                _scrollView.Add(button);
            }
        }

        private void OnButtonClicked(GraphAssetData graphData)
        {
            Debug.Log($"Clicked {graphData.AssetName}");
        }
    }
}
