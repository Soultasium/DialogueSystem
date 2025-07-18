using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Soulpace.Dialogues.Elements
{
    using Enumerations;
    using Utilities;
    using Windows;
    using Data.Save;
    
    public class DSMultipleChoiceNode : DSNode
    {
        public override void Initialize(string nodeName, DSGraphView graphView, Vector2 position)
        {
            base.Initialize(nodeName, graphView, position);

            DialogueType = DSDialoguesType.MultipleChoice;

            DSChoiceSaveData choiceData = new DSChoiceSaveData()
            {
                Text = "New Choice"
            };

            Choices.Add(choiceData);
        }

        public override void Draw()
        {
            base.Draw();

            // MAIN CONTAINER
            
            Button addChoiceButton = DSElementUtilities.CreateButton("Add Choice", () =>
            {
                DSChoiceSaveData choiceData = new DSChoiceSaveData()
                {
                    Text = "New Choice"
                };

                Choices.Add(choiceData);
                
                Port choicePort = CreateChoicePort(choiceData);

                outputContainer.Add(choicePort);
            });
            
            addChoiceButton.AddToClassList("ds-node__button");
            
            mainContainer.Insert(1, addChoiceButton);

            // OUTPUT CONTAINER
            
            foreach (var choice in Choices)
            {
                Port choicePort = CreateChoicePort(choice);

                outputContainer.Add(choicePort);
            }

            RefreshExpandedState();
        }

        #region Elements Creation

        private Port CreateChoicePort(object userData)
        {
            Port choicePort = this.CreatePort();

            choicePort.userData = userData;
            
            DSChoiceSaveData choiceData = (DSChoiceSaveData)userData;
            
            Button deleteChoiceButton = DSElementUtilities.CreateButton("X", () =>
            {
                if (Choices.Count == 1)
                    return;

                if (choicePort.connected)
                {
                    GraphView.DeleteElements(choicePort.connections);
                }

                Choices.Remove(choiceData);
                GraphView.RemoveElement(choicePort);
            });
            
            deleteChoiceButton.AddToClassList("ds-node__button");

            TextField choiceTextField = DSElementUtilities.CreateTextField(choiceData.Text, onValueChange: callback =>
            {
                choiceData.Text = callback.newValue;
            });

            choiceTextField.AddClasses(
                "ds-node__textfield",
                "ds-node__choice-textfield",
                "ds-node__textfield__hidden");
                
            choicePort.Add(choiceTextField);
            choicePort.Add(deleteChoiceButton);
            
            return choicePort;
        }

        #endregion
    }
}
