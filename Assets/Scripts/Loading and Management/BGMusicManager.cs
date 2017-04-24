using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMusicManager : MonoBehaviour {
    public static BGMusicManager inst { get; private set; }

    public AudioClip[] musicOptions;
    private int currMusicOption;

    [SerializeField]
    private AudioClip _targetMusic;
    public AudioClip TargetMusic
    {
        get { return _targetMusic; }
        set
        {
            _targetMusic = value;
            audioSource.loop = false;
        }
    }

    public AudioSource audioSource;

    void Start ()
    {
        inst = this;
    }
	
	// Update is called once per frame
	void Update () {
        if (currMusicOption < BossBase.highestKilled && currMusicOption < musicOptions.Length - 1)
        {
            currMusicOption++;
            TargetMusic = musicOptions[currMusicOption];
        }

        if (!audioSource.isPlaying) {
            audioSource.clip = _targetMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
	}
}
