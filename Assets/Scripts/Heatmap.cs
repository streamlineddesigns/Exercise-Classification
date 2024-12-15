using System;
using System.Collections.Generic;
using UnityEngine;

public class Heatmap
{
    public int gridSize;
    private float[,] grid;
    private MoveNetSinglePoseSample _poseSample;

    public Heatmap(int size, MoveNetSinglePoseSample PoseSample)
    {
        gridSize = size;
        grid = new float[gridSize, gridSize];
        _poseSample = PoseSample;
    }

    public void AddPoint(Vector2 point)
    {
        int x = (int)Math.Floor(point.x * gridSize);
        int y = (int)Math.Floor(point.y * gridSize);
        if (x >= 0 && x < gridSize && y >= 0 && y < gridSize)
        {
            //grid[x, y] += 1;
            grid[x, y] = 1;
        }
    }

    public void AddPoints(List<Vector2> points, List<List<int>> connections)
    {
        //$$test only for testing //lets skip this for now
        /*foreach (var point in points)
        {
            AddPoint(point);
        }*/

        for (int i = 0; i < connections.Count; i++) {
            int startIndex = connections[i][0];
            int unsafeEndIndex = connections[i][1];//$$test only//essentially there may be 1 extra index here so it's not safe to use in _poseSample.poses
            int endIndex = (unsafeEndIndex <= _poseSample.poses.Count - 1) ? unsafeEndIndex : connections[i][0];
                
            Vector2 start = points[startIndex];
            Vector2 end = points[unsafeEndIndex];//$$test only//unsafe end index is needed here because it exists in points

            //$$test only for testing // require some confidence 
            if (_poseSample.poses != null && _poseSample.poses[startIndex].z > 0.01f /*_poseSample.threshold*/ && _poseSample.poses[endIndex].z > 0.01f /*_poseSample.threshold*/) {
                AddConnectionPoints(start, end);
            }
            
        }
    }

    private void AddConnectionPoints(Vector2 start, Vector2 end)
    {
        int numSteps = 28;
        for (int step = 0; step <= numSteps; step++)
        {
            float t = (float)step / numSteps;
            float x = start.x + t * (end.x - start.x);
            float y = start.y + t * (end.y - start.y);
            AddPoint(new Vector2(x, y));
        }
    }

    public float[][] GetHeatmap()
    {
        float[][] heatmap = new float[gridSize][];
        for (int x = 0; x < gridSize; x++)
        {
            heatmap[x] = new float[gridSize];
            
            for (int y = 0; y < gridSize; y++)
            {
                heatmap[x][y] = (grid[x,y] > 0) ? 1 : 0;
            }
        }
        return heatmap;
    }

    public float[] GetFlattenedHeatmap()
    {
        float[][] heatmap = GetHeatmap();
        float[] flattened = new float[gridSize * gridSize];

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                flattened[x * gridSize + y] = heatmap[x][y];
            }
        }

        return flattened;
    }

    public float[][] GetInverseFlattenedHeatmap()
    {
        float[] flattened = GetFlattenedHeatmap();
        float[][] inverseFlattened = new float[gridSize][];

        for (int x = 0; x < gridSize; x++)
        {
            inverseFlattened[x] = new float[gridSize];

            for (int y = 0; y < gridSize; y++)
            {
                inverseFlattened[x][y] = flattened[x * gridSize + y];
            }
        }

        return inverseFlattened;
    }

    public bool PointInHeatmap(Vector2 point)
    {
        int x = (int)Math.Floor(point.x * gridSize);
        int y = (int)Math.Floor(point.y * gridSize);
        if (x >= 0 && x < gridSize && y >= 0 && y < gridSize)
        {
            return grid[x, y] > 0;
        }
        return false;
    }
}