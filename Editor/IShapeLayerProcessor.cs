namespace com.superneko.medlay.Editor
{
    using Runtime;

    public interface IShapeLayerProcessor<T> where T : ShapeLayer
    {
        public void ProcessShapeLayer(T shapeLayer, IShapeProcessContext context);

        internal void ProcessShapeLayer(ShapeLayer shapeLayer, IShapeProcessContext context)
        {
            ProcessShapeLayer((T)shapeLayer, context);
        }
    }
}
