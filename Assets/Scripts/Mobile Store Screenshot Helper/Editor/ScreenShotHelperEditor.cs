using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEditor;

namespace NG
{
	[CustomEditor(typeof(ScreenshotHelper))]
	public class ScreenShotHelperEditor : Editor
	{
		static List<bool> foldoutState = new List<bool>();

        #region GUI Assets
        Texture shutterIcon;
        Texture ShutterIcon
        {
            get
            {
                if (shutterIcon == null)
                    shutterIcon = AssetDatabase.LoadAssetAtPath<Texture>(
                        "Assets/ScreenshotHelper/Editor/shutter_icon.png");
                return shutterIcon;
            }
        }

        Texture editIcon;
        Texture EditIcon
        {
            get
            {
                if (editIcon == null)
                    editIcon = AssetDatabase.LoadAssetAtPath<Texture>(
                        "Assets/ScreenshotHelper/Editor/edit_icon.png");
                return editIcon;
            }
        }

        Texture closeIcon;
        Texture CloseIcon
        {
            get
            {
                if (closeIcon == null)
                    closeIcon = AssetDatabase.LoadAssetAtPath<Texture>(
                        "Assets/ScreenshotHelper/Editor/close_icon.png");
                return closeIcon;
            }
        }


        Texture deleteIcon;
        Texture DeleteIcon
        {
            get
            {
                if (deleteIcon == null)
                    deleteIcon = AssetDatabase.LoadAssetAtPath<Texture>(
                        "Assets/ScreenshotHelper/Editor/delete_icon.png");
                return deleteIcon;
            }
        }

        Texture addIcon;
        Texture AddIcon
        {
            get
            {
                if (addIcon == null)
                    addIcon = AssetDatabase.LoadAssetAtPath<Texture>(
                        "Assets/ScreenshotHelper/Editor/add_icon.png");
                return addIcon;
            }
        }

        Texture openIcon;
        Texture OpenIcon
        {
            get
            {
                if (openIcon == null)
                    openIcon = AssetDatabase.LoadAssetAtPath<Texture>(
                        "Assets/ScreenshotHelper/Editor/open_icon.png");
                return openIcon;
            }
        }

        Texture saveIcon;
        Texture SaveIcon
        {
            get
            {
                if (saveIcon == null)
                    saveIcon = AssetDatabase.LoadAssetAtPath<Texture>(
                        "Assets/ScreenshotHelper/Editor/save_icon.png");
                return saveIcon;
            }
        }
        
        Texture folderIcon;
        Texture FolderIcon
        {
            get
            {
                if (folderIcon == null)
                    folderIcon = AssetDatabase.LoadAssetAtPath<Texture>(
                        "Assets/ScreenshotHelper/Editor/folder_icon.png");
                return folderIcon;
            }
        }

        GUILayoutOption buttonHeight;
        GUILayoutOption ButtonHeight
        {
            get
            {
                if (buttonHeight == null)
                    buttonHeight = GUILayout.Height(20);
                return buttonHeight;
            }
        }
        #endregion GUI Assets

        void OnEnable()
		{
			ScreenshotHelper ssHelper = (ScreenshotHelper)target;

			bool loadConfig = false;
			if (ssHelper.shotInfo.Count == 0 || !string.IsNullOrEmpty(ssHelper.configFile))
			{

				if (!string.IsNullOrEmpty(ssHelper.configFile))
				{
					if (File.Exists(ssHelper.configFile))
						loadConfig = true;
				}

				if (loadConfig)
				{
					LoadPresetFile(ssHelper.configFile, ssHelper);
				}
				else
				{
					ssHelper.SetDefaults();
				}
			}

			if (!loadConfig)
			{
				LoadPresetFile(SSHPreset.DefaultSavePath(), ssHelper);
				if (File.Exists(SSHPreset.DefaultSavePath()))
				{
					File.Delete(SSHPreset.DefaultSavePath());
				}
			}

			ssHelper.PathChange = PathChange;
			//myTarget.DefaultsSet = SaveSettings;
		}


		void PathChange(string newPath)
		{
			Debug.Log("PathChange to " + newPath);
			ScreenshotHelper myTarget = (ScreenshotHelper)target;
			myTarget.savePath = newPath;
			SSHPreset sshPreset = new SSHPreset();
			sshPreset.Save(myTarget);
		}


