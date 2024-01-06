using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DEGraphSaveUtility
{
    private DEGraphView graphView;
    private readonly DEEditorWindow window;
    private DialogueContainer dialogueContainerCache;
    private List<Edge> Edges => graphView.edges.ToList();
    private List<DENode> Nodes => graphView.nodes.ToList().Cast<DENode>().ToList();
    private List<DEGroup> Groups => graphView.graphElements.ToList().OfType<DEGroup>().Cast<DEGroup>().ToList();

    public DEGraphSaveUtility(DEGraphView graphview, DEEditorWindow window)
    {
        this.graphView = graphview;
        this.window = window;
    }

    public void SaveGraph(string fileName, bool exportToYarn)
    {
        if (!Edges.Any())
        {
            return;
        }

        var dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();
        var connectedPorts = Edges.Where(x => x.input.node != null || x.output.node != null).ToList();

        // Save connections between nodes
        foreach (var connectedPort in connectedPorts)
        {
            var outputNode = (DENode) connectedPort.output.node;
            var inputNode = (DENode) connectedPort.input.node;

            dialogueContainer.ConnectionData.Add(new DENodeConnectionData()
            {
                BaseNodeGUID = outputNode.GUID,
                BaseNodeTitle = outputNode.Title,
                PortGUID = Guid.NewGuid().ToString(),
                PortName = connectedPort.output.portName,
                TargetNodeTitle = inputNode.Title,
                TargetNodeGUID = inputNode.GUID
            });
        }

        // Save nodes
        foreach (var node in Nodes)
        {
            // Action nodes
            if (node is DEActionNode n)
            {
                DEActionNodeData action = new()
                {
                    GUID = n.GUID,
                    Title = n.Title,
                    Position = n.GetPosition().position,
                    ActionMethod = n.ActionMethod,
                    Group = n.Group,
                    ActionObject = n.ActionObject,
                    SelectedComponent = n.SelectedComponent,
                };
                dialogueContainer.ActionData.Add(action);
                continue;
            }

            // Dialogue nodes
            var item = new DENodeData()
            {
                GUID = node.GUID,
                Title = node.Title,
                Text = node.Text,
                IsEntryPoint = node.IsEntryPoint,
                Position = node.GetPosition().position,
                Group = node.Group
            };

            dialogueContainer.NodeData.Add(item);
        }

        // Save groups
        foreach (var group in Groups)
        {
            var item = new DEGroupData()
            {
                Title = group.Title,
                GUID = group.GUID,
                Position = group.GetPosition().position
            };

            dialogueContainer.GroupData.Add(item);
        }

        CreateFolder("Assets/Resources", "Dialogue");
        CreateFolder("Assets/Resources/Dialogue", fileName);
        CreateFolder($"Assets/Resources/Dialogue/{fileName}", "ScriptableObject");

        AssetDatabase.CreateAsset(dialogueContainer, $"Assets/Resources/Dialogue/{fileName}/ScriptableObject/{fileName}.asset");
        window.CurrentGraph = ScriptableObject.CreateInstance<CurrentGraphPathSO>();
        window.CurrentGraph.Path = $"Assets/Resources/Dialogue/{fileName}/ScriptableObject/{fileName}.asset";
        AssetDatabase.CreateAsset(window.CurrentGraph, $"Assets/Resources/Dialogue/{fileName}/ScriptableObject/{fileName}_Path.asset");
        AssetDatabase.SaveAssets();
        Debug.Log($"Saved Dialogue to Assets/Resources/Dialogue/{fileName}/ScriptableObject/{fileName}.asset");
        if (exportToYarn)
        {
            // Parse Yarn File
            for (int i = 0; i < Groups.Count; i++)
            {
                var nodes = Groups[i].containedElements.ToList().OfType<DENode>();

                string finalText = "";
                string file = $"Assets/Resources/Dialogue/{fileName}/{Groups[i].Title}.yarn";

                foreach (var node in nodes)
                {
                    if (node.IsEntryPoint)
                    {
                        continue;
                    }

                    string header = $"title: {node.Title}{Environment.NewLine}---{Environment.NewLine}";
                    string text = node.Text; 
                    
                    string footer = $"{Environment.NewLine}==={Environment.NewLine}";

                    var connections = dialogueContainer.ConnectionData.Where(x => x.BaseNodeGUID == node.GUID).ToList();

                    for (int j = 0; j < connections.Count; j++)
                    {
                        string choiceName = connections[j].PortName;
                        string targetNodeName = connections[j].TargetNodeTitle;

                        if (choiceName != "Next Dialogue" && choiceName != "Action")
                        {
                            text += $"{Environment.NewLine}->{choiceName}{Environment.NewLine}\t";
                        }
                        else
                        {
                            text += $"{Environment.NewLine}";
                        }
                        
                        text += $"<<jump {targetNodeName}>>";
                    }

                    finalText += (header + text + footer);
                }

                File.WriteAllText(file, finalText);
                Debug.Log($"Exported to Assets/Resources/Dialogue/{fileName}");
                AssetDatabase.Refresh();
            }
        }
    }

    public void LoadGraph(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var globalPath = fileName.Split('.').First();
        var localPath = globalPath.Split(new string[] { "Resources/" }, StringSplitOptions.RemoveEmptyEntries)[1];
        dialogueContainerCache = Resources.Load<DialogueContainer>(localPath);
        window.FileTextField.value = localPath.Split(new string[] { "ScriptableObject/" }, StringSplitOptions.RemoveEmptyEntries)[1].Split(".")[0];
      
        
        if (dialogueContainerCache == null)
        {
            EditorUtility.DisplayDialog("Invalid or Corrupted File", "The file cannot be loaded.", "Ok");
            return;
        }

        ClearGraph();

        var loadedGroups = new Dictionary<string, DEGroup>();
        foreach (var groupData in dialogueContainerCache.GroupData)
        {
            DEGroup group = (DEGroup)graphView.CreateGroup(groupData.Position, groupData.GUID, groupData.Title);
            group.Title = groupData.Title;

            graphView.AddElement(group);
            loadedGroups.Add(group.GUID, group);
        }

        // Load Dialogue node components
        var loadedNodes = new Dictionary<string, DENode>();
        var loadedPorts = new Dictionary<string, Port>();
        var loadedConnections = new Dictionary<Port, Port>();
        
        foreach (var nodeData in dialogueContainerCache.NodeData)
        {
            DEGroup nodeGroup = Groups.Find(x => x == loadedGroups[nodeData.Group.GUID]); // Retrieve the group for the node
            if (nodeData.IsEntryPoint)
            {
                DENode node = (DENode)graphView.CreateStartNode(nodeData.Position, nodeGroup, nodeData.GUID);
                node.IsEntryPoint = true;

                nodeGroup.AddElement(node);
                graphView.AddElement(node);
                loadedNodes.Add(node.GUID, node);
            }
            else
            {
                var node = (DENode) graphView.CreateNode(nodeData.Position, nodeGroup, nodeData.GUID);

                node.Text = nodeData.Text;
                node.IsEntryPoint = false;

                node.DialogueText.value = nodeData.Text;
                node.DialogueTitle.value = nodeData.Title;

                // Load choices if present
                var dePorts = dialogueContainerCache.ConnectionData
                    .Where(x => x.BaseNodeGUID == node.GUID)
                    .ToList();

                foreach (var dePort in dePorts)
                {
                    // Create output port
                    var outputPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
                    outputPort.portName = dePort.PortName;

                    if (outputPort.portName != "Action")
                    { 
                        outputPort.contentContainer.Q<Label>("type").style.display = DisplayStyle.None;
                    }

                    KeyValuePair<string, Port> portPair = new(dePort.PortGUID, outputPort);
                    
                    if (loadedPorts.Contains(portPair))
                    {
                        dialogueContainerCache.ConnectionData.Remove(dePort);
                        continue;
                    }

                    loadedPorts.Add(dePort.PortGUID, outputPort);

                    // Add output port to the node
                    node.extensionContainer.Add(outputPort);

                    // Create choice text field
                    var choiceTextField = new TextField()
                    {
                        name = string.Empty,
                        value = dePort.PortName
                    };
                    choiceTextField.RegisterValueChangedCallback(evt => outputPort.portName = evt.newValue);

                    // Create remove button
                    var removeButton = new Button(() =>
                    {
                        node.extensionContainer.Remove(outputPort);
                        node.DisconnectDeletedNode(outputPort);
                    })
                    {
                        text = "X"
                    };

                    // Add choice text field and remove button to the output port
                    if (outputPort.portName != "Action")
                    {
                        outputPort.Insert(1, choiceTextField);
                    }
                    else
                    {
                        if (node.GeneratedAction)
                        { 
                            return; 
                        }
                            node.GeneratedAction = true;
                    }
                    
                    outputPort.Insert(2, removeButton);
                }

                nodeGroup.AddElement(node);
                graphView.AddElement(node);
                loadedNodes.Add(node.GUID, node);
            }
        }

        // Load Action nodes
        foreach (var nodeData in dialogueContainerCache.ActionData) 
        {
            DEGroup nodeGroup = Groups.Find(x => x == loadedGroups[nodeData.Group.GUID]); 
            var node = (DEActionNode)graphView.CreateActionNode(nodeData.Position, nodeGroup, nodeData.GUID);
            var outputPort = (Port) node.outputContainer[0];
            
            node.ActionObject = nodeData.ActionObject;

            if (node.ActionObject != null)
            {
                node.SelectedComponent = nodeData.SelectedComponent;
                node.ActionMethod = nodeData.ActionMethod;
            }

            var connections = dialogueContainerCache.ConnectionData.Where(x => x.BaseNodeGUID == node.GUID).ToList();

            foreach (var connection in connections)
            {
                string targetNodeGUID = connection.TargetNodeGUID;
                DENode targetNode = loadedNodes[targetNodeGUID];
                graphView.LinkNodes(outputPort, (Port)targetNode.inputContainer[0]);
            }

            loadedNodes.Add(node.GUID, node);
            nodeGroup.AddElement(node);
            graphView.AddElement(node);
        }

        // Connect edges to Dialogue node ports
        foreach (DENode node in loadedNodes.Values)
        {
            var connections = dialogueContainerCache.ConnectionData.Where(x => x.BaseNodeGUID == node.GUID).ToList();

            foreach (var connection in connections)
            {
                string targetNodeGUID = connection.TargetNodeGUID;
                DENode targetNode = loadedNodes[targetNodeGUID];

                if (node.IsEntryPoint)
                {
                    var outputPort = node.outputContainer.Q<Port>();
                    graphView.LinkNodes(outputPort, (Port)targetNode.inputContainer[0]);
                }
                else
                {
                    if (node.extensionContainer.childCount > 1)
                    {
                        foreach (var child in node.extensionContainer.Children())
                        {
                            if (child is not Port port)
                            {
                                continue;
                            }

                            string tnGUID = connections.FirstOrDefault(x => x.PortName == port.portName)?.TargetNodeGUID;
                            DENode tn = Nodes.First(x => x.GUID == tnGUID);

                            KeyValuePair<Port, Port> connectionPair = new(port, (Port)tn.inputContainer[0]);
                            if (loadedConnections.Contains(connectionPair))
                            {
                                continue;
                            }

                            graphView.LinkNodes(port, (Port)tn.inputContainer[0]);
                            loadedConnections.Add(port, (Port)tn.inputContainer[0]);
                        }

                        if (!node.GeneratedChoices)
                        {
                            node.GeneratedChoices = true;
                        }
                    }
                }
            }
        }
    }

    private void ClearGraph()
    {
        foreach (var element in graphView.graphElements.ToList())
        { 
            graphView.RemoveElement(element);
        }
    }

    private void CreateFolder(string path, string name)
    {
        if (!AssetDatabase.IsValidFolder($"{path}/{name}"))
        {
            AssetDatabase.CreateFolder(path, name);
        }
    }

}