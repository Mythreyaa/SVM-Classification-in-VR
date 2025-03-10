using UnityEngine;
using System.Collections.Generic;
using TMPro; // Add this at the top of your script


public class DataManager : MonoBehaviour  
{

    public string inputfile;
    public List<DataPoint> dataPoints = new List<DataPoint>();

    public string x1_col_Name;
    public string x2_col_Name;
    public string x3_col_Name;
    public string class_col_Name;

    public GameObject pointPrefabClass0; // Assign in the inspector for class 0
    public GameObject pointPrefabClass1; // Assign in the inspector for class 1

    public GameObject axisPrefabX;
    public GameObject axisPrefabY;
    public GameObject axisPrefabZ;

    public GameObject textMeshProPrefab;
 
    public GameObject tickPrefab;

    public GameObject Axes;

    public float elevationHeight = 20.0f; // Set this to whatever height you want.


    void Start()
    {
        Axes = new GameObject("Axes");
        Axes.transform.parent = this.transform;
        LoadData(inputfile);
        CalcMAxandMin(); // First calculation to get the original bounds
        NormalizeDataPoints();
        CalculateDataBounds(); // Recalculate bounds after normalization
        CreateAxes();
        PlotDataPoints();
        CreateAxisLabels();// Now create axes with the updated bounds
        CreateAllTicks();
        
    }

    public float maxX = 0f;
    public float maxY = 0f;
    public float maxZ = 0f;

    public float maxXbeforeN = 0f;
    public float maxYbeforeN = 0f;
    public float maxZbeforeN = 0f;

    public float minX = float.MaxValue;
    public float minY = float.MaxValue;
    public float minZ = float.MaxValue;



    void NormalizeDataPoints()
    {
        // Find min and max for each axis
        
        foreach (var point in dataPoints)
        {
            if (point.x < minX) minX = point.x;
            if (point.y < minY) minY = point.y;
            if (point.z < minZ) minZ = point.z;
        }

        // Calculate the range (max - min) for each axis
        float rangeX = maxXbeforeN - minX;
        float rangeY = maxYbeforeN - minY;
        float rangeZ = maxZbeforeN - minZ;

        // Normalize data points to a range of 0 to 10
        for (int i = 0; i < dataPoints.Count; i++)
        {
            dataPoints[i].x = ((dataPoints[i].x - minX) / rangeX) * 100;
            dataPoints[i].y = ((dataPoints[i].y - minY) / rangeY) * 100;
            dataPoints[i].z = ((dataPoints[i].z - minZ) / rangeZ) * 100;

            
        }
    }

    void CalcMAxandMin()
    {
        foreach (var point in dataPoints)
        {
            if (point.x > maxXbeforeN) maxXbeforeN = point.x;
            if (point.y > maxYbeforeN) maxYbeforeN = point.y;
            if (point.z > maxZbeforeN) maxZbeforeN = point.z;
            
        }

        // Optionally, log the bounds to ensure they're calculated correctly
        Debug.Log($" Final MAx before Normalise  maxX: {maxXbeforeN}, maxY: {maxYbeforeN}, maxZ: {maxZbeforeN}");
    }


    void CalculateDataBounds()
    {
        foreach (var point in dataPoints)
        {
            if (point.x > maxX) maxX = point.x;
            if (point.y > maxY) maxY = point.y;
            if (point.z > maxZ) maxZ = point.z;
            
        }

        // Optionally, log the bounds to ensure they're calculated correctly
        Debug.Log($"Data Bounds - maxX: {maxX}, maxY: {maxY}, maxZ: {maxZ}");
    }


    public void CreateAxes()
    {
        // Instantiate axis prefabs
        var axisX = Instantiate(axisPrefabX, new Vector3(maxX / 2, 0, 0), Quaternion.identity);
        var axisY = Instantiate(axisPrefabY, new Vector3(0, maxY / 2, 0), Quaternion.identity);
        var axisZ = Instantiate(axisPrefabZ, new Vector3(0, 0, maxZ / 2), Quaternion.identity);

        // Scale axes to match data bounds
        axisX.transform.localScale = new Vector3(maxX + 10, axisX.transform.localScale.y, axisX.transform.localScale.z);
        axisY.transform.localScale = new Vector3(axisY.transform.localScale.x, maxY + 10, axisY.transform.localScale.z);
        axisZ.transform.localScale = new Vector3(axisZ.transform.localScale.x, axisZ.transform.localScale.y, maxZ + 10);


        axisX.transform.SetParent(Axes.transform, false);
        axisY.transform.SetParent(Axes.transform, false);
        axisZ.transform.SetParent(Axes.transform, false);

    }



    public GameObject dataPointLabelPrefab; // Assign your TextMeshPro Prefab in the inspector

