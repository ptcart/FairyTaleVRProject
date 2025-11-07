using UnityEngine;

public class ChoiceButtonLogger : MonoBehaviour, IButtonAction
{
    public string buttonName;

    public void OnButtonClick()
    {
        Debug.Log($"[ChoiceButtonLogger] 버튼 클릭됨: {buttonName}");
    }
}