using TMPro;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject mainCamera;
    public GameObject secondaryCamera;
    
    public TMP_InputField widthInput;
    public TMP_InputField heightInput;
    public TMP_InputField cellSizeInput;
    
    public float distanceMultiplier = 1f;
    public float minDistance = 50f;
    
    public void CameraOne()
    {
        secondaryCamera.SetActive(false);
        mainCamera.SetActive(true);
    }

    public void CameraTwo()
    {
        mainCamera.SetActive(false);
        secondaryCamera.SetActive(true);
    }

    private void LateUpdate()
    {
        int width = 32;
        int height = 32;
        float cellSize = 1f;
        
        if (widthInput != null && !string.IsNullOrEmpty(widthInput.text))
            int.TryParse(widthInput.text, out width);
        
        if (heightInput != null && !string.IsNullOrEmpty(heightInput.text))
            int.TryParse(heightInput.text, out height);
        
        if (cellSizeInput != null && !string.IsNullOrEmpty(cellSizeInput.text))
            float.TryParse(cellSizeInput.text, out cellSize);
    
        float maxSize = Mathf.Max(width * cellSize, height * cellSize);
        float distance = Mathf.Max(maxSize * distanceMultiplier, minDistance);
        float cameraHeight = 50f * (maxSize / 64f);

        if (mainCamera) 
        {
            mainCamera.transform.position = new Vector3(0, cameraHeight, -distance);
            mainCamera.transform.LookAt(Vector3.zero);
        }

        if (secondaryCamera)
        {
            secondaryCamera.transform.position = new Vector3(0, distance * 1.2f, 0);
            secondaryCamera.transform.rotation = Quaternion.Euler(90, -90, 0);
        }
    }
}