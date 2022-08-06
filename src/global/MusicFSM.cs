using GC = Godot.Collections;
using Godot;
using System;
using System.Collections.Generic;
namespace Game.Audio
{
	public class MusicFSM : AudioStreamPlayer
	{
		public enum MusicType { EXPLORE, TRIGGER }
		private readonly Dictionary<string, AudioStream> library;

		private MusicType musicType = MusicType.EXPLORE;
		private string[] exploreMusic = new string[0];
		private readonly Tween tween = new Tween();
		private readonly Timer timer = new Timer();

		public MusicFSM(Dictionary<string, AudioStream> library) { this.library = library; }
		public override void _Ready()
		{
			AddChild(tween, true);
			tween.Connect("tween_completed", this, nameof(OnTweenCompleted));

			AddChild(timer, true);
			timer.OneShot = true;
			timer.ProcessMode = Timer.TimerProcessMode.Physics;

			Connect("finished", this, nameof(OnMusicFinished));
			Bus = "Music";
		}
		public void SetExploreMusic(string[] exploreMusic)
		{
			this.exploreMusic = exploreMusic != null
				? exploreMusic
				: new string[0];
		}
		public void PlayMusic(string songName, bool play, MusicType musicType)
		{
			if (play)
			{
				if (Stream != null 
				// don't want to transition into the song we're already playing
				&& (Stream.ResourcePath.GetFile().BaseName().Equals(songName)
					&& (!timer.IsStopped() || tween.IsActive())))
				{
					return;
				}

				Globals.TryLinkSignal(timer, "timeout", this, nameof(OnTimerTimeout), false);
				Globals.TryLinkSignal(timer, "timeout", this, nameof(OnTimerTimeout), true,
					new GC.Array() { songName, musicType });

				if (Playing)
				{
					tween.InterpolateProperty(this, "volume_db", VolumeDb, -40.0f, 1.0f);
					tween.Start();
				}
				else
				{
					PlayMusic(songName, musicType);
				}
			}
			else
			{
				PlayExploreMusic();
			}
		}
		private void PlayMusic(string songName, MusicType musicType)
		{
			this.musicType = musicType;
			AudioStream audioStream;
			if (library.TryGetValue(songName, out audioStream))
			{
				Stream = audioStream;
				Play();
			}
		}
		public void PlayExploreMusic()
		{
			if (exploreMusic != null && exploreMusic.Length > 0)
			{
				PlayMusic(exploreMusic[new Random().Next(exploreMusic.Length)], true, MusicType.EXPLORE);
			}
		}
		public void OnMusicFinished()
		{
			switch (musicType)
			{
				case MusicType.EXPLORE:
					PlayExploreMusic();
					break;
				default:
					Play();
					break;
			}
		}
		private void OnTweenCompleted(Godot.Object gObject, NodePath key)
		{
			if (VolumeDb != 0.0f)
			{
				timer.Start(0.5f);
			}
		}
		private void OnTimerTimeout(string songName, MusicType musicType)
		{
			Stop();
			tween.InterpolateProperty(this, "volume_db", VolumeDb, 0.0f, 1.0f);
			tween.Start();
			PlayMusic(songName, musicType);
		}
	}
}