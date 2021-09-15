using UnityEngine;
using UnityEditor;

namespace NG
{
	public class ResetPlayerPrefs : MonoBehaviour
	{
		[MenuItem("Edit/Screenshot Helper/Reset Screenmanager Settings")]
		static void ResetScreenManPref()
		{
			if (PlayerPrefs.HasKey("Screenmanager Is Fullscreen mode"))
			{
				PlayerPrefs.DeleteKey("Screenmanager Is Fullscreen mode");
			}

			if (PlayerPrefs.HasKey("Screenmanager Resolution Height"))
			{
				PlayerPrefs.DeleteKey("Screenmanager Resolution Height");
			}

			if (PlayerPrefs.HasKey("Screenmanager Resolution Width"))
			{
				PlayerPrefs.DeleteKey("Screenmanager Resolution Width");
			}
		}

		[MenuItem("Edit/Screenshot Helper/Reset All PlayerPrefs")]
		static void ResetAllPref()
		{
			PlayerPrefs.DeleteAll();
		}

		[MenuItem("Edit/Screenshot Helper/Where's persistentDataPath")]
		static void WheresMyPath()
		{
			Debug.Log(Application.persistentDataPath);
		}
	}
}
