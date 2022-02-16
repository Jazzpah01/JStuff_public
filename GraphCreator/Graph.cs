using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace JStuff.GraphCreator
{
    public abstract class Graph : ScriptableObject
    {
        [SerializeReference] public Node rootNode;
        public abstract Type RootNodeType { get; }
        [SerializeReference] public List<Node> nodes = new List<Node>();
        [SerializeReference] public List<PortView> portViews = new List<PortView>();

        public abstract List<Type> NodeTypes { get; }

        public virtual Direction InputPortDirection => Direction.Input;
        public virtual Orientation Orientation => Orientation.Horizontal;


        public List<PortView> propertyPortViews = new List<PortView>();
        public List<Link> propertyPorts = new List<Link>();
        public List<object> oldpropertyValues = new List<object>();
        public List<string> propertyNames = new List<string>();
        public List<string> propertyTypeNames = new List<string>();
        private Dictionary<string, int> propertyIndexMap = new Dictionary<string, int>();

        [NonSerialized] public bool ProperyChanged = true;
        public int numberOfProperties;

        public bool initialized = false;

        public List<string> propertyValues = new List<string>();
        private PropertySetup propertySetup = PropertySetup.Editor;
        private enum PropertySetup
        {
            Runtime,
            Editor
        }

        public enum PropertyContext
        {
            Unique,
            Shared
        }

        public Context uniqueContext;
        public Context sharedContext;

        public void InitializeBaseGraph()
        {
            foreach (PortView view in propertyPortViews)
            {
                portViews.Remove(view);
                AssetDatabase.RemoveObjectFromAsset(view);
            }

            if (sharedContext == null)
            {
                sharedContext = CreateInstance<Context>();
                sharedContext.graph = this;
                EditorUtility.SetDirty(sharedContext);
                AssetDatabase.AddObjectToAsset(sharedContext, this);
            }

            if (uniqueContext == null)
            {
                uniqueContext = CreateInstance<Context>();
                uniqueContext.graph = this;
                EditorUtility.SetDirty(uniqueContext);
                AssetDatabase.AddObjectToAsset(uniqueContext, this);
            }

            sharedContext.Clear();
            uniqueContext.Clear();
            SetupProperties();

            AssetDatabase.SaveAssets();
        }

        protected virtual void SetupProperties()
        {

        }



        public object GetProperty(int index)
        {
            return oldpropertyValues[index];
        }

        public Link GetPropertyPort(int index)
        {
            Debug.Log(propertyPorts);
            Debug.Log(propertyPorts.Count);

            return propertyPorts[index];
        }

        public void CleanupPortViews()
        {
            foreach (PortView view in portViews.ToArray())
            {
                if (view == null)
                {
                    continue;
                }
                Debug.Log(view);
                foreach (PortView linked in view.linked.ToArray())
                {
                    if (linked == null)
                    {
                        continue;
                    }
                    Debug.Log(linked);
                    if (!linked.Valid)
                    {
                        view.UnLink(linked);
                    }
                }
            }

            for (int i = 0; i < portViews.Count; i++)
            {
                if (!portViews[i].Valid)
                {
                    AssetDatabase.RemoveObjectFromAsset(portViews[i]);
                    portViews.Remove(portViews[i]);
                    i--;
                }
            }

            AssetDatabase.SaveAssets();
        }

        protected PropertyPort<T> AddProperty<T>(T value, string name, PropertyContext context)
        {
            if ((uniqueContext.Contains(name) || sharedContext.Contains(name)) && !Application.isPlaying)
                throw new Exception("Property of the specified name already exists: " + name);

            switch (context)
            {
                case PropertyContext.Unique:
                    uniqueContext.AddPropertyPort(value, name);
                    if (Application.isPlaying)
                    {
                        return (PropertyPort<T>)uniqueContext.GetPropertyLink(name);
                    }
                    break;
                case PropertyContext.Shared:
                    sharedContext.AddPropertyPort(value, name);
                    if (Application.isPlaying)
                    {
                        return (PropertyPort<T>)sharedContext.GetPropertyLink(name);
                    }
                    break;
            }
            return null;
        }

        public string GetPropertyType(string propertyName)
        {
            if (uniqueContext.Contains(propertyName))
            {
                return uniqueContext.GetPropertyType(propertyName);
            }
            else
            if (sharedContext.Contains(propertyName))
            {
                return sharedContext.GetPropertyType(propertyName);
            }
            else
            {
                throw new Exception("Property with the specified name doesn't exist: " + name);
            }
        }

        //public PropertyPort<T> GetProperty<T>(string name)
        //{
        //    if (privateContext.Contains(name))
        //    {
        //        return privateContext.GetPropertyLink<T>(name);
        //    } else if (sharedContext.Contains(name))
        //    {
        //        return sharedContext.GetPropertyLink<T>(name);
        //    } else
        //    {
        //        throw new Exception("Property with the specified name doesn't exist!");
        //    }
        //}

        public Link GetProperty(string name)
        {
            if (uniqueContext.Contains(name))
            {
                return uniqueContext.GetPropertyLink(name);
            }
            else if (sharedContext.Contains(name))
            {
                return sharedContext.GetPropertyLink(name);
            }
            else
            {
                throw new Exception("Property with the specified name doesn't exist: " + name);
            }
        }

        public void SetupGraph()
        {
            if (!Application.isPlaying)
                throw new Exception("Can only setup graph in runtime.");

            ProperyChanged = true;
            Debug.Log("Before property setup");
            SetupProperties();
            Debug.Log("After property setup");
            LinkPorts();
        }

        public Graph Clone()
        {
            return Instantiate(this);
        }

        public void LinkPorts()
        {
            foreach (Node node in nodes)
            {
                node.InitializeNode();
            }
            foreach (Node node in nodes)
            {
                node.LinkNodePorts();
            }
        }

        public Node CreatePropertyNode(int propertyIndex)
        {
            Debug.Log("Property index: " + propertyIndex);
            PropertyNode node = CreateInstance<PropertyNode>() as PropertyNode;
            node.name = propertyNames[propertyIndex];
            node.guid = GUID.Generate().ToString();
            node.graph = this;
            //node.InitPropertyPortView(propertyIndex);
            nodes.Add(node);
            node.Valid = true;

            foreach (PortView portView in node.portViews)
            {
                portViews.Add(portView);
                EditorUtility.SetDirty(portView);
                AssetDatabase.AddObjectToAsset(portView, this);
            }

            AssetDatabase.AddObjectToAsset(node, this);
            AssetDatabase.SaveAssets();

            return node;
        }

        public Node CreateNode(Type type)
        {
            if (type == RootNodeType && rootNode != null)
            {
                throw new Exception("Cannot add multiple root nodes.");
            }

            Node node = CreateInstance(type) as Node;
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();
            node.graph = this;
            node.InitializeNode();
            nodes.Add(node);
            node.Valid = true;

            if (type == RootNodeType)
            {
                rootNode = node;
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }

            foreach (PortView portView in node.portViews)
            {
                //EditorUtility.SetDirty(portView);
                //AssetDatabase.AddObjectToAsset(portView, this);
                portViews.Add(portView);
            }

            AssetDatabase.AddObjectToAsset(node, this);
            AssetDatabase.SaveAssets();

            return node;
        }

        public void UpdateNodes()
        {
            foreach (Node node in nodes)
            {
                if (node.SignatureChanged)
                {
                    node.InitializeNode();
                    foreach (PortView view in node.portViews)
                    {
                        portViews.Add(view);
                    }
                }
            }
            AssetDatabase.SaveAssets();
            InitializeBaseGraph();
            CleanupPortViews();
        }

        public void DeleteNode(Node node)
        {
            if (node == rootNode)
            {
                rootNode = null;
            }
            nodes.Remove(node);

            foreach (PortView portView in node.portViews)
            {
                portViews.Remove(portView);
                AssetDatabase.RemoveObjectFromAsset(portView);
            }

            AssetDatabase.RemoveObjectFromAsset(node);
            AssetDatabase.SaveAssets();
        }

        public void DetectChange()
        {
            AssetDatabase.SaveAssets();
        }

        public void SaveAsset()
        {
            AssetDatabase.SaveAssets();
        }
    }
}



