using System;
using System.Collections.Generic;
using UnityEngine;

namespace Soulpace.Dialogues.Data.Save
{
    using Enumerations;
    
    [Serializable]
    public class DSNodeSaveData
    {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public List<DSChoiceSaveData> Choices { get; set; }
        [field: SerializeField] public string GroupID { get; set; }
        [field: SerializeField] public DSDialoguesType DialogueType { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }
    }
}
