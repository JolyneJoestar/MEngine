
using UnityEngine;

public class RandomMeshBall:MonoBehaviour
{
    static int m_baseColorId = Shader.PropertyToID("MTint"),
        m_metalicId = Shader.PropertyToID("MMetalic"),
        m_smoothnessId = Shader.PropertyToID("MSmoothness");

    [SerializeField]
    Mesh m_mesh = default;

    [SerializeField]
    Material m_material= default;

    Matrix4x4[] m_matrices = new Matrix4x4[1023];
    Vector4[] m_colors = new Vector4[1023];
    float[] m_metalic = new float[1023],
           m_smoothness = new float[1023];

    MaterialPropertyBlock m_block;

    void Awake()
    {
        for (int i = 0; i < m_matrices.Length; i++)
        {
            m_matrices[i] = Matrix4x4.TRS(Random.insideUnitSphere * 10f, Quaternion.identity, Vector3.one);
            m_colors[i] = new Vector4(Random.value, Random.value, Random.value, 1.0f);
            m_metalic[i] = Random.value;
            m_smoothness[i] = Random.value;
        }
    }

    void Update()
    {
        if (m_block == null)
        {
            m_block = new MaterialPropertyBlock();
            m_block.SetVectorArray(m_baseColorId, m_colors);
            m_block.SetFloatArray(m_metalicId, m_metalic);
            m_block.SetFloatArray(m_smoothnessId, m_smoothness);
        }
        Graphics.DrawMeshInstanced(m_mesh, 0, m_material, m_matrices, 1023, m_block);
    }
}
