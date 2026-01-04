using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.superneko.medlay.Core
{
    public class Medlay
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

        public Pipeline CreatePipeline(Renderer renderer, MeshEditLayer[] meshEditLayers)
        {
            
            var meshEditLayerProcessors = new IMeshEditLayerProcessor[meshEditLayers.Length];

            for (int i = 0; i < meshEditLayers.Length; i++)
            {
                GetProcessorForMeshEditLayer(meshEditLayers[i], out var processor);
                meshEditLayerProcessors[i] = processor;
            }

            return new Pipeline(renderer, meshEditLayers.Zip(meshEditLayerProcessors, (layer, processor) => (layer, processor)).ToArray());
        }

        void GetProcessorForMeshEditLayer(MeshEditLayer meshEditLayer, out IMeshEditLayerProcessor processor)
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
