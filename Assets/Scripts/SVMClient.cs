using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using TMPro;

public class SVMClient : MonoBehaviour
{
    public DataManager dataManager; // Assign in the inspector
    public GameObject decisionBoundaryPlane;
    public TextMeshPro svmInfoText; // Assign this in the Unity Inspector


    private void Start()
    {
        if (dataManager != null)
        {
            StartCoroutine(SendDataToServer());
        }
        else
        {
            Debug.LogError("DataManager is not assigned.");
        }
    }

    IEnumerator SendDataToServer()
    {
        SVMData dataToSend = new SVMData(dataManager.dataPoints);
        string jsonData = JsonUtility.ToJson(dataToSend);

        Debug.Log($"JSON : {jsonData}");
        UnityWebRequest request = new UnityWebRequest("http://localhost:5000/svm", "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData)),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
        }
        else
        {
            SVMParameters parameters = JsonUtility.FromJson<SVMParameters>(request.downloadHandler.text);
            Debug.Log("Recieved Values :");
            Debug.Log("Intercept: " + parameters.intercept);
            Debug.Log("Coefficients: " + string.Join(", ", parameters.coefficients));

            // Process and print the support vectors
          foreach (SupportVector sv in parameters.supportVectors)  
            {
                Debug.Log($"Support Vector: [x: {sv.vector[0]}, y: {sv.vector[1]}, z: {sv.vector[2]}]");
            }
            AdjustDecisionBoundary(parameters.coefficients, parameters.supportVectors);
            DisplaySVMInfo(parameters.coefficients, parameters.intercept, parameters.supportVectors);
            PlaceSupportVectorMarkers(parameters.supportVectors);

        }
    }
    void AdjustDecisionBoundary(float[] coefficients, SupportVector[] supportVectors)
    {
    // Calculate the normal vector of the decision boundary plane
        Vector3 planeNormal = new Vector3(coefficients[0], coefficients[1], coefficients[2]).normalized;

    // Calculate a central point on the decision boundary
        Vector3 planePoint = Vector3.zero;
        foreach (SupportVector sv in supportVectors)
        {
            planePoint += new Vector3(sv.vector[0], sv.vector[1], sv.vector[2]);
        }
        planePoint /= supportVectors.Length; // Average

    // Adjust the decision boundary plane in Unity
        decisionBoundaryPlane.transform.up = planeNormal; // Set plane's orientation
        decisionBoundaryPlane.transform.position = planePoint; // Set plane's position
        decisionBoundaryPlane.transform.localScale = new Vector3(10, 1, 10); // Set scale to 10x10, adjust thickness as needed
    }

    void DisplaySVMInfo(float[] coefficients, float intercept, SupportVector[] supportVectors)
    {
        // Construct the decision boundary equation as a string
        string equation = $"y = {coefficients[0]:F2}x + {coefficients[1]:F2}y + {coefficients[2]:F2}z + {intercept:F2}";

        // Optionally, add information about the support vectors
        string supportVectorsInfo = $"Support Vectors: {supportVectors.Length}";

        // Update the TextMeshPro text
        svmInfoText.text = $"Decision Boundary Equation:\n{equation}\n{supportVectorsInfo}";
    }

    public GameObject supportVectorPrefab; // Assign this in the inspector

    void PlaceSupportVectorMarkers(SupportVector[] supportVectors)
    {
        foreach (SupportVector sv in supportVectors)
        {
            // Convert the support vector into a position. Adjust as needed for your coordinate system.
            Vector3 position = new Vector3(sv.vector[0], sv.vector[1], sv.vector[2]);

            // Instantiate the prefab at the support vector's position
            GameObject marker = Instantiate(supportVectorPrefab, position, Quaternion.identity);

            // Optionally, parent the markers to a specific object to keep the hierarchy organized
            // marker.transform.SetParent(parentTransform, false);
        }
    }




}
[System.Serializable]
public class SVMData
{
    public float[] x;
    public float[] y;
    public float[] z;
    public int[] labels;

    public SVMData(List<DataPoint> dataPoints)
    {
        this.x = dataPoints.Select(dp => dp.x).ToArray();
        this.y = dataPoints.Select(dp => dp.y).ToArray();
        this.z = dataPoints.Select(dp => dp.z).ToArray();
        this.labels = dataPoints.Select(dp => dp.classLabel).ToArray();
    }
}


[System.Serializable]
public class SupportVector
{
    public float[] vector; // This matches each support vector wrapped in an object
}

[System.Serializable]
public class SVMParameters
{
    public float[] coefficients; // SVM coefficients
    public float intercept; // SVM intercept
    public SupportVector[] supportVectors; // Adjusted to hold the wrapped vectors
}


