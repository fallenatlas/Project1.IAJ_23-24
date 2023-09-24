using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Assets.Scripts.Grid;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding
{
    public class GoalBoundAStarPathfinding : NodeArrayAStarPathfinding
    {
        // You can create a bounding box in several differente ways, this is simply suggestion
        // Goal Bounding Box for each Node  direction - Bounding limits: minX, maxX, minY, maxY
        public Dictionary<Vector2,Dictionary<Direction, Vector4>> goalBounds;
        public GoalBoundAStarPathfinding(IHeuristic heuristic) : base(heuristic)
        {
            goalBounds = new Dictionary<Vector2, Dictionary<Direction, Vector4>>();

        }

        public void MapPreprocess()
        {
           
            for (int i = 0; i < grid.getHeight(); i++)
            {
                for (int j = 0; j < grid.getWidth(); j++)
                {
                    NodeRecord node = grid.GetGridObject(j, i);

                    if (node.isWalkable)
                    {
                        FloodFill(node, grid);
                        grid.getAll().ForEach(node => node.Reset());
                    }
                    
                }
            }
            
        }

        // Djikstra Flood Fill Algorithm 
        public void FloodFill(NodeRecord original, Grid<NodeRecord> grid)
        {
            NodeRecord CurrentNode = original;
            NodeRecordArray recordArray = new NodeRecordArray(grid.getAll());
            IOpenSet open = recordArray;
            IClosedSet closed = recordArray;
            Dictionary<Direction, Vector4> boxes = new Dictionary<Direction, Vector4>();
            
            CurrentNode.GetNeighbourList(grid).ForEach(neighbour => {
                neighbour.direction = GetDirectionFromNeighbour(CurrentNode, neighbour);
                neighbour.gCost = CalculateDistanceCost(CurrentNode, neighbour);
                neighbour.CalculateFCost();
                open.AddToOpen(neighbour);
                boxes.Add(neighbour.direction, new Vector4(neighbour.x, neighbour.x, neighbour.y, neighbour.y));
            });
            closed.AddToClosed(CurrentNode);
            CurrentNode.gCost = 0;
            CurrentNode.CalculateFCost();
            while (open.CountOpen() > 0)
            {

                CurrentNode = open.GetBestAndRemove();
                closed.AddToClosed(CurrentNode);

                foreach (var neighbourNode in CurrentNode.GetNeighbourList(grid)) 
                {
                    float newGCost = CurrentNode.gCost + CalculateDistanceCost(CurrentNode, neighbourNode);
                    
                    if (closed.SearchInClosed(neighbourNode) != null)
                    {
                        if (newGCost < neighbourNode.gCost)
                        {
                            closed.RemoveFromClosed(neighbourNode);
                            neighbourNode.parent = CurrentNode;
                            neighbourNode.direction = CurrentNode.direction;
                            Vector4 box = boxes[neighbourNode.direction]; // pass Vector4 by reference
                            UpdateBox(neighbourNode, ref box);
                            boxes[neighbourNode.direction] = box;
                            neighbourNode.gCost = newGCost;
                            neighbourNode.hCost = 0;
                            neighbourNode.CalculateFCost();
                            open.AddToOpen(neighbourNode);
                        }
                    }

                    else if (open.SearchInOpen(neighbourNode) != null)
                    {
                        if (newGCost < neighbourNode.gCost)
                        {
                            neighbourNode.parent = CurrentNode;
                            neighbourNode.direction = CurrentNode.direction;
                            Vector4 box = boxes[neighbourNode.direction]; // pass Vector4 by reference
                            UpdateBox(neighbourNode, ref box);
                            boxes[neighbourNode.direction] = box;
                            neighbourNode.gCost = newGCost;
                            neighbourNode.hCost = 0;
                            neighbourNode.CalculateFCost();
                        }
                    }
                    else
                    {
                        neighbourNode.parent = CurrentNode;
                        neighbourNode.direction = CurrentNode.direction;
                        Vector4 box = boxes[neighbourNode.direction]; // pass Vector4 by reference
                        UpdateBox(neighbourNode, ref box);
                        boxes[neighbourNode.direction] = box;
                        neighbourNode.gCost = newGCost;
                        neighbourNode.hCost = 0;
                        neighbourNode.CalculateFCost();
                        open.AddToOpen(neighbourNode);
                    }
                }
            }
            goalBounds.Add(new Vector2(original.x, original.y), boxes);
        }

        public override bool Search(out List<NodeRecord> solution, bool returnPartialSolution = false) {

            var ProcessedNodes = 0;
            NodeRecord CurrentNode;

            //While Open is not empty or if nodes havent been all processed 
            while (Open.CountOpen() > 0)
            {
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

                //Handle the neighbours/children with something like this
                foreach (var neighbourNode in CurrentNode.GetNeighbourList(grid)) 
                {
                    if (InsindeGoalBoundBox(CurrentNode.x, CurrentNode.y, GoalPositionX, GoalPositionY, GetDirectionFromNeighbour(CurrentNode, neighbourNode))) 
                        this.ProcessChildNode(CurrentNode, neighbourNode);
                }

                ProcessedNodes += 1;
                TotalProcessedNodes += 1;
                if (MaxOpenNodes < Open.CountOpen())
                {
                    MaxOpenNodes = Open.CountOpen();
                }
            }

            //Out of nodes on the openList
            solution = null;
            return true;

    }

        private Direction GetDirectionFromNeighbour(NodeRecord node, NodeRecord neighbour) {
            if (node.x == neighbour.x + 1 && node.y == neighbour.y - 1)
                return Direction.UpLeft;
            if (node.x == neighbour.x && node.y == neighbour.y - 1)
                return Direction.Up;
            if (node.x == neighbour.x - 1 && node.y == neighbour.y - 1)
                return Direction.UpRight;
            if (node.x == neighbour.x - 1 && node.y == neighbour.y)
                return Direction.Right;
            if (node.x == neighbour.x - 1 && node.y == neighbour.y + 1)
                return Direction.DownRight;
            if (node.x == neighbour.x && node.y == neighbour.y + 1)
                return Direction.Down;
            if (node.x == neighbour.x + 1 && node.y == neighbour.y + 1)
                return Direction.DownLeft;
            if (node.x == neighbour.x + 1 && node.y == neighbour.y)
                return Direction.Left;
            return Direction.Unknown; // this should never happen
        }


        private void UpdateBox(NodeRecord node, ref Vector4 box) {
            box.x = Mathf.Min(box.x, node.x);
            box.y = Mathf.Max(box.y, node.x);
            box.z = Mathf.Min(box.z, node.y);
            box.w = Mathf.Max(box.w, node.y);
        }
    
        // Checks is if node(x,Y) is in the node(startx, starty) bounding box for the direction: direction
        public bool InsindeGoalBoundBox(int startX, int startY, int x, int y, Direction direction)
        {
            if (!this.goalBounds.ContainsKey(new Vector2(startX, startY)))
                return false;

            if (!this.goalBounds[new Vector2(startX, startY)].ContainsKey(direction))
                return false;

            var box = this.goalBounds[new Vector2(startX, startY)][direction];
            
            //This is very ugly
            if(box.x >= -1 && box.y >= -1 && box.z >= -1 && box.w >= -1)
                if (x >= box.x && x <= box.y && y >= box.z && y <= box.w)
                    return true;

            return false;
        }
    }

}
