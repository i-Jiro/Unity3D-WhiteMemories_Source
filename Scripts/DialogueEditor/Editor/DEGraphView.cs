using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DEGraphView : GraphView
{
    private readonly DEEditorWindow window;
    public DEGraphView(DEEditorWindow window)
    {
        this.window = window;
        AddStyles();
        AddManipulators();
        AddGraphBG();
        CreateNewYarnDialogue(new Vector2(300, 280));
        AddMiniMap();
        window.SetSavedChanges();
    }


    #region Initialization
    private void AddManipulators()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        this.AddManipulator(CreateYarnDialogueContextualMenu());
    }

    private void AddGraphBG()
    {
        GridBackground gridBackground = new GridBackground();
        gridBackground.StretchToParentSize();
        Insert(0, gridBackground);
    }

    private void AddMiniMap()
    {
        MiniMap miniMap = new()
        {
            anchored = true
        };

        miniMap.SetPosition(new Rect(15, 50, 200, 180));

        Add(miniMap);
    }

    private void AddStyles()
    {
        StyleSheet stylesheet = (StyleSheet)EditorGUIUtility.Load("DialogueEditor/DEGraphViewStyles.uss");
        styleSheets.Add(stylesheet);
    }


    private IManipulator CreateYarnDialogueContextualMenu()
    {
        ContextualMenuManipulator contextualMenuManipulator = new(
            menuEvent => menuEvent.menu.AppendAction("Create New Yarn Dialogue", actionEvent => CreateNewYarnDialogue(viewTransform.matrix.inverse.MultiplyPoint(actionEvent.eventInfo.localMousePosition)))
            );
        return contextualMenuManipulator;
    }

    #endregion

    #region Custom Elements
    public void CreateNewYarnDialogue(Vector2 position)
    {
        var entryGroup = (DEGroup)CreateGroup(position);
        entryGroup.Draw();

        var entryNode = CreateStartNode(position, entryGroup);
        var dialogueNode = (DENode)CreateNode(position + new Vector2(200, 100), entryGroup);

        entryGroup.AddElement(entryNode);
        entryGroup.AddElement(dialogueNode);

        AddElement(entryGroup);
        AddElement(entryNode);
        AddElement(dialogueNode);

        LinkNodes(entryNode.Q<Port>(), (Port)dialogueNode.inputContainer[0]);
    }
    public GraphElement CreateNode(Vector2 position, DEGroup group, string guid = "")
    {
        DENode node = new(position, this, group)
        {
            IsEntryPoint = false,
            Group = group,
            GUID = CheckEmptyGUID(guid)
        };
        node.Title = $"Dialogue_ID_{node.GUID[..5]}";
        node.style.width = 350;
        node.titleContainer.style.backgroundColor = DEEditorWindow.GraphSettings.DialogueNodeTitleColour;

        var inputPort = GenerateInputPort(node, "Prev. Dialogue");
        node.inputContainer.Add(inputPort);

        window.SetUnsavedChanges();

        node.Draw();
        return node;
    }

    public GraphElement CreateStartNode(Vector2 position, DEGroup group, string guid = "")
    {
        DENode node = new(position, this, group)
        {
            title = "Start",
            IsEntryPoint = true,
            Group = group,
            GUID = CheckEmptyGUID(guid)
        };
        node.Title = $"StartNode_{node.GUID[..5]}";
        node.titleContainer.style.backgroundColor = DEEditorWindow.GraphSettings.StartNodeTitleColour;
        
        Port outputPort = GenerateOutputPort(node, "Next Dialogue");
        node.outputContainer.Add(outputPort);
        node.RefreshExpandedState();

        return node;
    }

    public GraphElement CreateActionNode(Vector2 position, DEGroup group, string guid = "")
    {
        DEActionNode node = new(position, this, group)
        {
            GUID = CheckEmptyGUID(guid),
        };
        node.Title = $"Action_ID_{node.GUID[..5]}";
        node.titleContainer.style.backgroundColor = DEEditorWindow.GraphSettings.ActionNodeTitleColour;
        
        Port inputPort = GenerateInputPort(node, "Prev. Dialogue");
        node.inputContainer.Add(inputPort);
        Port outputPort = GenerateOutputPort(node, "Next Dialogue");
        node.outputContainer.Add(outputPort);

        window.SetUnsavedChanges();

        node.Draw();
        return node;
    
    }

    public GraphElement CreateGroup(Vector2 position, string guid = "", string groupTitle = "")
    {
        DEGroup group = new(position)
        {
            GUID = CheckEmptyGUID(guid)
        };

        group.Title = CheckEmptyValue(groupTitle, $"Yarn_ID_{group.GUID[..5]}");
        group.title = group.Title;

        group.Draw();

        return group;
    }
    #endregion

    #region Ports & Edges
    public void LinkNodes(Port output, Port input)
    {
        Edge edge = new()
        {
            input = input,
            output = output
        };

        edge.input.Connect(edge);
        edge.output.Connect(edge);

        Add(edge);
    }

    public Port GenerateOutputPort(DENode node, string portName)
    {
        Port port = GeneratePort(node, Direction.Output);
        port.portName = portName;
        return port;
    }

    private Port GenerateInputPort(DENode node, string portName)
    {
        Port port = GeneratePort(node, Direction.Input);
        port.portName = portName;
        return port;
    }

    public Port GeneratePort(DENode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
    {
        return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(bool));
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        List<Port> compatiblePorts = new();

        ports.ForEach(port =>
        {
            if (startPort == port)
            {
                return;
            }

            if (startPort.node == port.node)
            {
                return;
            }

            if (startPort.direction == port.direction)
            {
                return;
            }

            compatiblePorts.Add(port);
        });

        return compatiblePorts;
    }
    #endregion

    #region Utility

    public string CheckEmptyValue(string value, string defaultValue)
    {
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }
        else
        {
            return value;
        }
    }

    public static string CheckEmptyGUID(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return Guid.NewGuid().ToString();
        }
        else
        {
            return value;
        }
    }

    public DENode ReturnCurrentNode(string currentNodeGUID)
    {
        var nodes = this.nodes.ToList().Cast<DENode>().ToList();
        var currentNode = nodes.Find(x => x.GUID == currentNodeGUID);

        return currentNode;
    }

    public Vector2 GetGraphMousePosition(Vector2 position)
    {
        return viewTransform.matrix.inverse.MultiplyPoint(position);
    }
    #endregion
}