using System.Collections.Generic;
using UnityEngine;

namespace Winglett
{
    /// <summary>
    /// (C) Matthew Inglis 2019
    /// </summary>
    public interface IAlgorithm
    {
        /// <summary>
        /// Solves the intermediate paths required to solve the Travelling
        /// Salesman Problem. Note: intermediate means the algorithm should
        /// not add the origin to the list.
        /// </summary>
        List<Vector3> SolvePath(Vector3 origin, List<Vector3> nodes, Network.Cost cost);
    }
}