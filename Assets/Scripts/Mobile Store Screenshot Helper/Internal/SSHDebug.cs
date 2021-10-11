using UnityEngine;

namespace NG
{
	public class SSHDebug
	{
		private static string tag = "[Screenshot Helper] ";
		public static void Log(string message)
		{
			Debug.Log(tag + message);
		}

		public static void LogWarning(string message)
		{
			Debug.LogWarning(tag + message);
		}

		public static void LogError(string message)
		{
			Debug.LogError(tag + message);
		}
	}
}