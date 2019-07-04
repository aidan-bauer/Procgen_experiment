﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour {

    [Tooltip("Values can only be 2^n+1 (5, 17, 33, etc.).")]
    public int dimension = 33;
    public int steps = 25;
    public int stepRange = 5;
    public float scale = 1f;

    public bool useRandomSeed = true;
    public string seed;

    Node[,] map;

    // Use this for initialization
    void Awake () {
        map = new Node[dimension, dimension];

        Generate();
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
            Generate();
	}

    void Generate() {
        Reset();

        if (useRandomSeed)
            seed = System.DateTime.Now.ToLongTimeString();

        RandomWalk rw = new RandomWalk(map, seed, stepRange);

        for (int i = 0; i < steps; i++)
        {
            rw.Step();
        }

        map = rw.ReturnSteps();

        //DiamondSquare ds = new DiamondSquare(map, 5);
        //ds.GenerateHeightMap(dimension-1);
        //float[,] heightMapValues = ds.ReturnHeightMap();

       /* for (int x = 0; x < dimension; x++)
        {
            for (int y = 0; y < dimension; y++)
            {
                map[x, y].Height = heightMapValues[x, y];
            }
        }*/

        for (int x = 0; x < dimension; x++)
        {
            for (int y = 0; y < dimension; y++)
            {
                map[x, y].DebugDrawConnections();
            }
        }

        //REPLACE: Does not work
        /*List<Node> oneConnectionList = new List<Node>();
        for (int x = 0; x < dimension; x++)
        {
            for (int y = 0; y < dimension; y++)
            {
                if (map[x,y].ConnectedNodes.Count == 1)
                {
                    print("found one");
                    oneConnectionList.Add(map[x, y]);
                }
            }
        }

        Node[] oneConnection = oneConnectionList.ToArray();
        oneConnection[UnityEngine.Random.Range(0, oneConnection.Length)].SetNodeType= 1;
        oneConnection[UnityEngine.Random.Range(0, oneConnection.Length)].SetNodeType = 2;*/

        map[UnityEngine.Random.Range(0, dimension), UnityEngine.Random.Range(0, dimension)].SetNodeType = 1;
        map[UnityEngine.Random.Range(0, dimension), UnityEngine.Random.Range(0, dimension)].SetNodeType = 2;
    }

    private void Reset()
    {
        for (int x = 0; x < dimension; x++)
        {
            for (int y = 0; y < dimension; y++)
            {
                map[x, y] = new Node(x, y, dimension, scale);     //completely reset the node
                map[x, y].weight = 0;
            }
        }
    }

    //display a point as if the it's centered around zero
    Vector3 CenterAroundZero(Vector3 point) {
        return new Vector3(point.x - (dimension / 2), point.y, point.z - (dimension / 2)) * scale;
    }

    private void OnDrawGizmos()
    {
        if (map != null)
        {
            Gizmos.color = Color.white;

            for (int x = 0; x < dimension; x++)
            {
                for (int y = 0; y < dimension; y++)
                {
                    int nodeType = map[x, y].GetNodeType;
                    switch(nodeType)
                    {
                        case 0:
                            Gizmos.color = Color.white;
                            break;
                        case 1:
                            Gizmos.color = Color.green;
                            break;
                        case 2:
                            Gizmos.color = Color.blue;
                            break;
                        default:
                            break;
                    }

                    /*if ((map[x, y].weight == 0))
                        Gizmos.color = Color.black;*/
                    //Gizmos.color = (map[x, y].weight > 0) ? Color.white : Color.black;

                    Gizmos.DrawCube(CenterAroundZero(map[x, y].ReturnPosition()), Vector3.one * (map[x,y].weight));
                }
            }
        }
    }

    //one section of the grid
    public struct Node
    {
        int nodeX, nodeY;
        int nodeType;
        float height;
        float dimensions, scale;    //cause we need them here to debug *sigh*
        public int weight;

        public int SetNodeType
        {
            set
            {
                nodeType = value;
            }
        }

        public int GetNodeType
        {
            get
            {
                return nodeType;
            }
        }

        List<Node> connectedNodes;

        public List<Node> ConnectedNodes
        {
            get
            {
                return connectedNodes;
            }
        }

        public float Height
        {
            set
            {
                height = value;
            }
        }

        public Node(int x, int y, float _dimensions, float _scale)
        {
            nodeX = x;
            nodeY = y;
            nodeType = 0;
            dimensions = _dimensions;
            scale = _scale;
            weight = 0;
            height = 0;
            connectedNodes = new List<Node>();
        }

        public Vector3 ReturnPosition()
        {
            return new Vector3(nodeX, height, nodeY);
        }

        /*Select Current Node
        Find Previous Node
        Add it to the connected nodes list*/
        public void AddNode(Node nodeToAdd) {
            connectedNodes.Add(nodeToAdd);
        }

        public bool isConnectedNode(Node comparisson) {
            return connectedNodes.Contains(comparisson);
        }

        public void DebugDrawConnections() {
            for (int i = 0; i < connectedNodes.Count; i++) {
                Debug.DrawLine(CenterAroundZero(ReturnPosition()), CenterAroundZero(connectedNodes[i].ReturnPosition()), Color.red, 200f);
            }
        }

        //display a point as if the it's centered around zero
        Vector3 CenterAroundZero(Vector3 point)
        {
            return new Vector3(point.x - (dimensions / 2) + 0.5f, point.y, point.z - (dimensions / 2) + 0.5f) * scale;
        }
    }

    //drunkards walk
    public class RandomWalk
    {
        int currentX, currentY;
        int x, y;
        int lastDirection;
        int stepDistance;
        int stepRange;
        Node[,] walkedSteps;
        List<Node> walkerPath;
        System.Random psuedoRand;

        public RandomWalk(Node[,] mapCopy, string seed, int _stepRange)
        {
            psuedoRand = new System.Random(seed.GetHashCode());

            currentX = UnityEngine.Random.Range(5, mapCopy.GetLength(0) - 6);
            currentY = UnityEngine.Random.Range(5, mapCopy.GetLength(1) - 6);
            walkedSteps = mapCopy;
            walkerPath = new List<Node>();
            stepRange = _stepRange;

            walkedSteps[currentX, currentY].weight++;

            lastDirection = psuedoRand.Next(0, 4);
            stepDistance = UnityEngine.Random.Range(1, stepRange);
        }

        public void Step()
        {
            //pick a direction at random but not the direction we just came from
            //move one unit in that direction
            //repeat
            Node previousNode = walkedSteps[currentX, currentY];
            int stepDirection = psuedoRand.Next(0, 4);
            stepDistance = UnityEngine.Random.Range(1, stepRange);

            while (stepDirection == lastDirection)
            {
                stepDirection = psuedoRand.Next(0, 4);
            }

            switch (stepDirection) {
                case 0:
                    //move up
                    y += stepDistance;
                    break;
                case 1:
                    //move right
                    x += stepDistance;
                    break;
                case 2:
                    //move down
                    y -= stepDistance;
                    break;
                case 3:
                    //move left
                    x -= stepDistance;
                    break;
            }

            //if out of bounds, step in opposite direction
            if (!IsInXBound())
            {
                x *= -1;
            }

            if (!IsInYBound())
            {
                y *= -1;
            }

            currentX += x;
            currentY += y;
            //if (walkedSteps[currentX,currentY].weight == 0)
                walkerPath.Add(walkedSteps[currentX, currentY]);

            walkedSteps[currentX, currentY].weight++;
            if (!previousNode.isConnectedNode(walkedSteps[currentX, currentY]))
                walkedSteps[currentX, currentY].AddNode(previousNode);

            lastDirection = stepDirection;

            //reset values
            x = 0;
            y = 0;
        }

        bool IsInXBound()
        {
            return currentX + x >= 0 && currentX + x < walkedSteps.GetLength(0);
        }

        bool IsInYBound()
        {
            return currentY + y >= 0 && currentY + y < walkedSteps.GetLength(1);
        }

        float map(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
        }

        int map(int value, int fromMin, int fromMax, int toMin, int toMax)
        {
            return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
        }

        public Node[,] ReturnSteps()
        {
            return walkedSteps;
        }

        public Node[] ReturnPath()
        {
            return walkerPath.ToArray();
        }
    }

    public class DiamondSquare
    {
        float[,] heightMap;
        float randomHeight;

        public DiamondSquare (Node[,] _heightMap, float _randomHeight)
        {
            heightMap = new float[_heightMap.GetLength(0), _heightMap.GetLength(1)];

            for (int x = 0; x < _heightMap.GetLength(0); x++)
            {
                for (int y = 0; y < _heightMap.GetLength(1); y++)
                {
                    heightMap[x, y] = (int)_heightMap[x, y].ReturnPosition().y;
                }
            }

            randomHeight = _randomHeight;

            //set initial values of the four corners
            heightMap[0, 0] = UnityEngine.Random.Range(-randomHeight, randomHeight);
            heightMap[0, heightMap.GetLength(0) - 1] = UnityEngine.Random.Range(-randomHeight, randomHeight);
            heightMap[heightMap.GetLength(1) - 1, 0] = UnityEngine.Random.Range(-randomHeight, randomHeight);
            heightMap[heightMap.GetLength(0) - 1, heightMap.GetLength(0) - 1] = UnityEngine.Random.Range(-randomHeight, randomHeight);
        }

        public void GenerateHeightMap(int startingStepSize)
        {
            int stepSize = startingStepSize;

            while (stepSize > 1)
            {
                int halfStep = Mathf.FloorToInt(stepSize / 2);

                //run diamond step first
                for (int x = 0; x < heightMap.GetLength(0) - 1; x += stepSize)
                {
                    for (int y = 0; y < heightMap.GetLength(1) - 1; y += stepSize)
                    {
                        DiamondStep(x, y, stepSize, UnityEngine.Random.Range(-randomHeight/4, randomHeight/4));
                    }
                }

                for (int x = 0; x < heightMap.GetLength(0); x += halfStep)
                {
                    for (int y = 0; y < heightMap.GetLength(1); y += halfStep)
                    {
                        SquareStep(x, y, halfStep, UnityEngine.Random.Range(-randomHeight/4, randomHeight/4));
                    }
                }

                stepSize /= 2;
            }
        }

        void DiamondStep(int x, int y, int stepSize, float randomOffset)
        {
            //assume x and y are the top left corner of the square
            //find average value
            float averageValue = (heightMap[x, y] + heightMap[x + stepSize, y] + heightMap[x, y + stepSize] + heightMap[x + stepSize, y + stepSize]) / 4;
            //set midpoint to average of nodesToAverage + randomized amount
            heightMap[x + stepSize / 2, y + stepSize / 2] = averageValue + randomOffset;
        }

        void SquareStep(int x, int y, int stepSize, float randomOffset)
        {
            //assume x and y are the center of the diamond
            //find average value while making sure its inside the array bounds
            float left = x - stepSize < 0 ? 0 : heightMap[x - stepSize / 2, 0];
            float right = x + stepSize >= heightMap.GetLength(0) ? 0 : heightMap[x + stepSize / 2, 0];
            float up = y - stepSize < 0 ? 0 : heightMap[0, y - stepSize / 2];
            float down = y + stepSize >= heightMap.GetLength(0) ? 0 : heightMap[0, y + stepSize / 2];
            float averageValue = (left + right + up + down) / 4f;
            //set midpoint to average of nodesToAverage + randomized amount
            heightMap[x, y] = averageValue + randomOffset;
        }

        public float[,] ReturnHeightMap()
        {
            return heightMap;
        }
    }
}
