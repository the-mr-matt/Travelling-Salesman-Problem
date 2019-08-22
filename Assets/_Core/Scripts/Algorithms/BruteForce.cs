using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Winglett
{
    /// <summary>
    /// (C) Matthew Inglis 2019
	///
	/// O(n!)
	/// 
    /// </summary>
    public class BruteForce : IAlgorithm
    {
        private class Path
        {
            public List<Vector3> path = new List<Vector3>();

            public float GetDistance(Vector3 origin)
            {
                Assert.IsTrue(path.Count > 0);

                // Initialize distance with origin to end points
                float dist = Vector3.Distance(origin, path[0]);
                dist += Vector3.Distance(path[path.Count - 1], origin);

                // Add distances between nodes
                for (int i = 0; i < path.Count - 1; i++)
                {
                    dist += Vector3.Distance(path[i], path[i + 1]);
                }

                return dist;
            }
        }

        public List<Vector3> SolvePath(Vector3 origin, List<Vector3> nodes, Network.Cost cost)
        {
            // Check cost is valid
            if (cost != Network.Cost.Distance)
            {
                cost = Network.Cost.Distance;
                Debug.LogError($"Cost type: {cost} is invalid for Brute Force Algorithm.");
            }

            // Init
            List<Path> paths = new List<Path>();

            // Recursively go through all nodes and calculate a path
            for (int i = 0; i < nodes.Count; i++)
            {
                Path path = new Path();
                path.path.Add(nodes[i]);

                TryPath(path);
            }

            void TryPath(Path currentPath)
            {
                int remaining = nodes.Count - currentPath.path.Count;

                // Check if there are no more nodes
                if (remaining == 0)
                {
                    // Save it
                    paths.Add(currentPath);

                    // We are finished
                    return;
                }

                for (int i = 0; i < nodes.Count; i++)
                {
                    // Go through all nodes we haven't been to yet
                    if (!currentPath.path.Contains(nodes[i]))
                    {
                        // Make a new path
                        Path path = new Path();

                        // Add the previous paths positions
                        for (int x = 0; x < currentPath.path.Count; x++)
                        {
                            path.path.Add(currentPath.path[x]);
                        }

                        // Add a new node
                        path.path.Add(nodes[i]);

                        TryPath(path);
                    }
                }
            }

            Debug.Log($"Iterations: {paths.Count}");

            // Find the shortest path out of our many choices
            float minDist = float.MaxValue;
            int index = 0;
            for (int i = 0; i < paths.Count; i++)
            {
                float dist = paths[i].GetDistance(origin);
                if (dist < minDist)
                {
                    minDist = dist;
                    index = i;
                }
            }

            return paths[index].path;
        }
    }
}