/*
[SerializeReference] public List<string> graphPropertyNames = new List<string>();
    private Dictionary<string, object> propertyDictionary = new Dictionary<string, object>();



    public object GetProperty(string propertyName)
    {
        if (!propertyDictionary.ContainsKey(propertyName))
            throw new System.Exception("No property exists with that name, or it is not set.");

        return propertyDictionary[propertyName];
    }

    public void SetProperty(string propertyName, object value)
    {
        if (!graphPropertyNames.Contains(propertyName))
            throw new System.Exception("No property exists with that name.");

        if (propertyDictionary.ContainsKey(propertyName))
        {
            propertyDictionary[propertyName] = value;
        } else
        {
            propertyDictionary.Add(propertyName, value);
        }
    }

    public void AddProperty(string propertyName)
    {
        if (graphPropertyNames.Contains(propertyName))
            throw new System.Exception("Cannot have multiple properties with the same name.");

        graphPropertyNames.Add(propertyName);
    }


*/


/*
bad code for cleaning up:
//List<PortView> safePortViews = new List<PortView>();
        //
        //foreach (SimpleNode node in nodes)
        //{
        //    foreach (PortView portView in node.portViews)
        //    {
        //        safePortViews.Add(portView);
        //    }
        //}
        //
        //for (int i = 0; i < portViews.Count; i++)
        //{
        //    if (!safePortViews.Contains(portViews[i]))
        //    {
        //        Debug.Log("Portview to be removed!: " + portViews[i]);
        //        foreach (SimpleNode node in nodes)
        //        {
        //            foreach (PortView nodePortView in node.portViews)
        //            {
        //                if (nodePortView.LinkedWith(portViews[i]))
        //                {
        //                    nodePortView.UnLink(portViews[i]);
        //                }
        //            }
        //        }
        //
        //        PortView portView = portViews[i];
        //        portViews.Remove(portViews[i]);
        //        //DestroyImmediate(portView, true);
        //        //EditorUtility.SetDirty(this);
        //        AssetDatabase.RemoveObjectFromAsset(portView);
        //        i--;
        //    }
        //    AssetDatabase.SaveAssets();
        //}













    public SimpleNodePort AddProperty<T>(T value, string name)
    {
        Direction direction = (InputPortDirection == Direction.Input) ? Direction.Output : Direction.Input;

        if (Application.isPlaying)
        {
            // Actual node port
            PropertyPort<T> nodePort = null;
            nodePort = new PropertyPort<T>();
            nodePort.Init(null, propertyPorts.Count, Orientation, direction, Port.Capacity.Multi);
            nodePort.graph = this;
            this.propertyPorts.Add(nodePort);
            this.propertyValues.Add(value);
            return nodePort;
        }
        else
        {
            // Editor port view
            PortView portView = ScriptableObject.CreateInstance<PortView>();
            portView.Init(null, Orientation, direction, Port.Capacity.Multi, typeof(T), portViews.Count);
            propertyNames.Add(name);
            propertyPortViews.Add(portView);
            propertyTypes.Add(typeof(T));
            propertyIndexMap.Add(name, numberOfProperties);
            numberOfProperties++;
            EditorUtility.SetDirty(portView);
            AssetDatabase.AddObjectToAsset(portView, this);
            AssetDatabase.SaveAssets();
            //portViews.Add(portView);
        }
        return null;
    }
*/

