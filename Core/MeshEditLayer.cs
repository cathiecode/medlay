namespace com.superneko.medlay.Core
{
    [System.Serializable]
    public abstract class MeshEditLayer
    {
        /// <summary>
        /// Id for this MeshEditLayer instance.
        /// Please note that this Id is unique per each pipeline, not globally.
        /// This restriction allows cloning/copying parent objects such as MonoBehaviours and ScriptableObjects without re-assigning Ids in Unity Editor.
        /// </summary>
        public long Id { get; set; } = LongId.Generate();
    }
}
