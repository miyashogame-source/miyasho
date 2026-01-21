using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;

public class MultiComponentCopyPaste : EditorWindow
{
    static List<Component> componentsToCopy = new List<Component>();
    static GameObject sourceGameObject;
    static bool[] componentSelections;
    static bool[] isCopiedComponents;
    static string[] labels;
    Vector2 scrollPos;

    private enum Language { English, Japanese }
    private static Language selectedLanguage = Language.Japanese;

    [MenuItem("Window/Orinal_tools/MultiComponentCopyPaste")]
    public static void ShowWindow()
    {
        MultiComponentCopyPaste window = GetWindow<MultiComponentCopyPaste>("MultiComponentCopyPaste");
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 300, 400);

        if (labels == null)
            SetLanguage(selectedLanguage);
    }

    [MenuItem("CONTEXT/Component/MultiComponentCopy", false, 100)]
    static void ShowComponentSelectionWindow(MenuCommand command)
    {
        Component selectedComponent = command.context as Component;
        
        if (selectedComponent == null)
        {
            Debug.LogError("選択したコンポーネントが無効です。");
            return;
        }
        
        sourceGameObject = selectedComponent.gameObject;
        Component[] allComponents = sourceGameObject.GetComponents<Component>();

        componentsToCopy.Clear();
        componentSelections = new bool[allComponents.Length];
        isCopiedComponents = new bool[allComponents.Length]; 
        
        MultiComponentCopyPaste window = GetWindow<MultiComponentCopyPaste>(labels != null ? labels[0] : "Select Components to Copy");
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 300, 400);
        window.Show();
    }

    [MenuItem("CONTEXT/Component/MultiComponentPaste", true)]
    static bool CanPasteComponents(MenuCommand command)
    {
        return componentsToCopy.Count > 0 && sourceGameObject != null;
    }

    [MenuItem("CONTEXT/Component/MultiComponentPaste", false, 101)]
    static void PasteComponents(MenuCommand command)
    {
        GameObject targetGameObject = ((Component)command.context).gameObject;
        
        foreach (Component component in componentsToCopy)
        {
            if (component == null) continue;

            System.Type componentType = component.GetType();
            Component existingComponent = targetGameObject.GetComponent(componentType);

            if (existingComponent != null)
            {
                ComponentUtility.CopyComponent(component);
                ComponentUtility.PasteComponentValues(existingComponent);
            }
            else
            {
                ComponentUtility.CopyComponent(component);
                ComponentUtility.PasteComponentAsNew(targetGameObject);
            }
        }

        Debug.Log($"Pasted {componentsToCopy.Count} components to {targetGameObject.name}.");
    }

    private void OnGUI()
    {
        if (labels == null)
            SetLanguage(selectedLanguage);

        selectedLanguage = (Language)EditorGUILayout.EnumPopup(labels[6], selectedLanguage);
        if (GUI.changed)
        {
            SetLanguage(selectedLanguage);
        }

        if (sourceGameObject == null)
        {
            EditorGUILayout.HelpBox(labels[1], MessageType.Warning);
            return;
        }

        GUILayout.Label(labels[2], EditorStyles.boldLabel);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        Component[] allComponents = sourceGameObject.GetComponents<Component>();

        for (int i = 0; i < allComponents.Length; i++)
        {
            Component component = allComponents[i];
            if (component.GetType() != typeof(Transform))
            {
                GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
                labelStyle.fontStyle = isCopiedComponents[i] ? FontStyle.Bold : FontStyle.Normal;

                componentSelections[i] = EditorGUILayout.ToggleLeft(component.GetType().Name, componentSelections[i], labelStyle);
            }
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button(labels[3]))
        {
            componentsToCopy.Clear();
            for (int i = 0; i < allComponents.Length; i++)
            {
                if (componentSelections[i])
                {
                    componentsToCopy.Add(allComponents[i]);
                    isCopiedComponents[i] = true;
                }
                else
                {
                    isCopiedComponents[i] = false;
                }
            }
            Debug.Log($"Selected {componentsToCopy.Count} components from {sourceGameObject.name}.");
        }
    }

    private static void SetLanguage(Language language)
    {
        if (language == Language.English)
        {
            labels = new string[]
            {
                "Select Components to Copy",
                "No source GameObject selected.",
                "Select Components to Copy",
                "Copy Selected Components",
                "Paste to Selected GameObjects",
                "Please select target GameObjects in the hierarchy.",
                "Language"
            };
        }
        else if (language == Language.Japanese)
        {
            labels = new string[]
            {
                "コピーするコンポーネントを選択",
                "コピー元のゲームオブジェクトが設定されていません。",
                "コピーするコンポーネントを選択",
                "選択したコンポーネントをコピー",
                "",
                "ヒエラルキーでペースト先のゲームオブジェクトを選択してください。",
                "言語"
            };
        }
    }
}
