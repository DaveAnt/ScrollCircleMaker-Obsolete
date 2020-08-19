//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://daveant.gitee.io/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UIPlugs.ScrollCircleMaker.Editor
{
    public sealed class ScrollCircleEditor : EditorWindow
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
            System.Diagnostics.Process.Start("https://daveant.gitee.io/");
        }

        [MenuItem("ScrollCircleMaker/Clear Order/Clear All Makers")]
        private static void ClerAllMakers()
        {
            List<string> baseMakers = TypesObtainer<BaseDirectMaker<dynamic>>.GetScripts();
            List<string> resultFiles = new List<string>();
            GetDirs("Assets/", baseMakers, resultFiles);
            foreach (var fileName in resultFiles)
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
            AssetDatabase.Refresh();
        }
        [MenuItem("ScrollCircleMaker/Clear Order/Clear MultipleRect Maker")]
        private static void ClerMultipleRectMaker()
        {
            List<string> resultFiles = SeekFeatureMaker("MultipleRectCircleHelper");
            foreach (var fileName in resultFiles)
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
            AssetDatabase.Refresh();
        }
        [MenuItem("ScrollCircleMaker/Clear Order/Clear SingleRect Maker")]
        private static void ClerSingleRectMaker()
        {
            List<string> resultFiles = SeekFeatureMaker("SingleRectCircleHelper");
            foreach (var fileName in resultFiles)
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
            AssetDatabase.Refresh();
        }
        [MenuItem("ScrollCircleMaker/Clear Order/Clear CustomRect Maker")]
        private static void ClerCustomRectMaker()
        {
            List<string> resultFiles = SeekFeatureMaker("CustomRectCircleHelper");
            foreach (var fileName in resultFiles)
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
            AssetDatabase.Refresh();
        }

        [MenuItem("GameObject/UI/ScrollCircle View")]
        private static void CrateScrollCircleView()
        {
            GameObject tmpScrollCircleView = Resources.Load<GameObject>("ScrollCircle View");
            GameObject scrollCircleView = GameObject.Instantiate(tmpScrollCircleView);
            scrollCircleView.transform.parent = Selection.activeTransform;
            scrollCircleView.transform.localPosition = Vector3.zero;
            scrollCircleView.name = "ScrollCircle View";
        }

        static string savePath = string.Empty;
        static string saveMakerName = string.Empty;
        static string saveItemName = string.Empty;
        static string saveDataType = string.Empty;
        static int selectHepler = 0;
        static bool itemPostfix = true;
        static bool makerPostfix = true;
        static bool forceGenerate = false;
        static List<string> helperNames = new List<string>();
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
                if (saveMakerName == string.Empty ||
                    saveItemName == string.Empty ||
                    saveDataType == string.Empty ||
                    savePath == string.Empty) {
                    Debug.LogError("please fill in !!!");
                    ShowNotification(new GUIContent("Empty!"));
                    return;
                }
                if (!Directory.Exists(savePath) ||
                    !isLetterMark(saveMakerName[0]) ||
                    !isLetterMark(saveItemName[0]) ||
                    !isLetterMark(saveDataType[0]))
                {
                    Debug.LogError("invalid path or first character is not a letter");
                    ShowNotification(new GUIContent("Invalid!"));
                    return;
                }
                string tmpMakerName = saveMakerName, tmpItemName = saveItemName;
                if (makerPostfix) tmpMakerName += "Maker";
                if (itemPostfix) tmpItemName += "Item";
                string filePath = savePath + tmpMakerName + ".cs";
                try
                {
                    string helperName = helperNames[selectHepler].Substring(26);
                    string tmpTemplate = Resources.Load<TextAsset>("TemplateMaker").ToString();
                    tmpTemplate = string.Format(tmpTemplate, '{', '}', tmpMakerName, saveDataType, helperName, tmpItemName);
                    if (File.Exists(filePath))
                    {
                        if (!forceGenerate)
                        {
                            Debug.LogWarning("maker already exists, you can try to tick force");
                            ShowNotification(new GUIContent("Exist!"));
                            return;
                        }
                        File.Delete(filePath);
                    }
                    using (File.Create(filePath)) { }
                    File.WriteAllText(filePath, tmpTemplate);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    ShowNotification(new GUIContent("Success!"));
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
            forceGenerate = EditorGUILayout.ToggleLeft("Force", forceGenerate);
            EditorGUILayout.EndHorizontal();
        }

        private void OnEnable()
        {
            helperNames.Clear();
            foreach (var helperName in TypesObtainer<BaseCircleHelper<dynamic>>.GetNames())
                helperNames.Add(helperName.Substring(0, helperName.Length - 2));
        }

        private bool isLetterMark(char Symbol)
        {
            return (Symbol >= 'a' && Symbol <= 'z') || (Symbol >= 'A' && Symbol <= 'Z');
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

        public static void GetDirs(string seekPath, List<string> seekFiles, List<string> resultFiles)//得到所有文件夹
        {
            string[] files = Directory.GetFiles(seekPath);//得到文件
            for (int i = seekFiles.Count - 1; i >= 0; --i)
            {
                for (int j = 0; j < files.Length; ++j)
                {
                    if (files[j].Contains(seekFiles[i]))
                    {
                        resultFiles.Add(files[j].Replace("\\", "/"));
                        seekFiles.RemoveAt(i);
                        break;
                    }
                }
            }
            if (seekFiles.Count == 0)
                return;
            try
            {
                string[] dirs = Directory.GetDirectories(seekPath.Replace("\\", "/"));
                foreach (string dir in dirs)
                    GetDirs(dir.Replace("\\", "/"), seekFiles, resultFiles);//递归
            }
            catch
            {

            }
        }

        //移除宏定义
        public static void RemoveScriptingSymbol(string symbol)
        {
            string symbolsString = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup);
            string[] symbols = symbolsString.Split(';');
            symbolsString = "";
            foreach (string s in symbols)
            {
                if (!s.StartsWith(symbol))
                {
                    if (symbolsString.Length > 0)
                        symbolsString += ';';
                    symbolsString += s;
                }
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup, symbolsString);
        }

        //添加宏定义
        public static void AddScriptingSymbol(string symbol)
        {
            if (!IsScriptingSymbolEnabled(symbol))
            {
                string symbolsString = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                        EditorUserBuildSettings.selectedBuildTargetGroup) + ";" + symbol;
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    EditorUserBuildSettings.selectedBuildTargetGroup, symbolsString);
            }
        }

        //判断宏定义是否存在
        public static bool IsScriptingSymbolEnabled(string symbol)
        {
            string symbolsString = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup);
            return symbolsString.Contains(symbol);
        }

        /// <summary>
        /// 寻找特征解决方案
        /// </summary>
        /// <param name="baseHelperIdx">辅助器索引</param>
        /// <returns></returns>
        public static Type[] SeekFeatureMaker(int baseHelperIdx)
        {   
            Assembly assembly = Assembly.GetAssembly(typeof(MakerHandleAttribute));
            Type[] types = assembly.GetExportedTypes();

            Func<Attribute[], bool> featureAttribute = MakerHandleAttributes =>
            {
                foreach (Attribute attribute in MakerHandleAttributes)
                {
                    if (attribute is MakerHandleAttribute)
                    {
                        MakerHandleAttribute makerHandleAttribute = attribute as MakerHandleAttribute;
                        if((int)makerHandleAttribute.makerHandle == baseHelperIdx)
                            return true;
                    }                
                }
                return false;
            };

            Type[] makerType = types.Where(element =>{
                return featureAttribute(Attribute.GetCustomAttributes(element, true));
            }).ToArray();
            return makerType;
        }

        /// <summary>
        /// 寻找特征解决方案
        /// </summary>
        /// <param name="helperName">辅助器名</param>
        /// <returns></returns>
        public static List<string> SeekFeatureMaker(string helperName)
        {
            List<string> baseMakers = TypesObtainer<BaseDirectMaker<dynamic>>.GetScripts();
            List<string> resultFiles = new List<string>();
            GetDirs("Assets/", baseMakers, resultFiles);
            string tmpContent;
            for (int i = resultFiles.Count - 1; i >= 0; --i)
            {
                if (File.Exists(resultFiles[i]))
                {
                    tmpContent = File.ReadAllText(resultFiles[i]);
                    if (!tmpContent.Contains(helperName))
                        resultFiles.RemoveAt(i);
                }
            }
            return resultFiles;
        }
    }
}