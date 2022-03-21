using System;
using UnityEngine;

public class UiMenu : MonoBehaviour
{
    public Action onHideAction;
    public Action onShowAction;
    
    public void Show()
    {
        gameObject.SetActive(true);
        onShowAction?.Invoke();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        onHideAction?.Invoke();
    }
}
