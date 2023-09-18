using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Grid;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using System.Runtime.CompilerServices;

namespace Assets.Scripts.IAJ.Unity.Pathfinding
{
    public class NodeArrayAStarPathfinding : AStarPathfinding
    {
        private static int index = 0;
        protected NodeRecordArray NodeRecordArray { get; set; }

        public NodeArrayAStarPathfinding(IHeuristic heuristic) : base(null, null, heuristic)
        {
            grid = new Grid<NodeRecord>((Grid<NodeRecord> global, int x, int y) => new NodeRecord(x, y, index++));
            this.InProgress = false;
            this.Heuristic = heuristic;
            this.NodesPerSearch = 100;
            this.NodeRecordArray = new NodeRecordArray(grid.getAll());
            this.Open = this.NodeRecordArray;
            this.Closed = this.NodeRecordArray;

        }
       
        // In Node Array A* the only thing that changes is how you process the child node, the search occurs the exact same way so you can the parent's method
        protected override void ProcessChildNode(NodeRecord parentNode, NodeRecord neighbourNode)
        {
            float newGCost = parentNode.gCost + CalculateDistanceCost(parentNode, neighbourNode);
            float newHCost = Heuristic.H(neighbourNode, GoalNode);

            // Calculate newCost: parent cost + Calculate Distance Cont 
            //bool exists = false;
            //NodeRecord existingNode = Closed.SearchInClosed(node);
            //If in Closed...
            if (NodeRecordArray.SearchInClosed(neighbourNode) != null)
            {
                if (newGCost < neighbourNode.gCost)
                {
                    NodeRecordArray.RemoveFromClosed(neighbourNode);
                    neighbourNode.parent = parentNode;
                    neighbourNode.gCost = newGCost;
                    neighbourNode.hCost = newHCost;
                    neighbourNode.CalculateFCost();
                    NodeRecordArray.AddToOpen(neighbourNode);
                }
            }

            //existingNode = Open.SearchInOpen(node);

            //If in Open..
            else if (NodeRecordArray.SearchInOpen(neighbourNode) != null)
            {
                if (newGCost < neighbourNode.gCost)
                {
                    neighbourNode.parent = parentNode;
                    neighbourNode.gCost = newGCost;
                    neighbourNode.hCost = newHCost;
                    neighbourNode.CalculateFCost();
                }
                //Open.RemoveFromOpen(node);
                //Open.AddToOpen(node);
            }

            //If node is not in any list ....
            else
            {
                neighbourNode.parent = parentNode;
                neighbourNode.gCost = newGCost;
                neighbourNode.hCost = newHCost;
                neighbourNode.CalculateFCost();
                NodeRecordArray.AddToOpen(neighbourNode);
            }

            // Finally don't forget to update the actual Grid value: grid.SetGridObject(childNode.x, childNode.y, childNode);
            grid.SetGridObject(neighbourNode.x, neighbourNode.y, neighbourNode);
        }
               
    }


       
}