using UnityEngine;
using UnityEngine.UI;

namespace Main.Scripts.RenderScene
{
public class RenderSceneController : MonoBehaviour
{
    [SerializeField]
    private Transform directionLightTransform = null!;
    [SerializeField]
    private Slider directionLightXSlider = null!;
    [SerializeField]
    private Slider directionLightYSlider = null!;

    private float directionLightXAngle;
    private float directionLightYAngle;

    private void Awake()
    {
        var eulerAngles = directionLightTransform.rotation.eulerAngles;
        
        directionLightXAngle = eulerAngles.x;
        directionLightYAngle = eulerAngles.y - 180f;

        directionLightXSlider.value = directionLightXAngle / 180f;
        directionLightYSlider.value = directionLightYAngle / 360f + 0.5f;

        directionLightXSlider.onValueChanged.AddListener(value =>
        {
            directionLightXAngle = 180 * value;
            directionLightTransform.rotation = Quaternion.Euler(directionLightXAngle, directionLightYAngle, 0);
        });

        directionLightYSlider.onValueChanged.AddListener(value =>
        {
            directionLightYAngle = 360 * (value - 0.5f);
            directionLightTransform.rotation = Quaternion.Euler(directionLightXAngle, directionLightYAngle, 0);
        });
    }
}
}