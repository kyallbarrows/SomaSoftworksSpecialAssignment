using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SpecialAssignment
{
    public class FileAssetSelector : MonoBehaviour
    {
        [FormerlySerializedAs("GroupId")] [SerializeField] private int _groupId;
        private List<FileAsset> _files;

        public void Start()
        {
            _files = transform.GetComponentsInChildren<FileAsset>().ToList();
            
            EventBetter.Listen(this, (FileAssetSelectedMessage msg) =>
            {
                if (msg.Group == _groupId)
                {
                    foreach (var fileAsset in _files)
                    {
                        if (fileAsset.Id != msg.Id) fileAsset.Deselect();
                    }
                }
            });
        }
    }
}