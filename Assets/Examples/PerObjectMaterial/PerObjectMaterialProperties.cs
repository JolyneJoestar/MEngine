
using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialProperties:MonoBehaviour
{
    static int m_baseColorId = Shader.PropertyToID("MTint");

    static MaterialPropertyBlock block;
    [SerializeField]
    Color m_baseColor = Color.white;

     
    void OnValidate()
    {
        if(block == null)
        {
            block = new MaterialPropertyBlock();
        }
        block.SetColor(m_baseColorId, m_baseColor);
        GetComponent<Renderer>().SetPropertyBlock(block);
    }

    void Awake()
    {
        OnValidate();
    }
}
