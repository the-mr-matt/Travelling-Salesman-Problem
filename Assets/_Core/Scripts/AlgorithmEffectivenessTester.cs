using System.Collections.Generic;
using System.IO;
using System.Text;
using NaughtyAttributes;
using UnityEngine;

namespace Winglett
{
    /// <summary>
    /// (C) Matthew Inglis 2019
    /// </summary>
    public class AlgorithmEffectivenessTester : MonoBehaviour
    {
        #region ----REFERENCES----
        [SerializeField, Required] protected Network m_Network;
        #endregion

        #region ----CONFIG----
        [Space]
        [SerializeField] protected float m_DelayInterval = 0.3f;
        [SerializeField, MinValue(1)] protected int m_Interations;
        [SerializeField] protected Vector2Int m_NodeCount;
        #endregion

        [Button]
        private async void CompareAlgorithms()
        {
            StringBuilder dist = new StringBuilder();
            StringBuilder time = new StringBuilder();
            dist.AppendLine("Nodes,Brute Force,Nearest Neighbour,Nearest Neighbour Optimized,Circular");
            time.AppendLine("Nodes,Brute Force,Nearest Neighbour,Nearest Neighbour Optimized,Circular");

            m_Network.m_UseSeed = true;

            for (int count = m_NodeCount.x; count <= m_NodeCount.y; count++)
            {
                for (int i = 0; i < m_Interations; i++)
                {
                    m_Network.m_Seed = UnityEngine.Random.Range(0, 10000);
                    m_Network.m_NodeCount = count;

                    await new WaitForSeconds(m_DelayInterval);

                    float bruteForceDist = 0;
                    long bruteForceTime = 0;

                    if (count < 12)
                    {
                        m_Network.m_Algorithm = Network.AlgorithmType.BruteForce;
                        m_Network.Generate();

                        bruteForceDist = GetTotalPathDistance(m_Network.m_Path);
                        bruteForceTime = m_Network.m_Ticks;
                    }

                    await new WaitForSeconds(m_DelayInterval);

                    m_Network.m_Algorithm = Network.AlgorithmType.NearestNeighbour;
                    m_Network.Generate();
                    float nearestNeighbourDist = GetTotalPathDistance(m_Network.m_Path);
                    long nearestNeighbourTime = m_Network.m_Ticks;

                    await new WaitForSeconds(m_DelayInterval);

                    m_Network.m_Algorithm = Network.AlgorithmType.NearestNeighbourOptimized;
                    m_Network.Generate();
                    float nearestNeighbourOptimizedDist = GetTotalPathDistance(m_Network.m_Path);
                    long nearestNeighbourOptimizedTime = m_Network.m_Ticks;

                    await new WaitForSeconds(m_DelayInterval);

                    m_Network.m_Algorithm = Network.AlgorithmType.Circular;
                    m_Network.Generate();
                    float circularDist = GetTotalPathDistance(m_Network.m_Path);
                    long circularTime = m_Network.m_Ticks;

                    string distLine = $"{count},{bruteForceDist},{nearestNeighbourDist},{nearestNeighbourOptimizedDist},{circularDist}";
                    string timeLine = $"{count},{bruteForceTime},{nearestNeighbourTime},{nearestNeighbourOptimizedTime},{circularTime}";
                    dist.AppendLine(distLine);
                    time.AppendLine(timeLine);
                }
            }

            string desktop = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            string distFileName = "algorithms-comparison-distance.csv";
            string timeFileName = "algorithms-comparison-time.csv";

            File.WriteAllText(Path.Combine(desktop, distFileName), dist.ToString());
            File.WriteAllText(Path.Combine(desktop, timeFileName), time.ToString());

            UnityEngine.Debug.Log("Finished Algorithm Effectivenss Test");
        }

        private float GetTotalPathDistance(Vector3[] path)
        {
            float totalDist = 0f;
            for (int i = 0; i < path.Length - 1; i++)
            {
                totalDist += Vector2.Distance(new Vector2(path[i].x, path[i].z), new Vector2(path[i + 1].x, path[i + 1].z));
            }

            // Add the origin distance too
            totalDist += Vector2.Distance(new Vector2(path[path.Length - 1].x, path[path.Length - 1].z), new Vector2(path[0].x, path[0].z));

            return totalDist;
        }
    }
}