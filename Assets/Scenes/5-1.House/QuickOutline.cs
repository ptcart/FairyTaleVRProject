using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Renderer))]
public class QuickOutline : MonoBehaviour
{
    [Header("Emission 설정")]
    public Color emissionColor = new Color32(118, 79, 7, 255); // 노란빛    // 빛 색상
    [Range(0f, 5f)] public float emissionIntensity = 1f; // 빛 세기

    private Renderer rend;
    private Material originalMat;
    private Material emissionMat;
    private bool isEmitting = false;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        originalMat = rend.material;

        // ✅ 원본 복사해서 Emission 가능한 새 머티리얼 생성
        emissionMat = new Material(Shader.Find("Standard"));
        emissionMat.CopyPropertiesFromMaterial(originalMat);

        // 🔹 Emission 기능 활성화 상태로 만들어두고, 처음엔 검정으로 꺼두기
        emissionMat.EnableKeyword("_EMISSION");
        emissionMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        emissionMat.SetColor("_EmissionColor", Color.black);

        isEmitting = false;
    }

    public void SetOutline(bool enable)
    {
        if (enable && !isEmitting)
        {
            // ✅ Emission 켜기
            emissionMat.EnableKeyword("_EMISSION");
            emissionMat.SetColor("_EmissionColor", emissionColor * emissionIntensity);
            rend.material = emissionMat;
            isEmitting = true;
        }
        else if (!enable && isEmitting)
        {
            // ✅ Emission 끄기
            emissionMat.SetColor("_EmissionColor", Color.black);
            emissionMat.DisableKeyword("_EMISSION");
            rend.material = originalMat;
            isEmitting = false;
        }
    }
}