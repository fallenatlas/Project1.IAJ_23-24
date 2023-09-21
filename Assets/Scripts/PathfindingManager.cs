using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using Assets.Scripts.Grid;
using UnityEngine.UIElements;
using UnityEngine.UI;
using System.IO;
using System;
using Assets.Scripts.IAJ.Unity.Pathfinding;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using UnityEngine.Networking;

public class PathfindingManager : MonoBehaviour
{

    //Struct for default positions
    [Serializable]
    public struct SearchPos
    {
        public Vector2 startingPos;
        public Vector2 goalPos;
    }

    //Struct to store default positions by grid
    [Serializable]
    public struct SearchPosPerGrid
    {
        public string gridName;
        public List<SearchPos> searchPos;
    }

    // "Default Positions are quite useful for testing"
    public List<SearchPosPerGrid> defaultPositions;

    [Header("Grid Settings")]
    [Tooltip("Change grid name to change grid properties")]
    public string gridName;

    [Header("Pahfinding Settings")]
    [Tooltip("Add settings to your liking, useful for faster testing")]
    //public properties useful for testing, you can add other booleans here such as which heuristic to use
    public bool partialPath;
    public bool AStarAlgorithm;
    public bool NodeArrayAStarAlgorithm;
    public bool useGoalBound;
   
    //Grid configuration
    public static int width;
    public static int height;
    public static float cellSize;
 
    //Essential Pathfind classes 
    public AStarPathfinding pathfinding { get; set; }

    //The Visual Grid
    private VisualGridManager visualGrid;
    private string[,] textLines;

    //Private fields for internal use only
    public static int startingX = -1;
    public static int startingY = -1;
    public static int goalX = -1;
    public static int goalY = -1;

    //Path
    List<NodeRecord> solution;

    private void Start()
    {
        // Finding reference of Visual Grid Manager
        visualGrid = GameObject.FindObjectOfType<VisualGridManager>();

        // Creating the Path for the Grid and Creating it
        var gridPath = "Assets/Resources/Grid/" + gridName + ".txt";
        this.LoadGrid(gridPath);

       // Creating and Initializing the Pathfinding class, you can change the open, closed and heuristic sets here new ZeroHeuristic()
        if (AStarAlgorithm)
            this.pathfinding = new AStarPathfinding(new NodePriorityHeap(), new ClosedDictionary(), new EuclideanDistance());
        else if (NodeArrayAStarAlgorithm)
            this.pathfinding = new NodeArrayAStarPathfinding(new EuclideanDistance());
        // else if (useGoalBound)
        //     this.pathfinding = new GoalBoundAStarPathfinding(new NodePriorityHeap(), new ClosedDictionary(), new EuclideanDistance());
        else
            this.pathfinding = new AStarPathfinding(new NodePriorityHeap(), new ClosedDictionary(), new EuclideanDistance());

        visualGrid.GridMapVisual(textLines, this.pathfinding.grid);

       /* if (this.pathfinding is GoalBoundAStarPathfinding)
        {
            var p = this.pathfinding as GoalBoundAStarPathfinding;
            p.MapPreprocess();
            visualGrid.ClearGrid();
        }
       */
        pathfinding.grid.OnGridValueChanged += visualGrid.Grid_OnGridValueChange;
    }

