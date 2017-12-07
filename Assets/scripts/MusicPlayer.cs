using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class MusicPlayer : MonoBehaviour
{
    public AudioSource source;
    //public KeyHandler keyHandler;
    public List<Song> songs = new List<Song>();

    public AudioImporter importer;

    [Range(0f, 1f)]
    public float volume = 0.5f;
    public bool playing = false;
    public bool displayed = false;
    
    private FileObject[] soundFiles;
    private List<string> validExtensions = new List<string> { ".ogg", ".wav" , ".mp3" };
    private string absolutePath = "./";

    public int lastSongIndex;
    public bool doneLoading = false;

    void Start()
    {
        importer.Loaded += OnLoaded;
        if (Application.isEditor)
            absolutePath = "songs/";
        else if (Application.platform == RuntimePlatform.WindowsPlayer)
            absolutePath = Application.dataPath + "/../songs/";
        else
            absolutePath = Application.persistentDataPath + "/songs/";
        if (source == null) source = gameObject.AddComponent<AudioSource>();
        source.volume = 0.5f;
        LoadSongs();
    }

    void Update()
    {
        //show qy how to start playing a song
        if (!playing && doneLoading)
            PlaySong(0);
        else if (playing && doneLoading) {
            doneLoading = false;
            initAudio();
        }
    }

    public static void setPlaybackTime(float time)
    {
        MusicPlayer mp = (MusicPlayer)GameObject.Find("MusicPlayer").GetComponent(typeof(MusicPlayer));
        mp.GetComponent<AudioSource>().time = time;
    }
    
    static IEnumerator WaitForRealSeconds(float time)
    {
        float start = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup < start + time)
        {
            yield return null;
        }
    }

    public void PlaySong(int index)
    {
        playing = true;
        doneLoading = false;
        loadSong(songs[index].path);
    }

    public void initAudio()
    {
        source.Play();
    }

    public void pauseAudio()
    {
        source.Pause();
    }

    public void resumeAudio()
    {
        source.UnPause();
    }

    public void stopPlaying()
    {
        source.Stop();
        playing = false;
    }

    void LoadSongs()
    {
        songs.Clear();

        // get all valid files
        FileObject[] files = FileHandler.getFiles(absolutePath);
        if (files != null)
        {
            for (int i = 0; i < files.Length; i++)
            {
                if (!IsValidFileType(files[i].name))
                    files[i] = null;
            }


            // and load them
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i] != null)
                {
                    LoadFile(files[i].name);
                }
            }

            doneLoading = true;
        }
        Debug.Log("Loaded: " + songs.Count() + " songs");
    }

    bool IsValidFileType(string fileName)
    {
        return validExtensions.Contains(Path.GetExtension(fileName));
    }

    public class WWWSongWrapper
    {
        WWW www;
        string path;

        public WWWSongWrapper(WWW www, string path)
        {
            this.www = www;
            this.path = path;
        }

        public WWW getWWW() { return this.www; }
        public string getPath() { return this.path; }
    }

    public void loadSong(string path)
    {
        WWW www = new WWW("file://" + path);
        StartCoroutine(loadSongCoroutine(new WWWSongWrapper(www, path)));
    }

    private void OnLoaded(AudioClip clip)
    {
        source.clip = clip;
    }

    public IEnumerator loadSongCoroutine(WWWSongWrapper sw)
    {
        source.clip = null;
        System.GC.Collect();
        yield return sw.getWWW();

        if (sw.getWWW().error == null)
        {
            //if its ogg or wav no problem!
            if (!Path.GetExtension(sw.getPath()).Equals(".mp3"))
            {
                source.clip = sw.getWWW().GetAudioClip(false, true);
                while (!sw.getWWW().isDone)
                    yield return new WaitForEndOfFrame();
            }
            //or else mp3 file use NAudio!
            else
            {
                //NAudio Importer
                importer.ImportStreaming(sw.getPath());
            }
        }
        while (source.clip == null)
            yield return new WaitForEndOfFrame();
        doneLoading = true;
    }

    void LoadFile(string path)
    {
        string SongName = Path.GetFileNameWithoutExtension(path);
        string newSongName = "";
        for (int i = 0; i < SongName.Length; i++)
        {
            if (SongName[i].Equals('_'))
                newSongName += ' ';
            else
                newSongName += SongName[i];
        }
        // Song player system
        //songs.Add(new Song(newSongName, keyHandler.LoadSongKeys(path, Path.GetFileNameWithoutExtension(path), Path.GetFileName(path)), path));
        songs.Add(new Song(newSongName, null, path));
    }

    public class AudioWrapper
    {
        public string songName;
        public float[] floatArr;
        public int channelCount;
        public int floatArrLength;
        public int freq;

        public AudioWrapper(string songName, float[] floatArr, int channelCount, int floatArrLength, int freq)
        {
            this.songName = songName;
            this.floatArr = floatArr;
            this.channelCount = channelCount;
            this.floatArrLength = floatArrLength;
            this.freq = freq;
        }
    }
}