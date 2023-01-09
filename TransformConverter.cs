#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

namespace JustSleightly
{
    public class TransformConverter : EditorWindow
    {
        private readonly Dictionary<string, List<EditorCurveBinding>> transformProperties = new Dictionary<string, List<EditorCurveBinding>>();

        private AnimationClip[] animationClips;
        private Vector2 scrollPos = Vector2.zero;

        private bool useParentConstraint;

        [MenuItem("JustSleightly/Transform Converter")]
        private static void ShowWindow()
        {
            TransformConverter window = GetWindow<TransformConverter>();
            GUIContent titleContent = new GUIContent("TransformConverter", EditorGUIUtility.IconContent("Transform Icon").image);
            window.titleContent = titleContent;
        }

        private void OnSelectionChange()
        {
            animationClips = Selection.GetFiltered<AnimationClip>(SelectionMode.Assets);
            FillModel();
            Repaint();
        }

        private void OnGUI()
        {
            if (animationClips == null || animationClips.Length == 0)
            {
                DrawTitle("Please select an Animation Clip");
                return;
            }

            scrollPos = GUILayout.BeginScrollView(scrollPos, GUIStyle.none);

            using (new GUILayout.VerticalScope("helpbox"))
            {
                using (new GUILayout.HorizontalScope())
                {
                    if (animationClips.Length == 1)
                    {
                        animationClips[0] = (AnimationClip)EditorGUILayout.ObjectField("Target Clip:", animationClips[0], typeof(AnimationClip), false);
                    }
                    else
                    {
                        EditorGUIUtility.labelWidth = 95;
                        EditorGUILayout.LabelField("Target Clips:", GUILayout.ExpandWidth(false));
                        EditorGUIUtility.labelWidth = 1;

                        EditorGUILayout.LabelField($"Multiple Anim Clips ({animationClips.Length})");
                        EditorGUIUtility.labelWidth = 0;
                    }
                }
            }

            GUILayout.Space(12);

            using (new GUILayout.VerticalScope("helpbox"))
            {
                using (new EditorGUILayout.HorizontalScope("box"))
                {
                    using (new GUILayout.VerticalScope())
                    {
                        using (new EditorGUI.DisabledScope(transformProperties.Count == 0))
                        {
                            if (GUILayout.Button(new GUIContent("Convert Transform Properties", "Replaces any transform properties with constraint offset properties.")))
                                ConvertTransforms();
                        }
                    }

                    using (new GUILayout.VerticalScope())
                    {
                        if (useParentConstraint)
                        {
                            if (GUILayout.Button(new GUIContent("Using Parent Constraints", "Replaces any Position/Rotation transform properties with Parent constraint offset properties."))) useParentConstraint = false;
                        }
                        else
                        {
                            if (GUILayout.Button(new GUIContent("Using Position/Rotation Constraints", "Replaces any Position/Rotation transform properties with Position/Rotation constraint offset properties."))) useParentConstraint = true;
                        }
                    }
                }
            }

            GUILayout.Space(12);

            using (new GUILayout.VerticalScope("helpbox"))
            {
                var style = new GUIStyle(GUI.skin.label) { richText = true };

                using (new EditorGUILayout.HorizontalScope("box"))
                {
                    GUILayout.Label("<b>Transform Properties</b>", style);

                    GUILayout.FlexibleSpace();

                    GUILayout.Label("<b>Animation Property Path</b>", style);
                }

                if (transformProperties.Count > 0) DisplayPathItems();
            }

            GUILayout.Space(40);
            GUILayout.EndScrollView();
        }

        private void DisplayPathItems()
        {
            foreach (var kvp in transformProperties)
                using (new GUILayout.HorizontalScope("box"))
                {
                    using (new GUILayout.VerticalScope())
                    {
                        var hasPosition = false;
                        var hasRotation = false;
                        var hasScale = false;

                        foreach (var binding in kvp.Value)
                        {
                            if (binding.propertyName.StartsWith("m_LocalPosition.")) hasPosition = true;

                            if (binding.propertyName.StartsWith("localEulerAnglesRaw.")) hasRotation = true;

                            if (binding.propertyName.StartsWith("m_LocalScale.")) hasScale = true;
                        }

                        if (hasPosition)
                            GUILayout.Label("Position", GUILayout.ExpandWidth(false));
                        if (hasRotation)
                            GUILayout.Label("Rotation", GUILayout.ExpandWidth(false));
                        if (hasScale)
                            GUILayout.Label("Scale", GUILayout.ExpandWidth(false));
                    }

                    GUILayout.FlexibleSpace();

                    GUILayout.Label(kvp.Key, GUILayout.ExpandWidth(false));
                }
        }

