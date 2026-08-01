using System.Collections.Generic;
using UnityEngine;

public class BuildingHighlight : MonoBehaviour
{
    [Header("Highlight Overlay")]
    [Tooltip("Assign a transparent yellow URP material.")]
    [SerializeField] private Material highlightMaterial;

    [Header("Pulse")]
    [SerializeField] private bool pulseHighlight = true;
    [SerializeField] private float pulseSpeed = 2f;

    [Range(0.1f, 1f)]
    [SerializeField] private float minimumBrightness = 0.4f;

    [Range(1f, 5f)]
    [SerializeField] private float maximumBrightness = 2f;

    private readonly List<Renderer> targetRenderers =
        new List<Renderer>();

    private readonly List<Material[]> originalMaterials =
        new List<Material[]>();

    private readonly List<Material[]> highlightedMaterials =
        new List<Material[]>();

    private Material runtimeHighlightMaterial;

    private bool initialized;
    private bool isHighlighted;

    private static readonly int BaseColorID =
        Shader.PropertyToID("_BaseColor");

    private static readonly int EmissionColorID =
        Shader.PropertyToID("_EmissionColor");

    private Color baseOverlayColor = Color.yellow;
    private Color baseEmissionColor = Color.yellow;

    private void Awake()
    {
        Initialize();
    }

    private void Update()
    {
        if (!isHighlighted ||
            !pulseHighlight ||
            runtimeHighlightMaterial == null)
        {
            return;
        }

        float pulseRange =
            Mathf.Max(
                0.01f,
                maximumBrightness - minimumBrightness
            );

        float brightness =
            Mathf.PingPong(
                Time.time * pulseSpeed,
                pulseRange
            ) + minimumBrightness;

        UpdateHighlightBrightness(brightness);
    }

    private void Initialize()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;

        targetRenderers.Clear();
        originalMaterials.Clear();
        highlightedMaterials.Clear();

        if (highlightMaterial == null)
        {
            Debug.LogError(
                name +
                ": Highlight Material is not assigned.",
                this
            );

            return;
        }

        runtimeHighlightMaterial =
            new Material(highlightMaterial);

        runtimeHighlightMaterial.name =
            highlightMaterial.name + " Runtime";

        runtimeHighlightMaterial.EnableKeyword("_EMISSION");

        if (runtimeHighlightMaterial.HasProperty(BaseColorID))
        {
            baseOverlayColor =
                runtimeHighlightMaterial.GetColor(BaseColorID);
        }

        if (runtimeHighlightMaterial.HasProperty(EmissionColorID))
        {
            baseEmissionColor =
                runtimeHighlightMaterial.GetColor(EmissionColorID);
        }

        Renderer[] renderers =
            GetComponentsInChildren<Renderer>(true);

        foreach (Renderer targetRenderer in renderers)
        {
            if (targetRenderer == null)
            {
                continue;
            }

            Material[] originals =
                targetRenderer.sharedMaterials;

            if (originals == null ||
                originals.Length == 0)
            {
                continue;
            }

            Material[] combinedMaterials =
                new Material[originals.Length + 1];

            for (int i = 0; i < originals.Length; i++)
            {
                combinedMaterials[i] = originals[i];
            }

            combinedMaterials[
                combinedMaterials.Length - 1
            ] = runtimeHighlightMaterial;

            targetRenderers.Add(targetRenderer);
            originalMaterials.Add(originals);
            highlightedMaterials.Add(combinedMaterials);
        }

        Debug.Log(
            name + " prepared " +
            targetRenderers.Count +
            " renderers for overlay highlighting.",
            this
        );
    }

    public void EnableHighlight()
    {
        Initialize();

        if (runtimeHighlightMaterial == null)
        {
            return;
        }

        isHighlighted = true;

        for (int i = 0; i < targetRenderers.Count; i++)
        {
            Renderer targetRenderer =
                targetRenderers[i];

            if (targetRenderer == null)
            {
                continue;
            }

            targetRenderer.sharedMaterials =
                highlightedMaterials[i];
        }

        UpdateHighlightBrightness(
            maximumBrightness
        );

        Debug.Log(
            "Highlight enabled on " + name,
            this
        );
    }

    public void DisableHighlight()
    {
        Initialize();

        isHighlighted = false;

        for (int i = 0; i < targetRenderers.Count; i++)
        {
            Renderer targetRenderer =
                targetRenderers[i];

            if (targetRenderer == null)
            {
                continue;
            }

            targetRenderer.sharedMaterials =
                originalMaterials[i];
        }
    }

    private void UpdateHighlightBrightness(
        float brightness)
    {
        if (runtimeHighlightMaterial == null)
        {
            return;
        }

        if (runtimeHighlightMaterial.HasProperty(BaseColorID))
        {
            Color overlayColor = baseOverlayColor;

            // Preserve the transparency set on the material.
            overlayColor.r *= Mathf.Clamp(
                brightness,
                0.5f,
                1.5f
            );

            overlayColor.g *= Mathf.Clamp(
                brightness,
                0.5f,
                1.5f
            );

            overlayColor.b *= Mathf.Clamp(
                brightness,
                0.5f,
                1.5f
            );

            runtimeHighlightMaterial.SetColor(
                BaseColorID,
                overlayColor
            );
        }

        if (runtimeHighlightMaterial.HasProperty(EmissionColorID))
        {
            runtimeHighlightMaterial.EnableKeyword(
                "_EMISSION"
            );

            runtimeHighlightMaterial.SetColor(
                EmissionColorID,
                baseEmissionColor * brightness
            );
        }
    }

    private void OnDestroy()
    {
        if (runtimeHighlightMaterial != null)
        {
            Destroy(runtimeHighlightMaterial);
        }
    }
}