using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    class ClosedDictionary : IClosedSet
    {

        //Tentative dictionary type structure, it is possible that there are better solutions...
        private Dictionary<Vector2, NodeRecord> Closed { get; set; }

        public ClosedDictionary()
        {
            this.Closed = new Dictionary<Vector2, NodeRecord>();
        }

        public void Initialize()
        {
            this.Closed.Clear();
        }

        public void AddToClosed(NodeRecord nodeRecord)
        {
            //TODO implement
            this.Closed.Add(new Vector2(nodeRecord.x, nodeRecord.y), nodeRecord);
            nodeRecord.status = NodeStatus.Closed;
        }

        public void RemoveFromClosed(NodeRecord nodeRecord)
        {
            //TODO implement
            this.Closed.Remove(new Vector2(nodeRecord.x, nodeRecord.y));
        }

        public NodeRecord SearchInClosed(NodeRecord nodeRecord)
        {
            //TODO implement
            var found = this.Closed.TryGetValue(new Vector2(nodeRecord.x, nodeRecord.y), out NodeRecord node);
            return found ? node : null;
        }   

        public ICollection<NodeRecord> All()
        {
            //TODO implement
            return this.Closed.Values;
        }

      
    }

}

