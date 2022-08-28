using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Varollo.SpriteAnimation;

public namespace Varollo.SpriteAnimator.Tests
{	
	public class SpriteAnimatorTests
	{
		[UnityTest]
		public IEnumerator TestGetOrAddSpriteRendererToSpriteAnimator()
		{
			var go = new GameObject("test game object", typeof(SpriteAnimator));
			yield return null;
			Assert.NotNull(go.GetComponent<SpriteRenderer>());
		}

		[UnityTest]
		public IEnumerator TestSpriteDrawnAndUpdateEveryFrame()
		{
			SpriteAnimation animation = ScriptableObject.CreateInstance(typeof(SpriteAnimation)) as SpriteAnimation;
			Assert.NotNull(animation, "Failed to create instance of sprite animation.");

			var sprite1 = Sprite.Create(Texture2D.whiteTexture, Rect.zero, Vector2.zero);
			var sprite2 = Sprite.Create(Texture2D.whiteTexture, Rect.zero, Vector2.zero);

			sprite1.name = "sprite1";
			sprite2.name = "sprite2";

			animation.SetFrames(new Sprite[2] { sprite1, sprite2 });

			var go = new GameObject("test");
			var renderer = go.AddComponent<SpriteRenderer>();
			var animator = go.AddComponent<SpriteAnimator>();
			
			animator.AddAnimation(animation);
			animator.PlayAnimation(0);

			Assert.AreEqual(sprite1, renderer.sprite, $"Sprite \"{renderer.sprite.name}\" not the animator sprite.");
			yield return animation.YieldFrame(0);
			Assert.AreEqual(sprite2, renderer.sprite, $"Sprite \"{renderer.sprite.name}\" not the animator sprite.");
			yield return animation.YieldFrame(1);
		}
	}
}