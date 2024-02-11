using System;
using UnityEngine;
using UnityEngine.UI;

namespace Main.Scripts.UI
{
public class SelectionImage : MonoBehaviour
{
    private static readonly int Bounds = Shader.PropertyToID("_Bounds");
    
    private Image image = null!;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void SetBounds(Vector2 from, Vector2 to)
    {
        image.materialForRendering.SetVector(Bounds, new Vector4(from.x, from.y, to.x, to.y));
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
}