using System.Collections.Generic;

namespace Soulpace.Inputs
{
    public interface IHoverable
    {
        static readonly Dictionary<int, IHoverable> AllHoverables = new ();
        void OnHoverEnter();
        void OnHoverExit();
    }
}
