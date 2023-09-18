using Assets.Scripts.IAJ.Unity.Pathfinding;
using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    //Pathfinding Manager reference
    [HideInInspector]
    public PathfindingManager manager;

    //Debug Components you can add your own here
    Text debugCoordinates;
    Text debugG;
    Text debugF;
    Text debugH;
    Text debugWalkable;
    Text debugtotalProcessedNodes;
    Text debugtotalProcessingTime;
    Text debugMaxNodes;
    Text debugDArray;
    Text debugBounds;

    bool useGoal;

    private int currentX, currentY;
    VisualGridManager visualGrid;

    // Start is called before the first frame update
    void Start()
    {

        // Simple way of getting the manager's reference
        manager = GameObject.FindObjectOfType<PathfindingManager>();
        visualGrid = GameObject.FindObjectOfType<VisualGridManager>();

        // Retrieving the Debug Components
        var debugTexts = this.transform.GetComponentsInChildren<Text>();
        debugCoordinates = debugTexts[0];
        debugH = debugTexts[1];
        debugG = debugTexts[2];
        debugF = debugTexts[3];
        debugtotalProcessedNodes = debugTexts[4];
        debugtotalProcessingTime = debugTexts[5];
        debugMaxNodes = debugTexts[6];
        debugWalkable = debugTexts[7];
        debugDArray = debugTexts[8];
        useGoal = manager.useGoalBound;
        currentX = -2;
        currentY = -2;
    }

    // Update is called once per frame
    void Update()
    {
        // A Long way of printing useful information regarding the algorithm
        var currentPosition = UtilsClass.GetMouseWorldPosition();
        if (currentPosition != null)
        {
            int x, y;
            if (manager.pathfinding.grid != null)
            {
                manager.pathfinding.grid.GetXY(currentPosition, out x, out y);

                currentX = x;
                currentY = y;
                if (x != -1 && y != -1)
                {
                    var node = manager.pathfinding.grid.GetGridObject(x, y);
                    if (node != null)
                    {
                        debugCoordinates.text = " x:" + x + "; y:" + y;
                        debugG.text = "G:" + node.gCost;
                        debugF.text = "F:" + node.fCost;
                        debugH.text = "H:" + node.hCost;
                        debugWalkable.text = "IsWalkable:" + node.isWalkable;

                        if (node.isWalkable)
                        {
                           if (useGoal)
                            {
 /*                               var array = "";
                                var goalBoundingPathfinder = (GoalBoundAStarPathfinding)manager.pathfinding;
                                if (goalBoundingPathfinder.goalBounds.ContainsKey(new Vector2(x, y)))
                                {
                                    var boundingBox = goalBoundingPathfinder.goalBounds[new Vector2(x, y)];
                                    array += "Left" + boundingBox["left"] + "\n";
                                    array += "Right" + boundingBox["right"] + "\n";
                                    array += "Up" + boundingBox["up"] + "\n";
                                    array += "Down" + boundingBox["down"] + "\n";
                                    debugDArray.text = array;
                                    visualGrid.fillBoundingBox(node);
*/
                            }

                        }
                    }
                }

            }
        }

        //if (this.manager.pathfinding.InProgress)
        //{
                debugMaxNodes.text = "MaxOpenNodes:" + manager.pathfinding.MaxOpenNodes;
                debugtotalProcessedNodes.text = "TotalPNodes:" + manager.pathfinding.TotalProcessedNodes;
                debugtotalProcessingTime.text = "TotalPTime:" + manager.pathfinding.TotalProcessingTime;
            //}
        }
}
