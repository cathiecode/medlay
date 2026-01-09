using com.superneko.medlay.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace com.superneko.medlay.Runtime
{
    public class DebugMeshEditLayer : MeshEditLayer { }

    public class DebugMeshEditLayerProcessor : MeshEditLayerProcessor<DebugMeshEditLayer>
    {
        #if UNITY_EDITOR
        [InitializeOnLoadMethod]
        #else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        #endif
        static void Register()
        {
            Medlay.Instance.RegisterMeshEditLayerProcessor<DebugMeshEditLayer>(() => new DebugMeshEditLayerProcessor());
        }

        public Mesh mesh = new Mesh();

        public override void ProcessMeshEditLayer(DebugMeshEditLayer meshEditLayer, IMeshEditContext context)
        {
            Object.DestroyImmediate(mesh);

            mesh = Object.Instantiate(context.Mesh);

            mesh.RecalculateBounds();

            DebugMeshEditLayerInspector.inspectedLayerMesh = mesh;
            DebugMeshEditLayerInspector.inspectedLayerMaterials = context.OriginalRenderer.sharedMaterials;
            DebugMeshEditLayerInspector.inspectedLayerLastProcessedOn = Time.realtimeSinceStartup;
        }
    }

    [DefaultExecutionOrder(10000)]
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    [ExecuteAlways]
    public class DebugMeshEditLayerInspector : MonoBehaviour
    {
        MeshRenderer mr;
        MeshFilter mf;

        public static Mesh inspectedLayerMesh;
        public static Material[] inspectedLayerMaterials;
        public static float inspectedLayerLastProcessedOn = 0;

        public float clearAfterSeconds = 30f;
        public float showingProcessedMeshOn = 0;

        void Start()
        {
            mr = GetComponent<MeshRenderer>();
            mf = GetComponent<MeshFilter>();
        }

        void Update()
        {
            mf.sharedMesh = inspectedLayerMesh;
            if (inspectedLayerMaterials != null) mr.materials = inspectedLayerMaterials;
            showingProcessedMeshOn = inspectedLayerLastProcessedOn;

            if (inspectedLayerLastProcessedOn + clearAfterSeconds < Time.realtimeSinceStartup)
            {
                inspectedLayerMesh = null;
                inspectedLayerMaterials = null;
            }
        }
    }
}
