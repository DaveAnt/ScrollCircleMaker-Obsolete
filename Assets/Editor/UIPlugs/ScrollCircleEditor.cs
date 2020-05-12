//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://dagamestudio.top/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
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
            window.minSize = window.maxSize = new Vector2(300f, 370f);
            string templateMaker = System.IO.File.ReadAllText(@"Assets\Editor\UIPlugs\TemplateMaker");
        }

        [MenuItem("ScrollCircleMaker/Document")]
        private static void OpenWebSite()
        {
            System.Diagnostics.Process.Start("https://dagamestudio.top/");
        }
    }
}