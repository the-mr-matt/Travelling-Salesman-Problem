using System.Collections.Generic;
using UnityEngine;

namespace Winglett
{
    /// <summary>
    /// (C) Matthew Inglis 2019
    ///
    /// O((n^2 + n)/2)
    /// 
    /// </summary>
    public class NearestNeighbourOptimized : IAlgorithm
    {
        public List<Vector3> SolvePath(Vector3 origin, List<Vector3> nodes, Network.Cost cost)
        {
            // Initalize the line renderer nodes
            List<Vector3> lineNodes = new List<Vector3>();

            // The node we are currently at
            Vector3 currentPos = origin;

            int iterations = 0;

            // Wait until we have gone through every node
            while(nodes.Count > 0)
            {
                // Find the closest node that we have NOT yet visited
                float minDist = float.MaxValue;
                int index = 0;
                for (int i = 0; i < nodes.Count; i++)
                {
					iterations++;

                    // Calculate the distance from our current node to this potential node
                    float dist = Vector3.Distance(nodes[i], currentPos);

                    // If it's a minimum so far, save the distance and the index
                    if (dist < minDist)
                    {
                        minDist = dist;
                        index = i;
                    }
                }

                // Save the new node
                currentPos = nodes[index];

                // Remove this node from the list so we don't accidentally double back
                nodes.RemoveAt(index);

                // Save this node in our final path
                lineNodes.Add(currentPos);
            }

            Debug.Log($"Iterations: {iterations}");

            return lineNodes;
        }
    }
}