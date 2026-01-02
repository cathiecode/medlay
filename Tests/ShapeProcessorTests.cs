using NUnit.Framework;

namespace com.superneko.medlay.Tests
{
    using Editor;
    using Runtime;

    class ShapeProcessorTests
    {
        class MockShapeLayer : ShapeLayer
        {
            int id = 0;
        }

        class MockShapeLayerProcessor: IShapeLayerProcessor<MockShapeLayer>
        {
            internal bool called = false;

            public void ProcessShapeLayer(MockShapeLayer shapeLayer, IShapeProcessContext context)
            {
                called = true;
            }
        }

        [Test]
        public void RegisterShapeLayer_DoesNotThrow()
        {
            var processor = new ShapeProcessor();
            var shapeLayerProcessor = new MockShapeLayerProcessor();

            Assert.DoesNotThrow(() =>
            {
                processor.RegisterShapeLayer(shapeLayerProcessor);
            });
        }

        [Test]
        public void Process_DoesNotThrow()
        {
            var processor = new ShapeProcessor();
            var shapeLayerProcessor = new MockShapeLayerProcessor();
            processor.RegisterShapeLayer(shapeLayerProcessor);

            var shapeLayer = new MockShapeLayer();

            Assert.DoesNotThrow(() =>
            {
                processor.Process(new ShapeLayer[] { shapeLayer });
            });
        }

        [Test]
        public void Process_CallsRegisteredProcessor()
        {
            var processor = new ShapeProcessor();
            var shapeLayerProcessor = new MockShapeLayerProcessor();
            processor.RegisterShapeLayer(shapeLayerProcessor);

            var shapeLayer = new MockShapeLayer();

            processor.Process(new ShapeLayer[] { shapeLayer });

            Assert.IsTrue(shapeLayerProcessor.called);
        }
    }
}
