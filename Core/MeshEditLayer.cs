namespace com.superneko.medlay.Core
{
    [System.Serializable]
    public abstract class MeshEditLayer
    {
        public long Id { get; set; } = LongId.Generate();
    }
}
