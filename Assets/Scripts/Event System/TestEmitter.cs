using UnityEngine;

public class TestEmitter : MonoBehaviour
{

    private void Start()
    {
        EventManager.TriggerEvent("Test");
    }

}