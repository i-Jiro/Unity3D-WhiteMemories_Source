using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DENode : Node
{
    public string GUID;
    public string Title;
    public string Text;
    public bool IsEntryPoint;

    private TextField dialogueTitle;
    public TextField DialogueTitle { get { return dialogueTitle; } set { dialogueTitle = value; } }

    private TextField dialogueText;
    public TextField DialogueText { get { return dialogueText; } set { dialogueText = value; } }

    private Foldout textFoldout;
    private readonly DEGraphView graphView;
    private DEGroup group;
    public DEGroup Group { get { return group; } set { group = value; } }

    private bool generatedChoices;
    private bool generatedAction;
    public bool GeneratedChoices { get { return generatedChoices; } set { generatedChoices = value; } }
    public bool GeneratedAction { get { return generatedAction; } set { generatedAction = value; } }
    private Vector2 generatedChoicePosition;
    private readonly string actionPortName = "Action";
    public DENode(Vector2 position, DEGraphView graphView, DEGroup group)
    {
        this.graphView = graphView;
        this.group = group;

        Text = "New dialogue text...";

        SetPosition(new Rect(position, Vector2.zero));
    }
    #region Initialization
    public void Draw()
    {
        dialogueTitle = new TextField()
        {
            value = Title
        };

        dialogueTitle.RegisterValueChangedCallback(text =>
        {
            Title = text.newValue;
        });
        
        dialogueTitle.style.minWidth = 160;
        dialogueTitle.style.maxWidth = 240;
        extensionContainer.style.backgroundColor = new Color(34f / 255f, 34f / 255f, 34f / 255f);
        titleContainer.Insert(0, dialogueTitle);
       
        textFoldout = new Foldout()
        {
            text = "Dialogue Text"
        };

        dialogueText = new TextField()
        {
            multiline = true,
            value = Text
        };

        dialogueText.style.whiteSpace = WhiteSpace.Normal;
        dialogueText.style.minHeight = 40;

        dialogueText.RegisterValueChangedCallback(text =>
        {
            Text = text.newValue;

            if (Text.Contains(DEEditorWindow.GraphSettings.GenerateChoicesString))
            {
                if (generatedAction)
                {
                    return;
                }

                AddButton("Generate Choices", GenerateChoices);
            }
            else if (Text.Contains(DEEditorWindow.GraphSettings.GenerateActionString))
            {
                if (generatedChoices || generatedAction)
                {
                    return;
                }
                GenerateAction();
            }
            else
            {
                RemoveButton();
            }

        });

        textFoldout.Add(dialogueText);
        extensionContainer.Add(textFoldout);

        AddNodeStyles();
        RefreshExpandedState();
        RefreshPorts();
    }
    #endregion

    #region Styling
    protected void AddNodeStyles()
    {
        titleContainer.style.paddingTop = 5;
        titleContainer.style.paddingBottom = 5;
        titleContainer.style.paddingLeft = 5;
        titleContainer.style.paddingRight = 5;

        extensionContainer.style.paddingTop = 10;
        extensionContainer.style.paddingBottom = 20;
        extensionContainer.style.paddingLeft = 10;
        extensionContainer.style.paddingRight = 10;
    }
    #endregion

    #region Generation Functions
    private void GenerateAction()
    {
        string t = Text;
        string[] tt = t.Split(DEEditorWindow.GraphSettings.GenerateActionString);

        if (tt.Length == 0)
        {
            return;
        }

        DEActionNode node = (DEActionNode)graphView.CreateActionNode(GetPosition().position + new Vector2(480, 200), group);
        group.AddElement(node);
        graphView.AddElement(node);

        Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
        outputPort.portName = actionPortName;
        graphView.LinkNodes(outputPort, (Port)node.inputContainer[0]);

        Button removeButton = new(() =>
        {
            DisconnectDeletedNode(outputPort);
            extensionContainer.Remove(outputPort);
            generatedAction = false;
        })
        {
            text = "X"
        };

        extensionContainer.Add(outputPort);
        outputPort.Insert(2, removeButton);
        generatedAction = true;
        dialogueText.value = tt[0];
    }

    private void GenerateChoices()
    {
        string t = Text;
        string[] tt = t.Split(DEEditorWindow.GraphSettings.GenerateChoicesString);

        if (tt.Length == 0)
        {
            return;
        }

        int existingChoicesCount = extensionContainer.childCount - 1;
        generatedChoicePosition = new Vector2(GetPosition().position.x + 480, GetPosition().position.y + (existingChoicesCount * 200));

        for (int i = 1; i < tt.Length; i++)
        {
            Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            outputPort.portName = tt[i].Trim();

            outputPort.contentContainer.Q<Label>("type").style.display = DisplayStyle.None;

            TextField choiceTextField = new()
            {
                name = string.Empty,
                value = outputPort.portName
            };
            choiceTextField.RegisterValueChangedCallback(evt => outputPort.portName = evt.newValue);

            Button removeButton = new(() =>
            {
                DisconnectDeletedNode(outputPort);
                extensionContainer.Remove(outputPort);
                
                if (extensionContainer.childCount < 2)
                {
                    generatedChoices = false;
                }
            })
            {
                text = "X"
            };

            extensionContainer.Add(outputPort);
            outputPort.Insert(1, choiceTextField);
            outputPort.Insert(2, removeButton);

            var nodePosition = generatedChoicePosition + new Vector2(0, i * 200);

            var node = (DENode)graphView.CreateNode(nodePosition, group);
            graphView.AddElement(node);
            group.AddElement(node);
            graphView.LinkNodes(generatedChoices ?
                (Port)extensionContainer.Children().ToList()[^1] :
                (Port)extensionContainer.Children().ToList()[i], (Port)node.inputContainer[0]);
        }

        textFoldout.Remove(textFoldout.Children().ToList()[1]);

        RefreshExpandedState();
        RefreshPorts();

        dialogueText.value = tt[0];
        generatedChoices = true;
    }
    #endregion

    #region Button 
    private void AddButton(string text, Action method)
    {
        if (textFoldout.childCount < 2)
        {
            Button button = new(method)
            {
                text = text
            };

            textFoldout.Add(button);
        }
    }

    private void RemoveButton()
    {
        if (textFoldout.childCount >= 2)
        {
            textFoldout.Remove(textFoldout.Children().ToList()[1]);
        }
    }
    #endregion

    #region Ports & Edges
    public void DisconnectDeletedNode(Port outputPort)
    {
        var outputEdge = outputPort.connections.First();
        var targetInputPort = outputPort.connections.First().input;
        var targetInputEdge = targetInputPort.connections.First();

        DisconnectEdge(outputEdge);
        DisconnectEdge(targetInputEdge);

        graphView.RemoveElement(targetInputPort.node);
    }

    private void DisconnectEdge(Edge edge)
    {
        edge.input = null;
        edge.output = null;
        edge.RemoveFromHierarchy();
    }

    public override Port InstantiatePort(Orientation orientation, Direction direction, Port.Capacity capacity, Type type)
    {
        Port port = base.InstantiatePort(orientation, direction, capacity, type);
        port.RegisterCallback<MouseUpEvent>(OnPortStartDrag);
        return port;
    }

    protected override void OnPortRemoved(Port port)
    {
        port.UnregisterCallback<MouseUpEvent>(OnPortStartDrag);
        base.OnPortRemoved(port);
    }
    
    private void OnPortStartDrag(MouseUpEvent evt)
    {
        var port = evt.currentTarget as Port;

        if (port.connections.ToList().Count > 0)
        {
            return;
        }

        if (port != null && port.direction == Direction.Output)
        {
            DENode node;
            
            if (port.portName != actionPortName)
            {

                node = (DENode)graphView.CreateNode(graphView.GetGraphMousePosition(evt.mousePosition), group);
            }
            else
            {
                node = (DEActionNode)graphView.CreateActionNode(graphView.GetGraphMousePosition(evt.mousePosition), group);
            }

            graphView.AddElement(node);
            group.AddElement(node);
            graphView.LinkNodes(port, (Port)node.inputContainer[0]);
        }
    }
    #endregion
}