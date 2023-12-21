using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Varollo.SpriteAnimator
{
	[CreateAssetMenu(fileName = "New Sprite Animation", menuName = "Animation/Sprite Animation")]
	public class SpriteAnimation : ScriptableObject, IEnumerable<SpriteAnimation.Frame>
	{
		[SerializeField] private bool _loop;
		[SerializeField] private AnimatorUpdateMode _updateMode;
		[SerializeField] [Min(0)] private float _playbackSpeed = 1f;
		[SerializeField] private Frame[] _frames;
		[SerializeField] private Vector3 _offsetPosition;

        public int FrameCount => _frames.Length;
		public AnimatorUpdateMode UpdateMode => _updateMode;
		public bool IsLooping => _loop;

		public float PlaybackSpeed { get => _playbackSpeed; set => _playbackSpeed = Mathf.Max(0, value); }
		public Vector3 OffsetPosition => _offsetPosition;

		public float Duration => _frames.Sum(frame => frame.Duration) * PlaybackSpeed;

        public void SetFrames(Sprite[] sprites, float fixedDuration = 0.1f)
		{
			_frames = new Frame[sprites.Length];

			for(int i = 0; i < sprites.Length; i++)
			{
				_frames[i] = new Frame { Sprite = sprites[i], Duration = fixedDuration };
			}
		}

		public Frame GetFrame(int index)
		{
			return _frames[index];
		}

		public Frame GetFrameWrapped(ulong index)
		{
			return _frames[(int)(IsLooping ? (index % (ulong)FrameCount) : Mathf.Min(index, FrameCount - 1))];
		}

		public void SetFrames(IEnumerable<Frame> frames)
		{
			_frames = (Frame[])frames;
		}

		public Vector3 GetOffsetWrapped(ulong frameIndex)
		{
			return _frames[frameIndex % (ulong)_frames.Length].OffsetPosition + OffsetPosition;
		}

		public Vector3 GetOffset(int frameIndex)
		{
			return _frames[frameIndex].OffsetPosition + OffsetPosition;
		}

		public Vector3 GetOffset(Frame frame)
		{
			return frame.OffsetPosition + OffsetPosition;
		}

		public float YieldFrame(Frame frame, float speedScale = 1)
		{
			return frame.Yield(this, 1 / (speedScale * PlaybackSpeed));
		}

		public float YieldFrame(int frameIndex, float speedScale = 1)
		{
			return YieldFrame(GetFrame(frameIndex), speedScale);
		}

		public IEnumerator<Frame> GetEnumerator()
		{
			return ((IEnumerable<Frame>)_frames).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _frames.GetEnumerator();
		}

		[Serializable]
		public struct Frame
		{
			public Sprite Sprite;
			public float Duration;
			public Vector3 OffsetPosition;

			public float Yield(SpriteAnimation contextAnimation, float scale = 1)
			{
				switch (contextAnimation.UpdateMode)
				{
					case AnimatorUpdateMode.UnscaledTime:
						return Duration * scale;

					default:
						return Duration * scale * Time.timeScale;
				}
			}
		}
	}
}