using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class heatMapRow
{
    public GameObject rowParent;
    [HideInInspector]
    public List<SpriteRenderer> data;
}

public class HeatmapVisual : MonoBehaviour
{
    public Color OnColor;
    public Color OffColor;
    public List<heatMapRow> heatMapRows = new List<heatMapRow>();

    protected void Start()
    {
        for (int i = 0; i < heatMapRows.Count; i++) {

            heatMapRows[i].data = new List<SpriteRenderer>();

            for (int j = 0; j < heatMapRows[i].rowParent.transform.childCount; j++) {
                heatMapRows[i].data.Add(heatMapRows[i].rowParent.transform.GetChild(j).gameObject.GetComponent<SpriteRenderer>());
            }
        }
    }

    public void SetHeatMap(Heatmap heatMap)
    {
        float[][] cells = heatMap.GetHeatmap();

        for (int i = 0; i < cells.Length; i++) {

            for (int j = 0; j < cells[i].Length; j++) {
                bool isActivated = cells[i][j] > 0;

                if (isActivated) {
                    heatMapRows[i].data[j].color = OnColor;
                } else {
                    heatMapRows[i].data[j].color = OffColor;
                }
            }
        }
    }

    public void SetHeatMapFlattened(Heatmap heatMap)
    {
        float[] cells = heatMap.GetFlattenedHeatmap();

        for (int i = 0; i < heatMap.gridSize; i++) {
            for (int j = 0; j < heatMap.gridSize; j++) {
                int index = i * heatMap.gridSize + j;
                bool isActivated = cells[index] > 0;

                if (isActivated) {
                    heatMapRows[i].data[j].color = OnColor;
                } else {
                    heatMapRows[i].data[j].color = OffColor;
                }
            }
        }
    }
}
