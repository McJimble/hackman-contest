using UnityEngine;

public class DisableCinemachineGUI : MonoBehaviour
{
    Cinemachine.CinemachineBrain cinemachineBrain;

    private void Awake()
    {
        cinemachineBrain = GetComponent<Cinemachine.CinemachineBrain>();
        cinemachineBrain.useGUILayout = false;
    }

}
