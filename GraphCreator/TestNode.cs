using JStuff.GraphCreator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TestNode : Node
{
    string dialogue = "Option";

    EventCallback<ChangeEvent<string>> eventCallback => SetOption;

    PopupWindow popup;

    protected override void SetupPorts()
    {
        AddOutputLink<int>(() => 3, UnityEditor.Experimental.GraphView.Port.Capacity.Multi);
    }

    public override void OnGUIStart(INodeView nodeView)
    {
        //nodeView.Node.titleContainer.Clear();



        TextField text = new TextField(dialogue);
        text.RegisterValueChangedCallback(eventCallback);
        nodeView.Node.outputContainer[0].Add(text);

        Button button2 = new Button(() => Debug.Log("Hej"));
        button2.text = "Hej";
        nodeView.Node.titleContainer.Add(button2);

        //Button l  = new Button(() => Debug.Log("Hej"));
        //Button l2 = new Button();
        //Button l3 = new Button();
        //Button l4 = new Button();
        //Button l5 = new Button();

        //UnityEngine.UIElements.PopupWindow pop = new UnityEngine.UIElements.PopupWindow();
        //pop.Add(l2);
        //pop.Add(l3);
        //pop.Add(l4);
        //pop.Add(l5);

        //nodeView.Node.Add(pop);

        Label l = new Label("hej");

        UnityEngine.UIElements.PopupWindow pop = new UnityEngine.UIElements.PopupWindow();
        pop.Add(l);

        popup = new UnityEngine.UIElements.PopupWindow();
    }

    public override void OnGUISelected(INodeView nodeView)
    {
        nodeView.Node.extensionContainer.Add(popup);
    }

    public override void OnGUIUnselected(INodeView nodeView)
    {
        nodeView.Node.extensionContainer.Remove(popup);
    }

    public void SetOption(ChangeEvent<string> s)
    {
        Debug.Log(s.newValue);
    }
}