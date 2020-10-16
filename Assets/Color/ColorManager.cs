using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ColorManager : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    RectTransform rt;

    private ColorRGB CRGB;
    private ColorPanel CP;
    private ColorCircle CC;

    public Slider sliderCRGB;
    public Image colorShow;
	private UnityAction<Color> colorCallback;

	private void CC_getPos(Vector2 pos)
    {
        Color getColor= CP.GetColorByPosition(pos);
        colorShow.color = getColor;
		colorCallback?.Invoke(getColor);
	}

    // Use this for initialization
    void Start () {
        rt = GetComponent<RectTransform>();

        CRGB = GetComponentInChildren<ColorRGB>();
        CP = GetComponentInChildren<ColorPanel>();
        CC = GetComponentInChildren<ColorCircle>();

        sliderCRGB.onValueChanged.AddListener(OnCRGBValueChanged);

		CC.getPos += CC_getPos;
	}

	public void OnColorChanged(UnityAction<Color> callback)
	{
		colorCallback = callback;
	}

	void IDragHandler.OnDrag(PointerEventData e)
	{
		e.Use();
        //Vector3 wordPos;
        ////将UGUI的坐标转为世界坐标  
        //if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, eventData.position, eventData.pressEventCamera, out wordPos))
        //    rt.position = wordPos;
    }

    void OnCRGBValueChanged(float value)
    {
        Color endColor=CRGB.GetColorBySliderValue(value);
        CP.SetColorPanel(endColor);
        CC.setShowColor();
    }

	void IPointerDownHandler.OnPointerDown(PointerEventData e)
	{
		e.Use();
	}
}
