using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AudioLoop : MonoBehaviour {
	public AudioSource source;
	
	[Range(0, 60)] public float firstDelay;
	[Range(0, 60)] public float delayMin;
	[Range(0, 60)] public float delayMax;
	
	[Range(0, 10)] public int loopMin;
	[Range(0, 10)] public int loopMax;
	
	void OnEnable()
	{
		StartCoroutine(DoLoop());	
	}
	
	IEnumerator DoLoop()
	{
		yield return StartCoroutine(PlayClip(firstDelay));
		while (true)
		{
			var delay = delayMin == delayMax ? delayMin : Random.Range(delayMin, delayMax);
			yield return StartCoroutine(PlayClip(delay));
		}
	}
	
	IEnumerator PlayClip(float delay)
	{
		if (delay > 0) yield return new WaitForSeconds(delay);
		
		source.Play();
		var loop = loopMax == 0 ? 0 : Random.Range(loopMin, loopMax);
		source.loop = loop > 0;
		
		yield return new WaitForSeconds(source.clip.length * loop);
		source.loop = false;
		while (source.isPlaying) yield return 0;
	}
}
