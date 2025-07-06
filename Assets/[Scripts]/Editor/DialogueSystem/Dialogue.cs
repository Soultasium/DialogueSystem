using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Soulpace
{
	public class Dialogue : MonoBehaviour
	{
		private static Dialogue s_instance;

		[SerializeField] private TMP_Text _nameText;
		[SerializeField] private TMP_Text _dialogueText;


		public static void StartDialogue(DialogueData dialogue)
		{
			
		}
	}

	public struct DialogueData
	{
		public string DialogueId;
		
	}

	public struct DialogueStep
	{
		public string CharacterName;
		public string Text;
		public List<IDialogueEvent> DialogueEvents;
	}

	public interface IDialogueEvent
	{
		public string Id { get; set; }
	}
}
