using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Varollo.SpriteAnimator
{
    public abstract class SpriteAnimatorBase : MonoBehaviour, IEnumerable<SpriteAnimation>
	{
		public enum PlaybackState
		{
			Stoped = 0,
			Paused = 1,
			Running = 2
		}

		public virtual bool IsReady { get; protected set; } = false;
		public virtual float PlaybackSpeed { get; set; } = 1f;
		public virtual PlaybackState AnimatorPlaybackState => IsRunning ? PlaybackState.Running : CurrentAnimationIndex >= 0 ? PlaybackState.Paused : PlaybackState.Stoped;
		protected Coroutine RunningCoroutine { get; set; }
		public virtual int CurrentAnimationIndex { get; protected set; } = -1;
		public virtual bool FlipX { get; set; }
		public virtual bool FlipY { get; set; }
		public ulong FrameCounter { get; set; }
		public bool IsRunning => RunningCoroutine != null;

		public abstract int AnimationCount { get; }

        protected virtual void Start()
        {
			IsReady = Init();
        }

        public virtual bool PlayAnimation(int animationIndex, ulong frameCounter, bool flipX = false, bool flipY = false)
		{
			FlipX ^= flipX;
			FlipY ^= flipY;

			if(!ValidateIndex(animationIndex))
			{
				StopPlayback();
				Debug.LogError($"[ SPRITE ANIMATOR ERROR!!! ] Animation of index \"{animationIndex}\" not present in animator \"{name}\".");
				return false;
			}

			CurrentAnimationIndex = animationIndex;
			FrameCounter = frameCounter;
			if(!IsRunning)
				StartPlayback();

			return true;
		}

		public void PlayAnimation(int animationIndex) => PlayAnimation(animationIndex, FrameCounter);

		public bool PlayAnimation(int animationIndex, bool flipX = false, bool flipY = false)
		{
			return PlayAnimation(animationIndex, FrameCounter, flipX, flipY);
		}

		protected virtual IEnumerator PlaybackCo()
		{
			do
			{
				yield return new WaitForSecondsRealtime(UpdateFrame(CurrentAnimationIndex, FrameCounter++));
			} while (IsRunning);
		}

		public virtual void StartPlayback()
		{
			if (IsRunning || !IsReady || !isActiveAndEnabled) return;

			CurrentAnimationIndex = Mathf.Max(0, CurrentAnimationIndex);
			RunningCoroutine = StartCoroutine(PlaybackCo());
		}

		public virtual void PausePlayback()
		{
			if (!IsRunning) return;

			StopCoroutine(RunningCoroutine);
		}

		public virtual void StopPlayback()
		{
			if (!IsRunning) return;

			StopCoroutine(RunningCoroutine);

			RunningCoroutine = null;
			CurrentAnimationIndex = -1;
			FrameCounter = 0;
		}

		protected virtual bool ValidateIndex(int index)
		{
			return index >= 0 && index < AnimationCount;
		}

		public abstract bool Init();
		public abstract SpriteAnimation GetAnimation(int animationIndex);
		public abstract float UpdateFrame(int animationIndex, ulong frameCounter);
		protected abstract bool GetAnimationLoop(int animationIndex);
		public abstract IEnumerator<SpriteAnimation> GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	}
}