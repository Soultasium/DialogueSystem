using UnityEngine;

namespace Soulpace.Dialogues.ScriptableObjects
{
    public class DSDialogueGroupSO : ScriptableObject
    {
        [field: SerializeField] public string GroupName { get; set; }


        public void Initialize(string groupName)
        {
            GroupName = groupName;
        }
    }
}
