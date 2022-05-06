using UnityEngine;
using System.Runtime.InteropServices;

public class ParticleEmitter : MonoBehaviour
{
    public const string VERSION = "2212";


    public const int THREAD_COUNT = 256;
    public int maxParticles = 100000;
    public Transform simulationParent;
    public float timeScale = 1f;



    public ColorMode colorMode;
    public Color color = Color.white;
    public Gradient colorOverLife;
    public int colorSteps = 16;
    private Texture2D colorOverLifeTexture;

    public SizeMode sizeMode;
    public float size = 1f;
    public AnimationCurve sizeOverLife;
    private ComputeBuffer sizeOverLifeBuffer;
    public int sizeSteps = 16;


    public float minLifetime = 3f;
    public float maxLifetime = 5f;

    public bool enableEmission = true;
    public float emissionRate = 15000f;
    public float minInitialSpeed = 0.1f;
    public float maxInitialSpeed = 0.5f;
    public EmissionShape emissionShape;
    public float sphereRadius = 0.2f;
    public MeshType meshType;
    public Mesh emissionMesh;
    public MeshRenderer meshRenderer;
    public SkinnedMeshRenderer skinnedMeshRenderer;

    public DirectionType directionType;
    public Vector3 direction;

    public bool enableInheritVelocity = true;
    public float inheritVelocity = 0.3f;
    public float extrapolation = 0f;
    private Vector3 previousPositon;

    public bool enableNoise = true;
    public float convergence = 150f;
    public float convergenceFrequency = 10f;
    public float convergenceAmplitude = 200f;
    public float viscosity = 0.1f;

    public bool enableConstantInfluence = true;
    public Vector3 constantVelocity;
    public new Vector3 constantForce = Vector3.up;
    public float linearDrag = 1f;

    public ComputeShader computeShader;
    public Material renderMaterial;


    private int initKernel, emitKernel, updateKernel;
    private struct Particle
    {
        public bool alive;
        public Vector3 position;
        public Vector3 velocity;
        public Vector2 life; //x = age, y = lifetime
        public Color color;
        float size;
    }
    private ComputeBuffer particles, dead, quad, counter;
    private int bufferSize = 100096;
    private int groupCount;
    private int[] counterArray;
    private int deadCount = 0;

    private void Awake()
    {
        Camera.main.depthTextureMode = DepthTextureMode.Depth;

        DispatchInit();
    }

    private void Update()
    {
        DispatchUpdate();

        DispatchEmit(Mathf.RoundToInt(Time.deltaTime * emissionRate * timeScale));
    }

    private void OnRenderObject()
    {
        renderMaterial.SetBuffer("particles", particles);
        renderMaterial.SetBuffer("quad", quad);

        renderMaterial.SetPass(0);

        Graphics.DrawProceduralNow(MeshTopology.Quads, 6, dead.count);
    }

    private void OnDestroy()
    {
        ReleaseBuffers();
    }

    public int GetAliveCount()
    {
        return bufferSize - deadCount;
    }

    public void UpdateColorOverLifeTexture()
    {
        colorSteps = Mathf.Max(2, colorSteps);

        colorOverLifeTexture = new Texture2D(colorSteps, 1);

        for (int i = 0; i < colorSteps; i++)
        {
            colorOverLifeTexture.SetPixel(i, 0, colorOverLife.Evaluate(1f / colorSteps * i));
        }
        colorOverLifeTexture.Apply();
    }

    public void UpdateSizeOverLifeBuffer()
    {
        sizeSteps = Mathf.Max(2, sizeSteps);

        if (sizeOverLifeBuffer != null) sizeOverLifeBuffer.Release();
        sizeOverLifeBuffer = new ComputeBuffer(sizeSteps, Marshal.SizeOf(typeof(float)));

        float[] temp = new float[sizeSteps];
        for (int i = 0; i < sizeSteps; i++)
        {
            temp[i] = sizeOverLife.Evaluate(1f / sizeSteps * i);
        }

        sizeOverLifeBuffer.SetData(temp);
    }

