using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class solvableTest : MonoBehaviour {

	public KMAudio Audio;
	public AudioClip[] sounds;
	public KMBombInfo BombInfo;
	public KMSelectable Orb;
	public KMBombModule Module;

	// Use this for initialization
	void Awake () {
		Orb.OnInteract += delegate () { Module.HandlePass(); return false; };
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
