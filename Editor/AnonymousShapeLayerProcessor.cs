namespace com.superneko.medlay.Editor
{
    using Runtime;
    internal class AnonymousShapeLayerProcessor<T> : IAnonymousShapeLayerProcessor where T : ShapeLayer
    {
        private readonly IShapeLayerProcessor<T> shapeLayerProcessor;

        public AnonymousShapeLayerProcessor(IShapeLayerProcessor<T> shapeLayerProcessor)
        {
            this.shapeLayerProcessor = shapeLayerProcessor;
        }

        public void ProcessShapeLayer(ShapeLayer shapeLayer, IShapeProcessContext context)
        {
            shapeLayerProcessor.ProcessShapeLayer((T)shapeLayer, context);
        }
    }
}
