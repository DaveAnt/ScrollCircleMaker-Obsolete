//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://dagamestudio.top/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

namespace UIPlugs.ScrollCircleMaker.Editor
{
    [CustomEditor(typeof(ScrollCircleComponent))]
    internal sealed class ScrollCircleComponentInspector : UnityEditor.Editor
    {
        private int m_MakerIdx;
        private List<string> makerNames;
        private List<string> helperNames;       
        private SerializedProperty baseItem;
        private SerializedProperty baseHelperIdx;
        private SerializedProperty scrollMaker;
        private SerializedProperty scrollType;
        private SerializedProperty scrollDir;
        private SerializedProperty scrollSort;
        private SerializedProperty refreshRatio;
        private SerializedProperty autoMoveRatio;
        private SerializedProperty padding;
        private SerializedProperty spacing;
        private SerializedProperty isUpdateEnable;
        private SerializedProperty isCircleEnable;
        private SerializedProperty isSlideEnable;
        private SerializedProperty limitNum;
        //编辑器运行时显示
        private SerializedProperty maxItems;
        private SerializedProperty initItems;
        private SerializedProperty itemIdx;
        private SerializedProperty dataIdx;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                EditorGUILayout.PropertyField(baseItem);
                if (baseItem.objectReferenceValue == null)
                    EditorGUILayout.HelpBox("You must set BaseItem", MessageType.Error);
                int helperIndex = EditorGUILayout.Popup("Base Helper", baseHelperIdx.intValue, helperNames.ToArray());
                if (helperIndex != baseHelperIdx.intValue)
                {
                    //改变宏开关
                    baseHelperIdx.intValue = helperIndex;
                }
                if (string.IsNullOrEmpty(scrollMaker.stringValue) || m_MakerIdx == -1)
                    EditorGUILayout.HelpBox("You must set BaseHelper", MessageType.Error);
                int makerIndex = EditorGUILayout.Popup("Scroll Maker", m_MakerIdx, makerNames.ToArray());
                if (makerIndex != m_MakerIdx)
                {
                    m_MakerIdx = makerIndex;
                    scrollMaker.stringValue = makerNames[makerIndex];
                }
#if _Scroll_Mu
                if ( string.IsNullOrEmpty(scrollMaker.stringValue) || m_MakerIdx == -1)
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
                refreshRatio.floatValue = (EditorGUILayout.IntSlider("Refresh Ratio", (int)(refreshRatio.floatValue*10+1), 1, 3)-1)/10f;
                autoMoveRatio.intValue = EditorGUILayout.IntSlider("AutoMove Ratio", autoMoveRatio.intValue/10,1,10)*10;
                EditorGUILayout.PropertyField(padding,true);
                EditorGUILayout.PropertyField(spacing);
                EditorGUILayout.PropertyField(isUpdateEnable);
                EditorGUILayout.PropertyField(isCircleEnable);
                EditorGUILayout.PropertyField(isSlideEnable);
                if (EditorApplication.isPlaying)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("MaxItems:"+maxItems.intValue.ToString(), GUILayout.Width(100)) ;
                    EditorGUILayout.LabelField("InitItems:"+initItems.intValue.ToString(), GUILayout.Width(100));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("ItemIdx:" + itemIdx.intValue.ToString(), GUILayout.Width(100));
                    EditorGUILayout.LabelField("DataIdx:" + dataIdx.intValue.ToString(), GUILayout.Width(100));
                    EditorGUILayout.EndHorizontal();
                }
#endif
            }
            EditorGUI.EndDisabledGroup();
            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            makerNames = TypesObtainer<BaseScrollCircleMaker<dynamic>>.GetNames();
            helperNames = TypesObtainer<BaseScrollCircleHelper<dynamic>>.GetNames();
            scrollMaker = serializedObject.FindProperty("_scrollMaker");
            baseHelperIdx = serializedObject.FindProperty("_baseHelperIdx");
            baseItem = serializedObject.FindProperty("_baseItem");         
            scrollDir = serializedObject.FindProperty("_scrollDir");
            scrollType = serializedObject.FindProperty("_scrollType");
            scrollSort = serializedObject.FindProperty("_scrollSort");
            refreshRatio = serializedObject.FindProperty("_refreshRatio");
            autoMoveRatio = serializedObject.FindProperty("_autoMoveRatio");
            padding = serializedObject.FindProperty("_padding");
            spacing = serializedObject.FindProperty("_spacing");

            isUpdateEnable = serializedObject.FindProperty("_isUpdateEnable");
            isCircleEnable = serializedObject.FindProperty("_isCircleEnable");
            isSlideEnable = serializedObject.FindProperty("_isSlideEnable");
            limitNum = serializedObject.FindProperty("_limitNum");
            itemIdx = serializedObject.FindProperty("_itemIdx");
            dataIdx = serializedObject.FindProperty("_dataIdx");
            maxItems = serializedObject.FindProperty("_maxItems");
            initItems = serializedObject.FindProperty("_initItems");
            m_MakerIdx = makerNames.IndexOf(scrollMaker.stringValue);
        }
    }
}
