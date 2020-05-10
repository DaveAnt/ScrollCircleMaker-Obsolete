//------------------------------------------------------------
// ScrollCircleMaker
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://dagamestudio.top/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using UnityEditor;
using System.Collections.Generic;

namespace UIPlugs.ScrollCircleMaker.Editor
{
    [CustomEditor(typeof(ScrollCircleComponent))]
    internal sealed class ScrollCircleComponentInspector : UnityEditor.Editor
    {
        private int m_SelectIdx;
        private List<string> makerNames;
        private SerializedProperty scrollMaker;
        private SerializedProperty baseItem;  
        private SerializedProperty scrollType;
        private SerializedProperty scrollDir;
        private SerializedProperty scrollSort;
        private SerializedProperty refreshRatio;
        private SerializedProperty padding;
        private SerializedProperty spacing;
        private SerializedProperty isUpdateEnable;
        private SerializedProperty isCircleEnable;
        private SerializedProperty isSlideEnable;
        private SerializedProperty limitNum;
        //编辑器运行时显示
        private SerializedProperty maxItems;
        private SerializedProperty initItems;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                EditorGUILayout.PropertyField(baseItem);
                if (baseItem.objectReferenceValue == null)
                    EditorGUILayout.HelpBox("You must set BaseItem", MessageType.Error);
                int selectedIndex = EditorGUILayout.Popup("Scroll Maker", m_SelectIdx, makerNames.ToArray());
                if (selectedIndex != m_SelectIdx)
                {
                    m_SelectIdx = selectedIndex;
                    scrollMaker.stringValue =  makerNames[selectedIndex];
                }
                if ( string.IsNullOrEmpty(scrollMaker.stringValue) || m_SelectIdx == -1)
                    EditorGUILayout.HelpBox("You must set ScrollCircleMaker", MessageType.Error);
                EditorGUILayout.PropertyField(scrollDir);
                EditorGUILayout.PropertyField(scrollSort);
                EditorGUILayout.PropertyField(scrollType);
                if (scrollType.enumValueIndex == 1)
                {
                    EditorGUILayout.PropertyField(limitNum);
                    if (limitNum.intValue <= 0)
                        limitNum.intValue = 1;
                }
                refreshRatio.floatValue = EditorGUILayout.IntSlider("Refresh Ratio", (int)(refreshRatio.floatValue*10), 0, 10)/10f;
                EditorGUILayout.PropertyField(padding,true);
                EditorGUILayout.PropertyField(spacing);
                EditorGUILayout.PropertyField(isUpdateEnable);
                EditorGUILayout.PropertyField(isCircleEnable);
                EditorGUILayout.PropertyField(isSlideEnable);
                if (EditorApplication.isPlaying)
                {
                    EditorGUILayout.PropertyField(maxItems);
                    EditorGUILayout.PropertyField(initItems);
                }
            }
            EditorGUI.EndDisabledGroup();
            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            makerNames = TypesObtainer<BaseMaker>.GetNames();
            makerNames.RemoveAt(makerNames.Count-1);
            scrollMaker = serializedObject.FindProperty("_scrollMaker");
            baseItem = serializedObject.FindProperty("_baseItem");         
            scrollDir = serializedObject.FindProperty("_scrollDir");
            scrollType = serializedObject.FindProperty("_scrollType");
            scrollSort = serializedObject.FindProperty("_scrollSort");
            refreshRatio = serializedObject.FindProperty("_refreshRatio");
            padding = serializedObject.FindProperty("_padding");
            spacing = serializedObject.FindProperty("_spacing");

            isUpdateEnable = serializedObject.FindProperty("_isUpdateEnable");
            isCircleEnable = serializedObject.FindProperty("_isCircleEnable");
            isSlideEnable = serializedObject.FindProperty("_isSlideEnable");
            limitNum = serializedObject.FindProperty("_limitNum");
            maxItems = serializedObject.FindProperty("_maxItems");
            initItems = serializedObject.FindProperty("_initItems");
            m_SelectIdx = makerNames.IndexOf(scrollMaker.stringValue);
        }
    }
}
