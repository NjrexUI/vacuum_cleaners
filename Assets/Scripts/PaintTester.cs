using UnityEngine;

public class PaintTester : MonoBehaviour
{
    public PaintManager paintManager;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            paintManager.PaintAt(mouseWorld);
        }
    }
}
