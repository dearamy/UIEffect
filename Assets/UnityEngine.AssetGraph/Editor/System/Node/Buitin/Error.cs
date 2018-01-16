using UnityEngine;
using UnityEditor;

using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine.AssetGraph;
using Model=UnityEngine.AssetGraph.DataModel.Version2;

namespace UnityEngine.AssetGraph {
    [CustomNode("Assert/Error", 80)]
    public class Error : Node {

        [SerializeField] private string m_message;
        [SerializeField] private bool m_raiseErrorPerAsset;

    	public override string ActiveStyle {
    		get {
    			return "node 7 on";
    		}
    	}

    	public override string InactiveStyle {
    		get {
    			return "node 7";
    		}
    	}

    	public override string Category {
    		get {
    			return "Assert";
    		}
    	}

    	public override Model.NodeOutputSemantics NodeInputType {
    		get {
                return Model.NodeOutputSemantics.Any;
    		}
    	}

    	public override Model.NodeOutputSemantics NodeOutputType {
    		get {
                return Model.NodeOutputSemantics.None;
    		}
    	}

    	public override void Initialize(Model.NodeData data) {
            m_message = "Error occured.";
            m_raiseErrorPerAsset = false;
    		data.AddDefaultInputPoint();
    	}

    	public override Node Clone(Model.NodeData newData) {
            var newNode = new Error();
            newNode.m_message = this.m_message;
            newNode.m_raiseErrorPerAsset = this.m_raiseErrorPerAsset;
    		newData.AddDefaultInputPoint();
    		return newNode;
    	}

    	public override void OnInspectorGUI(NodeGUI node, AssetReferenceStreamManager streamManager, NodeGUIEditor editor, Action onValueChanged) {

    		EditorGUILayout.HelpBox("Error: Raise error if there is any input asset.", MessageType.Info);
    		editor.UpdateNodeName(node);

    		GUILayout.Space(10f);

            var newMessage = EditorGUILayout.TextArea(m_message, GUILayout.MaxHeight(100f));
            if(newMessage != m_message) {
    			using(new RecordUndoScope("Change Message", node, true)) {
                    m_message = newMessage;
    				onValueChanged();
    			}
    		}
            EditorGUILayout.HelpBox ("'{0}' will be replaced with Asset name", MessageType.Info);

            var newBool = EditorGUILayout.ToggleLeft ("Raise Error/Asset", m_raiseErrorPerAsset);
            if(newBool != m_raiseErrorPerAsset) {
                using(new RecordUndoScope("Change Raise Error/Asset", node, true)) {
                    m_raiseErrorPerAsset = newBool;
                    onValueChanged();
                }
            }
    	}

    	/**
    	 * Prepare is called whenever graph needs update. 
    	 */ 
        public override void Prepare (
            BuildTarget target, 
    		Model.NodeData node, 
    		IEnumerable<PerformGraph.AssetGroups> incoming, 
    		IEnumerable<Model.ConnectionData> connectionsToOutput, 
    		PerformGraph.Output Output) 
    	{
            if(string.IsNullOrEmpty(m_message)) {
    			throw new NodeException(node.Name + ":Error message is empty.", node.Id);
    		}

            bool isError = false;

            if(incoming != null) {
                foreach(var ag in incoming) {
                    foreach (var assets in ag.assetGroups.Values) {
                        foreach(var a in assets) {
                            if (m_raiseErrorPerAsset) {
                                LogUtility.Logger.LogError ("AssetGraph.Error." + node.Name, String.Format ("{0} file:{1}", m_message, a.importFrom));
                            }
                            isError = true;
                        }
                    }
                }
            }

            if (isError) {
                if (!m_raiseErrorPerAsset) {
                    LogUtility.Logger.LogError ("AssetGraph.Error." + node.Name, m_message);
                }
                throw new NodeException(node.Name + ":Found incoming asset(s) to Error node.", node.Id);
            }
    	}
    }
}