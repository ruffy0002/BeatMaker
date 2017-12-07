using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class KeyHandler : MonoBehaviour {
	public MusicPlayer msplayer;
	public int songIndex = -1;
	public bool playing = false;
	public GameObject key;
	public GameObject start;
	public static float scorePerKey = 0f;
	public static float scorePerCombo = 0f;
    public static int num_keys = 0;
    public static int runs = 0;
    public static float leftScore = 0f;
    public static float comboDenominator = 0f;

	private int keyCounter = 0;
	private float songStartTime = 0f;
	
	// Update is called once per frame
	void Update () {
		if (playing) {
			for (int i = keyCounter; i < msplayer.songs[songIndex].songKeys.keys.Count; i++)	{
            if (msplayer.songs[songIndex].songKeys.keys[i].timing + songStartTime - 0.5f < Time.time)
            {
                spawnKey(msplayer.songs[songIndex].songKeys.keys[i].button);
                keyCounter += 1;
            }
            else
                break;
			}
		}
	}

	void spawnStartKey(int button, float desTime)	{

	}

	void spawnKey(int button)	{
        GameObject newKey = Instantiate(key, GameObject.Find("Cube " + button).transform.position, Quaternion.identity);
        newKey.transform.SetParent(GameObject.Find("Cube " + button).transform);
	}

	public SongKeys LoadSongKeys(string path, string name, string namewithextension)	{
		try {
			string contents = FileHandler.readFile(name ,path + ".keys", namewithextension + ".keys");

			SongKeys newSong = new SongKeys (name, new List<Key> ());

			string current = string.Empty;
			int currentInt = 0;
			float currentTiming = 0f;
			bool comments = false;

			for (int i = 0; i < contents.Length; i++) {
				if ((int)contents[i] >= 48 && (int)contents[i] <= 67 || contents[i] == '\n' || contents[i] == ',' || contents[i] == '/' || contents[i] == '.')
					if (contents[i].Equals('/'))
						comments = true;
					else if (comments == false) {
						if (!contents[i].Equals(',') && !contents[i].Equals('\n'))
							current = current + contents[i];
						else if (contents[i].Equals(','))	{
							currentInt = int.Parse(current);
							current = string.Empty;
						}
						else {
							if(!current.Equals(string.Empty))	{
								newSong.addKey(new Key(currentInt, float.Parse(current)));
								currentTiming = float.Parse(current);
								currentInt = 0;
								current = string.Empty;
							}
							else if (currentInt != 0) {
								newSong.addKey(new Key(currentInt, currentTiming));
								currentInt = 0;
								current = string.Empty;
							}
						}
					}
					if (comments)	{
						if (contents[i].Equals('\n'))
							comments = false;
					}
			}
			newSong.keys.Sort();
			return newSong;
		}
		catch(System.Exception)	{
			return null;
		}
	}

	void destroyAllStartKeys()	{
		foreach (var gameObj in FindObjectsOfType(typeof(GameObject)) as GameObject[])
		{
			if(gameObj.name == "start(Clone)")
			{
				Destroy (gameObj);
			}
		}
	}

	public void startSong(int index, float delay)	{
		songIndex = index;
		if (msplayer.songs [index].songKeys != null) {

			songStartTime = Time.time + delay;
			playing = true;
			keyCounter = 0;
		    KeyHandler.num_keys = (msplayer.songs [songIndex].songKeys.keys.Count);
			scorePerKey = 800000f / num_keys;
            
            //combo stuff
            float perc = 0.5f;
		    KeyHandler.runs = (int)((float)num_keys * perc);
		
		    float t_percent = 0f;
		    for (int i = 1; i <=(perc * (float)num_keys); i++)	{
			    t_percent += Mathf.Sin(Mathf.PI * i / (perc * (float)num_keys));
		    }

            KeyHandler.leftScore = (float)(200000f * (t_percent / (t_percent + Mathf.Ceil((1f - (float)perc) * (float)num_keys))));

            KeyHandler.comboDenominator = 0f;
            for (int i = 1; i <= runs; i++)
            {
                KeyHandler.comboDenominator += (Mathf.Sin(((float)i / (float)runs) * (Mathf.PI / 2)));
            }

            scorePerCombo = (200000f - leftScore) / (float)(num_keys - runs);
            /////////////////

			float startTiming = msplayer.songs[songIndex].songKeys.keys[0].timing;
			spawnStartKey(msplayer.songs[songIndex].songKeys.keys[0].button, startTiming);

			for (int i = 1; i < msplayer.songs[songIndex].songKeys.keys.Count; i++)	{
				if (msplayer.songs[songIndex].songKeys.keys[i].timing == startTiming)	{
					spawnStartKey(msplayer.songs[songIndex].songKeys.keys[i].button, startTiming);
				}
				else
					break;
			}
		}
	}

	public void stopSong()	{
		if (msplayer.songs [songIndex].songKeys != null) {
			foreach(GameObject key in GameObject.FindObjectsOfType(typeof(GameObject)))
			{
				if (key.name.Equals("Key(Clone)"))
					Destroy (key);
			}
			destroyAllStartKeys();
			playing = false;
			songIndex = -1;
		}
	}
}
