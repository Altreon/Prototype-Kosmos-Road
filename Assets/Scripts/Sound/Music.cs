using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Enum
//public enum Track {Intro, Phase1, Phase2, HammerActive};
public enum Track
    {
        Intro = 0,
        Phase1 = 1,
		Phase2 = 2,
		HammerActive = 3
    };

public class Music : MonoBehaviour
{
	
	public float transitionTime = 2f; // Temps de transition entre les musiques
	public float volume = 0.5f; // Volume des musique
	
	//les sources des musiques
	private AudioSource[] audio;
	
	//Buffer pour stocker l'heure à lequelle le changement a été demandé
	private float changeTime;
	
	private Track currentMusicTrack = Track.Intro;
	private Track newMusicTrack = Track.Intro;
	private bool changeInProgress = false;
	
    // Start is called before the first frame update
    void Start()
    {
        audio = GetComponents<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
		int currentMusicID = (int) currentMusicTrack;
		
		if(changeInProgress){
			int newMusicID = (int) newMusicTrack; 
			
			float pastTime = Time.time - changeTime;
			if(pastTime >= transitionTime){
				audio[currentMusicID].Stop();
				audio[currentMusicID].volume = 0f;
				audio[newMusicID].volume = volume;
				changeInProgress = false;
				currentMusicTrack = newMusicTrack;
				return;
			}
			
			float ratio = pastTime / transitionTime;
			audio[newMusicID].volume = volume * ratio;
			audio[currentMusicID].volume = volume * (1 - ratio);
			
			return;
		}
				
		if(currentMusicTrack == Track.Intro && !audio[currentMusicID].isPlaying){
			int newMusicID = currentMusicID + 1; 
			
			if(newMusicID == audio.Length){
				newMusicID = 0;
			}
			
			audio[currentMusicID].volume = volume;
			audio[newMusicID].volume = volume;
			changeInProgress = false;
			currentMusicTrack = (Track) newMusicID;
			audio[newMusicID].Play();
			return;
		}
    }
	
	//Change la musique jouée par celle à l'indice stat (retourne false si demande rejeté)
	public bool changeMusicTrack (Track track) {
		//Si une musique est déjà en transition, la demande de changement est rejeté 
		if(changeInProgress || currentMusicTrack == track){
				return false;
		}
		
		int IDTrack = (int) track;
		Debug.Log(IDTrack);
		
		audio[IDTrack].Play();
		changeInProgress = true;
		newMusicTrack = track;
		changeTime = Time.time;
		
		Debug.Log("Transition vers la musique " + track);
		
		return true;
	}
	
	public Track CurrentMusicTrack () {
		return currentMusicTrack;
	}
}