		void OnApplicationQuit()
		{
			ScreenshotHelper myTarget = (ScreenshotHelper)target;
			SSHPreset sshPreset = new SSHPreset();
			sshPreset.Save(myTarget);
		}

        static bool didStartTaking = false;
        
		public override void OnInspectorGUI()
		{
			ScreenshotHelper ssHelper = (ScreenshotHelper)target;

            serializedObject.Update();
            

            var cameraArrayProp = serializedObject.FindProperty("cameras");

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                cameraArrayProp, 
                new GUIContent("Cameras (leave empty to autodetect)"), true);

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            
            RenderTextureToggleAndWarning(ssHelper);

			CameraSolidColorTransparencyWarning(ssHelper);

			MakeSpace(1);

			EditorGUI.BeginChangeCheck();

			MakeSpace(1);
			ssHelper.orientation = (SSHOrientation)EditorGUILayout.EnumPopup(
                "Orientation", ssHelper.orientation);

			if (EditorGUI.EndChangeCheck())
				ssHelper.UpdateDimensions();

			//sizes header
			MakeSpace(1);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Screen Shot Sizes", EditorStyles.boldLabel);
			EditorGUILayout.EndHorizontal();

            ssHelper.fileNamePrefix = 
                EditorGUILayout.TextField("Filename prefix", ssHelper.fileNamePrefix);
			MakeSpace(1);

			SetSizesSubs(ssHelper);

			//add a size
			EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(ssHelper.IsTakingShots);
            if (GUILayout.Button(new GUIContent(" Add a size", AddIcon), ButtonHeight))
			{
				ssHelper.shotInfo.Add(new ShotSize(0, 0));
			}
            
            //ssHelper.didPressEditorScreenshotButton = false; didStartTaking = false;

            if (GUILayout.Button(new GUIContent("Take Shots!", ShutterIcon)))
            {
                EditorApplication.isPlaying = true;
                ssHelper.didPressEditorScreenshotButton = true;
            }

            //Debug.LogFormat("didPress: {0}  isPlay: {1}", ssHelper.didPressEditorScreenshotButton, EditorApplication.isPlaying);

            if (ssHelper.didPressEditorScreenshotButton && EditorApplication.isPlaying)
            {
                didStartTaking = true;
                ssHelper.didPressEditorScreenshotButton = false;

                ssHelper.OnComplete += () => {
                    //Debug.Log("OnComplete");
                    EditorApplication.isPlaying = false;
                };
                //Debug.Log("Call runtake");
                ssHelper.GetScreenShots();
            }

            if (didStartTaking && !EditorApplication.isPlaying)
            {
                didStartTaking = false;
                ssHelper.didPressEditorScreenshotButton = false;
            }

            MakeSpace(3);

            ssHelper.openFolderWhenDone = EditorGUILayout.ToggleLeft(
                "Open folder when done?", 
                ssHelper.openFolderWhenDone);

			EditorGUILayout.HelpBox(
                "In-editor Save location: " + ssHelper.savePath, MessageType.None);

            if (GUILayout.Button(
                new GUIContent(" Set in-editor save location", FolderIcon), ButtonHeight))
			{
				ssHelper.savePath = GameViewUtils.SelectFileFolder(
                    Directory.GetCurrentDirectory(), "");
				PathChange(ssHelper.savePath);
			}

			MakeSpace(1);

			EditorGUILayout.HelpBox(
                "Build Save location example: " + ssHelper.BuildSaveLocation(), MessageType.None);


            if (GUILayout.Button(
                new GUIContent(" Set build save location", FolderIcon), ButtonHeight))
			{
				ScreenshotHelperBuildSaveLocationWindow.ShowWindow(ssHelper);
			}


			MakeSpace(2);

            if (GUILayout.Button(new GUIContent(" Save Presets", SaveIcon), ButtonHeight))
			{
				string newConfig = SavePreset(ssHelper);
				if (!string.IsNullOrEmpty(newConfig))
				{
					ssHelper.configFile = newConfig;
				}
			}

            LoadPresetsButton(ssHelper);

			MakeSpace(2);
            
			if (GUILayout.Button(new GUIContent(" Load Defaults", OpenIcon), ButtonHeight))
			{
				ssHelper.SetDefaults();
				ssHelper.configFile = "";
			}

            EditorGUI.EndDisabledGroup();

