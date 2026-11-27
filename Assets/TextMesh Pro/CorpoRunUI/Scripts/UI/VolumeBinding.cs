using UnityEngine;

namespace CorpoRun.UI
{
    // Dodaj do AudioSource z muzyką albo efektami - głośność będzie brana z Opcji.
    [RequireComponent(typeof(AudioSource))]
    public class VolumeBinding : MonoBehaviour
    {
        public enum Channel { Music, Sfx }

        public Channel channel = Channel.Music;
        [Range(0f, 1f)] public float baseVolume = 1f;

        AudioSource source;

        void Awake() => source = GetComponent<AudioSource>();

        void OnEnable()
        {
            GameSettings.Changed += Apply;
            Apply();
        }

        void OnDisable() => GameSettings.Changed -= Apply;

        void Apply()
        {
            if (source == null) return;
            source.volume = baseVolume * (channel == Channel.Music ? GameSettings.MusicVolume : GameSettings.SfxVolume);
        }
    }
}
