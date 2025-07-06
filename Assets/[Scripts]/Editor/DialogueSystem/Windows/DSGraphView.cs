using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Soulpace.Dialogues.Windows
{
    using Data.Error;
    using Enumerations;
    using Utilities;
    using Elements;
    using Data.Save;
    
    public class DSGraphView : GraphView
    {
        private DSEditorWindow _editorWindow;
        private DSSearchWindow _searchWindow;
        
        private MiniMap _minimap;

        private SerializableDictionary<string, DSNodeErrorData> _ungroupedNodes;
        private SerializableDictionary<string, DSGroupErrorData> _groups;
        private SerializableDictionary<Group, SerializableDictionary<string, DSNodeErrorData>> _groupedNodes;

        private int _nameErrorsAmount = 0;

        public int NameErrorsAmount
        {
            get => _nameErrorsAmount;
            set
            {
                _nameErrorsAmount = value;

                if (_nameErrorsAmount == 0)
                {
                    _editorWindow.EnableSaving();
                }
                else
                {
                    _editorWindow.DisableSaving();
                }
            }
        }
        
        
        public DSGraphView(DSEditorWindow editorWindow)
        {
            _editorWindow = editorWindow;
            
            _ungroupedNodes = new SerializableDictionary<string, DSNodeErrorData>();
            _groups = new SerializableDictionary<string, DSGroupErrorData>();
            _groupedNodes = new SerializableDictionary<Group, SerializableDictionary<string, DSNodeErrorData>>();
            
            AddManipulators();
            AddSearchWindow();
            AddMiniMap();
            AddGridBackground();
            
            OnElementsDeleted();
            OnGroupElementsAdded();
            OnGroupElementsRemoved();
            OnGroupRenamed();
            OnGraphViewChanged();
            
            AddStyles();
            AddMiniMapStyles();
        }

        #region Overrides

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort == port)
                    return;

                if (startPort.node == port.node)
                    return;

                if (startPort.direction == port.direction)
                    return;

                compatiblePorts.Add(port);
            });
            
            return compatiblePorts;
        }

        #endregion
        
        #region Manipulators
        
        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale * 2);

            // Order of these is important!
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(CreateNodeContextualMenu("Add Node (Single Choice)", DSDialoguesType.SingleChoice));
            this.AddManipulator(CreateNodeContextualMenu("Add Node (Multiple Choice)", DSDialoguesType.MultipleChoice));
            
            this.AddManipulator(CreateGroupContextualMenu());
        }

        private IManipulator CreateNodeContextualMenu(string actionTitle, DSDialoguesType dialogueType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(
                    actionTitle,
                    actionEvent => AddElement(CreateNode("DialogueName", dialogueType,
                        GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))));
            
            return contextualMenuManipulator;
        }

        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(
                    "Add Group",
                    actionEvent => CreateGroup("DialogueGroup",
                        GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))));
            
            return contextualMenuManipulator;
        }
        
        #endregion

        #region Elements Creation

        public DSNode CreateNode(string nodeName, DSDialoguesType dialoguesType, Vector2 mousePosition,
            bool shouldDraw = true)
        {
            Type nodeType = Type.GetType($"Soulpace.Dialogues.Elements.DS{dialoguesType}Node");

            Debug.Assert(nodeType != null, nameof(nodeType) + " != null");
            
            DSNode node = (DSNode)Activator.CreateInstance(nodeType);
            
            node.Initialize(nodeName, this, mousePosition);

            if (shouldDraw)
            {
                node.Draw();
            }

            AddUngroupedNode(node);
            
            return node;
        }
        
        public DSGroup CreateGroup(string title, Vector2 mousePosition)
        {
            DSGroup group = new DSGroup(title, mousePosition);
            
            AddGroup(group);
            
            AddElement(group);

            foreach (var selectable in selection)
            {
                var selectedElement = (GraphElement)selectable;
                if(selectedElement is not DSNode node)
                    continue;
                
                group.AddElement(node);
            }
            
            return group;
        }

        #endregion

        #region Callbacks

        private void OnElementsDeleted()
        {
            deleteSelection = (operationName, askUser) =>
            {
                int count = selection.Count;
                for (int i = count -1; i >= 0; i--)
                {
                    switch (selection[i])
                    {
                        case DSGroup group:
                        {
                            List<DSNode> nodesToDelete = new List<DSNode>();
                            foreach (GraphElement groupElement in group.containedElements)
                            {
                                if (groupElement is not DSNode groupNode)
                                    continue;

                                nodesToDelete.Add(groupNode);
                            }

                            for (int j = nodesToDelete.Count - 1; j >= 0; j--)
                            {
                                group.RemoveElement(nodesToDelete[j]);
                            }

                            RemoveGroup(group);
                            RemoveElement(group);
                            break;
                        }                        
                        case DSNode node:
                        {
                            node.Group?.RemoveElement(node);
                            RemoveUngroupedNode(node);
                            
                            node.DisconnectAllPorts();
                            
                            RemoveElement(node);
                            break;
                        }
                        case Edge edge:
                        {
                            RemoveElement(edge);
                            break;
                        }
                    }
                }
            };
        }

        private void OnGroupElementsAdded()
        {
            elementsAddedToGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (element is not DSNode node)
                        continue;

                    DSGroup dsGroup = (DSGroup)group;
                    
                    RemoveUngroupedNode(node);
                    AddGroupedNode(node, dsGroup);
                }
            };
        }

        private void OnGroupElementsRemoved()
        {
            elementsRemovedFromGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (element is not DSNode node)
                        continue;

                    RemoveGroupedNode(node, group);
                    AddUngroupedNode(node);
                }
            };
        }

        private void OnGroupRenamed()
        {
            groupTitleChanged = (group, newTitle) =>
            {
                DSGroup dsGroup = (DSGroup)group;

                dsGroup.title = newTitle.RemoveSpecialCharacters();

                if (string.IsNullOrEmpty(dsGroup.title))
                {
                    if (!string.IsNullOrEmpty(dsGroup.OldTitle))
                    {
                        ++NameErrorsAmount;
                    }
                }
                else if (string.IsNullOrEmpty(dsGroup.OldTitle))
                {
                    --NameErrorsAmount;
                }

                RemoveGroup(dsGroup);
                
                dsGroup.OldTitle = dsGroup.title;
                
                AddGroup(dsGroup);
            };
        }

        private void OnGraphViewChanged()
        {
            graphViewChanged = (changes) =>
            {
                if (changes.edgesToCreate != null)
                {
                    foreach (var edge in changes.edgesToCreate)
                    {
                        DSNode nextNode = (DSNode)edge.input.node;

                        DSChoiceSaveData choiceData = (DSChoiceSaveData)edge.output.userData;

                        choiceData.NodeID = nextNode.ID;
                    }
                }

                if (changes.elementsToRemove != null)
                {
                    foreach (var element in changes.elementsToRemove)
                    {
                        if (element is not Edge edge)
                            continue;

                        DSChoiceSaveData choiceData = (DSChoiceSaveData)edge.output.userData;

                        choiceData.NodeID = "";
                    }
                }

                return changes;
            };
        }

        #endregion
        
        #region Repeated Elements
        
        public void AddUngroupedNode(DSNode node)
        {
            string nodeName = node.DialogueName.ToLower();

            if (!_ungroupedNodes.TryGetValue(nodeName, out var ungroupedNode))
            {
                DSNodeErrorData nodeErrorData = new DSNodeErrorData();
                
                nodeErrorData.Nodes.Add(node);
                
                _ungroupedNodes.Add(nodeName, nodeErrorData);

                return;
            }

            List<DSNode> ungroupedNodesList = ungroupedNode.Nodes;

            ungroupedNodesList.Add(node);

            Color errorColor = _ungroupedNodes[nodeName].ErrorData.Color;

            node.SetErrorStyle(errorColor);

            if (ungroupedNodesList.Count == 2)
            {
                
                ++NameErrorsAmount
                    ;
                ungroupedNodesList[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveUngroupedNode(DSNode node)
        {
            string nodeName = node.DialogueName.ToLower();
             
            List<DSNode> ungroupedNodesList = _ungroupedNodes[nodeName].Nodes;
             
            ungroupedNodesList.Remove(node);
             
            node.ResetStyle();

            if (ungroupedNodesList.Count == 1)
            {
                --NameErrorsAmount;
                
                ungroupedNodesList[0].ResetStyle();
                
                return;
            }

            if (ungroupedNodesList.Count == 0)
            {
                _ungroupedNodes.Remove(nodeName);
            }
        }

        private void AddGroup(DSGroup group)
        {
            string groupName = group.title.ToLower();

            if (!_groups.ContainsKey(groupName))
            {
                DSGroupErrorData groupErrorData = new DSGroupErrorData();

                groupErrorData.Groups.Add(group);

                _groups.Add(groupName, groupErrorData);

                return;
            }
            
            List<DSGroup> groupsList = _groups[groupName].Groups;
            
            groupsList.Add(group);
            
            Color errorColor = _groups[groupName].ErrorData.Color;
            
            group.SetErrorStyle(errorColor);

            if (groupsList.Count == 2)
            {
                ++NameErrorsAmount
                    ;
                
                groupsList[0].SetErrorStyle(errorColor);
            }
        }

        private void RemoveGroup(DSGroup group)
        {
            string oldGroupName = group.OldTitle.ToLower();

            List<DSGroup> groupsList = _groups[oldGroupName].Groups;
            
            _groups[oldGroupName].Groups.Remove(group);
            
            group.ResetStyle();

            if (groupsList.Count == 1)
            {
                --NameErrorsAmount
                    ;
                
                groupsList[0].ResetStyle();
                
                return;
            }

            if (groupsList.Count == 0)
            {
                _groups.Remove(oldGroupName);
            }
        }

        public void AddGroupedNode(DSNode node, DSGroup group)
        {
            string nodeName = node.DialogueName.ToLower();
            
            node.Group = group;

            if (!_groupedNodes.ContainsKey(group))
            {
                _groupedNodes.Add(group, new SerializableDictionary<string, DSNodeErrorData>());
            }

            if (!_groupedNodes[group].ContainsKey(nodeName))
            {
                DSNodeErrorData nodeErrorData = new DSNodeErrorData();

                nodeErrorData.Nodes.Add(node);
                
                _groupedNodes[group].Add(nodeName, nodeErrorData);
                
                return;
            }
            
            List<DSNode> groupedNodesList = _groupedNodes[group][nodeName].Nodes;
            
            groupedNodesList.Add(node);

            Color errorColor = _groupedNodes[group][nodeName].ErrorData.Color;

            node.SetErrorStyle(errorColor);

            if (groupedNodesList.Count == 2)
            {
                ++NameErrorsAmount
                    ;
                
                groupedNodesList[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveGroupedNode(DSNode node, Group group)
        {
            string nodeName = node.DialogueName.ToLower();

            node.Group = null;

            List<DSNode> groupedNodesList = _groupedNodes[group][nodeName].Nodes;

            groupedNodesList.Remove(node);
            
            node.ResetStyle();

            if (groupedNodesList.Count == 1)
            {
                --NameErrorsAmount
                    ;
                
                groupedNodesList[0].ResetStyle();
                
                return;
            }

            if (groupedNodesList.Count == 0)
            {
                _groupedNodes[group].Remove(nodeName);

                if (_groupedNodes[group].Count == 0)
                {
                    _groupedNodes.Remove(group);
                }
            }
        }

        #endregion

        #region Elements Addition
        
        private void AddSearchWindow()
        {
            if (_searchWindow == null)
            {
                _searchWindow = ScriptableObject.CreateInstance<DSSearchWindow>();
                
                _searchWindow.Initialize(this);
            }

            nodeCreationRequest = context =>
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }

        private void AddMiniMap()
        {
            _minimap = new MiniMap()
            {
                anchored = false,
            };
            
            _minimap.SetPosition(new Rect(15, 50, 200, 180));
            
            Add(_minimap);

            _minimap.visible = false;
        }
        
        private void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();
            
            gridBackground.StretchToParentSize();
            
            Insert(0, gridBackground);
        }

        private void AddStyles()
        {
            this.AddStyleSheets(
                "DialogueSystem/DSGraphViewStyles.uss",
                "DialogueSystem/DSNodeStyles.uss");
        }

        private void AddMiniMapStyles()
        {
            StyleColor backgroundColor = new StyleColor(new Color32(29, 29, 30, 255));
            StyleColor borderColor = new StyleColor(new Color32(51, 51, 51, 255));
            
            _minimap.style.backgroundColor = backgroundColor;
            
            _minimap.style.borderTopColor = borderColor;
            _minimap.style.borderBottomColor = borderColor;
            _minimap.style.borderLeftColor = borderColor;
            _minimap.style.borderRightColor = borderColor;
        }

        #endregion

        #region Utilities

        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            Vector2 worldMousePosition = mousePosition;
            
            if (isSearchWindow)
            {
                worldMousePosition -= _editorWindow.position.position;
            }
            
            Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);
            
            return localMousePosition;
        }

        public void ClearGraph()
        {
            graphElements.ForEach(graphElement => RemoveElement(graphElement));
            
            _groups.Clear();
            _groupedNodes.Clear();
            _ungroupedNodes.Clear();

            NameErrorsAmount = 0;
        }

        public void ToggleMiniMap()
        {
            _minimap.visible = !_minimap.visible;
        }

        #endregion
    }
}
