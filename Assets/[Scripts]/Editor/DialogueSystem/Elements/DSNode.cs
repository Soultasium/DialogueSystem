using System;
using System.Collections.Generic;
using System.Linq;
using Soulpace.Dialogues.Windows;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Soulpace.Dialogues.Elements
{
    using Enumerations;
    using Utilities;
    using Data.Save;
    
    public class DSNode : Node
    {
        public string ID { get; set; }
        public string DialogueName { get; set; }
        public List<DSChoiceSaveData> Choices { get; set; }
        public string Text { get; set; }
        public DSDialoguesType DialogueType { get; set; }
        public DSGroup Group { get; set; }

        protected DSGraphView GraphView;
        private Color _defaultBackgroundColor;

        public virtual void Initialize(string nodeName, DSGraphView graphView, Vector2 position)
        {
            ID = Guid.NewGuid().ToString();
            
            DialogueName = nodeName;
            Choices = new List<DSChoiceSaveData>();
            Text = "Dialogue text";
            
            GraphView = graphView;
            _defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30 / 255f);
            
            SetPosition(new Rect(position, Vector2.zero));
            
            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");
        }
        
        public virtual void Draw()
        {
            /* TITLE CONTAINER */

            TextField dialogueNameTextField = DSElementUtilities.CreateTextField(DialogueName, 
                onValueChange: callback =>
                {
                    TextField target = (TextField)callback.target;

                    target.value = callback.newValue.RemoveSpecialCharacters();

                    if (string.IsNullOrEmpty(target.value))
                    {
                        if (!string.IsNullOrEmpty(DialogueName))
                        {
                            ++GraphView.NameErrorsAmount;
                        }
                    }
                    else if (string.IsNullOrEmpty(DialogueName))
                    {
                        --GraphView.NameErrorsAmount;
                    }
                    
                    if (Group == null)
                    {
                        GraphView.RemoveUngroupedNode(this);

                        DialogueName = target.value;
                    
                        GraphView.AddUngroupedNode(this);
                        
                        return;
                    }
                    
                    DSGroup currentGroup = Group;
                    
                    GraphView.RemoveGroupedNode(this, Group);
                    
                    DialogueName = target.value;
                    
                    GraphView.AddGroupedNode(this, currentGroup);
                }
            );

            dialogueNameTextField.AddClasses(
                "ds-node__textfield",
                "ds-node__filename-textfield",
                "ds-node__textfield__hidden");
            
            titleContainer.Insert(0, dialogueNameTextField);

            /* INPUT CONTAINER */

            Port inputPort = this.CreatePort("Dialogue Connection", Orientation.Horizontal, Direction.Input,
                Port.Capacity.Multi);
            
            inputContainer.Add(inputPort);

            /* EXTENSION CONTAINER */
            
            VisualElement customDataContainer = new VisualElement();
            
            customDataContainer.AddToClassList("ds-node__custom-data-container");
            
            Foldout textFoldout = DSElementUtilities.CreateFoldout("Dialogue Text");

            TextField textTextField = DSElementUtilities.CreateTextArea(Text, onValueChange: callback =>
            {
                Text = callback.newValue;
            });

            textTextField.AddClasses(
                "ds-node__textfield",
                "ds-node__quote-textfield");
            
            textFoldout.Add(textTextField);
            
            customDataContainer.Add(textFoldout);
            
            extensionContainer.Add(customDataContainer);
        }

        #region Overrided Methods

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Disconnect Input Ports", action => DisconnectInputPorts());
            evt.menu.AppendAction("Disconnect Output Ports", action => DisconnectOutputPorts());
            
            base.BuildContextualMenu(evt);
        }

        #endregion

        #region Utility Methods

        public void DisconnectAllPorts()
        {
            DisconnectInputPorts();
            DisconnectOutputPorts();
        }

        public void DisconnectInputPorts()
        {  
            DisconnectPorts(inputContainer);
        }

        public void DisconnectOutputPorts()
        {
            DisconnectPorts(outputContainer);
        }
        
        private void DisconnectPorts(VisualElement container)
        {
            foreach (var element in container.Children())
            {
                if(element is not Port port)
                    continue;

                if (!port.connected)
                    continue;

                GraphView.DeleteElements(port.connections);
            }
        }

        public bool IsStartingNode()
        {
            Port inputPort = (Port)inputContainer.Children().First();

            return !inputPort.connected;
        }

        public void SetErrorStyle(Color color)
        {
            mainContainer.style.backgroundColor = color;
        }

        public void ResetStyle()
        {
            mainContainer.style.backgroundColor = _defaultBackgroundColor;
        }
        
        #endregion
    }
}
