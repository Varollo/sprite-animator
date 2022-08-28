using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEngine.U2D;
using System.Linq;
using System;
using Varollo.SpriteAnimator;

public namespace Varollo.SpriteAnimator.Editor
{
	[CustomEditor(typeof(SpriteAnimation))]
	public class SpriteAnimationEditor : Editor
	{
		private SerializedProperty _loopProperty, _updateModeProperty, _framesProperty, _playbackSpeedProperty, _offsetPositionProperty;
		private SpriteAnimation _target;

		private Texture _loadedTexture;
		private float _fixedDuration = 0.1f;

		private void OnEnable()
		{
			_loopProperty = serializedObject.FindProperty("_loop");
			_updateModeProperty = serializedObject.FindProperty("_updateMode");
			_framesProperty = serializedObject.FindProperty("_frames");
			_playbackSpeedProperty = serializedObject.FindProperty("_playbackSpeed");
			_offsetPositionProperty = serializedObject.FindProperty("_offsetPosition");

			_target = (SpriteAnimation)target;
		}

		bool _useCustomInspector = true;
		
		public override void OnInspectorGUI()
		{
			_useCustomInspector = EditorGUILayout.Toggle("Use Custom Inspector?:", _useCustomInspector);
			
			if(!_useCustomInspector)
			{
				base.OnInspectorGUI();
				return;
			}
			
			GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
			titleStyle.fontSize = 48;
			titleStyle.wordWrap = true;
			titleStyle.alignment = TextAnchor.MiddleCenter;

			EditorGUILayout.LabelField("Sprite Animation", titleStyle, GUILayout.Height(64));
			EditorGUILayout.LabelField(_target.name, EditorStyles.centeredGreyMiniLabel);

			EditorGUILayout.Space(32);

			EditorGUILayout.LabelField("Animation Settings", EditorStyles.boldLabel);

			Rect fgRect = EditorGUILayout.GetControlRect();
			fgRect.max += Vector2.up * 70f;
			Rect bgRect = fgRect;
			bgRect.max += Vector2.one * 2;
			bgRect.min -= Vector2.one * 2;

			EditorGUI.DrawRect(bgRect, new Color(0, 0, 0, 0.1f));
			EditorGUI.DrawRect(fgRect, new Color(1, 1, 1, 0.075f));

			GUILayout.Space(-15);

			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Space(10);

				#region Left Area
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.BeginVertical();
					{
						EditorGUILayout.LabelField("Update Mode", GUILayout.Width(100));
						EditorGUILayout.LabelField("Loop", GUILayout.Width(100));
						EditorGUILayout.LabelField("Playback Speed", GUILayout.Width(100));
						EditorGUILayout.LabelField("Offset Position", GUILayout.Width(100));
					}
					EditorGUILayout.EndVertical();

					GUILayout.Space(-20);

					EditorGUILayout.BeginVertical();
					{
						_updateModeProperty.enumValueIndex = (int)(AnimatorUpdateMode)EditorGUILayout.EnumPopup(_target.UpdateMode, GUILayout.MaxWidth(175));
						_loopProperty.boolValue = EditorGUILayout.Toggle(_loopProperty.boolValue, GUILayout.MaxWidth(175));
						_playbackSpeedProperty.floatValue = EditorGUILayout.FloatField(_playbackSpeedProperty.floatValue, EditorStyles.numberField, GUILayout.MaxWidth(175));
						_offsetPositionProperty.vector3Value = EditorGUILayout.Vector3Field(GUIContent.none, _offsetPositionProperty.vector3Value, GUILayout.MaxWidth(175));
					}
					EditorGUILayout.EndVertical();

				}
				EditorGUILayout.EndHorizontal();
				#endregion

				GUILayout.Space(10);

				#region Right Area
				EditorGUI.BeginDisabledGroup(_loadedTexture == null);
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.BeginVertical();
					{
						GUILayout.Space(10);

						if (GUILayout.Button("Load Texture", GUILayout.Width(109), GUILayout.Height(32)) && _loadedTexture != null)
						{
							Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(_loadedTexture)).OfType<Sprite>().ToArray();

							if (sprites != null && sprites.Length > 0)
							{
								_target.SetFrames(sprites, _fixedDuration);
								_framesProperty = serializedObject.FindProperty("_frames");

								Selection.SetActiveObjectWithContext(null, null);
							}
							else
							{
								Debug.LogError("[ SPRITE ANIMATION IMPORT ERROR!!! ] Could not retrieve sprites from given texture, remember to configure it as multiple and set the sprites first.");
							}
						}

						GUILayout.FlexibleSpace();

						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("Duration: ", EditorStyles.label, GUILayout.Width(60));
							_fixedDuration = EditorGUILayout.FloatField(_fixedDuration, GUILayout.Width(48));
						}
						EditorGUILayout.EndHorizontal();
					
