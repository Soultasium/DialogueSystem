using UnityEngine;

namespace Soulpace.Dialogues.Data.Error
{
    public class DSErrorData
    {
        public Color Color { get; set; }


        public DSErrorData()
        {
            GenerateRandomColor();
        }

        private void GenerateRandomColor()
        {
            Color = new Color32(
                (byte)Random.Range(62, 255),
                (byte)Random.Range(50, 175),
                (byte)Random.Range(50, 175),
                255);
        }
    }
}
