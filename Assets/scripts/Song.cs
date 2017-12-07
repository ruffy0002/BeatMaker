using UnityEngine;
using System.Collections;

public class Song {
	public string name;
	public SongKeys songKeys;
	public string path;

	public Song(string name, SongKeys songKeys, string path)	{
		this.name = name;
		this.songKeys = songKeys;
		this.path = path;
	}
}
