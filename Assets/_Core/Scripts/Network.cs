using UnityEngine;
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Diagnostics;

namespace Winglett
{
    /// <summary>
    /// (C) Matthew Inglis 2019
    /// </summary>
    public class Network : MonoBehaviour
    {
        public enum AlgorithmType { BruteForce, NearestNeighbour, NearestNeighbourOptimized, Circular }
        public enum Cost { Distance, Direction, Elevation, DistanceDirection, DistanceElevation, DistanceDirectionElevation }

        #region ----REFERENCES----
        [SerializeField, Required] protected Transform m_Container;
        [SerializeField, Required] protected GameObject m_NodePrefab;
        [SerializeField, Required] protected LineRenderer m_Line;
        [SerializeField, Required] protected Material m_OriginMaterial;
        [SerializeField, Required] protected GameObject m_OrthographicCamera;
        [SerializeField, Required] protected GameObject m_TerrainCamera;
        [SerializeField] protected bool m_UseTerrain;
        [SerializeField, ShowIf("m_UseTerrain")] protected GameObject m_Terrain;
        #endregion

        #region ----CONFIG----
        [Space]
        public AlgorithmType m_Algorithm;
        [SerializeField] protected Cost m_Cost;
        [SerializeField] protected Vector2 m_Size = new Vector2(100f, 50f);
        [SerializeField] protected float m_Radius = 50f;
        [MinValue(3)] public int m_NodeCount = 5;
        [SerializeField] protected bool m_ManuallySelectOrigin;
        [SerializeField, ShowIf("m_ManuallySelectOrigin")] protected int m_OriginIndex;
        public bool m_UseSeed;
        [ShowIf("m_UseSeed")] public int m_Seed;
        #endregion

        #region ----STATE----
        private System.Random m_Random;
        private GameObject m_SpawnedTerrain;
        [HideInInspector] public Vector3[] m_Path;
        [HideInInspector] public long m_Milliseconds;
        [HideInInspector] public long m_Ticks;
        #endregion

        [Button]
        public void Generate()
        {
            // Setup the cameras
            m_OrthographicCamera.SetActive(!m_UseTerrain);
            m_TerrainCamera.SetActive(m_UseTerrain);

            // Clear existing
            for (int i = m_Container.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(m_Container.GetChild(i).gameObject);
            }

            // If we're not using a seed, make one
            if (!m_UseSeed) m_Seed = UnityEngine.Random.Range(0, 10000);

            // Use a seed
            m_Random = new System.Random(m_Seed);

            // Get the maximum extents
            Vector2 size = m_Size;

            if (m_UseTerrain)
            {
                Vector3 boundsSize = m_Terrain.GetComponent<MeshRenderer>().bounds.size;
                size = new Vector2(boundsSize.x, boundsSize.z);
            }

            // Spawn nodes
            List<GameObject> nodes = new List<GameObject>();
            for (int i = 0; i < m_NodeCount; i++)
            {
                GameObject go = Instantiate(m_NodePrefab, m_Container);
                go.name = $"Node {i}";

                Vector3 position = Vector3.zero;
                int maxAttempts = 25;
                while (--maxAttempts > 0)
                {
                    // Generate a random position
                    position = new Vector3(size.x * (float)m_Random.NextDouble() - size.x / 2f, 0f, size.y * (float)m_Random.NextDouble() - size.y / 2f);

                    // Margin around edges
                    const float MARGIN_PERCENTAGE = 0.05f;
                    Vector2 margin = size * MARGIN_PERCENTAGE;
                    position.x = Mathf.Clamp(position.x, m_Radius + margin.x - size.x / 2f, size.x / 2f - m_Radius - margin.y);
                    position.z = Mathf.Clamp(position.z, m_Radius + margin.x - size.y / 2f, size.y / 2f - m_Radius - margin.y);

                    // Make sure it's sufficiently far away from other nodes
                    bool farAway = true;
                    for (int x = 0; x < nodes.Count; x++)
                    {
                        if (Vector2.Distance(nodes[x].transform.position, position) < 4f)
                        {
                            farAway = false;
                            break;
                        }
                    }

                    if (farAway) break;
                }

                // Remove existing terrain
                if (m_SpawnedTerrain) Destroy(m_SpawnedTerrain);

                // If we're using a terrain, set the height appropriately and spawn the object
                if (m_UseTerrain)
                {
                    // Spawn the terrain prefab
                    m_SpawnedTerrain = Instantiate(m_Terrain);

                    // Raycast the terrain to get it's height
                    Ray ray = new Ray(position + Vector3.up * 20f, Vector3.down * 100f);

                    Collider col = m_SpawnedTerrain.GetComponent<MeshCollider>();
                    col.Raycast(ray, out RaycastHit hit, 100f);

                    // Adjust the height plus a margin
                    const float HEIGHT_MARGIN = 1f;
                    position.y = hit.point.y + HEIGHT_MARGIN;
                }

                // Set the position
                go.transform.position = position;

                // Set the radius
                go.transform.localScale = Vector3.one * m_Radius;

                // Save the new node
                nodes.Add(go);
            }

            // Solve the path
            GameObject origin = DesignateOrigin(nodes);
            SolvePath(nodes, origin);
        }

