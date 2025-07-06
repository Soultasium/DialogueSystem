using Soulpace.Inputs;
using UnityEngine;
using UnityEngine.Events;

namespace Soulpace
{
	public class OnHoverEvent : MonoBehaviour, ISelectable
	{
		private readonly Color _defaultColor = Color.white;
		private readonly Color _hoverColor = new Color(0.7f, 0.7f, 0.7f, 1.0f);

		private static OnHoverEvent s_selectedInstance;

		[SerializeField] private SpriteRenderer[] _sprites;

		[SerializeField] private UnityEvent _onSelected;
		[SerializeField] private UnityEvent _onDeselected;

		public void OnHoverEnter()
		{
			SetColor(_hoverColor);
			Debug.Log("OnHoverEnter");
		}

		public void OnHoverExit()
		{

		}

		public void OnClicked()
		{
			if (s_selectedInstance == this)
				return;

			if (s_selectedInstance != null)
				s_selectedInstance.Deselect();

			s_selectedInstance = this;
			Select();
		}

		public void Select()
		{

		}

		private void Deselect()
		{

		}

		private void SetColor(Color color)
		{
			foreach (var sprite in _sprites)
			{
				sprite.color = color;
			}
		}
	}
}