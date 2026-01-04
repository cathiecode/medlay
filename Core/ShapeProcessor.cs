using System.Collections.Generic;

namespace com.superneko.medlay.Core
{
    public class ShapeProcessor
    {
        Dictionary<System.Type, IAnonymousShapeLayerProcessor> shapeLayerProcessors = new Dictionary<System.Type, IAnonymousShapeLayerProcessor>();

        public void RegisterShapeLayer<T>(IShapeLayerProcessor<T> processor) where T : ShapeLayer
        {
            shapeLayerProcessors[typeof(T)] = new AnonymousShapeLayerProcessor<T>(processor);
        }

        public void Process(ShapeLayer[] shapeLayers)
        {
            var context = new ShapeProcessContext();

            foreach (var shapeLayer in shapeLayers)
            {
                var type = shapeLayer.GetType();
                if (shapeLayerProcessors.TryGetValue(type, out var processor))
                {
                    processor.ProcessShapeLayer(shapeLayer, context);
                }
            }
        }
    }
}
