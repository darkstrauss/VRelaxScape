using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AmbientSoundsData : UpdateableData {

    public List<AudioClip> m_ambientMusicList = new List<AudioClip>();

    public List<AudioClip> m_ambientSoundList = new List<AudioClip>();
}
