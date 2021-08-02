using UnityEditor;
using UnityEditor.Macros;
using UnityEngine;

/// <summary>
/// Allows use of the undocumented MacroEvaluator class. Use at your own risk.
/// </summary>
public class Macros : EditorWindow
{
	string macro = "";
	
	/// <summary>
	/// Adds a menu named "Macros" to the Window menu.
	/// </summary>
	[MenuItem ("Window/Macros")]
	static void Init ()
	{
		CreateInstance<Macros>().ShowUtility();
	}
    
	void OnGUI ()
	{
		macro = EditorGUILayout.TextArea(macro, GUILayout.ExpandHeight(true));
		
		if (GUILayout.Button("Execute")) {
			MacroEvaluator.Eval(macro);
		}
	}
}