        private void FillModel()
        {
            transformProperties.Clear();

            foreach (var animationClip in animationClips) FillModelWithCurves(AnimationUtility.GetCurveBindings(animationClip));
        }

        private void FillModelWithCurves(EditorCurveBinding[] curves)
        {
            foreach (var curveData in curves)
            {
                var key = curveData.path;

                if (curveData.propertyName.StartsWith("m_LocalPosition.") || curveData.propertyName.StartsWith("localEulerAnglesRaw.") || curveData.propertyName.StartsWith("m_LocalScale."))
                {
                    if (transformProperties.ContainsKey(key))
                        transformProperties[key].Add(curveData);
                    else
                        transformProperties.Add(key, new List<EditorCurveBinding> { curveData });
                }
            }
        }

        private void ConvertTransforms()
        {
            var propertiesEdited = 0;
            try
            {
                AssetDatabase.StartAssetEditing();
                for (var clipIndex = 0; clipIndex < animationClips.Length; clipIndex++)
                {
                    var animationClip = animationClips[clipIndex];
                    Undo.RecordObject(animationClip, "Convert Transform Properties");

                    var curveBindings = AnimationUtility.GetCurveBindings(animationClip).ToList();

                    foreach (var curve in curveBindings)
                    {
                        var oldBinding = curve;
                        var animationCurve = AnimationUtility.GetEditorCurve(animationClip, oldBinding);
                        EditorCurveBinding newBinding;

                        if (oldBinding.propertyName.StartsWith("m_LocalPosition."))
                        {
                            if (useParentConstraint)
                                newBinding = new EditorCurveBinding { path = curve.path, propertyName = oldBinding.propertyName.Replace("m_LocalPosition.", "m_TranslationOffsets.Array.data[0]."), type = typeof(ParentConstraint) };
                            else
                                newBinding = new EditorCurveBinding { path = curve.path, propertyName = oldBinding.propertyName.Replace("m_LocalPosition.", "m_TranslationOffset."), type = typeof(PositionConstraint) };
                        }
                        else if (oldBinding.propertyName.StartsWith("localEulerAnglesRaw."))
                        {
                            if (useParentConstraint)
                                newBinding = new EditorCurveBinding { path = curve.path, propertyName = oldBinding.propertyName.Replace("localEulerAnglesRaw.", "m_RotationOffsets.Array.data[0]."), type = typeof(ParentConstraint) };
                            else
                                newBinding = new EditorCurveBinding { path = curve.path, propertyName = oldBinding.propertyName.Replace("localEulerAnglesRaw.", "m_RotationOffset."), type = typeof(RotationConstraint) };
                        }
                        else if (oldBinding.propertyName.StartsWith("m_LocalScale."))
                        {
                            newBinding = new EditorCurveBinding { path = curve.path, propertyName = oldBinding.propertyName.Replace("m_LocalScale.", "m_ScaleOffset."), type = typeof(ScaleConstraint) };
                        }
                        else
                        {
                            Debug.Log("Skip");
                            continue;
                        }

                        AnimationUtility.SetEditorCurve(animationClip, oldBinding, null);
                        AnimationUtility.SetEditorCurve(animationClip, newBinding, animationCurve);
                        propertiesEdited++;
                    }

                    DisplayProgress(clipIndex);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
            }

            Debug.Log("<color=yellow>[TransformConverter]</color> Converted " + propertiesEdited + " properties across " + animationClips.Length + "clip" + (animationClips.Length > 1 ? "s" : string.Empty));

            FillModel();
            Repaint();
        }

        #region Automated Methods

        private void OnFocus()
        {
            OnSelectionChange();
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed -= FillModel;
            Undo.undoRedoPerformed += FillModel;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= FillModel;
        }

        #endregion

        #region Helper Methods

        private static void DrawTitle(string title)
        {
            using (new GUILayout.HorizontalScope("in bigtitle"))
            {
                GUILayout.Label(title, new GUIStyle("boldLabel") { alignment = TextAnchor.MiddleCenter });
            }
        }

        private void DisplayProgress(int clipIndex)
        {
            var fChunk = 1f / animationClips.Length;
            var fProgress = fChunk * clipIndex;
            EditorUtility.DisplayProgressBar("Animation Hierarchy Progress", "Editing animations.", fProgress);
        }

        #endregion
    }
}
#endif