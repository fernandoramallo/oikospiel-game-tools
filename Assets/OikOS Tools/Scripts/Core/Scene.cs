﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace OikosTools {
	public class Scene : MonoBehaviour {

		public static System.Action OnSceneInitialized;

		public static Scene current { get {
				return _instance;
			}
		}
		private static Scene _instance;

		[Range(0,1)]
		public float volume = 1;

		internal TextBlock dialogBlock;
		internal bool inputEnabled = true;
		internal AudioSource[] sceneSounds;

		internal string name;

		float _fade_timer = -1;
		float _fade_targetVolume;
		float _fade_fadeDuration;
		float[] _fade_previousVolumes;

		// Use this for initialization
		void Awake () {
			_instance = this;

			if (Application.isEditor)
				Initialize();

			dialogBlock = GetComponentInChildren<TextBlock>();

			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}

		void Initialize() {
			if (GameObject.Find("_CORE") == null) { // TODO: find it in a better way
				Instantiate((GameObject)Resources.Load("_CORE"));
			}

			SaveLoad.instance.data.lastScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
			SaveLoad.instance.Save();

			if (Application.isEditor && GameObject.FindObjectOfType<AudioListener>() == null) {
				gameObject.AddComponent<AudioListener>();
				AudioListener.volume = volume;
			}

			// populate scene sounds
			var sounds = new List<AudioSource>();
			foreach(var sound in GameObject.FindObjectsOfType<AudioSource>()) {
				if (sound.transform.root.name != "_SCENE" && sound.transform.root.name != "_PauseMenu" && sound.transform.root.name != "_CORE")
					sounds.Add(sound);
			}
			sceneSounds = sounds.ToArray();
			
		}

		void Start() {
			if (OnSceneInitialized != null)
				OnSceneInitialized.Invoke();
		}

		void Update() {
			if (_fade_timer > 0) {
				_fade_timer -= Time.deltaTime;
				float r = 1 - Mathf.Clamp01(_fade_timer / _fade_fadeDuration);
				for(int i=0; i < sceneSounds.Length; i++) {
					if (sceneSounds[i] != null && sceneSounds[i].isPlaying) {
						sceneSounds[i].volume = Mathf.Lerp(_fade_previousVolumes[i], _fade_targetVolume, r);
					}
				}
			}
		}

		public void FadeAllSounds(float Volume = 0, float Duration = 3) {
			_fade_fadeDuration = _fade_timer = Duration;
			_fade_targetVolume = Volume;
			_fade_previousVolumes = new float[sceneSounds.Length];
			for(int i=0; i < sceneSounds.Length; i++) {
				if (sceneSounds[i] != null) _fade_previousVolumes[i] = sceneSounds[i].volume;
			}
		}
	}
}