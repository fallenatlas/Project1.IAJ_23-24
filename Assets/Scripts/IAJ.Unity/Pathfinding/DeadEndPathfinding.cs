using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Assets.Scripts.Grid;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding
{
    public class DeadEndPathfinding : AStarPathfinding
    {
        public List<Cluster> clusters;
        public Dictionary<int, Cluster> clustersInPath;
        public DeadEndPathfinding(IOpenSet open, IClosedSet closed, IHeuristic heuristic) : base(open, closed, heuristic) {
            clusters = new List<Cluster>();
        }

        public void MapPreprocess()
        {
            FloodFill(grid);
            CreateClusterGraph(grid);
        }

        public override void InitializePathfindingSearch(int startX, int startY, int goalX, int goalY)
        {
            base.InitializePathfindingSearch(startX, startY, goalX, goalY);

            clustersInPath = new Dictionary<int, Cluster>();
            int startCluster = grid.GetGridObject(startX, startY).cluster;
            int goalCluster = grid.GetGridObject(goalX, goalY).cluster;

            clusters[startCluster - 1].CalculatePathsTo(clustersInPath, goalCluster, new HashSet<Cluster>());
            
            CalculateHeuristics(grid);
        }

        public void FloodFill(Grid<NodeRecord> grid) {
            int currCluster = 1;

            for (int x = 0; x < grid.getWidth(); x++) {
                for (int y = grid.getHeight() - 1; 0 <= y; y--) {
                    NodeRecord node = grid.GetGridObject(x, y);

                    if (node.Available()) {
                        CreateCluster(grid, currCluster, x, y);
                        clusters.Add(new Cluster(currCluster));
                        currCluster++;
                    }
                }
            }
        }

        private void CreateCluster(Grid<NodeRecord> grid, int currCluster, int x, int y) {
            int nodesInLine = 0;
            int nodesInPrevLine = -1;
            bool firstGrow = true;
            int firstX = x;
            int lastX = grid.getWidth() - 1;

            for(;  x < grid.getWidth() && y < grid.getHeight() && grid.GetGridObject(x + 1, y + 1).Available(); x++, y++);

            for (; 0 <= y; y--) {
                //finds next available x
                for(x = firstX - 1; !grid.GetGridObject(x, y).Available(); x++) {
                    if (x >= lastX)
                        return;
                }
                firstX = x;
                for (; x < grid.getWidth(); x++) {
                    NodeRecord node = grid.GetGridObject(x, y);

                    //if reached a wall or node opens upwards
                    if (!node.isWalkable || (y != grid.getHeight() && grid.GetGridObject(x, y + 1).Available())) {
                        if (firstGrow && nodesInLine < nodesInPrevLine)
                            firstGrow = false;
                        nodesInPrevLine = nodesInLine;
                        nodesInLine = 0;
                        lastX = x;
                        break;
                    }

                    node.cluster = currCluster;
                    nodesInLine++;

                    //if starts expanding again
                    if (!firstGrow && nodesInLine > nodesInPrevLine) {
                        //clear that line
                        for (; 0 <= x && node.cluster == currCluster; x--) {
                            node.cluster = 0;
                            node = grid.GetGridObject(x - 1, y);
                        }
                        return;
                    }
                }
            }
        }

        private void CreateClusterGraph(Grid<NodeRecord> grid) {
            foreach(NodeRecord node in grid.getAll()) {
                if (node.cluster == 0) continue;

                Cluster nodeCluster = clusters[node.cluster - 1];
                foreach(NodeRecord neighbor in node.GetNeighbourList(grid)) {
                    if (neighbor.cluster != 0 && neighbor.cluster != node.cluster) {
                        Cluster neighborCluster = clusters[neighbor.cluster - 1];
                        nodeCluster.neighbors.Add(neighborCluster);
                    }
                }
            }
        }

        private void CalculateHeuristics(Grid<NodeRecord> grid) {
            foreach(NodeRecord node in grid.getAll()) {
                if (!clustersInPath.ContainsKey(node.cluster))
                    node.hCost = float.MaxValue;
                else
                    node.hCost = 0;
            }
        }
    }

}