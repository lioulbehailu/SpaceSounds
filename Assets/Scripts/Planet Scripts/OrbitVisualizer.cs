using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class OrbitVisualizer : MonoBehaviour
{
    private OrbitManager orbitManager;
    private MeshFilter meshFilter;
    private ParticleSystem particleSys;

    [Header("Tube Geometry")]
    [Range(30, 200)] public int orbitSegments = 60; // Smoothness around the planet circumference
    [Range(3, 12)] public int tubeSides = 6;       // More sides = rounder tube cross-section
    public float tubeRadius = 0.03f;               // Thickness of the glowing line

    [Header("Particle Settings")]
    public int totalParticles = 150;
    public float particleSpread = 0.2f;
    public float particleOrbitSpeed = 1f;

    private ParticleSystem.Particle[] particles;
    private float[] particleTimeOffsets;

    void Start()
    {
        orbitManager = GetComponent<OrbitManager>();
        meshFilter = GetComponent<MeshFilter>();
        particleSys = GetComponent<ParticleSystem>();

        Build3DOrbitMesh();
    }

    void Build3DOrbitMesh()
    {
        if (orbitManager == null || orbitManager.planet == null) return;

        Mesh mesh = new Mesh();
        mesh.name = "OrbitTubeMesh";

        int vertexCount = orbitSegments * tubeSides;
        int triangleCount = orbitSegments * tubeSides * 6;

        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[triangleCount];

        // 1. Calculate positions along the core circumference
        Vector3[] corePath = new Vector3[orbitSegments];
        for (int i = 0; i < orbitSegments; i++)
        {
            float progress = (float)i / orbitSegments;
            float timeOffset = progress * orbitManager.orbitalPeriod;
            corePath[i] = orbitManager.GetPositionAtTime(timeOffset);
        }

        // 2. Build the 3D tube vertices circling around the core path
        for (int i = 0; i < orbitSegments; i++)
        {
            Vector3 currentPoint = corePath[i];
            Vector3 nextPoint = corePath[(i + 1) % orbitSegments];
            Vector3 forward = (nextPoint - currentPoint).normalized;

            // Generate perpendicular axes for the cross-section circle
            Vector3 up = Vector3.up;
            Vector3 right = Vector3.Cross(forward, up).normalized;
            up = Vector3.Cross(right, forward).normalized;

            for (int side = 0; side < tubeSides; side++)
            {
                float angle = ((float)side / tubeSides) * 2 * Mathf.PI;
                Vector3 offset = (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * tubeRadius;

                int vertexIndex = i * tubeSides + side;
                // Convert to local position relative to this visualizer transform
                vertices[vertexIndex] = transform.InverseTransformPoint(currentPoint + offset);
            }
        }

        // 3. Connect the vertices into solid 3D triangles
        int triIndex = 0;
        for (int i = 0; i < orbitSegments; i++)
        {
            int nextSegment = (i + 1) % orbitSegments;

            for (int side = 0; side < tubeSides; side++)
            {
                int nextSide = (side + 1) % tubeSides;

                int currentLeft = i * tubeSides + side;
                int currentRight = i * tubeSides + nextSide;
                int nextLeft = nextSegment * tubeSides + side;
                int nextRight = nextSegment * tubeSides + nextSide;

                // Triangle 1
                triangles[triIndex++] = currentLeft;
                triangles[triIndex++] = nextLeft;
                triangles[triIndex++] = currentRight;

                // Triangle 2
                triangles[triIndex++] = currentRight;
                triangles[triIndex++] = nextLeft;
                triangles[triIndex++] = nextRight;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
    }
}
