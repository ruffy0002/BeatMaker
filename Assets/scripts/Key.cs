using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class Key : IComparable<Key>	{
	public int button;
	public float timing;

	public Key(int button, float timing)	{
		this.button = button;
		this.timing = timing;
	}

	public int CompareTo(Key otherKey)	{
		return timing.CompareTo (otherKey.timing);
	}
}
