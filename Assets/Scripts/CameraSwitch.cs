using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    public GameObject mainCamera;
    public GameObject secondaryCamera;

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
}
