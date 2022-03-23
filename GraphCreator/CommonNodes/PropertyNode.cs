using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using System;
using UnityEngine.UIElements;
using JStuff.Collections;

namespace JStuff.GraphCreator
{
    [CreateNodePath("Common/Property Node")]
    public class PropertyNode : CommonNode
    {
        public string propertyName = "";

        public int index = -1;

        public Link port;

        public string typeName;

        public Link output;
        public Link input;

        public DropDownList list;

        public override void OnGUIStart(INodeView nodeView)
        {
            list.OnValueChanged = null;
            OnNodeChange = null;
            list.Clear();
            foreach (string s in graph.uniqueContext.propertyNames)
            {
                list.Add(s);
            }
            foreach (string s in graph.sharedContext.propertyNames)
            {
                list.Add(s);
            }
            list.value = propertyName;
            list.OnValueChanged = ValueChanged;
        }

        public void InitPropertyPortView()
        {

        }

        protected override void SetupPorts()
        {
            output = AddPropertyOutputLink(propertyName);
        }

        private void ValueChanged(string value)
        {
            Valid = false;
            portViews[0].Valid = false;
            propertyName = value;
            graph.UpdateNodes();
        }

        protected override void SetupNode()
        {
            if (propertyName == "")
            {
                if (graph.uniqueContext.Length > 0)
                {
                    propertyName = graph.uniqueContext.propertyNames[0];
                }
                else if (graph.sharedContext.Length > 0)
                {
                    propertyName = graph.sharedContext.propertyNames[0];
                }
            }
        }

        public override Node Clone()
        {
            PropertyNode retval = base.Clone() as PropertyNode;
            retval.propertyName = propertyName;
            retval.typeName = typeName;
            return retval;
        }
    }
}