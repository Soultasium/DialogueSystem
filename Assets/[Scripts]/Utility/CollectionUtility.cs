using System.Collections.Generic;

namespace Soulpace.Dialogues.Utility
{
    public static class CollectionUtility
    {
	    public static void AddItem<K, V>(this SerializableDictionary<K, List<V>> serializedDictionary, K key, V value)
	    {
		    if (serializedDictionary.ContainsKey(key))
		    {
			    serializedDictionary[key].Add(value);

			    return;
		    }
		    
		    serializedDictionary.Add(key, new List<V>() { value });
	    }
    }
}
