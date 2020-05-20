//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://dagamestudio.top/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using UnityEditor;
using UnityEngine;

namespace UIPlugs.ScrollCircleMaker.Editor
{
    public sealed class ScrollCircleEditor:EditorWindow
    {
        [MenuItem("ScrollCircleMaker/Generate Maker")]
        private static void GenerateMaker()
        {            
            ScrollCircleEditor window = GetWindow<ScrollCircleEditor>("ScrollCircle Maker", true);
            window.minSize = window.maxSize = new Vector2(300f, 180f);            
        }

        [MenuItem("ScrollCircleMaker/Document")]
        private static void OpenWebSite()
        {
            System.Diagnostics.Process.Start("https://dagamestudio.top/");
        }

        [MenuItem("ScrollCircleMaker/Clear Order/Clear All Makers")]
        private static void ClerAllMakers()
        {
            
        }
        [MenuItem("ScrollCircleMaker/Clear Order/Clear MultipleRect Maker")]
        private static void ClerMultipleRectMaker()
        {
            
        }
        [MenuItem("ScrollCircleMaker/Clear Order/Clear SingleRect Maker")]
        private static void ClerSingleRectMaker()
        {
            
        }
        [MenuItem("ScrollCircleMaker/Clear Order/Clear CustomRect Maker")]
        private static void ClerCustomRectMaker()
        {
            
        }

        static string savePath = string.Empty;
        static string saveMakerName = string.Empty;
        static string saveItemName = string.Empty;
        static string saveDataType = string.Empty;
        static int selectHepler = 0;
        static bool itemPostfix = true;
        static bool makerPostfix = true;
        static bool forceGenerate = false;
        static List<string> helperNames;
        private void OnGUI()
        {
            //输入框控件        
            GUIStyle buttonStyle = new GUIStyle();
            buttonStyle.alignment = TextAnchor.MiddleCenter;
            buttonStyle.fontSize = 15;
            buttonStyle.fixedWidth = 15;
            buttonStyle.normal.textColor = Color.white;
            buttonStyle.normal.background = null;

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Maker Name:", GUILayout.Width(80));
            saveMakerName = EditorGUILayout.TextField(string.Empty, saveMakerName, GUILayout.Width(100));
            makerPostfix = EditorGUILayout.ToggleLeft("Maker Postfix", makerPostfix);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Item Name:", GUILayout.Width(80));
            saveItemName = EditorGUILayout.TextField(string.Empty, saveItemName, GUILayout.Width(100));
            itemPostfix = EditorGUILayout.ToggleLeft("Item Postfix", itemPostfix);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Type Name:", GUILayout.Width(80));
            saveDataType = EditorGUILayout.TextField(string.Empty, saveDataType, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Circle Helper:", GUILayout.Width(80));
            selectHepler = EditorGUILayout.Popup(selectHepler, helperNames.ToArray());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Generate Path:", GUILayout.Width(90));
            savePath = EditorGUILayout.TextField(string.Empty, savePath, GUILayout.Width(176));
            if (GUILayout.Button("⊙", buttonStyle))
                savePath = OpenFolder();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate", GUILayout.Width(240)))
            {
                if (saveMakerName == string.Empty)
                {
                    this.ShowNotification(new GUIContent("MakerName Empty!"));
                    return;
                }

                if (saveItemName == string.Empty)
                {
                    this.ShowNotification(new GUIContent("ItemName Empty!"));
                    return;
                }

                if (saveDataType == string.Empty)
                {
                    this.ShowNotification(new GUIContent("TypeName Empty!"));
                    return;
                }

                if (savePath == string.Empty)
                {
                    this.ShowNotification(new GUIContent("SavePath Empty!"));
                    return;
                }

                string templateMaker = File.ReadAllText(@"Assets\Editor\UIPlugs\TemplateMaker");
                string helperName = helperNames[selectHepler].Substring(26);
                Debug.LogError(helperName);
                templateMaker = string.Format(templateMaker, saveMakerName+"Maker", saveDataType,helperNames[selectHepler],saveItemName+"Item");
                File.WriteAllText(savePath,templateMaker);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                this.ShowNotification(new GUIContent("Success!"));
            }
            forceGenerate = EditorGUILayout.ToggleLeft("Force", forceGenerate);
            EditorGUILayout.EndHorizontal();
        }

        private void OnEnable()
        {
            helperNames = TypesObtainer<BaseScrollCircleHelper<dynamic>>.GetNames();
        }

        private string OpenFolder()
        {
            string tmpPath = string.Empty;
            tmpPath = EditorUtility.OpenFolderPanel("Resource Folder", "Assets", string.Empty);
            if (!string.IsNullOrEmpty(tmpPath))
            {
                var gamePath = Path.GetFullPath(".");//TODO - FileUtil.GetProjectRelativePath??
                gamePath = gamePath.Replace("\\", "/");
                if (tmpPath.Length > gamePath.Length && tmpPath.StartsWith(gamePath))
                    tmpPath = tmpPath.Remove(0, gamePath.Length + 1);
            }
            return tmpPath + '/';
        }

    }
}