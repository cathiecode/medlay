using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace com.superneko.medlay.Core
{
    internal class MeshEditContext : IMeshEditContext
    {
        MedlayWritableMeshData writableMeshData;

        public MedlayWritableMeshData WritableMeshData
        {
            get
            {
                if (writableMeshData == null)
                {
                    writableMeshData = MedlayWritableMeshData.Create(mesh, Allocator.Temp);
                }

                return writableMeshData;
            }
        }

        Mesh mesh;

        public Mesh Mesh
        {
            get
            {
                WritebackIfNeed();

                return mesh;
            }
        }

        public Renderer OriginalRenderer { get; internal set; }

        Matrix4x4 worldToBaseMatrix;
        public Matrix4x4 WorldToBaseMatrix => worldToBaseMatrix;
        internal Dictionary<System.Type, object> ContextData = new Dictionary<System.Type, object>();

        Transform[] bones;
        public Transform[] Bones
        {
            get
            {
                switch (OriginalRenderer)
                {
                    case SkinnedMeshRenderer smr:
                        if (bones == null)
                        {
                            bones = smr.bones;
                        }

                        return bones;
                    default:
                        return null;
                }
            }
            set
            {
                bones = value;
                _bonesAreChanged = true;
            }
        }

        bool _bonesAreChanged = false;
        bool IMeshEditContext.BoneIsChanged => _bonesAreChanged;

        public void WritebackIfNeed()
        {
            if (writableMeshData != null)
            {
                MedlayWritableMeshData.WritebackAndDispose(writableMeshData, mesh);
                writableMeshData = null;
            }
        }

        internal void DisposeWritableMeshIfAvailable()
        {
            if (writableMeshData != null)
            {
                writableMeshData.Dispose();
                writableMeshData = null;
            }
        }

        public static MeshEditContext FromRenderer(Renderer renderer, Mesh writebackMesh, Matrix4x4 worldToBaseMatrix)
        {
            Mesh mesh;

            switch (renderer)
            {
                case SkinnedMeshRenderer smr:
                    mesh = smr.sharedMesh;
                    break;
                case MeshRenderer mr:
                    var mf = mr.GetComponent<MeshFilter>();
                    mesh = mf.sharedMesh;
                    break;
                default:
                    throw new System.Exception("Unsupported renderer type: " + renderer.GetType().Name);
            }

            if (mesh == null)
            {
                throw new System.Exception("Renderer has no mesh: " + renderer.name);
            }

            var context = new MeshEditContext()
            {
                writableMeshData = MedlayWritableMeshData.Create(mesh, Allocator.Temp),
                mesh = writebackMesh,
                OriginalRenderer = renderer,
                worldToBaseMatrix = worldToBaseMatrix
            };

            return context;
        }

        public void TryGetProcessContextData<T>(out T data) where T : class
        {
            if (ContextData == null)
            {
                data = null;
                return;
            }

            if (ContextData.TryGetValue(typeof(T), out var obj))
            {
                data = obj as T;
            }
            else
            {
                data = null;
            }
        }
    }
}
