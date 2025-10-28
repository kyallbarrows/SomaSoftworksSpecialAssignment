using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace SpecialAssignment
{
    public class DirectorReferences : MonoBehaviour
    {
        public List<GameObject> directorObjects;

        private Dictionary<string, PlayableDirector> directors = new();
        
        private void Awake()
        {
            int numChildren = transform.childCount;
            for (int i = 0; i < numChildren; i++)
            {
                var child = transform.GetChild(i);
                var director = child.GetComponent<PlayableDirector>();
                if (director == null)
                    continue;
                directors.Add(child.name, director);
            }
        }

        public PlayableDirector GetDirector(string assetId)
        {
            if (directors.ContainsKey(assetId))
                return directors[assetId];

            return null;
        }
    }
}
