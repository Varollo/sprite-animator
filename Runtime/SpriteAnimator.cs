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

		public override float AnimatorPlaybackSpeed { get => _animatorPlaybackSpeed; set => _animatorPlaybackSpeed = Mathf.Max(0, value); }
		public override bool FlipX { get => ValidateRenderer().flipX; set => ValidateRenderer().flipX = value; }
		public override bool FlipY { get => ValidateRenderer().flipY; set => ValidateRenderer().flipY = value; }
		public override int AnimationCount => _animations.Count;

		private Vector3 _originalRendererPosition;

		private void Start()
		{
			ValidateRenderer();
			_originalRendererPosition = _targetRenderer.transform.localPosition;

			if (_playOnStart && _animations.Count > 0)
				StartPlayback();
		}

		public override object UpdateFrame(int animationIndex, ulong frameCounter)
		{
			var animation = _animations[animationIndex];
			var frame = animation.GetFrameWrapped(frameCounter);

			_targetRenderer.sprite = frame.Sprite;
			_targetRenderer.transform.localPosition = animation.GetOffset(frame);

			return animation.YieldFrame(frame, AnimatorPlaybackSpeed);
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
				_targetRenderer = GetComponentInChildren<SpriteRenderer>(true) ?? gameObject.AddComponent<SpriteRenderer>();

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
	}
}