        private GameObject DesignateOrigin(List<GameObject> nodes)
        {
            int index = m_ManuallySelectOrigin ? Mathf.Clamp(m_OriginIndex, 0, m_NodeCount) : m_Random.Next(0, nodes.Count);

            GameObject origin = nodes[index];
            origin.GetComponent<Renderer>().sharedMaterial = m_OriginMaterial;

            return origin;
        }

        private void SolvePath(List<GameObject> gNodes, GameObject gOrigin)
        {
            // Get all the nodes positions
            List<Vector3> nodes = new List<Vector3>();
            for (int i = 0; i < gNodes.Count; i++)
            {
                // Don't add the origin as a node
                if (gNodes[i] != gOrigin) nodes.Add(gNodes[i].transform.position);
            }

            // Choose the algorithm
            IAlgorithm algorithm = null;
            switch (m_Algorithm)
            {
                case AlgorithmType.BruteForce:
                    if(m_NodeCount > 11)
                    {
                        UnityEngine.Debug.LogError("Brute Force is too slow for 12 or more nodes.");
                        return;
                    }

                    algorithm = new BruteForce();
                    break;
                case AlgorithmType.NearestNeighbour:
                    algorithm = new NearestNeighbour();
                    break;
                case AlgorithmType.NearestNeighbourOptimized:
                    algorithm = new NearestNeighbourOptimized();
                    break;
                case AlgorithmType.Circular:
                    algorithm = new Circular();
                    break;
            }

            // Get the origin position
            Vector3 origin = gOrigin.transform.position;

            // Solve the path
            UnityEngine.Debug.Log("Starting Path Solve");
            Stopwatch clock = new Stopwatch();
            clock.Start();
            List<Vector3> positions = algorithm.SolvePath(origin, nodes, m_Cost);
            clock.Stop();
            UnityEngine.Debug.Log($"Solved path in {clock.ElapsedTicks} ticks ({clock.ElapsedMilliseconds}ms)");

            m_Milliseconds = clock.ElapsedMilliseconds;
            m_Ticks = clock.ElapsedTicks;

            // Apply an offset to all points so the line renders behind the nodes
            const float nodeOffset = -1f;

            // Use an array (+2 for the origin at start and end)
            Vector3[] finalPositions = new Vector3[positions.Count + 2];
            finalPositions[0] = new Vector3(origin.x, nodeOffset, origin.z);
            finalPositions[finalPositions.Length - 1] = new Vector3(origin.x, nodeOffset, origin.z);

            // Assign the positions
            for (int i = 0; i < positions.Count; i++)
            {
                finalPositions[i + 1] = new Vector3(positions[i].x, nodeOffset, positions[i].z);
            }

            // Save this for later
            m_Path = finalPositions;

            // If we're using a terrain, adjust the line to fit
            if (m_UseTerrain)
            {
                // Get the collider on the terrain
                Collider col = m_SpawnedTerrain.GetComponent<MeshCollider>();

                // Make a list for a new path
                List<Vector3> subdividedPath = new List<Vector3>();

                const int SUBDIVISIONS = 30;

                // For each node, raycast and adjust height
                // TODO: subdivide for a smoother path
                for (int i = 0; i < finalPositions.Length - 1; i++)
                {
                    // Get the direction from one node to the next
                    Vector3 direction = finalPositions[i + 1] - finalPositions[i];
                    direction.Normalize();

                    // Get the distance from one node to the next
                    float dist = Vector3.Distance(finalPositions[i + 1], finalPositions[i]);

                    // Make the subdivision
                    for (int x = 0; x < SUBDIVISIONS; x++)
                    {
                        // Calculate the position
                        float t = (float)x / (float)SUBDIVISIONS;
                        Vector3 position = finalPositions[i] + direction * dist * t;

                        // Raycast the terrain to find the height
                        Ray ray = new Ray(position + Vector3.up * 50f, Vector3.down * 100f);
                        col.Raycast(ray, out RaycastHit hit, 100f);

                        //// Set the new height
                        const float HEIGHT_MARGIN = 0.5f;
                        position = hit.point + Vector3.up * HEIGHT_MARGIN;

                        // Add the subdivided path
                        subdividedPath.Add(position);
                    }
                }

                // Add the origin
                subdividedPath.Add(subdividedPath[0]);

                // Save the new subdivided path
                finalPositions = subdividedPath.ToArray();
            }
            
            // Assign the positions to the line
            m_Line.positionCount = finalPositions.Length;
            m_Line.SetPositions(finalPositions);
        }
    }
}
