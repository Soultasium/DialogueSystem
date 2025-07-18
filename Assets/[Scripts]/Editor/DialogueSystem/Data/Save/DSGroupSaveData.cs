using System;
using UnityEngine;

namespace Soulpace.Dialogues.Data.Save
{
	[Serializable]
    public class DSGroupSaveData
    {
	    [field: SerializeField] public string ID { get; set; }
	    [field: SerializeField] public string Name { get; set; }
	    [field: SerializeField] public Vector2 Position { get; set; }
    }
}