using DanielLochner.Assets.SimpleScrollSnap;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [Header("Character Selection")]
    [SerializeField] private SimpleScrollSnap simpleScrollSnap;
    
    [Space]
    [Header("Controller Type")]
    [SerializeField] private Toggle tiltingToggle;
    [SerializeField] private Toggle draggingToggle;

    private void Start()
    {
        simpleScrollSnap.GoToPanel(SettingsDataHolder.SelectedCharacterIndex);

        // Temporarily remove listeners
        tiltingToggle.onValueChanged.RemoveAllListeners();
        draggingToggle.onValueChanged.RemoveAllListeners();
    
        // Set toggle states without triggering events
        tiltingToggle.isOn = SettingsDataHolder.ControlType == 0;
        draggingToggle.isOn = SettingsDataHolder.ControlType == 1;
    
        // Re-add listeners
        tiltingToggle.onValueChanged.AddListener(OnTiltingControllerSelection);
        draggingToggle.onValueChanged.AddListener(OnDraggingControllerSelection);
    }
    
    public void OnCharacterIndexChanged(int targetIndex, int _)
    {
        Debug.Log($"On Character Index Changed: {targetIndex}");
        EventManager.TriggerEvent(EventName.OnCharacterChanged, targetIndex);
    }

    public void OnTiltingControllerSelection(bool value)
    {
        Debug.Log($"On Tilting Controller Selection: {value}");
        if(value)
            EventManager.TriggerEvent(EventName.OnControlTypeChanged, 0);
    }

    public void OnDraggingControllerSelection(bool value)
    {
        Debug.Log($"On Dragging Controller Selection: {value}");
        if (value)
            EventManager.TriggerEvent(EventName.OnControlTypeChanged, 1);
    }

    public void OnBackButtonClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
