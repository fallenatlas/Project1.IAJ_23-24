using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using UnityEngine;
using System;


namespace Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics
{
    public class EuclideanDistance : IHeuristic
    {
        public float H(NodeRecord node, NodeRecord goalNode)
        {
            var xDistance = goalNode.x - node.x;
            var yDistance = goalNode.y - node.y;

            var result = (float) Math.Sqrt(Math.Pow(xDistance, 2f) + Math.Pow(yDistance, 2f));
          
            return result;
        }
    }
}
