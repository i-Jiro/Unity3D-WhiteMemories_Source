using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Yarn.Unity;
public class DEActionNode : DENode
{
    public string ActionMethod;
    public GameObject ActionObject { get { return actionObject; } set { actionObject = value; } }
    public string SelectedComponent { get { return selectedComponent; } set { selectedComponent = value; } }
    private GameObject actionObject;
    private string selectedComponent;
    
    private TextField titleText;
    private readonly DEGraphView graphView;
    private readonly DEGroup group;

    public DEActionNode(Vector2 position, DEGraphView graphView, DEGroup group) : base(position, graphView, group)
    {
        this.graphView = graphView;
        this.group = group;

        SetPosition(new Rect(position, Vector2.zero));
    }

    public new void Draw()
    {
        titleText = new TextField()
        {
            value = Title
        };

        titleText.RegisterValueChangedCallback(text =>
        {
            Title = text.newValue;
        });

        titleContainer.Insert(0, titleText);
        extensionContainer.style.backgroundColor = new Color(34f / 255f, 34f / 255f, 34f / 255f);
        var actionFieldContainer = new IMGUIContainer(() =>
        {
            EditorGUILayout.BeginVertical();
            actionObject = EditorGUILayout.ObjectField(actionObject, typeof(GameObject), false) as GameObject;

            if (actionObject != null)
            {
                // Get list of components and convert to string
                var components = actionObject.GetComponents(typeof(Component));
                string[] componentList = new string[components.Length];

                for (int i = 0; i < components.Length; i++)
                {
                    var component = components[i].ToString();
                    Match match = Regex.Match(component, @"\((.*?)\)");
                    if (match.Success)
                    {
                        componentList[i] = match.Groups[1].Value;
                    }
                }

                // Create a component dropdown
                int componentIndex = Array.IndexOf(componentList, selectedComponent);
                componentIndex = EditorGUILayout.Popup("Component", componentIndex, componentList);
                selectedComponent = componentIndex >= 0 ? componentList[componentIndex] : null;

                if (!string.IsNullOrEmpty(selectedComponent))
                {
                    // When component is selected, find all public methods
                    Type objectType = components[componentIndex].GetType();
                    MethodInfo[] methods = objectType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                    List<string> functionNames = new();

                    foreach (MethodInfo method in methods)
                    {
                        if (Attribute.IsDefined(method, typeof(YarnCommandAttribute)))
                        {
                            YarnCommandAttribute yarnCommandAttribute = (YarnCommandAttribute)Attribute.GetCustomAttribute(method, typeof(YarnCommandAttribute));
                            functionNames.Add(yarnCommandAttribute.Name);
                        }
                    }

                    int actionIndex = functionNames.IndexOf(ActionMethod);
                    actionIndex = EditorGUILayout.Popup("Action", actionIndex, functionNames.ToArray());
                    ActionMethod = actionIndex >= 0 ? functionNames[actionIndex] : null;
                    Text = $"<<{ActionMethod}>>";
                }
            }
   

            EditorGUILayout.EndVertical();
        });

        extensionContainer.Add(actionFieldContainer);
        AddNodeStyles();
        RefreshExpandedState();
        RefreshPorts();
    }
}
