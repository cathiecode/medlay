using UnityEngine;

namespace com.superneko.medlay.Core
{
    [System.Serializable]
    public abstract class MeshEditLayer
    {
        [SerializeField] private long _id = LongId.Generate();

        /// <summary>
        /// Id for this MeshEditLayer instance.
        /// Please note that this Id is unique per each pipeline, not globally.
        /// This restriction allows cloning/copying parent objects such as MonoBehaviours and ScriptableObjects without re-assigning Ids in Unity Editor.
        /// </summary>
        public long Id
        {
            get => _id;
            set => _id = value;
        }
    }
}
