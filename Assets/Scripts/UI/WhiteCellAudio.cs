using UnityEngine;

public class WhiteCellAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip loseCellSound;
    [SerializeField] private AudioClip restoreCellSound;
    
    public void Initialize()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
    
    public void PlayLoseSound()
    {
        if (audioSource != null && loseCellSound != null)
        {
            audioSource.PlayOneShot(loseCellSound);
        }
    }
    
    public void PlayRestoreSound()
    {
        if (audioSource != null && restoreCellSound != null)
        {
            audioSource.PlayOneShot(restoreCellSound);
        }
    }
}