using NUnit.Framework;

namespace com.superneko.medlay.Tests
{
    using Core;
    using UnityEngine;

    class PipelineTests
    {
        class MockShapeLayer : MeshEditLayer
        {
            int id = 0;
        }

        class MockShapeLayerProcessor : MeshEditLayerProcessor<MockShapeLayer>
        {
            internal bool called = false;

            public override void ProcessMeshEditLayer(MockShapeLayer meshEditLayer, IMeshEditContext context)
            {
                called = true;
            }
        }

        [Test]
        public void RegisterShapeLayer_DoesNotThrow()
        {
            var medlay = new Medlay();
            var meshEditLayerProcessor = new MockShapeLayerProcessor();

            Assert.DoesNotThrow(() =>
            {
                medlay.RegisterMeshEditLayerProcessor<MockShapeLayer>(() => meshEditLayerProcessor);
            });
        }

        [Test]
        public void EmptyPipeline_DoesNotThrow()
        {
            var medlay = new Medlay();

            var smr = TestUtils.LoadAvatarSMR("440876361ce1587438eaa1b04b593d6e");

            Assert.DoesNotThrow(() =>
            {
                medlay.CreatePipeline(smr, new MeshEditLayer[0]).Process();
            });
        }

        [Test]
        public void EmptyPipeline_ProducesSameMesh()
        {
            var medlay = new Medlay();

            var smr = TestUtils.LoadAvatarSMR("55eb1dc98a84ca547b4c78dd8ab9ff3a");
            var originalMesh = smr.sharedMesh;

            var pipeline = medlay.CreatePipeline(smr, new MeshEditLayer[0]);

            pipeline.Process();

            var deformedMesh = pipeline.GetDeformedMesh();

            TestUtils.AssertMeshesAreSame(originalMesh, deformedMesh, allowExactMatch: true);
        }

        [Test]
        public void ProcessPipeline_CallsProcessor()
        {
            var medlay = new Medlay();
            var meshEditLayerProcessor = new MockShapeLayerProcessor();

            medlay.RegisterMeshEditLayerProcessor<MockShapeLayer>(() => meshEditLayerProcessor);

            var smr = TestUtils.LoadAvatarSMR("440876361ce1587438eaa1b04b593d6e");
            var meshEditLayer = new MockShapeLayer();

            var pipeline = medlay.CreatePipeline(smr, new MeshEditLayer[] { meshEditLayer });

            pipeline.Process();

            Assert.IsTrue(meshEditLayerProcessor.called);
        }

        [Test]
        public void ProcessPipeline_CanProcessMeshRenderer()
        {
            var medlay = new Medlay();
            
            var mr = TestUtils.LoadAssetByGUID<MeshRenderer>("2c2374503af4cd64da6dced339c0d2bd");

            Assert.DoesNotThrow(() => {
                medlay.CreatePipeline(mr, new MeshEditLayer[0]).Process();
            });
        }

        [Test]
        public void ProcessPipeline_CanProcessInvalidSMR()
        {
            var medlay = new Medlay();

            var smr = TestUtils.LoadAvatarSMR("10a61e39ad8edab4bbc379ca484bd361");

            var pipeline = medlay.CreatePipeline(smr, new MeshEditLayer[0]);

            pipeline.Process();

            var deformedMesh = pipeline.GetDeformedMesh();

            TestUtils.AssertMeshesAreSame(smr.sharedMesh, deformedMesh, allowExactMatch: true);
            TestUtils.AssertMeshDoesNotHaveNaN(deformedMesh);
        }

        [Test]
        public void ProcessPipeline_ProcessMeshRenderer_ProducesSameMesh()
        {
            var medlay = new Medlay();

            var mr = TestUtils.LoadAssetByGUID<MeshRenderer>("2c2374503af4cd64da6dced339c0d2bd");
            var mf = mr.GetComponent<MeshFilter>();

            var originalMesh = mf.sharedMesh;

            var pipeline = medlay.CreatePipeline(mr, new MeshEditLayer[0]);

            pipeline.Process();

            var deformedMesh = pipeline.GetDeformedMesh();

            TestUtils.AssertMeshesAreSame(originalMesh, deformedMesh, allowExactMatch: true);
            TestUtils.AssertMeshDoesNotHaveNaN(deformedMesh);
        }
    }
}
