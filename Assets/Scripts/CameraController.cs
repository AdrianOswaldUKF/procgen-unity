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
        int width = string.IsNullOrEmpty(widthInput?.text) ? 32 : int.Parse(widthInput.text);
        int height = string.IsNullOrEmpty(heightInput?.text) ? 32 : int.Parse(heightInput.text);
        float cellSize = string.IsNullOrEmpty(cellSizeInput?.text) ? 1f : float.Parse(cellSizeInput.text);
    
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