    void PlotDataPoints()
    {
        foreach (var point in dataPoints)
        {
            GameObject prefabToUse = point.classLabel == 0 ? pointPrefabClass0 : pointPrefabClass1;
            var pointInstance = Instantiate(prefabToUse, new Vector3(point.x, point.y, point.z), Quaternion.identity);
            pointInstance.transform.parent = this.transform; // Organize under the DataManager object

            // Create a label for the data point
            var labelInstance = Instantiate(dataPointLabelPrefab, pointInstance.transform.position, Quaternion.identity);
            labelInstance.transform.SetParent(pointInstance.transform); // Make the label a child of the data point
            labelInstance.transform.localPosition = new Vector3(0, 0.5f, 0); // Adjust if needed to position the label right above the data point

            // Calculate the original values based on normalization
            float originalX = DeNormalize(point.x, minX, maxXbeforeN, 100);
            float originalY = DeNormalize(point.y, minY, maxYbeforeN, 100);
            float originalZ = DeNormalize(point.z, minZ, maxZbeforeN, 100);

            // Set the text of the label
            TextMeshPro labelTMP = labelInstance.GetComponent<TextMeshPro>();
            //labelTMP.text = string.Format("({0:0.0}, {1:0.0}, {2:0.0})", originalX, originalY, originalZ);
            labelTMP.text = string.Format("({0:0.0}, {1:0.0}, {2:0.0})", point.x, point.y, point.z);
            labelTMP.fontSize = 7;
        }
    }

    float DeNormalize(float normalizedValue, float min, float max, float range)
    {
        return (normalizedValue / range) * (max - min) + min;
    }

    void LoadData(string fileName)
    {
        var data = CSVReader.Read(fileName); // Assuming CSVReader is your CSV parsing utility
        foreach (var row in data)
        {
            float x = float.Parse(row[x1_col_Name].ToString());
            float y = float.Parse(row[x2_col_Name].ToString());
            float z = float.Parse(row[x3_col_Name].ToString());
            int classLabel = int.Parse(row[class_col_Name].ToString());
            Debug.Log("X : " + x + " Y : " + y + " Z: " + z + " Class: " + classLabel);

            DataPoint point = new DataPoint { x = x, y = y, z = z, classLabel = classLabel };
            dataPoints.Add(point);
        }
    }

    float ConvertToFloat(object value)
    {
        if (value is double)
        {
            return (float)(double)value;
        }
        else if (value is int)
        {
            return (float)(int)value;
        }
        else
        {
            return (float)value;
        }
    }

    // Assign your TextMesh Prefab in the inspector

     // Assign your TextMeshPro Prefab in the inspector

    public void CreateAxisLabels()
    {

        float offset = 15.0f;
        // Instantiate TextMeshPro for X Axis label
        GameObject labelX = Instantiate(textMeshProPrefab, new Vector3(maxX + offset, 0, -20), Quaternion.identity);
        labelX.GetComponent<TextMeshPro>().text = x1_col_Name; // Change TextMesh to TextMeshPro
        
        

        // Instantiate TextMeshPro for Y Axis label
        GameObject labelY = Instantiate(textMeshProPrefab, new Vector3(0, maxY + offset, 0), Quaternion.identity);
        labelY.GetComponent<TextMeshPro>().text = x2_col_Name; // Change TextMesh to TextMeshPro
        

        // Instantiate TextMeshPro for Z Axis label
        GameObject labelZ = Instantiate(textMeshProPrefab, new Vector3(0, 0, maxZ + offset), Quaternion.identity);
        labelZ.GetComponent<TextMeshPro>().text = x3_col_Name; // Change TextMesh to TextMeshPro
        


        labelX.transform.SetParent(Axes.transform, false);
        labelY.transform.SetParent(Axes.transform, false);
        labelZ.transform.SetParent(Axes.transform, false);
    }




    private void CreateTicksForAxis(GameObject axis, int numberOfTicks, float actualMin, float actualMax, string axisName)
    {
        // Calculate the interval between actual values for each tick mark
        float actualInterval = (actualMax - actualMin) / numberOfTicks;

        for (int i = 0; i <= numberOfTicks; i++)
        {
            // Calculate the position of the tick mark based on the normalized axis length
            float normalizedPosition = i * (1f / numberOfTicks);

            // Calculate the actual value for the tick label
            float actualValue = actualMin + i * actualInterval;

            // Instantiate tick prefab and position it
            GameObject tick = Instantiate(tickPrefab);
            tick.transform.SetParent(Axes.transform, false);

            Vector3 tickPosition = Vector3.zero;

            // Set the position based on the axis
            switch (axisName)
            {
                case "X":
                    tickPosition = new Vector3(normalizedPosition * maxX, 0, 0);
                    break;
                case "Y":
                    tickPosition = new Vector3(-15, normalizedPosition * maxY, 0);
                    break;
                case "Z":
                    tickPosition = new Vector3(-15, 0, normalizedPosition * maxZ);
                    break;
            }

            tick.transform.localPosition = tickPosition;

            // Set the text label to the correct value formatted to two decimal places
            TextMeshPro labelTMP = tick.GetComponentInChildren<TextMeshPro>();
            labelTMP.text = $"{actualValue:0.00}";
        }
    }


    public void CreateAllTicks()
    {
        int numberOfTicks = 3; // You can adjust this to the number of ticks you want

        // Call the CreateTicksForAxis function for each axis with their actual min/max values
        CreateTicksForAxis(axisPrefabX, numberOfTicks, 0, maxXbeforeN, "X");
        CreateTicksForAxis(axisPrefabY, numberOfTicks, 0, maxYbeforeN, "Y");
        CreateTicksForAxis(axisPrefabZ, numberOfTicks, 0, maxZbeforeN, "Z");
    }






}
