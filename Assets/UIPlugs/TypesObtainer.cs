//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://daveant.gitee.io/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UIPlugs.ScrollCircleMaker
{
    public static class TypesObtainer<T>
    {
        private static readonly string[] AssemblyNames ={
            "Assembly-CSharp"
        };
        public static List<Type> GetTypes()
        {
            var targetType = typeof(T);
            List<Type> getTypes = new List<Type>();
            foreach (string assemblyName in AssemblyNames)
            {
                var allTypes = Assembly.Load(assemblyName).GetTypes();
                foreach (var tmpType in allTypes)
                {
                    var baseType = tmpType.BaseType;  //获取基类
                    while (baseType != null)  //获取所有基类
                    {
                        if (baseType.Name == targetType.Name)
                        {
                            getTypes.Add(tmpType);
                            break;
                        }
                        else
                        {
                            baseType = baseType.BaseType;
                        }
                    }
                }
            }
            return getTypes;
        }

        public static List<string> GetScripts()
        {
            var targetType = typeof(T);
            List<string> getNames = new List<string>();
            foreach (string assemblyName in AssemblyNames)
            {
                var allTypes = Assembly.Load(assemblyName).GetTypes();
                foreach (var tmpType in allTypes)
                {
                    var baseType = tmpType.BaseType;  //获取基类
                    while (baseType != null)  //获取所有基类
                    {
                        if (baseType.Name == targetType.Name)
                        {
                            getNames.Add(tmpType.FullName.Split('.')[2]+".cs");
                            break;
                        }
                        else
                        {
                            baseType = baseType.BaseType;
                        }
                    }
                }
            }
            return getNames;
        }

        public static List<string> GetNames()
        {
            var targetType = typeof(T);
            List<string> getNames = new List<string>();
            foreach (string assemblyName in AssemblyNames)
            {
                var allTypes = Assembly.Load(assemblyName).GetTypes();
                foreach (var tmpType in allTypes)
                {
                    var baseType = tmpType.BaseType;  //获取基类
                    while (baseType != null)  //获取所有基类
                    {
                        if (baseType.Name == targetType.Name)
                        {
                            getNames.Add(tmpType.FullName);
                            break;
                        }
                        else
                        {
                            baseType = baseType.BaseType;
                        }
                    }
                }
            }
            return getNames;
        }

        public static Type GetTypeByName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                foreach (string assemblyName in AssemblyNames)
                {
                    var allTypes = Assembly.Load(assemblyName).GetTypes();
                    foreach (var tmpType in allTypes)
                    {
                        if (tmpType.FullName == name)
                            return tmpType;
                    }
                }
            }
            Debug.LogError("can't get type by " + name);
            return null;
        }

        public static T CreateInstanceByName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                T _instance;
                foreach (string assemblyName in AssemblyNames)
                {
                    _instance = (T)Assembly.Load(assemblyName).CreateInstance(name);
                    if (_instance != null)
                        return _instance;
                }
            }
            Debug.LogError("can't create instance by " + name);
            return default(T);
        }
    }
}