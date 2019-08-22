using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Winglett
{
    /// <summary>
    /// (C) Matthew Inglis 2019
    ///
    /// O(n)
    /// 
    /// </summary>
    public class Circular : IAlgorithm
    {
        public List<Vector3> SolvePath(Vector3 origin, List<Vector3> nodes, Network.Cost cost)
        {
            // Find the average position of all nodes
            Vector3 averagePosition = origin;

            for (int i = 0; i < nodes.Count; i++)
            {
                averagePosition += nodes[i];
            }

            averagePosition /= (nodes.Count + 1);

            // Calculate the angle between the points and the origin about the average center
            List<(float, Vector3)> pointAngles = new List<(float, Vector3)>();
            Vector3 from = origin - averagePosition;
            for (int i = 0; i < nodes.Count; i++)
            {
                Vector3 to = nodes[i] - averagePosition;

                float angle = Vector2.SignedAngle(new Vector2(from.x, from.z), new Vector2(to.x, to.z));
                angle = Mathf.Repeat(angle, 360f);

                pointAngles.Add((angle, nodes[i]));
            }

            // Order the points. This should create circular order.
            pointAngles = pointAngles.OrderBy(x => x.Item1).ToList();

            // Get the raw points
            List<Vector3> orderedNodes = new List<Vector3>();
            for (int i = 0; i < pointAngles.Count; i++)
            {
                orderedNodes.Add(pointAngles[i].Item2);
            }

            return orderedNodes;
        }
    }
}