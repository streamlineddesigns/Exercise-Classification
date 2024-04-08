using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorUtils
{
    public static Heatmap GetHeatMap(List<Vector3> poses)
    {
        Heatmap heatmap = new Heatmap(28);

        List<List<int>> connections = new List<List<int>>();

        List<int> connectionOne = new List<int>() {
            0,17
        };

        List<int> connectionTwo = new List<int>() {
            5,6
        };

        List<int> connectionThree = new List<int>() {
            5,7
        };

        List<int> connectionFour = new List<int>() {
            6,8
        };

        List<int> connectionFive = new List<int>() {
            7,9
        };

        List<int> connectionSix = new List<int>() {
            8,10
        };

        List<int> connectionSeven = new List<int>() {
            5,11
        };

        List<int> connectionEight = new List<int>() {
            6,12
        };

        List<int> connectionNine = new List<int>() {
            11,12
        };

        List<int> connectionTen = new List<int>() {
            11,13
        };

        List<int> connectionEleven = new List<int>() {
            12,14
        };

        List<int> connectionTwelve = new List<int>() {
            13,15
        };

        List<int> connectionThirteen = new List<int>() {
            14,16
        };

        List<int> connectionFourteen = new List<int>() {
            5,12
        };

        List<int> connectionFifteen = new List<int>() {
            6,11
        };

        connections.Add(connectionOne);
        connections.Add(connectionTwo);
        connections.Add(connectionThree);
        connections.Add(connectionFour);
        connections.Add(connectionFive);
        connections.Add(connectionSix);
        connections.Add(connectionSeven);
        connections.Add(connectionEight);
        connections.Add(connectionNine);
        connections.Add(connectionTen);
        connections.Add(connectionEleven);
        connections.Add(connectionTwelve);
        connections.Add(connectionThirteen);

        //connections.Add(connectionFourteen);
        //connections.Add(connectionFifteen);

        float[] fiveFloat = new float[2] {poses[5].x, poses[5].y};
        float[] sixFloat = new float[2] {poses[6].x, poses[6].y};
        float[] FiveSixCentroidFloat = GetCentroid(new float[][]{fiveFloat, sixFloat});
        Vector2 FiveSixCentroidVector2 = new Vector2(FiveSixCentroidFloat[0], FiveSixCentroidFloat[1]);

        List<Vector2> positionsOfPoses = poses.Select(x => new Vector2(x.x, x.y)).ToList();
        positionsOfPoses.Add(FiveSixCentroidVector2);

        heatmap.AddPoints(positionsOfPoses, connections);

        return heatmap;
    }

    public static List<Vector3> TranslateRelativeToOffset(List<Vector3> poses, Vector3 offset)
    {
        List<Vector3> translatedPoses = new List<Vector3>();

        for (int i = 0; i < poses.Count; i++) {
            Vector3 offsetTarget = new Vector3(poses[i].x - offset.x, poses[i].y - offset.y, poses[i].z);
            Vector3 offsetDirection = GetDirection(poses[i], offsetTarget);
            Vector3 normalizedOffsetDirection = offsetDirection.normalized;
            Vector3 targetTranslatePosition = poses[i] + (normalizedOffsetDirection * offsetDirection.magnitude * 0.5f);
            translatedPoses.Add(new Vector3(targetTranslatePosition.x, targetTranslatePosition.y, poses[i].z));
        }

        return translatedPoses;
    }

    public static List<Vector3> GetDirectionVectors(List<Vector3> poses)
    {
        List<Vector3> resampledPoses = new List<Vector3>();
        
        float[] fiveFloat = new float[3] {poses[5].x, poses[5].y, poses[5].z};
        float[] sixFloat = new float[3] {poses[6].x, poses[6].y, poses[6].z};
        float[] FiveSixCentroidFloat = GetCentroid(new float[][]{fiveFloat, sixFloat});
        Vector3 FiveSixCentroidVector3 = new Vector3(FiveSixCentroidFloat[0], FiveSixCentroidFloat[1], 0.0f);
        Vector3 ZeroTwo = FiveSixCentroidVector3;

        Vector3 ZeroTargetOne  = poses[0];
        Vector3 ZeroTargetTwo  = GetDirection(poses[0], ZeroTwo).normalized;

        Vector3 OneTarget      = GetDirection(poses[0], poses[1]).normalized;
        Vector3 TwoTarget      = GetDirection(poses[0], poses[2]).normalized;
        Vector3 ThreeTarget    = GetDirection(poses[1], poses[3]).normalized;
        Vector3 FourTarget     = GetDirection(poses[2], poses[4]).normalized;

        Vector3 FiveTarget     = GetDirection(poses[6], poses[5]).normalized;
        Vector3 SixTarget      = GetDirection(poses[5], poses[6]).normalized;
        Vector3 SevenTarget    = GetDirection(poses[5], poses[7]).normalized;
        Vector3 EightTarget    = GetDirection(poses[6], poses[8]).normalized;
        Vector3 NineTarget     = GetDirection(poses[7], poses[9]).normalized;
        Vector3 TenTarget      = GetDirection(poses[8], poses[10]).normalized;
        Vector3 ElevenTarget   = GetDirection(poses[5], poses[11]).normalized;
        Vector3 TwelveTarget   = GetDirection(poses[6], poses[12]).normalized;
        Vector3 ThirteenTarget = GetDirection(poses[11], poses[13]).normalized;
        Vector3 FourteenTarget = GetDirection(poses[12], poses[14]).normalized;
        Vector3 FifteenTarget  = GetDirection(poses[13], poses[15]).normalized;
        Vector3 SixteenTarget  = GetDirection(poses[14], poses[16]).normalized;

        resampledPoses.Add(ZeroTargetTwo);
        resampledPoses.Add(OneTarget);
        resampledPoses.Add(TwoTarget);
        resampledPoses.Add(ThreeTarget);
        resampledPoses.Add(FourTarget);
        resampledPoses.Add(FiveTarget);
        resampledPoses.Add(SixTarget);
        resampledPoses.Add(SevenTarget);
        resampledPoses.Add(EightTarget);
        resampledPoses.Add(NineTarget);
        resampledPoses.Add(TenTarget);
        resampledPoses.Add(ElevenTarget);
        resampledPoses.Add(TwelveTarget);
        resampledPoses.Add(ThirteenTarget);
        resampledPoses.Add(FourteenTarget);
        resampledPoses.Add(FifteenTarget);
        resampledPoses.Add(SixteenTarget);

        return resampledPoses;
    }

    public static List<Vector3> ResampleToUniformMagnitude(List<Vector3> poses, float magnitude = 0.2f)
    {
        List<Vector3> resampledPoses = new List<Vector3>();
        
        float[] fiveFloat = new float[3] {poses[5].x, poses[5].y, poses[5].z};
        float[] sixFloat = new float[3] {poses[6].x, poses[6].y, poses[6].z};
        float[] FiveSixCentroidFloat = GetCentroid(new float[][]{fiveFloat, sixFloat});
        Vector3 FiveSixCentroidVector3 = new Vector3(FiveSixCentroidFloat[0], FiveSixCentroidFloat[1], 0.0f);
        Vector3 ZeroTwo = FiveSixCentroidVector3;

        Vector3 ZeroTargetOne  = poses[0];
        Vector3 ZeroTargetTwo  = ZeroTargetOne  + (GetDirection(poses[0], ZeroTwo).normalized    * magnitude);

        Vector3 OneTarget      = ZeroTargetOne  + (GetDirection(poses[0], poses[1]).normalized   * magnitude * 0.25f);
        Vector3 TwoTarget      = ZeroTargetOne  + (GetDirection(poses[0], poses[2]).normalized   * magnitude * 0.25f);
        Vector3 ThreeTarget    = OneTarget      + (GetDirection(poses[1], poses[3]).normalized   * magnitude * 0.25f);
        Vector3 FourTarget     = TwoTarget      + (GetDirection(poses[2], poses[4]).normalized   * magnitude * 0.25f);

        Vector3 FiveTarget     = ZeroTargetTwo  + (GetDirection(poses[6], poses[5]).normalized   * magnitude * 0.5f);
        Vector3 SixTarget      = ZeroTargetTwo  + (GetDirection(poses[5], poses[6]).normalized   * magnitude * 0.5f);
        Vector3 SevenTarget    = FiveTarget     + (GetDirection(poses[5], poses[7]).normalized   * magnitude);
        Vector3 EightTarget    = SixTarget      + (GetDirection(poses[6], poses[8]).normalized   * magnitude);
        Vector3 NineTarget     = SevenTarget    + (GetDirection(poses[7], poses[9]).normalized   * magnitude);
        Vector3 TenTarget      = EightTarget    + (GetDirection(poses[8], poses[10]).normalized  * magnitude);
        Vector3 ElevenTarget   = FiveTarget     + (GetDirection(poses[5], poses[11]).normalized  * magnitude);
        Vector3 TwelveTarget   = SixTarget      + (GetDirection(poses[6], poses[12]).normalized  * magnitude);
        Vector3 ThirteenTarget = ElevenTarget   + (GetDirection(poses[11], poses[13]).normalized * magnitude);
        Vector3 FourteenTarget = TwelveTarget   + (GetDirection(poses[12], poses[14]).normalized * magnitude);
        Vector3 FifteenTarget  = ThirteenTarget + (GetDirection(poses[13], poses[15]).normalized * magnitude);
        Vector3 SixteenTarget  = FourteenTarget + (GetDirection(poses[14], poses[16]).normalized * magnitude);

        resampledPoses.Add(ZeroTargetOne);
        resampledPoses.Add(OneTarget);
        resampledPoses.Add(TwoTarget);
        resampledPoses.Add(ThreeTarget);
        resampledPoses.Add(FourTarget);
        resampledPoses.Add(FiveTarget);
        resampledPoses.Add(SixTarget);
        resampledPoses.Add(SevenTarget);
        resampledPoses.Add(EightTarget);
        resampledPoses.Add(NineTarget);
        resampledPoses.Add(TenTarget);
        resampledPoses.Add(ElevenTarget);
        resampledPoses.Add(TwelveTarget);
        resampledPoses.Add(ThirteenTarget);
        resampledPoses.Add(FourteenTarget);
        resampledPoses.Add(FifteenTarget);
        resampledPoses.Add(SixteenTarget);

        return resampledPoses;
    }

    public static Vector3 GetDirection(Vector3 point1, Vector3 point2)
    {
        Vector3 direction = new Vector3(point2.x, point2.y, 0.0f) - new Vector3(point1.x, point1.y, 0.0f);
        return direction;
    }

    public static float[] GetDirection(float[] point1, float[] point2) {
        // Array for direction vector
        float[] direction = new float[point1.Length];
        
        // Calculate difference using loops 
        for (int i = 0; i < point1.Length; i++) {
            direction[i] = point2[i] - point1[i];
        }
        
        return direction;
    }

    public static float[] NormalizeDirection(float[] direction) {
        // Result array
        float[] normDirection = new float[direction.Length];
        
        // Get magnitude
        float magnitude = 0;
        for (int i = 0; i < direction.Length; i++) {
            magnitude += direction[i] * direction[i]; 
        }
        magnitude = (float)Math.Sqrt(magnitude);
        
        // Normalize using loops
        for (int i = 0; i < normDirection.Length; i++) {
            normDirection[i] = direction[i] / magnitude;
        }
        
        return normDirection;
    }


    public static float GetDistance(float[] p1, float[] p2)
    {
        float distance = 0;

        if (p1.Length != p2.Length) {
            Debug.LogError("Input vectors must be of equal length");
        }

        for (int i = 0; i < p1.Length; i++) {
            float a = p1[i] - p2[i];
            distance += (float) (a * a);
        }

        return (float)Mathf.Sqrt(distance);
    }

    public static float[] GetCentroid(float[][] data)
    {
        if ((data.Select(x => x.Length).Sum() / data.Length != data[0].Length)) {
            Debug.LogError("Input vectors must be of equal length");
        }

        float[] centroid = new float[data[0].Length];
        float[] counter = new float[data[0].Length];

        for (int i = 0; i < data.Length; i++) {
            for (int j = 0; j < centroid.Length; j++) {
                centroid[j] += data[i][j];
                counter[j]++;
            }
        }

        for (int k = 0; k < centroid.Length; k++) {
            centroid[k] /= counter[k];
        }

        return centroid;
    }

    public static float[] GetSummation(float[][] data)
    {
        float[] sum = new float[data[0].Length];

        for (int i = 0; i < data.Length; i++) {
            for (int j = 0; j < data[i].Length; j++) {
                sum[j] += data[i][j];
            }
        }

        return sum;
    }

    public static float[] GetWeightedVector(float[] vector, float[] weights)
    {
        float[] weightedVector = new float[vector.Length];

        if (vector.Length != weights.Length)
        {
            throw new ArgumentException("Vector and weights must have the same length.");
        }

        for (int i = 0; i < vector.Length; i++)
        {
            weightedVector[i] = vector[i] * weights[i];
        }

        return weightedVector;
    }

    public static float CosineSimilarity(float[] vector1, float[] vector2)
    {
        if (vector1.Length != vector2.Length)
        {
            throw new ArgumentException("Vectors must have the same length.");
        }

        float dotProduct = 0;
        float norm1 = 0;
        float norm2 = 0;

        for (int i = 0; i < vector1.Length; i++)
        {
            dotProduct += vector1[i] * vector2[i];
            norm1 += vector1[i] * vector1[i];
            norm2 += vector2[i] * vector2[i];
        }

        norm1 = (float)Math.Sqrt(norm1);
        norm2 = (float)Math.Sqrt(norm2);

        if (norm1 == 0 || norm2 == 0)
        {
            return 0;
        }

        return dotProduct / (norm1 * norm2);
    }

    public static float WeightedCosineSimilarity(float[] vector1, float[] vector2, float[] weights)
    {
        float[] weightedVector1 = GetWeightedVector(vector1, weights);
        float[] weightedVector2 = GetWeightedVector(vector2, weights);
        return CosineSimilarity(weightedVector1, weightedVector2);
    }

}