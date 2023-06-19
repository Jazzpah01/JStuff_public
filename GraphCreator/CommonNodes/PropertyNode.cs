using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

        public DropDownList list = new DropDownList();

#if UNITY_EDITOR
        public override void OnGUIStart(/*INodeView nodeView*/)
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
#endif

        public override bool Valid { 
            get {
                bool retval = base.Valid;

                if (ports.Count == 0 || ports[0].portType != typeName || ports[0].portName != propertyName)
                {
                    valid = false;
                    return false;
                }

                return retval;
            } 
            set { 
            } 
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
            Debug.Log("Changed!");
            propertyName = value;
            typeName = graph.GetPropertyType(value);
            graph.UpdateGraph();
            base.OnNodeChange();
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

        public override bool IsConstant()
        {
            return false;
        }
    }
}