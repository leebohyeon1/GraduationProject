using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace INab.Dissolve
{
    [ExecuteInEditMode]
    public class Dissolver : MonoBehaviour
    {
        #region Enums
        public enum CurveSettings
        {
            TwoCurves,          // Two separate curves for dissolve and materialize
            OneCurve,           // One curve for both dissolve and materialize
        }

        public enum DissolveState
        {
            Dissolved,      // Object is fully dissolved
            Materialized    // Object is fully materialized
        }

        [System.Flags]
        public enum KeywordsFlags
        {
            UseDissolve = 1,            // Flag for using dissolve keyword in materials
            UseVertexDisplacement = 2,   // Flag for using vertex displacement keyword in materials
        }

        #endregion

        #region ManualControl
        public bool manualControl = false;

        [Tooltip("When turned on, material values will always be updated in the update() function. Turn it off when you want to modify the dissolve amount property in materials. It is automatically turned on in the Start() function and needs to be on during runtime.")]
        public bool updateValues = true;

        [SerializeField, Range(-1, 2)]
        [Tooltip("Value of the dissolve amount property in the materials list.")]
        private float MaterialsDissolveValue = 1f;

        [SerializeField,Range(-1,2)]
        [Tooltip("Value of the dissolve amount property in the inverted materials list.")]
        private float MaterialsInvertedDissolveValue = 0f;

        #endregion

        #region AutomaticControl
        [Tooltip("How to evaluate the curves.")]
        public CurveSettings curveSettings = CurveSettings.OneCurve;

        public AnimationCurve dissolveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public AnimationCurve materializeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Tooltip("Duration of the effect.")]
        public float duration = 2f;

        [Tooltip("Initial state the object will be set on Start().")]
        public DissolveState initialState = DissolveState.Materialized;

        [Tooltip("Current state of the object.")]
        public DissolveState currentState;

        [Tooltip("Indicates which keywords should be automatically enabled and disabled when needed.")]
        public KeywordsFlags keywordsFlags;

        [Tooltip("Whether to use automatic keywords which make sure shader do not render any unnessesery stuff.")]
        public bool useAutomaticKeywords = false;

        #endregion

        [Tooltip("Materials the effect will be performed on.")]
        public List<Material> materials = new List<Material>();

        [Tooltip("Materials the effect will be performed on in inverted manner.")]
        public List<Material> materialsInverted = new List<Material>();

        private const string VfxRendererTypeName = "UnityEngine.VFX.VFXRenderer";
        private static readonly int DissolveAmountId = Shader.PropertyToID("_DissolveAmount");

        private readonly Dictionary<Material, List<RendererMaterialBinding>> rendererBindings = new Dictionary<Material, List<RendererMaterialBinding>>();
        private readonly List<RuntimeMaterialSet> runtimeMaterialSets = new List<RuntimeMaterialSet>();
        private MaterialPropertyBlock propertyBlock;

        private sealed class RendererMaterialBinding
        {
            public Renderer Renderer;
            public int MaterialIndex;
        }

        private sealed class RuntimeMaterialSet
        {
            public Renderer Renderer;
            public Material[] OriginalMaterials;
            public Material[] RuntimeMaterials;
        }


        #region VFXGraph
        // Delegates used with visual effect graph

        public delegate void DissolveAmountChange(float value);
        public event DissolveAmountChange OnPropertyUpdate;

        public delegate void DissolveEvent(bool start, bool materialize);
        public event DissolveEvent OnDissolveStateChange;

        #endregion

        private void OnEnable()
        {
            currentState = initialState;
        }

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        private void OnDestroy()
        {
            ReleaseRuntimeMaterials();
        }

        public void Start()
        {
            if (Application.isPlaying)
            {
                // 이미 리스트가 있더라도 인스턴스로 다시 찾아와야 합니다.
                // 만약 특정 자식만 포함해야 한다면 별도 로직이 필요하지만, 
                // 기본적으로는 전체를 다시 찾는 것이 가장 안전합니다.
                FindMaterialsInChildren();
            }

            materials.RemoveAll(mat => mat == null);
            materialsInverted.RemoveAll(mat => mat == null);

            updateValues = true;

            if (initialState == DissolveState.Dissolved)
            {
                foreach (var material in materials)
                {
                    ChangeDissolveAmount(material, 1);
                }

                foreach (var material in materialsInverted)
                {
                    ChangeDissolveAmount(material, 0);
                }
            }
            else
            {
                foreach (var material in materials)
                {
                    ChangeDissolveAmount(material, 0);
                }

                foreach (var material in materialsInverted)
                {
                    ChangeDissolveAmount(material, 1);
                }
            }

        }

        public void Update()
        {
            if (manualControl && updateValues)
            {
                ManualValuesUpdate();
            }
        }

        #region PublicFunctions

        /// <summary>
        /// Find and initialize the materials to be dissolved/materialized using GetComponentsInChildren
        /// </summary>
        public void FindMaterialsInChildren()
        {
            FindMaterials(GetComponentsInChildren<Renderer>());
        }

        /// <summary>
        /// Find and initialize the materials to be dissolved/materialized using GetComponents
        /// </summary>
        public void FindMaterials()
        {
            FindMaterials(GetComponents<Renderer>());
        }

        /// <summary>
        /// Materialize the object
        /// </summary>
        public void Materialize()
        {
            EnableKeywords();

            if (materials.Count == 0)
            {
                Debug.LogWarning("There are no materials to materialize in " + name);
                return;
            }

            //if (currentState == DissolveState.Materialized)
            //{
            //    Debug.LogWarning("You are trying to materialize an already materialized object in " + name);
            //    return;
            //}

            StartCoroutine(MaterializeEnumerator());
        }

        /// <summary>
        /// Dissolve the object
        /// </summary>
        public void Dissolve()
        {
            EnableKeywords();

            if (materials.Count == 0)
            {
                Debug.LogWarning("There are no materials to dissolve in " + name);
                return;
            }

            //if (currentState == DissolveState.Dissolved)
            //{
            //    Debug.LogWarning("You are trying to dissolve an already dissolved object in " + name);
            //    return;
            //}

            StartCoroutine(DissolveEnumerator());
        }

        /// <summary>
        /// Update dissolve amount properties in materials and materials inverted.
        /// </summary>
        public void ManualValuesUpdate()
        {
            foreach (var material in materials)
            {
                ChangeDissolveAmount(material, MaterialsDissolveValue);
            }

            foreach (var material in materialsInverted)
            {
                // Change material value
                // Do not call Dissolver VFX update event
                SetDissolveAmount(material, MaterialsInvertedDissolveValue);
            }
        }

        #endregion

        #region PrivateFunctions

        /// <summary>
        /// Update _DissolveAmount property in material and call OnPropertyUpdate() event.
        /// </summary>
        /// <param name="material"></param>
        /// <param name="dissolveAmount"></param>
        private void ChangeDissolveAmount(Material material, float dissolveAmount)
        {
            if (material == null)
            {
                return;
            }

            SetDissolveAmount(material, dissolveAmount);

            // Call event for visual effect
            if (OnPropertyUpdate != null) OnPropertyUpdate(dissolveAmount);
        }

        private void FindMaterials(Renderer[] renderers)
        {
            ReleaseRuntimeMaterials();
            rendererBindings.Clear();
            materials.Clear();

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null || renderer.GetType().FullName == VfxRendererTypeName) continue;

                Material[] sharedMaterials = renderer.sharedMaterials;
                if (Application.isPlaying && useAutomaticKeywords)
                {
                    Material[] runtimeMaterials = CreateRuntimeMaterials(sharedMaterials);
                    renderer.sharedMaterials = runtimeMaterials;
                    runtimeMaterialSets.Add(new RuntimeMaterialSet
                    {
                        Renderer = renderer,
                        OriginalMaterials = sharedMaterials,
                        RuntimeMaterials = runtimeMaterials
                    });
                    materials.AddRange(runtimeMaterials);
                    continue;
                }

                for (int materialIndex = 0; materialIndex < sharedMaterials.Length; materialIndex++)
                {
                    Material material = sharedMaterials[materialIndex];
                    if (material == null) continue;

                    materials.Add(material);
                    if (!Application.isPlaying) continue;

                    if (!rendererBindings.TryGetValue(material, out List<RendererMaterialBinding> bindings))
                    {
                        bindings = new List<RendererMaterialBinding>();
                        rendererBindings.Add(material, bindings);
                    }

                    bindings.Add(new RendererMaterialBinding
                    {
                        Renderer = renderer,
                        MaterialIndex = materialIndex
                    });
                }
            }
        }

        private static Material[] CreateRuntimeMaterials(Material[] sourceMaterials)
        {
            Material[] runtimeMaterials = new Material[sourceMaterials.Length];
            for (int i = 0; i < sourceMaterials.Length; i++)
            {
                Material source = sourceMaterials[i];
                runtimeMaterials[i] = source == null ? null : new Material(source);
            }

            return runtimeMaterials;
        }

        private void SetDissolveAmount(Material material, float dissolveAmount)
        {
            if (material == null) return;

            if (!rendererBindings.TryGetValue(material, out List<RendererMaterialBinding> bindings))
            {
                material.SetFloat(DissolveAmountId, dissolveAmount);
                return;
            }

            foreach (RendererMaterialBinding binding in bindings)
            {
                if (binding.Renderer == null) continue;

                binding.Renderer.GetPropertyBlock(propertyBlock, binding.MaterialIndex);
                propertyBlock.SetFloat(DissolveAmountId, dissolveAmount);
                binding.Renderer.SetPropertyBlock(propertyBlock, binding.MaterialIndex);
            }
        }

        private void ReleaseRuntimeMaterials()
        {
            foreach (RuntimeMaterialSet materialSet in runtimeMaterialSets)
            {
                if (materialSet.Renderer != null)
                {
                    materialSet.Renderer.sharedMaterials = materialSet.OriginalMaterials;
                }

                foreach (Material material in materialSet.RuntimeMaterials)
                {
                    if (material == null) continue;

                    if (Application.isPlaying)
                    {
                        Destroy(material);
                    }
                    else
                    {
                        DestroyImmediate(material);
                    }
                }
            }

            runtimeMaterialSets.Clear();
        }


        /// <summary>
        /// Calls delegate that sends vfx graph events.
        /// </summary>
        /// <param name="start">Whether the dissolve starts or not.</param>
        /// <param name="materialize"></param>
        private void ChangeDissolveState(bool start, bool materialize = false)
        {
            // Call event for visual effect
            if (OnDissolveStateChange != null) OnDissolveStateChange(start,materialize);
        }

        // Check if a flag is set in a bitmask
        private static bool HasFlag(KeywordsFlags a, KeywordsFlags b)
        {
            return (a & b) == b;
        }

        // Enable keywords in the materials based on the flags
        private void EnableKeywords()
        {
            if (!useAutomaticKeywords) return;

            if (HasFlag(keywordsFlags, KeywordsFlags.UseDissolve))
            {
                foreach (var material in materials)
                {
                    material.EnableKeyword("_USE_DISSOLVE");
                }

                foreach (var material in materialsInverted)
                {
                    material.EnableKeyword("_USE_DISSOLVE");
                }
            }

            if (HasFlag(keywordsFlags, KeywordsFlags.UseVertexDisplacement))
            {
                foreach (var material in materials)
                {
                    material.EnableKeyword("_USE_VERTEX_DISPLACEMENT");
                }

                foreach (var material in materialsInverted)
                {
                    material.EnableKeyword("_USE_VERTEX_DISPLACEMENT");
                }
            }

        }

        // Disable keywords in the materials based on the flags
        private void DisableKeywords()
        {
            if (!useAutomaticKeywords) return;

            if (HasFlag(keywordsFlags, KeywordsFlags.UseDissolve))
            {
                foreach (var material in materials)
                {
                    material.DisableKeyword("_USE_DISSOLVE");
                }

                foreach (var material in materialsInverted)
                {
                    material.DisableKeyword("_USE_DISSOLVE");
                }
            }

            if (HasFlag(keywordsFlags, KeywordsFlags.UseVertexDisplacement))
            {
                foreach (var material in materials)
                {
                    material.DisableKeyword("_USE_VERTEX_DISPLACEMENT");
                }

                foreach (var material in materialsInverted)
                {
                    material.DisableKeyword("_USE_VERTEX_DISPLACEMENT");
                }
            }
        }

        // Coroutine to gradually materialize the object
        private IEnumerator MaterializeEnumerator()
        {
            float dissolveAmount;
            float elapsedTime = 0f;

            AnimationCurve curve;
            if (curveSettings == CurveSettings.TwoCurves)
            {
                curve = materializeCurve;   // Use the materialize curve for materialization
            }
            else
            {
                curve = dissolveCurve;      // Use the dissolve curve for materialization
            }

            // Called for VFX graph events
            ChangeDissolveState(true,true);

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;

                if (curveSettings == CurveSettings.OneCurve)
                {
                    dissolveAmount = curve.Evaluate(1 - elapsedTime / duration);   // Evaluate the curve in reverse if it is flipped
                }
                else
                {
                    dissolveAmount = curve.Evaluate(elapsedTime / duration);
                }

                foreach (var material in materials)
                {
                    ChangeDissolveAmount(material, dissolveAmount);
                }

                foreach (var material in materialsInverted)
                {
                    ChangeDissolveAmount(material, 1 - dissolveAmount);
                }

                yield return null;
            }

            currentState = DissolveState.Materialized;
            DisableKeywords();

            // Called for VFX graph events
            ChangeDissolveState(false,true);
        }

        // Coroutine to gradually dissolve the object
        private IEnumerator DissolveEnumerator()
        {
            float dissolveAmount;
            float elapsedTime = 0f;

            AnimationCurve curve = dissolveCurve;   // Use the materialize curve for dissolution

            ChangeDissolveState(true);

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;

                dissolveAmount = curve.Evaluate(elapsedTime / duration);   // Evaluate the curve in reverse for dissolution

                foreach (var material in materials)
                {
                    ChangeDissolveAmount(material, dissolveAmount);
                }

                foreach (var material in materialsInverted)
                {
                    ChangeDissolveAmount(material, 1 - dissolveAmount);
                }

                yield return null;
            }

            currentState = DissolveState.Dissolved;
            DisableKeywords();

            // Called for VFX graph events
            ChangeDissolveState(false);
        }

        #endregion

        #region DebugAndDev

        private float coroutnieTimeOffset = .2f;

        /// <summary>
        /// Used ONLY for debug purposes and in inspector editor.
        /// </summary>
        /// <param name="dissolver"></param>
        /// <returns></returns>
        public IEnumerator AutomaticDissolveCoroutine()
        {
            float timeLasted = duration;

            currentState = DissolveState.Materialized;
            Dissolve();

            while (true)
            {
                timeLasted -= Time.deltaTime;

                if (timeLasted < -coroutnieTimeOffset)
                {
                    currentState = DissolveState.Materialized;
                    Dissolve();

                    timeLasted = duration;
                }

                yield return null; // Wait for the next frame
            }
        }

        /// <summary>
        /// Used ONLY for debug purposes and in inspector editor.
        /// </summary>
        /// <param name="dissolver"></param>
        /// <returns></returns>
        public IEnumerator AutomaticMaterializeCoroutine()
        {
            float timeLasted = duration;

            currentState = DissolveState.Dissolved;
            Materialize();

            while (true)
            {
                timeLasted -= Time.deltaTime;

                if (timeLasted < -coroutnieTimeOffset)
                {
                    currentState = DissolveState.Dissolved;
                    Materialize();

                    timeLasted = duration;
                }

                yield return null; // Wait for the next frame
            }
        }

        #endregion

    }
}
