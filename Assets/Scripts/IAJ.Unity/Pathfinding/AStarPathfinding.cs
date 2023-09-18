using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Grid;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using System.Runtime.CompilerServices;
using System;
using System.Net.WebSockets;

namespace Assets.Scripts.IAJ.Unity.Pathfinding
{
    [Serializable]
    public class AStarPathfinding
    {
        // Cost of moving through the grid
        protected const float MOVE_STRAIGHT_COST = 1;
        protected const float MOVE_DIAGONAL_COST = 1.5f;
        public Grid<NodeRecord> grid { get; set; }
        public uint NodesPerSearch { get; set; }
        public uint TotalProcessedNodes { get; protected set; }
        public int MaxOpenNodes { get; protected set; }
        public float TotalProcessingTime { get; set; }
        public bool InProgress { get; set; }
        public IOpenSet Open { get; protected set; }
        public IClosedSet Closed { get; protected set; }
        public IHeuristic Heuristic { get; protected set; }

        public NodeRecord GoalNode { get; set; }
        public NodeRecord StartNode { get; set; }
        public int StartPositionX { get; set; }
        public int StartPositionY { get; set; }
        public int GoalPositionX { get; set; }
        public int GoalPositionY { get; set; }

        public AStarPathfinding(IOpenSet open, IClosedSet closed, IHeuristic heuristic)
        {
            grid = new Grid<NodeRecord>((Grid<NodeRecord> global, int x, int y) => new NodeRecord(x, y));
            this.Open = open;
            this.Closed = closed;
            this.InProgress = false;
            this.Heuristic = heuristic;
            this.NodesPerSearch = 100; //by default we process all nodes in a single request, but you should change this

        }
        public virtual void InitializePathfindingSearch(int startX, int startY, int goalX, int goalY)
        {
            this.StartPositionX = startX;
            this.StartPositionY = startY;
            this.GoalPositionX = goalX;
            this.GoalPositionY = goalY;
            this.StartNode = grid.GetGridObject(StartPositionX, StartPositionY);
            this.GoalNode = grid.GetGridObject(GoalPositionX, GoalPositionY);

            //if it is not possible to quantize the positions and find the corresponding nodes, then we cannot proceed
            if (this.StartNode == null || this.GoalNode == null) return;

            // Reset debug and relevat variables here
            this.InProgress = true;
            this.TotalProcessedNodes = 0;
            this.TotalProcessingTime = 0.0f;
            this.MaxOpenNodes = 0;

            //Starting with the first node
            var initialNode = new NodeRecord(StartNode.x, StartNode.y)
            {
                gCost = 0,
                hCost = this.Heuristic.H(this.StartNode, this.GoalNode),
                index = StartNode.index
            };

            initialNode.CalculateFCost();
            this.Open.Initialize();
            this.Closed.Initialize();
            this.Open.AddToOpen(initialNode);
            grid.SetGridObject(initialNode.x, initialNode.y, initialNode);
        }
        public virtual bool Search(out List<NodeRecord> solution, bool returnPartialSolution = false) {

            var ProcessedNodes = 0;
            NodeRecord CurrentNode;

            // TODO implement

            //While Open is not empty or if nodes havent been all processed 
            while (Open.CountOpen() > 0)
            {
                //ProcessedNodes < grid.getAll().Count
                //grid.getAll().Count;
                //TotalProcessedNodes += 1;
                //ProcessedNodes += 1;
                if (ProcessedNodes == NodesPerSearch)
                {
                    if (returnPartialSolution) 
                    {
                        solution = CalculatePath(Open.PeekBest());
                        return false;
                    }

                    solution = null;
                    return false;
                }

                CurrentNode = Open.GetBestAndRemove();
                Closed.AddToClosed(CurrentNode);

                if (GoalNode.Equals(CurrentNode))
                {
                    solution = CalculatePath(CurrentNode);
                    return true;
                }

                foreach (var neighbourNode in CurrentNode.GetNeighbourList(grid)) 
                {
                    this.ProcessChildNode(CurrentNode, neighbourNode);
                }

                ProcessedNodes += 1;
                TotalProcessedNodes += 1;
                if (MaxOpenNodes < Open.CountOpen())
                {
                    MaxOpenNodes = Open.CountOpen();
                }
            }

            solution = null;
            return true;

            // CurrentNode is the best one from the Open set, start with that
            // var CurrentNode = Open......
            // 

            //Handle the neighbours/children with something like this
            //foreach (var neighbourNode in currentNode.GetNeighbourList(grid))
            //{
            // this.ProcessChildNode(currentNode, neighbourNode)
            //}

            //Out of nodes on the openList
    }

