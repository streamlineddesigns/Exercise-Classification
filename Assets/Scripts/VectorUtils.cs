using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorUtils
{
    public static List<Vector3> TranslateRelativeToOffset(List<Vector3> poses, Vector3 offset)
    {
        List<Vector3> translatedPoses = new List<Vector3>();

        for (int i = 0; i < poses.Count; i++) {
            translatedPoses.Add(new Vector3(poses[i].x - offset.x, poses[i].y - offset.y, poses[i].z));
        }

        return translatedPoses;
    }

    public static List<Vector3> ResampleToUniformMagnitude(List<Vector3> poses, float magnitude = 0.1f)
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
}