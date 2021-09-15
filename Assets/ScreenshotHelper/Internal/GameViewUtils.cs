using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NG
{
	public static class GameViewUtils
	{
#if UNITY_EDITOR
		static object gameViewSizesInstance;
		static MethodInfo getGroup;
		private static List<int> indices = new List<int>();

		static GameViewUtils()
		{
			var sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
			var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
			var instanceProp = singleType.GetProperty("instance");
			getGroup = sizesType.GetMethod("GetGroup");
			gameViewSizesInstance = instanceProp.GetValue(null, null);
		}


		public enum GameViewSizeType
		{
			AspectRatio, FixedResolution
		}

		public static void SetSize(int index)
		{
			if (!SizeExists(index))
			{
				Debug.LogError("cannot find size for index: " + index.ToString());
				index = 0;
			}

			var gvWndType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
			var selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex",
					BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var gvWnd = EditorWindow.GetWindow(gvWndType);
			selectedSizeIndexProp.SetValue(gvWnd, index, null);
		}

        //static bool doOnce = false;

        // Constructors as of Unity 2018.3
        /*
        .ctor
	    (UnityEditor.GameViewSizeType) type
	    (System.Int32) width
	    (System.Int32) height
	    (System.String) baseText

        .ctor
	        (UnityEditor.GameViewSize) other
        */

        public static void AddCustomSize(GameViewSizeType viewSizeType, GameViewSizeGroupType sizeGroupType, int width, int height, string text)
		{
			var group = GetGroup(sizeGroupType);
			var addCustomSize = getGroup.ReturnType.GetMethod("AddCustomSize");
			var gvsType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
            ConstructorInfo ctor = null;
            Exception ctorException = null;
            try
            {
                //ctor = gvsType.GetConstructors()[0];
                ctor = gvsType.GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(string) });
                /*
                if (!doOnce)
                {
                    doOnce = true;
                    string ctorInfo = "";
                    foreach (var item in gvsType.GetConstructors())
                    {
                        ctorInfo += item.Name + "\r\n";
                        foreach (var par in item.GetParameters())
                        {
                            ctorInfo += $"\t({par.ParameterType}) {par.Name}\r\n";
                        }
                        ctorInfo += "\r\n";
                    }

                    Debug.Log(ctorInfo);
                }
                */

                if (ctor == null)
                    throw new Exception("Can't find constructor with necessary signature!");

                var parameters = ctor.GetParameters();
                if (parameters == null || parameters.Length != 4)
                    throw new Exception("Construtor with params doesn't have the right parameters anymore... Unity API change.");
            }
            catch (Exception e)
            {
                ctorException = e;
                try
                {
                    ctor = gvsType.GetConstructors()[0];
                    //ctor = gvsType.GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(string) });

                    if (ctor == null)
                        throw new Exception("Can't even get contructor 0, Unity may have removed functions?");

                    var parameters = ctor.GetParameters();
                    if (parameters == null || parameters.Length != 4)
                        throw new Exception("Construtor 0 doesn't have the right parameters anymore... Unity API change.");
                }
                catch (Exception ex)
                {
                    ctorException = ex;
                }
            }

            if (ctor != null)
            {
                try
                {
                    var newSize = ctor.Invoke(new object[] { (int)viewSizeType, width, height, text });
                    addCustomSize.Invoke(group, new object[] { newSize });
                }
                catch (Exception e)
                {
                    Debug.LogError("Something went seriously wrong! Ctor found, but can't invoke! Invoke on " +
                        "UnityEditor.GameViewSize has failed. Please contact naplandgames@gmail.com " +
                        "with a log of messages from the editor.");
                    Debug.LogException(e);
                }
            }
            else
            {
                Debug.LogError("Something went seriously wrong! Ctor not found. Reflection on " +
                        "UnityEditor.GameViewSize has failed. Please contact naplandgames@gmail.com " +
                        "with a log of messages from the editor.");

                if (ctorException != null)
                    Debug.LogException(ctorException);
            }
        }

		public static void RemoveCustomSize(int width, int height)
		{
			GameViewSizeGroupType gvsgt = GetCurrentGroupType();
			RemoveCustomSize(gvsgt, width, height);
		}


		public static void RemoveCustomSize(GameViewSizeGroupType sizeGroupType, int width, int height)
		{
			int index = FindSize(sizeGroupType, width, height);
			if (index < 0)
				return;

			if (!indices.Contains(index))
				return;

			var group = GetGroup(sizeGroupType);

			var isCustomSize = getGroup.ReturnType.GetMethod("IsCustomSize");
			bool isCustomSizeReturn = (bool)isCustomSize.Invoke(group, new object[] { index });
			if (!isCustomSizeReturn)
				return;

			var removeCustomSize = getGroup.ReturnType.GetMethod("RemoveCustomSize");
			removeCustomSize.Invoke(group, new object[] { index });

		}

		public static bool SizeExists(GameViewSizeGroupType sizeGroupType, string text)
		{
			return FindSize(sizeGroupType, text) != -1;
		}

		public static int FindSize(GameViewSizeGroupType sizeGroupType, string text)
		{
			var group = GetGroup(sizeGroupType);
			var getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
			var displayTexts = getDisplayTexts.Invoke(group, null) as string[];
			for (int i = 0; i < displayTexts.Length; i++)
			{
				string display = displayTexts[i];
				int pren = display.IndexOf('(');
				if (pren != -1)
					display = display.Substring(0, pren - 1);
				if (display == text)
					return i;
			}
			return -1;
		}

		public static int GetCurrentSizeIndex()
		{
			var gvWndType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
			var selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex",
					BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var gvWnd = EditorWindow.GetWindow(gvWndType);
			int index = (int)selectedSizeIndexProp.GetValue(gvWnd, null);
			return index;
		}


		public static bool SizeExists(GameViewSizeGroupType sizeGroupType, int width, int height)
		{
			return FindSize(sizeGroupType, width, height) != -1;
		}

		public static int FindSize(GameViewSizeGroupType sizeGroupType, int width, int height)
		{
			var group = GetGroup(sizeGroupType);
			var groupType = group.GetType();
			var getBuiltinCount = groupType.GetMethod("GetBuiltinCount");
			var getCustomCount = groupType.GetMethod("GetCustomCount");
			int sizesCount = (int)getBuiltinCount.Invoke(group, null) + (int)getCustomCount.Invoke(group, null);
			var getGameViewSize = groupType.GetMethod("GetGameViewSize");
			var gvsType = getGameViewSize.ReturnType;
			var widthProp = gvsType.GetProperty("width");
			var heightProp = gvsType.GetProperty("height");
			var indexValue = new object[1];
			for (int i = 0; i < sizesCount; i++)
			{
				indexValue[0] = i;
				var size = getGameViewSize.Invoke(group, indexValue);
				int sizeWidth = (int)widthProp.GetValue(size, null);
				int sizeHeight = (int)heightProp.GetValue(size, null);
				if (sizeWidth == width && sizeHeight == height)
					return i;
			}
			return -1;
		}


		static bool SizeExists(int index)
		{
			GameViewSizeGroupType sizeGroupType = GetCurrentGroupType();
			var group = GetGroup(sizeGroupType);
			var groupType = group.GetType();
			var getBuiltinCount = groupType.GetMethod("GetBuiltinCount");
			var getCustomCount = groupType.GetMethod("GetCustomCount");
			int sizesCount = (int)getBuiltinCount.Invoke(group, null) + (int)getCustomCount.Invoke(group, null);

			if (index >= 0 && index < sizesCount)
				return true;

			return false;

		}

		static object GetGroup(GameViewSizeGroupType type)
		{
			return getGroup.Invoke(gameViewSizesInstance, new object[] { (int)type });
		}

		public static GameViewSizeGroupType GetCurrentGroupType()
		{
			var getCurrentGroupTypeProp = gameViewSizesInstance.GetType().GetProperty("currentGroupType");
			return (GameViewSizeGroupType)(int)getCurrentGroupTypeProp.GetValue(gameViewSizesInstance, null);
		}

		[HideInInspector]
		public static bool wasMaximized = false;
		public static void MaximizeGameView(bool on)
		{
			if (!on)
			{
				Debug.LogError("Minimizing gameview while in play mode results in crash!");
				return;
			}
			Assembly assembly = typeof(EditorWindow).Assembly;
			Type type = assembly.GetType("UnityEditor.GameView");
			EditorWindow gameview = EditorWindow.GetWindow(type, true);
			if (gameview == null)
			{
				Debug.LogError("game view not found!");
				return;
			}

			if (on)
				wasMaximized = gameview.maximized;
			else
				on = wasMaximized;


			gameview.maximized = on;
		}

		public static string SelectFileFolder(string folder, string defaultName)
		{
			string path = EditorUtility.SaveFolderPanel("Select where to save your screenshots", folder, defaultName);
			return path;
		}

		public static void SetSize(int width, int height)
		{
			GameViewSizeGroupType currentType = GetCurrentGroupType();
			bool addToList = false;
			if (!SizeExists(currentType, width, height))
			{
				string viewName = GetViewName(width, height);
				AddCustomSize(GameViewSizeType.FixedResolution, currentType, width, height, viewName);
				addToList = true;
			}

			int index = FindSize(currentType, width, height);
			if (index < 0)
				Debug.LogError("Cannot find Game View Size for width: " + width.ToString() + "  height: " + height.ToString());
			else
			{
				if (addToList)
					indices.Add(index);

				SetSize(index);
			}
		}

		static string GetViewName(int width, int height)
		{
			return "SSHelper " + width.ToString() + "x" + height.ToString();
		}

		public static Vector2 GetMainGameViewSize()
		{
			Type T = Type.GetType("UnityEditor.GameView,UnityEditor");
			MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", BindingFlags.NonPublic | BindingFlags.Static);
			System.Object Res = GetSizeOfMainGameView.Invoke(null, null);
			return (Vector2)Res;
		}
#endif
	}
}