    public void DispatchInit()
    {
        ReleaseBuffers();

        UpdateColorOverLifeTexture();
        UpdateSizeOverLifeBuffer();

        previousPositon = transform.position;

        initKernel = computeShader.FindKernel("Init");
        emitKernel = computeShader.FindKernel("Emit");
        updateKernel = computeShader.FindKernel("Update");

        groupCount = Mathf.CeilToInt((float)maxParticles / THREAD_COUNT);
        bufferSize = groupCount * THREAD_COUNT;

        particles = new ComputeBuffer(bufferSize, Marshal.SizeOf(typeof(Particle)));
        dead = new ComputeBuffer(bufferSize, sizeof(int), ComputeBufferType.Append);
        dead.SetCounterValue(0);
        counter = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
        counterArray = new int[] { 0, 1, 0, 0 };

        computeShader.SetBuffer(initKernel, "particles", particles);
        computeShader.SetBuffer(initKernel, "dead", dead);

        computeShader.Dispatch(initKernel, groupCount, 1, 1);

        quad = new ComputeBuffer(6, Marshal.SizeOf(typeof(Vector3)));
        quad.SetData(new[]
        {
        new Vector3(-0.5f,0.5f),
        new Vector3(0.5f,0.5f),
        new Vector3(0.5f,-0.5f),
        new Vector3(0.5f,-0.5f),
        new Vector3(-0.5f,-0.5f),
        new Vector3(-0.5f,0.5f)
        });
    }
    public void DispatchEmit(int count)
    {
        if (enableEmission)
        {
            count = Mathf.Min(count, maxParticles - (bufferSize - deadCount));

            if (count > 0)
            {
                Vector3 velocity = (transform.position - previousPositon) / Time.deltaTime;
                previousPositon = transform.position;

                computeShader.SetBuffer(emitKernel, "particles", particles);
                computeShader.SetBuffer(emitKernel, "alive", dead);

                computeShader.SetVector("seeds", new Vector3(Random.Range(1f, 10000f), Random.Range(1f, 10000f), Random.Range(1f, 10000f)));
                computeShader.SetVector("initialSpeedRange", new Vector2(minInitialSpeed, maxInitialSpeed));
                computeShader.SetVector("inheritedPosition", transform.position);
                computeShader.SetVector("lifeRange", new Vector2(minLifetime, maxLifetime));
                computeShader.SetVector("time", new Vector2(Time.deltaTime, Time.time));

                computeShader.SetInt("colorMode", (int)colorMode);
                if (colorMode == ColorMode.Constant)
                {
                    computeShader.SetVector("color", color);
                }
                else if (colorMode == ColorMode.OverLife)
                {
                    computeShader.SetVector("color", colorOverLife.Evaluate(0f));
                }

                if (sizeMode == SizeMode.Constant)
                {
                    computeShader.SetFloat("size", size);
                }
                else if (colorMode == ColorMode.OverLife)
                {
                    computeShader.SetFloat("size", sizeOverLife.Evaluate(0f));
                }

                computeShader.SetInt("emissionShape", (int)emissionShape);
                if (emissionShape == EmissionShape.Sphere)
                {
                    computeShader.SetFloat("radius", Mathf.Max(0.01f, sphereRadius));
                }

                computeShader.SetInt("directionType", (int)directionType);
                if (directionType == DirectionType.Uniform)
                {
                    computeShader.SetVector("direction", direction);
                }

                if (enableInheritVelocity)
                {
                    computeShader.SetVector("inheritedVelocity", velocity * inheritVelocity);
                    computeShader.SetFloat("extrapolation", extrapolation);
                }

                computeShader.Dispatch(emitKernel, count, 1, 1);
            }
        }
    }

    private int GetDeadCount()
    {
        return deadCount;
    }
    private void SetDeadCount()
    {
        if (dead == null || counter == null || counterArray == null)
        {
            deadCount = bufferSize;
            return;
        }
        counter.SetData(counterArray);
        ComputeBuffer.CopyCount(dead, counter, 0);
        counter.GetData(counterArray);
        deadCount = counterArray[0];
    }

    private void DispatchUpdate()
    {
        if (timeScale > 0)
        {
            computeShader.SetBuffer(updateKernel, "particles", particles);
            computeShader.SetBuffer(updateKernel, "dead", dead);
            computeShader.SetInt("sizeMode", (int)sizeMode);
            computeShader.SetInt("colorMode", (int)colorMode);

            if (colorMode == ColorMode.OverLife)
            {
                computeShader.SetTexture(updateKernel, "colorOverLife", colorOverLifeTexture);
                computeShader.SetInt("colorSteps", colorSteps);
            }

            if (sizeMode == SizeMode.OverLife)
            {
                computeShader.SetBuffer(updateKernel, "sizeOverLife", sizeOverLifeBuffer);
                computeShader.SetInt("sizeSteps", sizeSteps);
            }
            else
            {
                computeShader.SetFloat("size", size);
            }

            if (enableNoise)
            {
                computeShader.SetFloat("viscosity", viscosity);
                computeShader.SetFloat("convergence", convergence + Mathf.PerlinNoise(Time.time * convergenceFrequency, Mathf.PingPong(Time.time * convergenceFrequency, 1.0f)) * convergenceAmplitude);
            }
            else
            {
                computeShader.SetFloat("viscosity", 0f);
            }

            if (enableConstantInfluence)
            {
                computeShader.SetVector("constantVelocity", constantVelocity);
                computeShader.SetVector("constantForce", constantForce);
                computeShader.SetFloat("linearDrag", linearDrag);
            }
            else
            {
                computeShader.SetVector("constantVelocity", Vector3.zero);
                computeShader.SetVector("constantForce", Vector3.zero);
                computeShader.SetFloat("linearDrag", 0f);
            }

            computeShader.SetVector("time", new Vector2(Time.deltaTime * timeScale, Time.time));

            computeShader.Dispatch(updateKernel, groupCount, 1, 1);

            SetDeadCount();
        }
    }

    private void ReleaseBuffers()
    {
        if (particles != null) particles.Release();
        if (dead != null) dead.Release();
        if (counter != null) counter.Release();
        if (quad != null) quad.Release();
        if (sizeOverLifeBuffer != null) sizeOverLifeBuffer.Release();
    }
}
