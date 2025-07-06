using System.Collections.Generic;
using System.IO;
using System.Linq;
using Soulpace.Dialogues.Data.Config;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Soulpace.Dialogues.Utilities
{
    using Data;
    using Data.Save;
    using ScriptableObjects;
    using Elements;
    using Windows;
    using Utility;
    
    public static class DSIOUtility
    {
        private const string DefaultAssetName = "GSGraphConfig.asset";
        private const string DefaultFolderPath = "Assets/Editor/Generated";
        
        private static DSGraphConfig s_graphConfig;
        
        private static DSGraphView s_graphView;
        
        private static string s_graphFileName;
        private static string s_containerFolderPath;
        
        private static List<DSGroup> s_groups;
        private static List<DSNode> s_nodes;
        
        private static Dictionary<string, DSDialogueGroupSO> s_createdDialogueGroups;
        private static Dictionary<string, DSDialogueSO> s_createdDialogues;
        
        private static Dictionary<string, DSGroup> s_loadedGroups;
        private static Dictionary<string, DSNode> s_loadedNodes;

        private static string DialoguesPath => s_graphConfig.SaveDialoguesPath;
        
        
        public static void Initialize(DSGraphView graphView, string graphName)
        {
            s_graphConfig = GetOrCreateGraphConfigAsset();
            
            s_graphView = graphView;
            s_graphFileName = graphName;
            s_containerFolderPath = $"Assets/DialogueSystem/Dialogues/{s_graphFileName}";
            
            s_groups = new List<DSGroup>();
            s_nodes = new List<DSNode>();
            
            s_createdDialogueGroups = new Dictionary<string, DSDialogueGroupSO>();
            s_createdDialogues = new Dictionary<string, DSDialogueSO>();
            
            s_loadedGroups = new Dictionary<string, DSGroup>();
            s_loadedNodes = new Dictionary<string, DSNode>();
        }
        
        #region Saving

        public static void Save()
        {
            CreateStaticFolders();

            GetElementsFromGraphView();

            DSGraphSaveDataSO graphData = CreateAsset<DSGraphSaveDataSO>("Assets/Editor/DialogueSystem/Graphs",
                $"{s_graphFileName}Graph");

            graphData.Initialize(s_graphFileName);

            DSDialogueContainerSO dialogueContainer
                = CreateAsset<DSDialogueContainerSO>(s_containerFolderPath, s_graphFileName);
            
            dialogueContainer.Initialize(s_graphFileName);
            
            SaveGroups(graphData, dialogueContainer);
            SaveNodes(graphData, dialogueContainer);
            
            SaveAsset(graphData);
            SaveAsset(dialogueContainer);
        }

        #endregion

        #region Loading

        public static void Load()
        {
            DSGraphSaveDataSO graphData =
                LoadAsset<DSGraphSaveDataSO>("Assets/Editor/DialogueSystem/Graphs", s_graphFileName);

            if (graphData == null)
            {
                EditorUtility.DisplayDialog(
                    "Couldn't load the File!",
                    "The file at the following path could not be found:\n\" + " +
                    $"Assets/Editor/DialogueSystem/Graphs/{s_graphFileName}\n\n" +
                    "Make sure you chose the right file and it's placed at the folder path mentioned above",
                    "Aye!");

                return;
            }

            DSEditorWindow.UpdateFileName(graphData.FileName);

            LoadGroups(graphData.Groups);
            LoadNodes(graphData.Nodes);
            LoadNodesConnections();
        }

        public static List<DSGraphSaveDataSO> GetAllSavedGraphSO()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(DSGraphSaveDataSO)}");
            
            List<DSGraphSaveDataSO> graphSOs = new List<DSGraphSaveDataSO>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                DSGraphSaveDataSO dataSO = AssetDatabase.LoadAssetAtPath<DSGraphSaveDataSO>(path);
                
                if(dataSO != null)
                    graphSOs.Add(dataSO);
            }
            
            return graphSOs;
        }

        private static void LoadNodes(List<DSNodeSaveData> nodes)
        {
            foreach (DSNodeSaveData nodeData in nodes)
            {
                List<DSChoiceSaveData> choices = CloneNodeChoices(nodeData.Choices);
                DSNode node = s_graphView.CreateNode(nodeData.Name, nodeData.DialogueType, nodeData.Position, false);
                
                node.ID = nodeData.ID;
                node.Choices = choices;
                node.Text = nodeData.Text;
                
                node.Draw();
                
                s_graphView.AddElement(node);
                
                s_loadedNodes.Add(node.ID, node);

                if (string.IsNullOrEmpty(nodeData.GroupID))
                    continue;
                
                DSGroup group = s_loadedGroups[nodeData.GroupID];
                node.Group = group;

                group.AddElement(node);
            }
        }

        private static void LoadNodesConnections()
        {
            foreach (KeyValuePair<string, DSNode> loadedNode in s_loadedNodes)
            {
                foreach (var visualElement in loadedNode.Value.outputContainer.Children())
                {
                    var choicePort = (Port)visualElement;
                    DSChoiceSaveData choiceData = (DSChoiceSaveData)choicePort.userData;
                    
                    if(string.IsNullOrEmpty(choiceData.NodeID))
                        continue;
                    
                    DSNode nextNode = s_loadedNodes[choiceData.NodeID];
                    Port nextNodeInputPort = (Port)nextNode.inputContainer.Children().First();
                    Edge edge = choicePort.ConnectTo(nextNodeInputPort);
                    
                    s_graphView.AddElement(edge);

                    loadedNode.Value.RefreshPorts();
                }
            }
        }

        private static void LoadGroups(List<DSGroupSaveData> groups)
        {
            foreach (DSGroupSaveData groupData in groups)
            {
                DSGroup group = s_graphView.CreateGroup(groupData.Name, groupData.Position);
                group.ID = groupData.ID;
                
                s_loadedGroups.Add(group.ID, group);
            }
        }

        #endregion
        
        #region Groups
        
        private static void SaveGroups(DSGraphSaveDataSO graphData, DSDialogueContainerSO dialogueContainer)
        {
            List<string> groupNames = new List<string>();
            
            foreach (var group in s_groups)
            {
                SaveGroupToGraph(group, graphData);
                SaveGroupToScriptableObject(group, dialogueContainer);
                
                groupNames.Add(group.title);
            }

            UpdateOldGroups(groupNames, graphData);
        }

        private static void SaveGroupToGraph(DSGroup group, DSGraphSaveDataSO graphData)
        {
            DSGroupSaveData groupData = new DSGroupSaveData()
            {
                ID = group.ID,
                Name = group.title,
                Position = group.GetPosition().position
            };
            
            graphData.Groups.Add(groupData);
        }

        private static void SaveGroupToScriptableObject(DSGroup group, DSDialogueContainerSO dialogueContainer)
        {
            string groupName = group.title;
            
            CreateFolder($"{s_containerFolderPath}/Groups", groupName);
            CreateFolder($"{s_containerFolderPath}/Groups/{groupName}", "Dialogues");

            DSDialogueGroupSO dialogueGroup =
                CreateAsset<DSDialogueGroupSO>($"{s_containerFolderPath}/Groups/{groupName}", groupName);
            
            dialogueGroup.Initialize(groupName);
            
            s_createdDialogueGroups.Add(group.ID, dialogueGroup);
            
            dialogueContainer.DialogueGroups.Add(dialogueGroup, new List<DSDialogueSO>());
            
            SaveAsset(dialogueGroup);
        }

        private static void UpdateOldGroups(List<string> currentGroupNames, DSGraphSaveDataSO graphData)
        {
            if (graphData.OldGroupNames != null && graphData.OldGroupNames.Count > 0)
            {
                List<string> groupsToRemove = graphData.OldGroupNames.Except(currentGroupNames).ToList();

                foreach (string groupToRemove in groupsToRemove)
                {
                    RemoveFolder($"{s_containerFolderPath}/Groups/{groupToRemove}");
                }
            }
            
            graphData.OldGroupNames = new List<string>(currentGroupNames);
        }

        #endregion

        #region Nodes
        
        private static void SaveNodes(DSGraphSaveDataSO graphData, DSDialogueContainerSO dialogueContainer)
        {
            SerializableDictionary<string, List<string>> groupedNodeNames =
                new SerializableDictionary<string, List<string>>();
            List<string> ungroupedNodeName = new List<string>();
            
            foreach (var node in s_nodes)
            {
                SaveNodeToGraph(node, graphData);
                SaveNodeToScriptableObject(node, dialogueContainer);

                if (node.Group != null)
                {
                    groupedNodeNames.AddItem(node.Group.title, node.DialogueName);
                }
                
                ungroupedNodeName.Add(node.DialogueName);
            }

            UpdateDialogueChoicesConnections();

            UpdateOldGroupedNodes(groupedNodeNames, graphData);
            UpdateOldUngroupedNodes(ungroupedNodeName, graphData);
        }

        private static void SaveNodeToGraph(DSNode node, DSGraphSaveDataSO graphData)
        {
            List<DSChoiceSaveData> choices = CloneNodeChoices(node.Choices);
            
            DSNodeSaveData nodeData = new DSNodeSaveData()
            {
                ID = node.ID,
                Name = node.DialogueName,
                Choices = choices,
                Text = node.Text,
                GroupID = node.Group?.ID,
                DialogueType = node.DialogueType,
                Position = node.GetPosition().position
            };
            
            graphData.Nodes.Add(nodeData);
        }

        private static void SaveNodeToScriptableObject(DSNode node, DSDialogueContainerSO dialogueContainer)
        {
            DSDialogueSO dialogue;

            if (node.Group != null)
            {
                dialogue = CreateAsset<DSDialogueSO>($"{s_containerFolderPath}/Groups/{node.Group.title}/Dialogues",
                    node.DialogueName);

                dialogueContainer.DialogueGroups.AddItem(s_createdDialogueGroups[node.Group.ID], dialogue);
            }
            else
            {
                dialogue = CreateAsset<DSDialogueSO>($"{s_containerFolderPath}/Global/Dialogues", node.DialogueName);
                
                dialogueContainer.UngroupedDialogue.Add(dialogue);
            }

            dialogue.Initialize(
                node.DialogueName,
                node.Text,
                ConvertNodeChoicesToDialogueChoices(node.Choices),
                node.DialogueType,
                node.IsStartingNode()
            );

            s_createdDialogues.Add(node.ID, dialogue);
            
            SaveAsset(dialogue);
        }

        private static List<DSDialogueChoiceData> ConvertNodeChoicesToDialogueChoices(List<DSChoiceSaveData> nodeChoices)
        {
            List<DSDialogueChoiceData> dialogueChoices = new List<DSDialogueChoiceData>();

            foreach (DSChoiceSaveData nodeChoice in nodeChoices)
            {
                DSDialogueChoiceData choiceData = new DSDialogueChoiceData()
                {
                    Text = nodeChoice.Text
                };

                dialogueChoices.Add(choiceData);
            }
            
            return dialogueChoices;
        }

        private static void UpdateDialogueChoicesConnections()
        {
            foreach (var node in s_nodes)
            {
                DSDialogueSO dialogue = s_createdDialogues[node.ID];

                for (int i = 0; i < node.Choices.Count; ++i)
                {
                    DSChoiceSaveData nodeChoice = node.Choices[i];
                    
                    if(string.IsNullOrEmpty(nodeChoice.NodeID))
                        continue;
                    
                    dialogue.Choices[i].NextDialogue = s_createdDialogues[nodeChoice.NodeID];

                    SaveAsset(dialogue);
                }
            }
        }

        private static void UpdateOldGroupedNodes(SerializableDictionary<string, List<string>> currentGroupedNodeNames,
            DSGraphSaveDataSO graphData)
        {
            if (graphData.OldGroupedNodeNames != null && graphData.OldGroupedNodeNames.Count > 0)
            {
                foreach (KeyValuePair<string, List<string>> oldGroupedNode in graphData.OldGroupedNodeNames)
                {
                    List<string> nodesToRemove = new List<string>();
                    
                    if(currentGroupedNodeNames.ContainsKey(oldGroupedNode.Key))
                    {
                        nodesToRemove = oldGroupedNode.Value.Except(currentGroupedNodeNames[oldGroupedNode.Key])
                            .ToList();
                    }

                    foreach (string nodeToRemove in nodesToRemove)
                    {
                        RemoveAsset($"{s_containerFolderPath}/Groups/{oldGroupedNode.Key}/Dialogues", nodeToRemove);
                    }
                }
            }
            
            graphData.OldGroupedNodeNames = new SerializableDictionary<string, List<string>>(currentGroupedNodeNames);
        }

        private static void UpdateOldUngroupedNodes(List<string> currentUngroupedNodeName, DSGraphSaveDataSO graphData)
        {
            if (graphData.OldUngroupedNodeNames != null && graphData.OldUngroupedNodeNames.Count > 0)
            {
                List<string> nodesToRemove = graphData.OldUngroupedNodeNames.Except(currentUngroupedNodeName).ToList();

                foreach (string nodeToRemove in nodesToRemove)
                {
                    RemoveAsset($"{s_containerFolderPath}/Global/Dialogues", nodeToRemove);
                }
            }

            graphData.OldUngroupedNodeNames = new List<string>(currentUngroupedNodeName);
        }

        #endregion

        #region Get Method
        
        public static DSGraphConfig GetOrCreateGraphConfigAsset()
        {
            if (s_graphConfig != null)
                return s_graphConfig;
            
            var config = AssetDatabase.FindAssets("t:DSGraphConfig")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<DSGraphConfig>)
                .FirstOrDefault();

            if (config == null)
            {
                Debug.LogWarning("No DSGraphConfig found. Creating a new one.");

                // Ensure the folder exists
                if (!AssetDatabase.IsValidFolder(DefaultFolderPath))
                {
                    Directory.CreateDirectory(DefaultFolderPath);
                    AssetDatabase.Refresh();
                }

                // Create and save a new DSGraphConfig
                config = ScriptableObject.CreateInstance<DSGraphConfig>();
                string assetPath = Path.Combine(DefaultFolderPath, DefaultAssetName);
                AssetDatabase.CreateAsset(config, assetPath);
                AssetDatabase.SaveAssets();

                Debug.Log("<color=cyan>Created new DSGraphConfig at: </color>" + assetPath);
                
                // Highlight the asset in the Project window
                Selection.activeObject = config;
            }

            return config;
        }

        private static void GetElementsFromGraphView()
        {
            s_graphView.graphElements.ForEach(element =>
            {
                if (element is DSNode node)
                {
                    s_nodes.Add(node);

                    return;
                }

                if (element is DSGroup group)
                {
                    s_groups.Add(group);

                    return;
                }
            });
        }

        #endregion

        #region Utility Methods

        private static void CreateStaticFolders()
        {
            CreateFolder("Assets", "Editor");
            CreateFolder("Assets/Editor", "DialogueSystem");
            CreateFolder("Assets/Editor/DialogueSystem", "Graphs");
            
            CreateFolder("Assets", "DialogueSystem");
            CreateFolder("Assets/DialogueSystem", "Dialogues");
            CreateFolder("Assets/DialogueSystem/Dialogues", s_graphFileName);
            CreateFolder(s_containerFolderPath, "Global");
            CreateFolder(s_containerFolderPath, "Groups");
            CreateFolder($"{s_containerFolderPath}/Global", "Dialogues");
        }

        private static void CreateMissingFolders(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                Debug.LogError("Provided path is null or empty.");
                return;
            }
            
            string[] folders = path.Split('/');
            if (folders.Length == 0 || folders[0] != "Assets")
            {
                Debug.LogError("Path must start with 'Assets'.");
                return;
            }

            string currentPath = "Assets";
            for (int i = 1; i < folders.Length; i++)
            {
                string folderName = folders[i];
                string nextPath = Path.Combine(currentPath, folderName);

                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folderName);
                    Debug.Log($"Created folder: {nextPath}");
                }

                currentPath = nextPath;
            }

            AssetDatabase.Refresh();
        }

        private static void CreateFolder(string path, string folderName)
        {
            if (AssetDatabase.IsValidFolder($"{path}/{folderName}"))
                return;

            AssetDatabase.CreateFolder(path, folderName);
        }

        private static void RemoveFolder(string fullPath)
        {
            FileUtil.DeleteFileOrDirectory($"fullPath.meta");
            FileUtil.DeleteFileOrDirectory($"fullPath/");
        }

        private static T CreateAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";
            
            T asset = LoadAsset<T>(path, assetName);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
            
                AssetDatabase.CreateAsset(asset, fullPath);
            }

            return asset;
        }

        private static T LoadAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";
            
            return AssetDatabase.LoadAssetAtPath<T>(fullPath);
        }

        private static void RemoveAsset(string path, string assetName)
        {
            AssetDatabase.DeleteAsset($"{path}/{assetName}.asset");
        }

        private static void SaveAsset(UnityEngine.Object asset)
        {
            EditorUtility.SetDirty(asset);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static List<DSChoiceSaveData> CloneNodeChoices(List<DSChoiceSaveData> nodeChoices)
        {
            List<DSChoiceSaveData> choices = new List<DSChoiceSaveData>();

            foreach (var choice in nodeChoices)
            {
                DSChoiceSaveData choiceData = new DSChoiceSaveData()
                {
                    Text = choice.Text,
                    NodeID = choice.NodeID
                };
                
                choices.Add(choiceData);
            }

            return choices;
        }

        #endregion
    }
}
