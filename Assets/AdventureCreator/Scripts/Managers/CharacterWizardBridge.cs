#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEngine;

namespace AC
{
    public static class CharacterWizardBridge
    {
        private const string EditorTypeName = "AC.CharacterWizardWindow, Assembly-CSharp-Editor";
        private static Type editorType;

        public static void Init()
        {
            InvokeEditorMethod(nameof(Init), null);
        }

        public static void InitForNPC(GameObject defaultObject = null)
        {
            InvokeEditorMethod(nameof(InitForNPC), new object[] { defaultObject });
        }

        public static void InitForPlayer(GameObject defaultObject = null, int assignPlayerPrefabID = -1)
        {
            InvokeEditorMethod(nameof(InitForPlayer), new object[] { defaultObject, assignPlayerPrefabID });
        }

        private static void InvokeEditorMethod(string methodName, object[] args)
        {
            editorType ??= Type.GetType(EditorTypeName);
            if (editorType == null)
            {
                return;
            }

            MethodInfo method = editorType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);

            if (method == null)
            {
                return;
            }

            method.Invoke(null, args);
        }
    }
}
#endif