					GUILayout.Space(10);
					}
					EditorGUILayout.EndVertical();

					EditorGUI.EndDisabledGroup();
					_loadedTexture = (Texture)EditorGUILayout.ObjectField(_loadedTexture, typeof(Texture), false, GUILayout.MaxWidth(64), GUILayout.MaxHeight(64), GUILayout.MinWidth(16), GUILayout.MinHeight(16));

					GUILayout.Space(10);
				}
				EditorGUILayout.EndHorizontal();
				#endregion

			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(30);

			EditorGUILayout.PropertyField(_framesProperty);

			serializedObject.ApplyModifiedProperties();
		}

		[CustomPropertyDrawer(typeof(SpriteAnimation))]
		public class SpriteAnimationPropertyDrawer : PropertyDrawer
		{
			public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
			{
				return 48f;
			}

			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				Rect drawRect = position;
				var labelStyle = EditorStyles.boldLabel;
				labelStyle.alignment = TextAnchor.UpperLeft;

				string[] split = label.text.Split(' ');

				EditorGUI.BeginProperty(position, label, property);

				if (int.TryParse(split[split.Length -1], out int index))
				{
					EditorGUI.LabelField(position, $"Animation #{(index + 1).ToString("00")}: ", labelStyle);
				}
				else
				{
					EditorGUI.LabelField(position, label, labelStyle);
				}

				string animName = (property.objectReferenceValue as SpriteAnimation)?.name ?? "Empty Animation";

				EditorGUI.DrawRect(new Rect(drawRect.x, drawRect.y + 16, drawRect.width, 32), new Color(0, 0, 0, 0.1f));

				drawRect.x += 10;
				drawRect.y += 20;
				drawRect.width = position.width / 2.5f;
				drawRect.height = 24;

				EditorGUI.SelectableLabel(drawRect, animName);

				drawRect.x = drawRect.max.x;
				drawRect.width = position.max.x - drawRect.x - 80;

				property.objectReferenceValue = EditorGUI.ObjectField(drawRect, property.objectReferenceValue, typeof(SpriteAnimation), false);

				drawRect.x = position.max.x - 70;
				drawRect.width = 50;

				EditorGUI.BeginDisabledGroup(animName == "Empty Animation");
				if(GUI.Button(drawRect, "Open"))
				{
					Selection.SetActiveObjectWithContext(property.objectReferenceValue, property.objectReferenceValue);
				}
				EditorGUI.EndDisabledGroup();

				EditorGUI.EndProperty();
			}
		}

		[CustomPropertyDrawer(typeof(SpriteAnimation.Frame))]
		public class FramePropertyDrawer : PropertyDrawer
		{
			public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
			{
				return property.isExpanded ? 124f : 16f;
			}

			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				SerializedProperty _durationProp = property.FindPropertyRelative("Duration");
				SerializedProperty _spriteProp = property.FindPropertyRelative("Sprite");
				SerializedProperty _offsetPositionProp = property.FindPropertyRelative("OffsetPosition");

				EditorGUI.BeginProperty(position, label, property);

				label.text = label.text.Replace("Element", "Frame");
				position.y -= property.isExpanded ? 54f : 0f;

				property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
				
				if (!property.isExpanded) return;

				EditorGUI.indentLevel++;

				Rect drawRect = position;

				drawRect.y += 74f;
				drawRect.x -= 8f;
				drawRect.height -= 32f;
				EditorGUI.DrawRect(drawRect, new Color(0, 0, 0, 0.15f));

				drawRect.y -= 30f;
				EditorGUI.LabelField(drawRect, "Frame duration: (s)", EditorStyles.label);

				drawRect.y += 56f;
				drawRect.width = 128f;
				drawRect.height = 20f;
				_durationProp.floatValue = EditorGUI.FloatField(drawRect, _durationProp.floatValue, EditorStyles.numberField);

				drawRect.y += 20f;
				EditorGUI.LabelField(drawRect, "Offset Position", EditorStyles.label);

				drawRect.y += 20;
				drawRect.width = position.width / 1.75f;
				_offsetPositionProp.vector3Value = EditorGUI.Vector3Field(drawRect, GUIContent.none, _offsetPositionProp.vector3Value);

				drawRect.width = 82f;
				drawRect.height = 66f;
				drawRect.x = position.width - 52f;
				drawRect.y = position.y +82; 
				_spriteProp.objectReferenceValue = (Sprite)EditorGUI.ObjectField(drawRect, _spriteProp.objectReferenceValue, typeof(Sprite), false);

				EditorGUI.indentLevel--;

				EditorGUI.EndProperty();
			}
		}
	}
}