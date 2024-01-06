using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DEEditorWindow : EditorWindow
{
    private string fileName;
    private DEGraphView graphView;
    public DEGraphView DialogueGraphView => graphView;
    private TextField fileNameTextField;
    public TextField FileTextField { get {  return fileNameTextField; } set { fileNameTextField = value; } }
    private DEGraphSaveUtility saveUtility;
    public CurrentGraphPathSO CurrentGraph;
    public static DEGraphSettingsSO GraphSettings;

    [MenuItem("Tools/Dialogue Editor")]
    public static void OpenDialogueEditorWindow()
    {
        DEEditorWindow window = GetWindow<DEEditorWindow>("Dialogue Editor");
        window.saveChangesMessage = "This window has unsaved changes. Would you like to save?";
    }

    #region Window Initialization & Overrides
    private void OnEnable()
    {
        fileName = $"New_Dialogue_ID_{Guid.NewGuid().ToString()[..5]}";
        CreateGraphView();
        CreateToolbar();

        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        rootVisualElement.RegisterCallback<KeyDownEvent>(OnKeyDown);
        saveUtility = new(graphView, this);
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(graphView);
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        rootVisualElement.UnregisterCallback<KeyDownEvent>(OnKeyDown);
    }

    private void OnGUI()
    {
        using (new EditorGUI.DisabledScope(!hasUnsavedChanges))
        {
            if (GUILayout.Button("Save"))
            { 
                SaveChanges();
            }

            if (GUILayout.Button("Discard"))
            { 
                DiscardChanges();
            }
        }
    }

    private void OnKeyDown(KeyDownEvent e)
    {
        if (graphView == null)
        {
            return;
        }

        if (e.keyCode == KeyCode.S && e.ctrlKey)
        {
            SaveChanges();
            SetSavedChanges();
            e.StopPropagation();
        }
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            if (!hasUnsavedChanges)
            {
                return;
            }

            SaveGraphChanges();

        }
        else if (state == PlayModeStateChange.EnteredPlayMode)
        {
            saveUtility.LoadGraph(CurrentGraph.Path);
            SetSavedChanges();
        }
    }
    #endregion

    #region Graph Initializtion
    private void CreateGraphView()
    {
        if (GraphSettings == null)
        {
            GraphSettings = AssetDatabase.LoadAssetAtPath<DEGraphSettingsSO>("Assets/Scripts/DialogueEditor/Settings/DEGraphSettings.asset");
        }
        graphView = new DEGraphView(this) { name = "Dialogue Editor" };
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }

    private void CreateToolbar()
    {
        Toolbar toolbar = new();

        fileNameTextField = new TextField
        {
            label = "Dialogue Group Name",
            value = fileName,
            tooltip = "New folder dialogue will be saved to"
        };

        fileNameTextField.RegisterValueChangedCallback(text => fileName = text.newValue);

        Button saveButton = new(() => RequestSaveOrLoad(true))
        {
            text = "Save",
            tooltip = "Save graph to scriptable object"
        };


        Button loadButton = new(() => RequestSaveOrLoad(false))
        {
            text = "Load",
            tooltip = "Load graph from scriptable object"
        };


        Button exportButton = new(() => RequestSaveOrLoad(true, true))
        {
            text = "Export",
            tooltip = "Export dialogue to Yarn Files"
        };

        Button newButton = new(() => CreateNewGraph())
        {
            text = "New",
            tooltip = "Start a new graph"
        };

        Button settingsButton = new(() => DEGraphSettingsWindow.Open())
        {
            text = "Settings",
            tooltip = "Edit graph properties"
        };

        VisualElement separator = CreateSeparator(1, Color.gray);
        VisualElement space = new();

        space.style.flexGrow = 1;
        newButton.style.marginRight = 10;
        loadButton.style.marginLeft = 10;
        toolbar.style.height = 25;

        toolbar.Add(fileNameTextField);
        toolbar.Add(saveButton);
        toolbar.Add(newButton);
        toolbar.Add(separator);
        toolbar.Add(loadButton);
        toolbar.Add(exportButton);
        toolbar.Add(space);
        toolbar.Add(settingsButton);
        rootVisualElement.Add(toolbar);
    }
    #endregion

    #region Graph Save, Load & New Logic
    private void RequestSaveOrLoad(bool save, bool exportToYarn = false)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            EditorUtility.DisplayDialog("Invalid File Name", "The file name you typed in is invalid", "OK");
            return;
        }

        if (save)
        {
            saveUtility.SaveGraph(fileName, exportToYarn);
            SetSavedChanges();
        }
        else
        {
            string path = EditorUtility.OpenFilePanel("Open Dialogue File", "", "asset");
            saveUtility.LoadGraph(path);
            SetSavedChanges();
        }
    }

    public void SetUnsavedChanges()
    {
        if (hasUnsavedChanges)
        {
            return;
        }

        hasUnsavedChanges = true;
    }

    public void SetSavedChanges()
    {
        if (!hasUnsavedChanges)
        {
            return;
        }

        hasUnsavedChanges = false;
    }

    private void SaveGraphChanges()
    {
        if (!hasUnsavedChanges)
        {
            return;
        }

        bool save = EditorUtility.DisplayDialog(
                "Unsaved Changes",
                "You have unsaved changes. Do you want to save them?",
                "Save", "Discard");

        if (save)
        {
            SaveChanges();
            SetSavedChanges();
        }
        else
        {
            DiscardChanges();
        }
    }

    private void CreateNewGraph()
    {
        foreach (var element in graphView.graphElements.ToList())
        {
            graphView.RemoveElement(element);
        }
        graphView.CreateNewYarnDialogue(new Vector2(300, 280));
        FileTextField.value = $"New_Dialogue_ID_{Guid.NewGuid().ToString()[..5]}";
        fileName = fileNameTextField.value;
    }

    public override void SaveChanges()
    {
        RequestSaveOrLoad(true);
        base.SaveChanges();
    }
    #endregion

    #region Utility
    private VisualElement CreateSeparator(int size, Color colour)
    {
        VisualElement separator = new();
        separator.style.width = size;
        separator.style.backgroundColor = colour;
        return separator;
    }
    #endregion
}
