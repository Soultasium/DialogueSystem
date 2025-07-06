using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Soulpace.Dialogues.Elements
{
    using Windows;
    using Enumerations;
    using Data.Save;
    using Utilities;
    
    public class DSSingleChoiceNode : DSNode
    {
        public override void Initialize(string nodeName, DSGraphView graphView, Vector2 position)
        {
            base.Initialize(nodeName, graphView, position);

            DialogueType = DSDialoguesType.SingleChoice;

            DSChoiceSaveData choiceSata = new DSChoiceSaveData()
            {
                Text = "Next Dialogue"
            };

            Choices.Add(choiceSata);
        }

        public override void Draw()
        {
            base.Draw();

            // OUTPUT CONTAINER

            foreach (var choice in Choices)
            {
                Port choicePort = this.CreatePort(choice.Text);

                choicePort.userData = choice;
                
                outputContainer.Add(choicePort);
            }
            
            RefreshExpandedState();
        }
    }
}