    // Update is called once per frame
    void Update()
    {

        // The first mouse click goes here, it defines the starting position;
        if (Input.GetMouseButtonDown(0))
        {

            //Retrieving clicked position
            var clickedPosition = UtilsClass.GetMouseWorldPosition();

            int positionX, positionY = 0;

            // Retrieving the grid's corresponding X and Y from the clicked position
            pathfinding.grid.GetXY(clickedPosition, out positionX, out positionY);

            // Getting the corresponding Node 
            var node = pathfinding.grid.GetGridObject(positionX, positionY);

            if (node != null && node.isWalkable)
            {

                if (startingX == -1)
                {
                    startingX = positionX;
                    startingY = positionY;
                    this.visualGrid.SetObjectColor(startingX, startingY, Color.cyan);

                }
                else if (goalX == -1)
                {
                    goalX = positionX;
                    goalY = positionY;
                    this.visualGrid.SetObjectColor(startingX, startingY, Color.cyan);
                    //We can now start the search
                    InitializeSearch(startingX, startingY, goalX, goalY);
                }
                else
                {
                    goalY = -1;
                    goalX = -1;
                    this.visualGrid.ClearGrid();
                    startingX = positionX;
                    startingY = positionY;
                    this.visualGrid.SetObjectColor(startingX, startingY, Color.cyan);
                }
            }
        }

        // We will use the right mouse to clean the selection and the grid
        if (Input.GetMouseButtonDown(1))
        {
            startingX = -1;
            startingY = -1;
            goalY = -1;
            goalX = -1;
            this.visualGrid.ClearGrid();
        }

        // Input Handler: deals with most keyboard inputs
        InputHandler();

        // Make sure you tell the pathfinding algorithm to keep searching
        if (this.pathfinding.InProgress)
        {
            var finished = this.pathfinding.Search(out this.solution, partialPath);
            if (finished)
            {
                this.pathfinding.InProgress = false;
                this.visualGrid.DrawPath(this.solution);
            }

            if (partialPath && !finished)
            {
                this.visualGrid.DrawPath(this.solution);
            } 

            this.pathfinding.TotalProcessingTime += Time.deltaTime;
        }
    }


    void InputHandler()
    {
        // Space clears the grid
        if (Input.GetKeyDown(KeyCode.Space))
            this.visualGrid.ClearGrid();

        // If you press 1-6 keys you pathfinding will use default positions
        int index = 0;
        if (Input.GetKeyDown(KeyCode.Alpha1))
            index = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            index = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            index = 3;
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            index = 4;
        else if (Input.GetKeyDown(KeyCode.Alpha5))
            index = 5;
        else if (Input.GetKeyDown(KeyCode.Alpha6))
            index = 6;
        if (index != 0 && index < 7)
        {
            var positions = defaultPositions.Find(x => x.gridName == this.gridName).searchPos;

            if (index - 1 <= positions.Count && index - 1 >= 0)
            {
                var actualPositions = positions[index - 1];

                startingX = (int)actualPositions.startingPos.x;
                startingY = (int)actualPositions.startingPos.y;
                // Getting the corresponding Node 
                var node = pathfinding.grid.GetGridObject(startingX, startingY);
                if (node != null && node.isWalkable)
                {
                    goalX = (int)actualPositions.goalPos.x;
                    goalY = (int)actualPositions.goalPos.y;

                    node = pathfinding.grid.GetGridObject(goalX, goalY);

                    if (node != null && node.isWalkable)
                    {
                        InitializeSearch(startingX, startingY, goalX, goalY);
                    }
                }
            }
        }

    }


    public void InitializeSearch(int _startingX, int _startingY, int _goalX, int _goalY)
    {
        this.visualGrid.ClearGrid();
        this.visualGrid.SetObjectColor(startingX, startingY, Color.cyan);
        this.visualGrid.SetObjectColor(goalX, goalY, Color.green);
        this.pathfinding.InitializePathfindingSearch(_startingX, _startingY, _goalX, _goalY);

    }

    // Reads the text file that where the grid "definition" is stored, I don't recomend changing this ^^ 
    public void LoadGrid(string gridPath)
    {

        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(gridPath);
        var fileContent = reader.ReadToEnd();
        reader.Close();
        var lines = fileContent.Split("\n"[0]);

        //Calculating Height and Width from text file
        height = lines.Length;
        width = lines[0].Length - 1;

        // CellSize Formula 
         cellSize = 700.0f / (width + 2);
      
        textLines = new string[height, width];
        int i = 0;
        foreach (var l in lines)
        {
            var words = l.Split();
            var j = 0;

            var w = words[0];

            foreach (var letter in w)
            {
                textLines[i, j] = letter.ToString();
                j++;

                if (j == textLines.GetLength(1))
                    break;
            }

            i++;
            if (i == textLines.GetLength(0))
                break;
        }

    }

}
