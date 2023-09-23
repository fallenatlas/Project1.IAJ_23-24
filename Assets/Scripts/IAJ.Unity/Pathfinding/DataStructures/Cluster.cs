using Assets.Scripts.Grid;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    public class Cluster
    {
        public int index;

        public HashSet<Cluster> neighbors;

        public Cluster(int index) {
            this.index = index;
            this.neighbors = new HashSet<Cluster>();
        }

        public override string ToString()
        {
            return "Cluster " + index;
        }

        public bool CalculatePathsTo(Dictionary<int, Cluster> clustersInPath, int goalIndex, HashSet<Cluster> expanded) {
            bool partOfPath = false;

            if (clustersInPath.ContainsKey(index)) return true;


            if (goalIndex == index && !clustersInPath.ContainsKey(index)) {
                clustersInPath.Add(index, this);
                return true;
            }

            expanded.Add(this);
            foreach(Cluster neighbor in neighbors) {
                if (expanded.Contains(neighbor))
                    continue;
                if (neighbor.CalculatePathsTo(clustersInPath, goalIndex, expanded) && !clustersInPath.ContainsKey(index)) {
                    clustersInPath.Add(index, this);
                    partOfPath = true;
                }
            }
            expanded.Remove(this);

            return partOfPath;
        }
    }
}