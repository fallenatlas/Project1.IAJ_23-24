
using Assets.Scripts.Grid;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    public enum NodeStatus
    {
        Unvisited,
        Open,
        Closed
    }

    public class NodeRecord  : IComparable<NodeRecord>
    {
        //Coordinates
        public int x;
        public int y;
        public bool isWalkable;

        //A* Stuff
        public NodeRecord parent;
        public float gCost;
        public float hCost;
        public float fCost;

        // Node Record Array Index
        public int index;
        public NodeStatus status;
        
        
        public override string ToString()
        {
            return x + ":" + y;
        }

        public NodeRecord(int x, int y)
        {
            
            this.x = x;
            this.y = y;
            gCost = int.MaxValue;
            hCost = 0;
            fCost = gCost + hCost;
            parent = null;
            index = 0;
            isWalkable = true;


        }

        public NodeRecord(int x, int y, int _index) : this(x,y)
        {
            index = _index;
        }

        public void CalculateFCost()
        {
            fCost = gCost + hCost;
        }

        public int CompareTo(NodeRecord other)
        {
            int value = this.fCost.CompareTo(other.fCost);
            return value == 0 ? this.hCost.CompareTo(other.hCost) : value;
        }

        //two node records are equal if they refer to the same node: Do NOT compare directly with "=="!
        public override bool Equals(object obj)
        {
            if (obj is NodeRecord target) return this.x == target.x && this.y == target.y;
            else return false;
        }

        public List<NodeRecord> GetNeighbourList(Grid<NodeRecord> grid)
        {
            NodeRecord currentNode = this;
            List<NodeRecord> neighbourList = new List<NodeRecord>();

            if (currentNode.x - 1 >= 0)
            {
                // Left
                neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y, grid));
                //Left down
                if (currentNode.y - 1 >= 0)
                    neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y - 1, grid));
                //Left up
                if (currentNode.y + 1 < grid.getHeight())
                    neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y + 1, grid));
            }
            if (currentNode.x + 1 < grid.getWidth())
            {
                // Right
                neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y, grid));
                //Right down
                if (currentNode.y - 1 >= 0)
                    neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y - 1, grid));
                //Right up
                if (currentNode.y + 1 < grid.getHeight())
                    neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y + 1, grid));
            }
            // Down
            if (currentNode.y - 1 >= 0)
                neighbourList.Add(GetNode(currentNode.x, currentNode.y - 1, grid));
            //Up
            if (currentNode.y + 1 < grid.getHeight())
                neighbourList.Add(GetNode(currentNode.x, currentNode.y + 1, grid));

            neighbourList.RemoveAll(x => !x.isWalkable);

            return neighbourList;
        }

        public NodeRecord GetNode(int x, int y, Grid<NodeRecord> grid)
        {
            return grid.GetGridObject(x, y);
        }




        // I wonder where this might be useful...
        public void Reset()
        {
            gCost = int.MaxValue;
            hCost = 0;
            fCost = gCost + hCost;
            parent = null;
            status = NodeStatus.Unvisited;
        }

    }
}
