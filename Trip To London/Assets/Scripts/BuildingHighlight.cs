using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingHighlight : MonoBehaviour
{
    [Header("Highlight Settings")]
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float emissionIntensity = 2f;
    [SerializeField] bool pulseHighlight = true;
    [SerializeField] float pulseSpeed = 2f;
    [SerializeField] float minimumIntensity = 0.5f;

    private readonly List<Material> hotelMaterials = new();
    private readonly List<Color> originalEmissionColors = new();

    private bool isHighlighted;

    private void Awake()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer hotelRenderer in renderers)
        {
            foreach (Material material in hotelRenderer.materials)
            {
                hotelMaterials.Add(material);

                if (material.HasProperty("_EmissionColor"))
                {
                    originalEmissionColors.Add(material.GetColor("_EmissionColor"));
                }
                else
                {
                    originalEmissionColors.Add(Color.black);
                }
            }
        }
    }
    private void Update()
    {
        if (!isHighlighted || !pulseHighlight)
            return;

        float pulse = Mathf.PingPong(Time.time * pulseSpeed, emissionIntensity - minimumIntensity) + minimumIntensity;

        ApplyEmission(pulse);
    }
     public void EnableHighlight()
    {
        isHighlighted = true;
        ApplyEmission(emissionIntensity);
    }
    public void DisableHighlight()
    {
        isHighlighted = false;

        for (int i = 0; i < hotelMaterials.Count; i++)
        {
            Material material = hotelMaterials[i];

            if (!material.HasProperty("_EmissionColor"))
                continue;

            material.SetColor("_EmissionColor", originalEmissionColors[i]);

            if (originalEmissionColors[i] == Color.black)
            {
                material.DisableKeyword("_EMISSION");
            }
        }
    }
    private void ApplyEmission(float intensity)
    {
        Color finalColor = highlightColor * intensity;

        foreach (Material material in hotelMaterials)

        {
            if (!material.HasProperty("_EmissionColor"))
                continue;

            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", finalColor);
        }
    }
}