            MakeSpace(1);
		}

		void MakeSpace(int numSpaces)
		{
			for (int i = 0; i < numSpaces; i++)
			{
				EditorGUILayout.Space();
			}
		}

		void RenderTextureToggleAndWarning(ScreenshotHelper ssHelper)
		{
			ssHelper.useRenderTexture = 
                EditorGUILayout.ToggleLeft("Use Render to Texture", ssHelper.useRenderTexture);

            if (!ssHelper.useRenderTexture)
            {
                ssHelper.SSHTextureFormat = 
                    (ScreenshotHelper.tSSHTextureFormat)EditorGUILayout.EnumPopup(
                        "Texture Format", ssHelper.SSHTextureFormat);
            }

            string rtMessage = "Render to Texture requires Unity Pro or Unity 5 (or newer). " +
                "\nImages will be scaled with a bilinear scaling method.";
			string message = "Render to Texture provides the best possible resolution.";
			MessageType messageType = MessageType.Warning;

			if (ssHelper.useRenderTexture)
			{
				string canvasWarning = GetIsCanvasesValidMessage();

				if (!GetIsRenderTextureAvailable())
				{
					message = rtMessage;
				}
				else if (!string.IsNullOrEmpty(canvasWarning))
				{
					message = canvasWarning;
				}
                else
                {
                    messageType = MessageType.Info;
                }

			}

			if (ssHelper.useRenderTexture && !GetIsRenderTextureAvailable())
			{
				ssHelper.useRenderTexture = false;

			}

			if (!ssHelper.useRenderTexture && GetIsRenderTextureAvailable())
			{
				message = "Use Render to texture for the best possible resolutions";
			}

			if (!ssHelper.useRenderTexture && !GetIsRenderTextureAvailable())
				message = rtMessage;

			if (!string.IsNullOrEmpty(message))
				EditorGUILayout.HelpBox(message, messageType, true);
		}

        void SetSizesSubs(ScreenshotHelper ssHepler)
		{
            for (int i = 0; i < ssHepler.shotInfo.Count; i++)
			{
				if (foldoutState.Count < i + 1)
				{
					foldoutState.Add(false);
				}

				string fileName = ssHepler.GetScreenShotName(ssHepler.shotInfo[i]);

                EditorGUILayout.BeginHorizontal();

                var icon = EditIcon;
                string edit = "Edit";
                if (foldoutState[i])
                {
                    icon = CloseIcon;
                    edit = "Close";
                }

                if (icon != null)
                    edit = "";

                var editBtnCont = new GUIContent(edit, icon);
                var width = GUILayout.Width(50);
                if (icon != null)
                    width = GUILayout.Width(25);

                if (GUILayout.Button(editBtnCont, width, ButtonHeight))
                {
                    foldoutState[i] = !foldoutState[i];
                }

                EditorGUILayout.LabelField(fileName);//, GUILayout.Width(100));

                width = GUILayout.Width(50);
                var deleteButtonCont = new GUIContent("Delete", DeleteIcon);
                if (DeleteIcon != null)
                {
                    width = GUILayout.Width(25);
                    deleteButtonCont = new GUIContent(DeleteIcon);
                }

                if (GUILayout.Button(deleteButtonCont, width, ButtonHeight))
                {
                    int index = i;
                    ssHepler.shotInfo.Remove(ssHepler.shotInfo[index]);
                    foldoutState.Remove(foldoutState[index]);
                }
                EditorGUILayout.EndHorizontal();

				if (foldoutState[i])
				{
					EditorGUILayout.BeginHorizontal();
					ssHepler.shotInfo[i].width = EditorGUILayout.IntField(
                        "\tWidth: ", ssHepler.shotInfo[i].width);
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
					ssHepler.shotInfo[i].height = EditorGUILayout.IntField(
                        "\tHeight: ", ssHepler.shotInfo[i].height);
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
					ssHepler.shotInfo[i].label = EditorGUILayout.TextField(
                        "\tPrefix: ", ssHepler.shotInfo[i].label);
					EditorGUILayout.EndHorizontal();
				}

			}
		}

		bool GetIsRenderTextureAvailable()
		{
			string unityVersion = UnityEditorInternal.InternalEditorUtility.GetFullUnityVersion();
			string[] major = unityVersion.Split('.');

			if (Convert.ToInt32(major[0]) < 5 && 
                !UnityEditorInternal.InternalEditorUtility.HasPro())
			{
				return false;
			}
			return true;
		}

