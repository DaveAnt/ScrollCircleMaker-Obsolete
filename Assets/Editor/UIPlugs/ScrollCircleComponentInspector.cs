//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://daveant.gitee.io/
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
        private List<string> makerNames = new List<string>();
        private List<string> helperNames = new List<string>();       
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
        private SerializedProperty stepLen;
        private SerializedProperty itemsPos;
        //编辑器运行时显示
        private SerializedProperty visibleItems;
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
                    makerNames.Clear();
                    baseHelperIdx.intValue = helperIndex;               
                    List<string> tmpMakerNames = TypesObtainer<BaseDirectMaker<dynamic>>.GetNames();
                    List<string> resultFiles = ScrollCircleEditor.SeekFeatureMaker(helperNames[baseHelperIdx.intValue].Split('.')[2]);
                    foreach (var filesName in resultFiles)
                    {
                        foreach (var tmpMaker in tmpMakerNames)
                        {
                            if (filesName.Contains(tmpMaker.Split('.')[2]))
                            {
                                makerNames.Add(tmpMaker);
                                break;
                            }
                        }
                    }
                    m_MakerIdx = -1;
                    tmpMakerNames.Clear();
                    resultFiles.Clear();
                }              
                if (baseHelperIdx.intValue == -1)
                    EditorGUILayout.HelpBox("You must set BaseHelper", MessageType.Error);
                int makerIndex = EditorGUILayout.Popup("Scroll Maker", m_MakerIdx, makerNames.ToArray());
                if (makerIndex != m_MakerIdx)
                {
                    m_MakerIdx = makerIndex;
                    scrollMaker.stringValue = makerNames[makerIndex];
                }
                if ( string.IsNullOrEmpty(scrollMaker.stringValue) || m_MakerIdx == -1)
                    EditorGUILayout.HelpBox("You must set ScrollCircleMaker", MessageType.Error);
                EditorGUILayout.PropertyField(scrollDir);
                EditorGUILayout.PropertyField(scrollSort);
                EditorGUILayout.PropertyField(scrollType);
                if (scrollType.enumValueIndex == 1)
                {
                    EditorGUILayout.PropertyField(stepLen);
                    if (stepLen.intValue < 0)
                        stepLen.intValue = 0;
                }
                refreshRatio.floatValue = (EditorGUILayout.IntSlider("Refresh Ratio", (int)(refreshRatio.floatValue*10+1), 1, 3)-1)/10f;
                autoMoveRatio.intValue = EditorGUILayout.IntSlider("AutoMove Ratio", autoMoveRatio.intValue/10,1,10)*10;
                if (helperIndex == 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Spacing", GUILayout.Width(116));
                    Vector2Int tmpSpacing = spacing.vector2IntValue;
                    if (scrollDir.enumValueIndex <= 1)
                        tmpSpacing.y = EditorGUILayout.IntField(spacing.vector2IntValue.y);
                    else
                        tmpSpacing.x = EditorGUILayout.IntField(spacing.vector2IntValue.x);
                    spacing.vector2IntValue = tmpSpacing;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.PropertyField(padding, true);
                    EditorGUILayout.PropertyField(itemsPos, true);
                }                   
                else if(helperIndex == 1)
                {
                    EditorGUILayout.PropertyField(spacing, true);
                    EditorGUILayout.PropertyField(padding, true);
                }
                else if (helperIndex == 2)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Spacing", GUILayout.Width(116));
                    Vector2Int tmpSpacing = spacing.vector2IntValue;
                    if(scrollDir.enumValueIndex <= 1)
                        tmpSpacing.y = EditorGUILayout.IntField(spacing.vector2IntValue.y);
                    else
                        tmpSpacing.x = EditorGUILayout.IntField(spacing.vector2IntValue.x);
                    spacing.vector2IntValue = tmpSpacing;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.PropertyField(padding, true);
                }

                EditorGUILayout.BeginHorizontal();
                isUpdateEnable.boolValue = EditorGUILayout.ToggleLeft("IsUpdateEnable",isUpdateEnable.boolValue, GUILayout.Width(140));
                isCircleEnable.boolValue = EditorGUILayout.ToggleLeft("IsCircleEnable", isCircleEnable.boolValue, GUILayout.Width(140));
                EditorGUILayout.EndHorizontal();
                if (EditorApplication.isPlaying)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("VisibleItems:" + visibleItems.intValue.ToString(), GUILayout.Width(100)) ;
                    EditorGUILayout.LabelField("InitItems:" + initItems.intValue.ToString(), GUILayout.Width(100));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("ItemIdx:" + itemIdx.intValue.ToString(), GUILayout.Width(100));
                    EditorGUILayout.LabelField("DataIdx:" + dataIdx.intValue.ToString(), GUILayout.Width(100));
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUI.EndDisabledGroup();
            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            baseHelperIdx = serializedObject.FindProperty("_baseHelperIdx");
            foreach (var helperName in TypesObtainer<BaseCircleHelper<dynamic>>.GetNames())
                helperNames.Add(helperName.Substring(0, helperName.Length - 2));   
            List<string> tmpMakerNames = TypesObtainer<BaseDirectMaker<dynamic>>.GetNames();
            List<string> resultFiles = ScrollCircleEditor.SeekFeatureMaker(helperNames[baseHelperIdx.intValue].Split('.')[2]);
            foreach (var filesName in resultFiles)
            {
                foreach (var tmpMaker in tmpMakerNames)
                {
                    if (filesName.Contains(tmpMaker.Split('.')[2]))
                    {
                        makerNames.Add(tmpMaker);
                        break;
                    }
                }
            }
            tmpMakerNames.Clear();
            resultFiles.Clear();

            scrollMaker = serializedObject.FindProperty("_scrollMaker");            
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
            stepLen = serializedObject.FindProperty("_stepLen");
            itemsPos = serializedObject.FindProperty("_itemsPos");
            itemIdx = serializedObject.FindProperty("_itemIdx");
            dataIdx = serializedObject.FindProperty("_dataIdx");
            visibleItems = serializedObject.FindProperty("_visibleItems");
            initItems = serializedObject.FindProperty("_initItems");
            m_MakerIdx = makerNames.IndexOf(scrollMaker.stringValue);
        }        
    }
}
