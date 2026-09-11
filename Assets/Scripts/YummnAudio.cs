using UnityEngine;
public sealed class YummnAudio : MonoBehaviour
{
    private static AudioSource source;
    public static AudioClip LoadClip(string name)
    {
        if (name == "Ki") name = "KiGainB";
        return Resources.Load<AudioClip>("Audio/Yummn/SelectedModel/" + name)
            ?? Resources.Load<AudioClip>("Audio/Yummn/V08/" + name)
            ?? Resources.Load<AudioClip>("Audio/Yummn/" + name);
    }
    // Use actual gross expenditure, not before/after Ki: a kill can refund it in the same action.
    public static bool HasKiExpenditure(YummnActionContext action) =>
        action != null && (action.totalKiCost > 0 || action.attackCostTenths > 0);
    public static float CueVolume(string name) => name == "PalmSeal" ? 1.25f : 1f;
    public static void Play(string name)
    {
        if(source==null){var go=new GameObject("Yummn Effects Audio");DontDestroyOnLoad(go);source=go.AddComponent<AudioSource>();source.playOnAwake=false;source.volume=.65f;}
        var clip=LoadClip(name);if(clip!=null)source.PlayOneShot(clip, CueVolume(name));
    }
}
