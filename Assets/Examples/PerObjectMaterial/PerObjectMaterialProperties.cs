
using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialProperties:MonoBehaviour
{
    static int m_baseColorId = Shader.PropertyToID("MTint");
    static int m_cutoffId = Shader.PropertyToID("MCutoff");
    static MaterialPropertyBlock block;
    [SerializeField]
    Color m_baseColor = Color.white;
    [SerializeField]
    float cutoff = 0.5f;
     
    void OnValidate()
    {
        if(block == null)
        {
            block = new MaterialPropertyBlock();
        }
        block.SetColor(m_baseColorId, m_baseColor);
        block.SetFloat(m_cutoffId, cutoff);
        GetComponent<Renderer>().SetPropertyBlock(block);
    }

    void Awake()
    {
        OnValidate();
    }
}
