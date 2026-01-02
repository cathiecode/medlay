namespace com.superneko.medlay.Editor
{
    using Runtime;

    internal interface IAnonymousShapeLayerProcessor
    {
        void ProcessShapeLayer(ShapeLayer shapeLayer, IShapeProcessContext context);
    }
}
