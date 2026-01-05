using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using UnityEngine;

namespace com.superneko.medlay.Core
{
    public sealed class Medlay
    {
        static Medlay instance;

        public static Medlay Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Medlay();
                }

                return instance;
            }
        }

        Dictionary<System.Type, Func<IMeshEditLayerProcessor>> meshEditLayerProcessorFactories = new Dictionary<System.Type, Func<IMeshEditLayerProcessor>>();

        public void RegisterMeshEditLayerProcessor<T>(Func<IMeshEditLayerProcessor> processorFactory) where T : MeshEditLayer
        {
            meshEditLayerProcessorFactories[typeof(T)] = processorFactory;
        }

        public MedlayPipeline CreatePipeline(Renderer renderer, IEnumerable<MeshEditLayer> meshEditLayers)
        {
            var meshEditLayerProcessors = new List<IMeshEditLayerProcessor>();

            var layers = meshEditLayers.Select(layer =>
            {
                GetProcessorForMeshEditLayer(layer, out var processor);

                if (processor == null)
                {
                    // TODO: Softer error handling?
                    throw new Exception($"Mesh Edit Layer Processor for {layer.GetType()} is not registered.");
                }

                return (layer, processor);
            });

            return new MedlayPipeline(renderer, layers.ToArray(), this);
        }

        internal void GetProcessorForMeshEditLayer(MeshEditLayer meshEditLayer, out IMeshEditLayerProcessor processor)
        {
            var meshEditLayerType = meshEditLayer.GetType();

            if (!meshEditLayerProcessorFactories.TryGetValue(meshEditLayerType, out var processorFactory))
            {
                throw new Exception($"Mesh Edit Layer Processor for {meshEditLayerType} is not registered.");
            }

            processor = processorFactory();
        }
    }
}
