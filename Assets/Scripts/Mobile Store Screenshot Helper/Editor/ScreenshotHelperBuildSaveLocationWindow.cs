using UnityEditor;

namespace NG
{
	public class ScreenshotHelperBuildSaveLocationWindow : EditorWindow
	{
		static ScreenshotHelper instance;
		public static void ShowWindow(ScreenshotHelper ssh)
		{
			GetWindow(typeof(ScreenshotHelperBuildSaveLocationWindow));
			instance = ssh;
		}

		void OnGUI()
		{
			if (instance == null)
			{
				EditorWindow sshWindow = GetWindow(typeof(ScreenshotHelperBuildSaveLocationWindow));
				if (sshWindow != null)
					sshWindow.Close();
				return;
			}

			EditorGUILayout.HelpBox(
				"Set the location to save screenshots when using a build. " + 
				"The exact location will change based on the machine the build is running. " + 
				"For example the user name will be corrected.", 
				MessageType.None);

			string example = instance.BuildSaveLocation();
			EditorGUILayout.HelpBox("Location example: " + example, MessageType.None);

			instance.buildSavePathRoot = (System.Environment.SpecialFolder)EditorGUILayout.EnumPopup("Root: ", instance.buildSavePathRoot);
			instance.buildSavePathExtra = EditorGUILayout.TextField("Extra directory: ", instance.buildSavePathExtra);
		}
	}
}