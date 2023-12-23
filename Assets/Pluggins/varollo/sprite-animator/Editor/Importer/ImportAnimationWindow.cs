using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace Varollo.SpriteAnimator.Editor.Importer
{
    [EditorWindowTitle(icon = "sprite", title = "Import Animation")]
    public class ImportAnimationWindow : EditorWindow
    {
        private Texture2D _cachedBlankTexture;

        public static ImportAnimationWindow Instance { get; private set; }

        private Texture2D TargetTexture { get; set; }
        private int RowCount { get; set; }
        private int ColumnCount { get; set; }

        private void OnGUI()
        {
            TargetTexture = EditorGUILayout.ObjectField(ImporterStyles.Labels.TargetTexture, TargetTexture, typeof(Texture2D), false) as Texture2D;

            GUILayout.BeginHorizontal();
            {
                RowCount = Mathf.Max(1, EditorGUILayout.IntField(ImporterStyles.Labels.RowCount, RowCount));
                ColumnCount = Mathf.Max(1, EditorGUILayout.IntField(ImporterStyles.Labels.ColumnCount, ColumnCount));
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Save texture"))
                OnSave();

            DrawPreview(TargetTexture, EditorGUILayout.GetControlRect());
        }

        private void OnSave()
        {
            if (TargetTexture == null)
                return;

            string defaultFileName = $"{TargetTexture.name} animation";
            string path = EditorUtility.SaveFilePanelInProject("Save Sprite Animation", defaultFileName, "asset", "Select the location to save the Sprite Animation");

            if (string.IsNullOrEmpty(path))
                return;

            // Try to load asset, if fails: creates it, if succeds: clears it
            SpriteAnimation anim = AssetDatabase.LoadAssetAtPath<SpriteAnimation>(path);
            if (!anim)
            {
                AssetDatabase.CreateAsset(anim = CreateInstance<SpriteAnimation>(), path);
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
            {
                UnityEngine.Object[] oldSprites = AssetDatabase.LoadAllAssetsAtPath(path);
                for (int i = 0; i < oldSprites.Length; i++)
                {
                    if (oldSprites[i] == anim)
                        continue;

                    AssetDatabase.RemoveObjectFromAsset(oldSprites[i]);
                    DestroyImmediate(oldSprites[i]);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            SpriteAnimation.Frame[] frames = new SpriteAnimation.Frame[RowCount * ColumnCount];
            for (int i = 0; i < frames.Length; i++)
            {
                Sprite sprite = Sprite.Create(
                    TargetTexture, 
                    GetSpriteRect(
                        TargetTexture.width, TargetTexture.height,
                        RowCount, ColumnCount,
                        i % ColumnCount,
                        i / ColumnCount),
                    Vector2.one * .5f
                );
                sprite.name = $"{TargetTexture.name}_{i:00}";

                AssetDatabase.AddObjectToAsset(sprite, anim);

                frames[i] = new()
                {
                    Duration = .1f,
                    Sprite = sprite
                };                
            }

            anim.SetFrames(frames);
            AssetDatabase.SaveAssets();
        }

        private void DrawPreview(Texture2D tex, Rect rect)
        {
            const float rectPadding = 10;
            const float aspect = .75f;

            // Setting preview height to a fraction of window width
            rect.height = rect.width * aspect;

            // adding a padding to the preview
            rect = new(rect.x + rectPadding, rect.y + rectPadding, rect.width - 2 * rectPadding, rect.height - 2 * rectPadding);

            if(tex != null)
            {
                EditorGUI.DrawTextureTransparent(rect, tex, ScaleMode.ScaleAndCrop, 1 / aspect);
                DrawPreviewCut(rect);
            }
            else
            {
                tex = GetBlankTexture((int)Mathf.Abs(rect.width), (int)Mathf.Abs(rect.height));
                EditorGUI.DrawTextureTransparent(rect, tex, ScaleMode.ScaleAndCrop, 1 / aspect);
            }
        }

        private void DrawPreviewCut(Rect previewRect)
        {
            Color c1 = new(.75f, .25f, .5f, .25f);
            Color c2 = new(.75f, .25f, .5f, .1f);

            for (int i = 0; i < ColumnCount; i++)
            {
                for (int j = 0; j < RowCount; j++)
                {
                    Rect cutRect = GetSpriteRect(
                        Mathf.RoundToInt(previewRect.width), Mathf.RoundToInt(previewRect.height),
                        RowCount, ColumnCount,
                        i, j
                    );
                    cutRect.position += previewRect.position;

                    EditorGUI.DrawRect(cutRect, (i * RowCount + j) % 2 == 0 ? c1 : c2);
                }
            }
        }

        private Rect GetSpriteRect(int width, int height, int rows, int cols, int i = 0, int j = 0)
        {
            Vector2Int size = new(width / cols, height / rows);
            Vector2Int pos = new(i * size.x, j * size.y);
            return new(pos, size);
        }

        private Texture2D GetBlankTexture(int width, int height)
        {
            if(_cachedBlankTexture == null)
                _cachedBlankTexture = new(width, height);

            return _cachedBlankTexture;
        }

        [MenuItem("Varollo/Sprite Animator/Import Animation")]
        public static void OpenWindow()
        {
            if (Instance == null)
                Instance = CreateWindow<ImportAnimationWindow>();            
            Instance.Show();
        }
    }
}