//for (int i = 0; i < nodes.Count; i++)
//{
//    PropertyNode node = nodes[i] as PropertyNode;
//    bool removePropertyNode = false;
//    if (node != null && !propertyIndexMap.ContainsKey(node.name))
//    {
//        Debug.Log("Removing node!");
//        removePropertyNode = true;
//    } else if (node != null && 
//        (node.index >= propertyNames.Count ||
//        node.name != propertyNames[node.index] ||
//        node.typeName != propertyTypes[node.index].FullName))
//    {
//        Debug.Log("Removing node!");
//        node.index = propertyIndexMap[node.name];
//
//        if (node.typeName != propertyTypes[node.index].FullName)
//        {
//            Debug.Log(node.typeName + " - " + propertyTypes[node.index].FullName);
//            removePropertyNode = true;
//        }
//    }
//
//    if (removePropertyNode)
//    {
//        Debug.Log("Removing node!");
//        foreach (PortView view in portViews)
//        {
//            if (view.LinkedWith(node.portViews[0]))
//            {
//                view.UnLink(node.portViews[0]);
//            }
//        }
//
//        DeleteNode(node);
//        i--;
//    }
//}


//public PortView GetPortView<T>(int index)
//{
//    Direction direction = (InputPortDirection == Direction.Input) ? Direction.Output : Direction.Input;
//
//    
//
//    return propertypo
//}