        protected virtual void ProcessChildNode(NodeRecord parentNode, NodeRecord node)
        {
            //NodeRecord child = new NodeRecord(node.x, node.y);
            //child.parent = parentNode;
            //child.gCost = parentNode.gCost + CalculateDistanceCost(parentNode, node);
            //child.CalculateFCost();
            //node.gCost = parentNode.gCost + CalculateDistanceCost(parentNode, node);
            //node.CalculateFCost();

            float newGCost = parentNode.gCost + CalculateDistanceCost(parentNode, node);

            // Calculate newCost: parent cost + Calculate Distance Cont 
            //bool exists = false;
            //NodeRecord existingNode = Closed.SearchInClosed(node);
            //If in Closed...
            if (Closed.SearchInClosed(node) != null)
            {
                if (newGCost < node.gCost)
                {
                    Closed.RemoveFromClosed(node);
                    node.parent = parentNode;
                    node.gCost = newGCost;
                    node.hCost = Heuristic.H(node, GoalNode);
                    node.CalculateFCost();
                    Open.AddToOpen(node);
                }
            }

            //existingNode = Open.SearchInOpen(node);

            //If in Open..
            else if (Open.SearchInOpen(node) != null)
            {
                if (newGCost < node.gCost)
                {
                    node.parent = parentNode;
                    node.gCost = newGCost;
                    node.hCost = Heuristic.H(node, GoalNode);
                    node.CalculateFCost();
                }
                //Open.RemoveFromOpen(node);
                //Open.AddToOpen(node);
            }

            //If node is not in any list ....
            else
            {
                node.parent = parentNode;
                node.gCost = newGCost;
                node.hCost = Heuristic.H(node, GoalNode);
                node.CalculateFCost();
                Open.AddToOpen(node);
            }

            // Finally don't forget to update the actual Grid value: grid.SetGridObject(childNode.x, childNode.y, childNode);
            grid.SetGridObject(node.x, node.y, node);
        }


        protected float CalculateDistanceCost(NodeRecord a, NodeRecord b)
        {
            // Math.abs is quite slow, thus we try to avoid it
            int xDistance = 0;
            int yDistance = 0;
            int remaining = 0;

            if (b.x > a.x)
                xDistance = Math.Abs(a.x - b.x);
            else xDistance = a.x - b.x;

            if (b.y > a.y)
                yDistance = Math.Abs(a.y - b.y);
            else yDistance = a.y - b.y;

            if (yDistance > xDistance)
                remaining = Math.Abs(xDistance - yDistance);
            else remaining = xDistance - yDistance;

            // Diagonal Cost * Diagonal Size + Horizontal/Vertical Cost * Distance Left
            return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }

        // You'll need to use this method during the Search, to get the neighboors
 
 

        // Method to calculate the Path, starts from the end Node and goes up until the beggining
        public List<NodeRecord> CalculatePath(NodeRecord endNode)
        {
            List<NodeRecord> path = new List<NodeRecord>();
            path.Add(endNode);

            var current = endNode.parent;
            while (!current.Equals(StartNode))
            {
                path.Add(current);
                current = current.parent;
            }

            // TODO implement
            // Start from the end node and go up until the beggining of the path

            path.Reverse();
            return path;
        }

    }
}
