using TMPro;
using UnityEngine;

public class PlaneResizer : MonoBehaviour
{
    public TMP_InputField widthInput;
    public TMP_InputField heightInput;
    public TMP_InputField cellSizeInput;
    
    private int _width;
    private int _height;
    
    private void LateUpdate()
    {
        int width = string.IsNullOrEmpty(widthInput?.text) ? 32 : int.Parse(widthInput.text);
        int height = string.IsNullOrEmpty(heightInput?.text) ? 32 : int.Parse(heightInput.text);
        float cellSize = string.IsNullOrEmpty(cellSizeInput?.text) ? 1f : float.Parse(cellSizeInput.text);

        transform.localScale = new Vector3(width * cellSize / 10f, 1f, height * cellSize / 10f);
    }
}