using Assets.Scripts.Grid;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    public enum ClusterStatus
    {
        Unvisited,
        Visited
    }
    public class Cluster
    {
        public int index;

        public HashSet<Cluster> neighbors;
        public ClusterStatus status;

        public Cluster(int index) {
            this.index = index;
            this.neighbors = new HashSet<Cluster>();
        }

        public override string ToString()
        {
            return "Cluster " + index;
        }

        public HashSet<Cluster> CalculatePathsTo(int goalIndex) {
            HashSet<Cluster> paths = new HashSet<Cluster>();

            return paths;
        }

        public bool CalculatePathsTo(Dictionary<int, Cluster> clustersInPath, int goalIndex, Cluster parent) {
            bool partOfPath = false;

            if (clustersInPath.ContainsKey(index)) return true;
            
            if (status == ClusterStatus.Unvisited) {
                this.status = ClusterStatus.Visited;

                if (goalIndex == index && !clustersInPath.ContainsKey(index)) {
                    clustersInPath.Add(index, this);
                    return true;
                }
                foreach(Cluster neighbor in neighbors) {
                    if (neighbor == parent)
                        continue;
                    if (neighbor.CalculatePathsTo(clustersInPath, goalIndex, this) && !clustersInPath.ContainsKey(index)) {
                        clustersInPath.Add(index, this);
                        partOfPath = true;
                    }
                }
            }
            
            return partOfPath;
        }
    }
}
