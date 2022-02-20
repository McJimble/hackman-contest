using UnityEngine;

public class PlayAudioOnStart : MonoBehaviour
{
    [SerializeField] private AudioClip[] sounds;

    private void Start()
    {
        foreach (var sound in sounds)
        {
            SoundManager.Instance.PlaySound(sound);
        }
    }
}
