using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Varollo.SpriteAnimator
{
	public class SpriteAnimator : SpriteAnimatorBase
	{
		[SerializeField] private SpriteRenderer _targetRenderer;
		[SerializeField][Tooltip("Should the first animation play by itself when this script loads?")] private bool _playOnStart;
		[SerializeField][Min(0)] private float _animatorPlaybackSpeed = 1f;
		[SerializeField] private List<SpriteAnimation> _animations = new List<SpriteAnimation>();

		private Vector3 _originalRendererPosition;

        public override float PlaybackSpeed { get => _animatorPlaybackSpeed; set => _animatorPlaybackSpeed = Mathf.Max(0, value); }
		public override bool FlipX { get => ValidateRenderer().flipX; set => ValidateRenderer().flipX = value; }
		public override bool FlipY { get => ValidateRenderer().flipY; set => ValidateRenderer().flipY = value; }
		public override int AnimationCount => _animations.Count;

        public override bool Init()
        {
			if (IsReady) 
				return false;
			
			IsReady = true;
			_originalRendererPosition = ValidateRenderer().transform.localPosition;

            if (_playOnStart && _animations.Count > 0)
				StartPlayback();

			return IsReady;
		}

        public override float UpdateFrame(int animationIndex, ulong frameCounter)
		{
			if (!IsReady)
				return -1;

			var animation = _animations[animationIndex % AnimationCount];
			var frame = animation.GetFrameWrapped(frameCounter);

			_targetRenderer.sprite = frame.Sprite;
			_targetRenderer.transform.localPosition = _originalRendererPosition + animation.GetOffset(frame);

			return animation.YieldFrame(frame, PlaybackSpeed);
		}

		public override void StopPlayback()
		{
			base.StopPlayback();
			_targetRenderer.transform.localPosition = _originalRendererPosition;
		}

		public void AddAnimation(SpriteAnimation animation)
		{
			_animations.Add(animation);
		}

		private SpriteRenderer ValidateRenderer()
		{
			if (_targetRenderer == null && !TryGetComponent(out _targetRenderer))
			{
				_targetRenderer = GetComponentInChildren<SpriteRenderer>(true);
				if (_targetRenderer == null)
					gameObject.AddComponent<SpriteRenderer>();
            }

			return _targetRenderer;
		}

		public override IEnumerator<SpriteAnimation> GetEnumerator()
		{
			return _animations.GetEnumerator();
		}

		public override SpriteAnimation GetAnimation(int animationIndex)
		{
			if (ValidateIndex(animationIndex))
				return _animations[animationIndex];
			else
				return null;
		}

		protected override bool GetAnimationLoop(int animationIndex)
		{
			return ValidateIndex(animationIndex) && _animations[animationIndex].IsLooping;
		}
	}
}