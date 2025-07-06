using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Soulpace.Dialogues.Utilities
{
	using Elements;
	
    public static class DSElementUtilities
    {
	    public static Button CreateButton(string text, Action onClick = null)
	    {
		    Button button = new Button(onClick)
		    {
			    text = text
		    };

		    return button;
	    }
	    
	    public static Foldout CreateFoldout(string title, bool collapsed = false)
	    {
		    Foldout foldout = new Foldout()
		    {
			    text = title,
			    value = collapsed
		    };
		    
		    return foldout;
	    }

	    public static Port CreatePort(this DSNode node, string portName = "", Orientation orientation = Orientation.Horizontal,
		    Direction direction = Direction.Output, Port.Capacity capacity = Port.Capacity.Single)
	    {
		    Port port = node.InstantiatePort(orientation, direction, capacity, typeof(bool));
		    
		    port.portName = portName;
		    
		    return port;
	    }
	    
	    public static TextField CreateTextField(string value = null, string label = null,
		    EventCallback<ChangeEvent<string>> onValueChange = null)
	    {
		    TextField textField = new TextField()
		    {
			    value = value,
			    label = label
		    };

		    if (onValueChange != null)
		    {
			    textField.RegisterValueChangedCallback(onValueChange);
		    }
		    
		    return textField;
	    }

	    public static TextField CreateTextArea(string value = null, string label = null,
		    EventCallback<ChangeEvent<string>> onValueChange = null)
	    {
		    TextField textArea = CreateTextField(value, label, onValueChange);
		    
		    textArea.multiline = true;
		    
		    return textArea;
	    }
    }
}
