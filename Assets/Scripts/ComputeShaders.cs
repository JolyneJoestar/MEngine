using UnityEngine;

public class ComputeShaders 
{
    string m_shaderName = "VSMCompute";
    int m_kernel;
    ComputeShader m_computeShader;

    static int ResultID = Shader.PropertyToID("_Result"),
        InputTextureID = Shader.PropertyToID("_InputTexture");
    int m_globalInputTexId;
    int m_globalOutputTexId;
    int m_globalTexSize;

    public void SetUp(int inputTexID, int outputTexID, int texSize)
    {
        m_globalInputTexId = inputTexID;
        m_globalOutputTexId = outputTexID;
        m_globalTexSize = texSize;
        m_kernel = m_computeShader.FindKernel(m_shaderName);
    }
    public void Start()
    {
        m_kernel = m_computeShader.FindKernel(m_shaderName);
        m_computeShader.SetTextureFromGlobal(m_kernel, ResultID, m_globalOutputTexId);
        m_computeShader.SetTextureFromGlobal(m_kernel, InputTextureID, m_globalInputTexId);

        m_computeShader.Dispatch(m_kernel, m_globalTexSize / 8, m_globalTexSize / 8, 1);
    }
}
