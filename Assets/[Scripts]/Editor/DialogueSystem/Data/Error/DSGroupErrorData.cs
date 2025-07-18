using System.Collections.Generic;

namespace Soulpace.Dialogues.Data.Error
{
    using Elements;
    
    public class DSGroupErrorData
    {
        public DSErrorData ErrorData { get; set; }
        public List<DSGroup> Groups { get; set; }

        
        public DSGroupErrorData()
        {
            ErrorData = new DSErrorData();
            Groups = new List<DSGroup>();
        }
    }
}