		string GetIsCanvasesValidMessage()
		{
			Canvas[] canvases = GameObject.FindObjectsOfType<Canvas>();
			string dOut = "";
			dOut = "Canvases need to use ScreenSpaceCamera or Worldspace to properly render to " +
                "texture and must have the main camera attached.\n";
			bool error = false;
			foreach (Canvas c in canvases)
			{
				if (c.renderMode == RenderMode.ScreenSpaceOverlay)
				{
					error = true;
					dOut += "\nCanvas '" + c.gameObject.name + "' is in Screen Space Overlay " +
                        "mode and will not render to texture.";
				}
                
				if (c.worldCamera == null)
				{
					error = true;
					dOut += "\nCanvas '" + c.gameObject.name + "' does not have a camera " +
                        "attached and cannot render to texture.";
				}
			}

			if (error)
				return dOut;

			return null;
		}

		void CameraSolidColorTransparencyWarning(ScreenshotHelper myTarget)
		{
			if (myTarget.useRenderTexture && IsAlphaFormat(myTarget.SSHTextureFormat))
			{
				if (Camera.main.clearFlags == CameraClearFlags.Color ||
					Camera.main.clearFlags == CameraClearFlags.SolidColor)
				{
					if (Camera.main.backgroundColor.a < 1.0)
					{
						EditorGUILayout.HelpBox(
                            "Main camera is using solid color with alpha < 1. This may " +
                            "result in images with transparent backgrounds.", 
                            MessageType.Warning, true);
					}
				}
			}
		}

		void LoadPresetsButton(ScreenshotHelper ssh)
		{
			if (GUILayout.Button(new GUIContent(" Load Presets", openIcon), ButtonHeight))
			{
				string newConfig = EditorUtility.OpenFilePanel(
                    "Select a preset file", Directory.GetCurrentDirectory(), "xml");
				if (!string.IsNullOrEmpty(newConfig))
				{
					if (LoadPresetFile(newConfig, ssh))
					{
						ssh.configFile = newConfig;
					}
				}
			}
		}

		bool LoadPresetFile(string fileName, ScreenshotHelper ssh)
		{
			if (string.IsNullOrEmpty(fileName))
				return false;

			if (!File.Exists(fileName))
				return false;

			SSHPreset sshPreset = SSHPreset.Load(fileName);
			if (sshPreset != null)
			{
				if (sshPreset.sizes.Count > 0)
				{
					ssh.shotInfo = sshPreset.sizes;
					ssh.savePath = sshPreset.lastSavePath;
					ssh.orientation = sshPreset.orientation;
					return true;
				}
			}

			return false;
		}

		string SavePreset(ScreenshotHelper ssh)
		{
			string file = EditorUtility.SaveFilePanel("Save your presets", 
                Directory.GetCurrentDirectory(), "screenshot_helper_preset", "xml");

			SSHPreset preset = new SSHPreset();
			preset.Save(file, ssh);

			return file;
		}

		void SortSizes()
		{
			ScreenshotHelper myTarget = (ScreenshotHelper)target;
			List<ShotSize> shotSizes = myTarget.shotInfo;
			List<string> fileNames = new List<string>();
			for (int i = 0; i < shotSizes.Count; i++)
			{
				fileNames.Add(myTarget.GetScreenShotName(shotSizes[i]));
			}

			fileNames.Sort();
			ShotSize[] tempShotSizes = new ShotSize[shotSizes.Count];

			for (int i = 0; i < fileNames.Count; i++)
			{
				for (int j = 0; j < shotSizes.Count; j++)
				{
					if (myTarget.GetScreenShotName(shotSizes[j]) == fileNames[i])
						tempShotSizes[i] = shotSizes[j];
				}
			}

			myTarget.shotInfo = new List<ShotSize>();
			for (int i = 0; i < tempShotSizes.Length; i++)
			{
				myTarget.shotInfo.Add(tempShotSizes[i]);
			}
		}

		bool IsAlphaFormat(ScreenshotHelper.tSSHTextureFormat textureFormat)
		{
			string tf = textureFormat.ToString().ToLower();
			if (tf.Contains("a"))
				return true;

			return false;
		}
	}
}

