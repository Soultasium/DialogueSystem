using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Soulpace.Dialogues.Windows
{
    using Elements;
    using Enumerations;
    
    public class DSSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DSGraphView _graphView;
        private Texture2D _indentationIcon;
        
        
        public void Initialize(DSGraphView graphView)
        {
            _graphView = graphView;

            _indentationIcon = new Texture2D(1, 1);
            _indentationIcon.SetPixel(0, 0, Color.clear);
            _indentationIcon.Apply();
        }
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Element")),
                new SearchTreeGroupEntry(new GUIContent("Dialogue Node"), 1),
                new SearchTreeEntry(new GUIContent("Single Choice", _indentationIcon))
                {
                    level = 2,
                    userData = DSDialoguesType.SingleChoice
                },
                new SearchTreeEntry(new GUIContent("Multiple Choice", _indentationIcon))
                {
                    level = 2,
                    userData = DSDialoguesType.MultipleChoice
                },
                new SearchTreeGroupEntry(new GUIContent("Dialogue Group"), 1),
                new SearchTreeEntry(new GUIContent("Single Group", _indentationIcon))
                {
                    level = 2,
                    userData = new Group()
                }
            };
            
            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            Vector2 localMousePosition = _graphView.GetLocalMousePosition(context.screenMousePosition, true);
            
            switch (searchTreeEntry.userData)
            {
                case DSDialoguesType.SingleChoice:
                    DSSingleChoiceNode singleChoiceNode =
                        (DSSingleChoiceNode)_graphView.CreateNode("DialogueName", DSDialoguesType.SingleChoice,
                            localMousePosition);
                    
                    _graphView.AddElement(singleChoiceNode);
                    
                    return true;
                
                case DSDialoguesType.MultipleChoice:
                    DSMultipleChoiceNode multipleChoiceNode =
                        (DSMultipleChoiceNode)_graphView.CreateNode("DialogueName", DSDialoguesType.MultipleChoice,
                            localMousePosition);
                    
                    _graphView.AddElement(multipleChoiceNode);
                    return true;

                case Group _:
                {
                    _graphView.CreateGroup("DialogueGroup", localMousePosition);
                    
                    return true;
                }
                
                default:
                    return false;
            }
        }
    }
}
