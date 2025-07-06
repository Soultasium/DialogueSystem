using System.Collections.Generic;
using UnityEngine;

namespace Soulpace.Dialogues.ScriptableObjects
{
    using Data;
    using Enumerations;
    
    public class DSDialogueSO : ScriptableObject
    {
        [field: SerializeField] public string DialogueName { get; set; }
        [field: SerializeField, TextArea()] public string Text { get; set; }
        [field: SerializeField] public List<DSDialogueChoiceData> Choices { get; set; }
        [field: SerializeField] public DSDialoguesType DialogueType { get; set; }
        [field: SerializeField] public bool IsStartingDialogue { get; set; }

        
        public void Initialize(string dialogueName, string text, List<DSDialogueChoiceData> choices, 
            DSDialoguesType dialogueType, bool isStartingDialogue)
        {
            DialogueName = dialogueName;
            Text = text;
            Choices = choices;
            DialogueType = dialogueType;
            IsStartingDialogue = isStartingDialogue;
        }
    }
}
