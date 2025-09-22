using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{

    //[SerializeField] private List<TKey> keys = new List<TKey>();
    //[SerializeField] private List<TValue> values = new List<TValue>();
    [SerializeField] private List<SerializableKeyValuePair<TKey, TValue>> entries = new List<SerializableKeyValuePair<TKey, TValue>>();

    // save the dictionary to lists
    public void OnBeforeSerialize()
    {
        //keys.Clear();
        //values.Clear();
        //foreach (KeyValuePair<TKey, TValue> pair in this) 
        //{
        //    keys.Add(pair.Key);
        //    values.Add(pair.Value);
        //}

        entries.Clear();
		foreach (KeyValuePair<TKey, TValue> pair in this)
		{
			entries.Add(new SerializableKeyValuePair<TKey, TValue>(pair.Key, pair.Value));
		}
	}

    // load the dictionary from lists
    public void OnAfterDeserialize()
    {
        this.Clear();

        //if (keys.Count != values.Count) 
        //{
        //    Debug.LogError("Tried to deserialize a SerializableDictionary, but the amount of keys ("
        //        + keys.Count + ") does not match the number of values (" + values.Count 
        //        + ") which indicates that something went wrong");
        //}

        //for (int i = 0; i < keys.Count; i++) 
        //{
        //    this.Add(keys[i], values[i]);
        //}

        foreach(SerializableKeyValuePair<TKey, TValue> sKVP in entries)
		{
            if (!this.ContainsKey(sKVP.key))
			{
                this.Add(sKVP.key, sKVP.value);
			}
            else
			{
                if (default(TKey) == null)
                {
                    Debug.LogError($"Can't add entry with key of default value to the dictionary because the default value of {typeof(TKey)} is null.");
                }
                else
                {
                    if (!this.ContainsKey(default))
                    {
                        this.Add(default, default);
                    }
                    else
                    {
                        Debug.LogError($"Can't add entry with key of default value ({default(TKey)}) because it already exists in the dictionary.");
                    }
                }
            }
		}
    }

    [System.Serializable]
    class SerializableKeyValuePair<TKVPKey, TKVPValue>
    {
        public TKVPKey key;
        public TKVPValue value;

		public SerializableKeyValuePair(TKVPKey key, TKVPValue value)
		{
			this.key = key;
			this.value = value;
		}
    }
}
