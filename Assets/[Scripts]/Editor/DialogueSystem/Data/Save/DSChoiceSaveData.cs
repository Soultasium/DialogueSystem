using System;
using UnityEngine;

namespace Soulpace.Dialogues.Data.Save
{
	[Serializable]
    public class DSChoiceSaveData
    {
	    [field: SerializeField] public string Text { get; set; }
	    [field: SerializeField] public string NodeID { get; set; }
    }
}
