using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SongKeys {
	public string songName;
	public List<Key> keys;

	public SongKeys(string songName, List<Key> keys)	{
		this.songName = songName;
		this.keys = keys;
	}

	public void addKey(Key newKey)	{
		keys.Add(newKey);
	}
}
