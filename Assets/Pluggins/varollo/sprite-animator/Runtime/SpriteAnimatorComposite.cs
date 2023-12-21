using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Varollo.SpriteAnimator
{
	public class SpriteAnimatorComposite : SpriteAnimatorBase, IEnumerable<SpriteAnimatorBase>
	{
		private const float FrameLength = 1f / 60f;

		public enum FlipControlType
		{
			SpriteRenderer = 0,
			LocalRotation = 1,
			LocalScale = 2
		}

		[SerializeField][Tooltip("Should we detect child SpriteAnimators automaticly?")]
		private bool _autoDetectChildren;
		[SerializeField][Tooltip("Should we detect inactive children?")]
		private bool _detectInactiveChildren;
		[SerializeField][Tooltip("Should we keep checking for new child SpriteAnimators periodilcly?")]
		private bool _autoUpdateChildren;
		[SerializeField][Min(FrameLength)][Tooltip("How long should we wait to check for new child SpriteAnimators again?")]
		private float _childUpdateDelay = FrameLength;
		[SerializeField][Tooltip("How the flip property controls the animators.\n\nSpriteRendrer: Toggles the sprite flip on the Sprite Renderers.\n\nLocalRotation: Flips the transform by adding 180° to the corresponding axis.\n\nLocalScale: Flips the transform multiplying the localScale by -1 on the corresponding axis.")]
		private FlipControlType _flipControl = FlipControlType.SpriteRenderer;
		[Space][SerializeField]
		private bool _playOnStart = true;
		[SerializeField]
		private List<SpriteAnimatorBase> _childAnimators = new List<SpriteAnimatorBase>();

		public override float PlaybackSpeed { get => base.PlaybackSpeed; set { base.PlaybackSpeed = value; foreach (var child in _childAnimators) child.PlaybackSpeed = value; } }
		public override int AnimationCount => _animationCount;
		public List<SpriteAnimatorBase> ChildAnimators { get => _childAnimators; set { value.Remove(this); _childAnimators = value; } }
		public AnimatorUpdateMode CompositeUpdateMode => _compositeUpdateMode;

		public override bool FlipX
		{
			get => base.FlipX;
			set
			{
				base.FlipX = value;
				UpdateFlip(0);
			}
		}

		public override bool FlipY
		{
			get => base.FlipY;
			set
			{
				base.FlipY = value;
				UpdateFlip(1);
			}
		}

		private int _animationCount;
		private float _childUpdateCounter;
		private AnimatorUpdateMode _compositeUpdateMode = AnimatorUpdateMode.Normal;

		private void Update()
		{
			if(_autoDetectChildren && _autoUpdateChildren && Time.time >= _childUpdateCounter)
			{
				_childUpdateCounter = Time.time + _childUpdateDelay;
				CheckForChildren(_detectInactiveChildren);
				UpdateAnimationConfig();
			}
		}

		private void OnValidate()
		{
			if (_autoDetectChildren)
				CheckForChildren(_detectInactiveChildren);
		}

        public override bool Init()
        {
			if (IsReady) return false;

			if (_autoDetectChildren)
				CheckForChildren(_detectInactiveChildren);

			UpdateAnimationConfig();

			//IsReady = _childAnimators.All(anim => anim.IsReady || anim.Init());

			foreach(var child in _childAnimators)
			{
				IsReady |= child.IsReady || child.Init();
				if(child.IsRunning)
					child.StopPlayback();
			}

            if (_playOnStart)
				PlayAnimation(0);

			return IsReady;
		}

        public override bool PlayAnimation(int animationIndex, ulong frameCount, bool flipX = false, bool flipY = false)
		{
            CurrentAnimationIndex = animationIndex;
			FrameCounter = frameCount;

            FlipX ^= flipX;
			FlipY ^= flipY;

            if (!IsRunning)
                StartPlayback();

            return IsReady;
		}

		public override float UpdateFrame(int animationIndex, ulong frameCounter)
		{
			float longestDuration = -1;

            for (int i = 0; i < _childAnimators.Count; i++)
			{
                SpriteAnimatorBase animator = _childAnimators[i];
				longestDuration = Mathf.Max(
					animator.UpdateFrame(animationIndex, frameCounter),
					longestDuration
				);
			}
			
			return longestDuration;
		}

		protected override bool ValidateIndex(int index)
		{
			return index >= 0; 
		}

        public SpriteAnimation GetAnimation(int animatorIndex, int animationIndex)
        {
            var animator = _childAnimators[animatorIndex];
            return animator.GetAnimation(animationIndex);
        }

        public override SpriteAnimation GetAnimation(int animatorIndex)
		{
			var animator = _childAnimators[animatorIndex];
			return animator.GetAnimation(animator.CurrentAnimationIndex);
		}

		private void UpdateFlip(int axis)
		{
			switch (_flipControl)
			{
				case FlipControlType.LocalRotation:
					Vector3 rot = transform.localEulerAngles;

					if (axis == 0)
						rot.x = FlipY ? 180 : 0;
					else if (axis == 1)
						rot.y = FlipX ? 180 : 0;

					transform.localEulerAngles = rot;
					break;

				case FlipControlType.LocalScale:
					Vector3 scale = transform.localScale;

					if (axis == 0)
						scale.x = Mathf.Abs(scale.x) * (FlipX ? -1 : 1);
					else if (axis == 1)
						scale.y = Mathf.Abs(scale.y) * (FlipY ? -1 : 1);

					transform.localScale = scale;
					break;

				default:
					foreach(var anim in ChildAnimators)
					{
						if(axis == 0)
							anim.FlipX = FlipX;
						else if(axis == 1)
							anim.FlipY = FlipY;
					}
					break;
			}
		}

		private void CheckForChildren(bool includeInactive)
		{
			ChildAnimators = ChildAnimators.Union(GetComponentsInChildren<SpriteAnimatorBase>(includeInactive)).ToList();
		}

		private void UpdateAnimationConfig()
		{
			_animationCount = _childAnimators.Sum(anim => anim.AnimationCount);
			_compositeUpdateMode = _childAnimators.Any(anim => anim.Any(ation => ation.UpdateMode == AnimatorUpdateMode.UnscaledTime)) ? AnimatorUpdateMode.UnscaledTime : AnimatorUpdateMode.Normal;
			_childAnimators.ForEach(anim => anim.enabled = false );
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)ChildAnimators).GetEnumerator();
		}

		IEnumerator<SpriteAnimatorBase> IEnumerable<SpriteAnimatorBase>.GetEnumerator()
		{
			return ChildAnimators.GetEnumerator();
		}

		public override IEnumerator<SpriteAnimation> GetEnumerator()
		{
			var querry = ChildAnimators.Select(anims => anims.AsEnumerable());
			IEnumerable<SpriteAnimation> result = default;

			foreach(var element in querry)
				result.Concat(element);

			return result.GetEnumerator();
		}

		protected override bool GetAnimationLoop(int animationIndex)
		{
            return ChildAnimators.Any(anim => anim.GetAnimation(animationIndex).IsLooping);
        }
	}
}