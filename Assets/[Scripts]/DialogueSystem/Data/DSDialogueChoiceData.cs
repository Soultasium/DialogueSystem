using System;
using UnityEngine;

namespace Soulpace.Dialogues.Data
{
    using ScriptableObjects;
    
    [Serializable]
    public class DSDialogueChoiceData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public DSDialogueSO NextDialogue { get; set; }
    }
}
