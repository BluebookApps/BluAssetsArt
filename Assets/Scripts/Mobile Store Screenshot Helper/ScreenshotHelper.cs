//#undef UNITY_EDITOR
//#define SSH_DIAG

#define USE_THREADING // comment out if you're having issues with threads executing.

#if SSH_DIAG
using System.Diagnostics;
using Debug = UnityEngine.Debug;
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

namespace NG
{
	public class ScreenshotHelper : MonoBehaviour
	{
		public delegate void ScreenChange();
		public ScreenChange OnScreenChanged;
        Queue<Action> unityThreadQueue = new Queue<Action>();
        public Action OnComplete;
        public bool useRenderTexture = true;
		public enum tSSHTextureFormat { ARGB32, RGB24, RGBAFloat, RGBAHalf }
		public tSSHTextureFormat SSHTextureFormat = tSSHTextureFormat.RGB24;

        public Camera[] cameras;

        public string fileNamePrefix;

        public bool openFolderWhenDone;

        public bool didPressEditorScreenshotButton;

		private TextureFormat textureFormat
		{
			get
			{
				switch (SSHTextureFormat)
				{
					case tSSHTextureFormat.ARGB32:
						return TextureFormat.ARGB32;
					case tSSHTextureFormat.RGBAFloat:
						return TextureFormat.RGBAFloat;
					case tSSHTextureFormat.RGBAHalf:
						return TextureFormat.RGBAHalf;
					case tSSHTextureFormat.RGB24:
					default:
						return TextureFormat.RGB24;
				}
			}
		}
        
		private int Depth
		{
			get
			{
                switch (SSHTextureFormat)
                {
                    case tSSHTextureFormat.ARGB32:
                        return 32;
                    case tSSHTextureFormat.RGBAFloat:
                        return 32;
                    case tSSHTextureFormat.RGBAHalf:
                        return 16;
                    case tSSHTextureFormat.RGB24:
                    default:
                        return 24;
                }
            }
		}

		public SSHOrientation orientation = SSHOrientation.portrait;
		public List<ShotSize> shotInfo = new List<ShotSize>();

#pragma warning disable 0414
		private ShotSize maxRes;
#pragma warning restore 0414

		public string savePath = "";
		public Environment.SpecialFolder buildSavePathRoot = Environment.SpecialFolder.MyPictures;
		public string buildSavePathExtra = "screenshots";
		public string configFile = "";

		

		private static ScreenshotHelper _instance;
		public static ScreenshotHelper instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = GameObject.FindObjectOfType<ScreenshotHelper>();
					if (Application.isPlaying)
						DontDestroyOnLoad(_instance.gameObject);
				}

				return _instance;
			}
		}

		void Awake()
		{
            if (OnComplete == null)
                OnComplete += () => { };

			if (_instance == null)
			{
				_instance = this;
				DontDestroyOnLoad(this);
			}
			else
			{
				if (this != _instance)
				{
					Destroy(gameObject);
				}
			}

#if !UNITY_EDITOR
			savePath = BuildSaveLocation();
#endif
			maxRes = new ShotSize(Screen.currentResolution.width, Screen.currentResolution.height);
		}

		public string BuildSaveLocation()
		{
			return Environment.GetFolderPath(buildSavePathRoot) + "/" + buildSavePathExtra;
		}


		void Update()
		{
            while (unityThreadQueue.Count > 0)
            {
                try
                {
                    unityThreadQueue.Dequeue().Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }       
            }
        }


		public void UpdateDimensions()
		{
            if (orientation == SSHOrientation.both)
            {
                List<ShotSize> newShotSizes = new List<ShotSize>();
                for (int i = 0; i < shotInfo.Count; i++)
                {
                    bool foundOpposite = false;
                    for (int j = 0; j < shotInfo.Count; j++)
                    {
                        if ((shotInfo[i].width == shotInfo[j].height) &&
                            (shotInfo[i].height == shotInfo[j].width))
                        {
                            foundOpposite = true;
                        }
                    }

                    if (!foundOpposite)
                    {
                        newShotSizes.Add(new ShotSize(shotInfo[i].height, shotInfo[i].width));
                    }

                    newShotSizes.Add(shotInfo[i]);
                }
                shotInfo = newShotSizes;
            }
            else
            {
                for (int i = 0; i < shotInfo.Count; i++)
                {
                    if (orientation == SSHOrientation.landscape &&
                        shotInfo[i].height > shotInfo[i].width)
                    {
                        int temp = shotInfo[i].width;
                        shotInfo[i].width = shotInfo[i].height;
                        shotInfo[i].height = temp;
                    }
                    else if (orientation == SSHOrientation.portrait &&
                        shotInfo[i].width > shotInfo[i].height)
                    {
                        int temp = shotInfo[i].width;
                        shotInfo[i].width = shotInfo[i].height;
                        shotInfo[i].height = temp;
                    }
                }
            }

            // remove duplicates
            shotInfo = shotInfo.Distinct(new ShotSizeComparer()).ToList<ShotSize>();            
		}

		private void WarnCanvases()
		{
			Canvas[] canvases = FindObjectsOfType<Canvas>();
			List<string> dOut = new List<string>();
			dOut.Add("Canvases need to use ScreenSpaceCamera or Worldspace to properly render " +
                "to texture and must have the main camera attached.");
			bool error = false;
			foreach (Canvas c in canvases)
			{
				if (c.renderMode == RenderMode.ScreenSpaceOverlay)
				{
					error = true;
					dOut.Add("Canvas " + c.gameObject.name + 
                        " is in Screen Space Overlay mode and will not render to texture.");
				}

				if (c.worldCamera == null)
				{
					error = true;
					dOut.Add("Canvas " + c.gameObject.name + 
                        " does not have a camera attached and cannot render to texture.");
				}
			}

			if (error)
			{
				foreach (string s in dOut)
				{
					SSHDebug.LogWarning(s);
				}
			}
		}

		public void GetScreenShots()
		{
			StartCoroutine(TakeScreenShots());
		}


		public int GetFileNum(string typeExt)
		{
			if (!Directory.Exists(savePath))
				return 0;

			string[] files = Directory.GetFiles(savePath, "*" + typeExt + "*");

			int min = -1;
			foreach (var fileName in files)
			{
				string baseName = Path.GetFileNameWithoutExtension(fileName);
				string[] splitString = baseName.Split('_');

				if (splitString.Length > 0)
				{
					int thisNum = Convert.ToInt32(splitString[splitString.Length - 1]);

					if (thisNum > min)
						min = thisNum;
				}
			}


			return min + 1;
		}

		public delegate void PathChangeDelegate(string newPath);
		public PathChangeDelegate PathChange;
        ShotSize initialRes;
        int shotCounter;
		public bool IsTakingShots { get; private set; }
#if SSH_DIAG
        Stopwatch stopwatch = null;
#endif

		IEnumerator TakeScreenShots()
		{
            if (IsTakingShots)
            {
                yield break;
            }

            IsTakingShots = true;


#if UNITY_EDITOR
            if (!Directory.Exists(savePath))
			{
				string newPath = GameViewUtils.SelectFileFolder(
                    Directory.GetCurrentDirectory(), "");
				if (!string.IsNullOrEmpty(newPath))
				{
					savePath = newPath;
					if (PathChange != null)
						PathChange(newPath);
				}
			}
#endif

			if (!Directory.Exists(savePath))
				Directory.CreateDirectory(savePath);

			float timeScaleStart = Time.timeScale;
			Time.timeScale = 0f;

#pragma warning disable 0219
			initialRes = new ShotSize(Screen.width, Screen.height);
#pragma warning restore 0219

			string fileName = "";

#if UNITY_EDITOR
			int currentIndex = GameViewUtils.GetCurrentSizeIndex();
            //Maximize game view seems to cause crashes 
            //GameViewUtils.MaximizeGameView(true);
#else
            int currentIndex = 0;
#endif

#if SSH_DIAG
            stopwatch = new Stopwatch();
            stopwatch.Start();
#endif
            shotCounter = shotInfo.Count;
            foreach (var shot in shotInfo)    
            {
                
				fileName = GetScreenShotName(shot);



#if UNITY_EDITOR
				GameViewUtils.SetSize(shot.width, shot.height);
				if (OnScreenChanged != null)
				{
					yield return new WaitForEndOfFrame();
					OnScreenChanged();
					yield return new WaitForEndOfFrame();
				}
				Canvas.ForceUpdateCanvases();
#else

				float ratio = (1f * shot.width) / (1f * shot.height);
				SSH_IntVector2 thisRes = new SSH_IntVector2(shot.width , shot.height);
            

				if (shot.height > maxRes.height)
				{
					thisRes.width = Mathf.FloorToInt(maxRes.height * ratio);
					thisRes.height = maxRes.height;
				}

				Screen.SetResolution(thisRes.width, thisRes.height, false);
				Canvas.ForceUpdateCanvases();
				yield return new WaitForEndOfFrame();

				int attempts = 0;
				while (Screen.width != thisRes.width && attempts < 10)
				{
					Screen.SetResolution(thisRes.width, thisRes.height, false);
					Canvas.ForceUpdateCanvases();
					yield return new WaitForEndOfFrame();
					attempts++;
				}
#endif

				yield return new WaitForEndOfFrame();
				yield return new WaitForEndOfFrame();
				Texture2D tex = null;

                if (cameras == null || cameras.Length <= 0)
                    cameras = FindObjectsOfType<Camera>();

                if (useRenderTexture)
				{
                    // sort cameras by depth lowest to heighest
                    cameras = cameras.OrderBy(x => x.depth).ToArray();

#if USE_THREADING
                    // Threaded with 2 cameras takes 0.5s per shot (13 shots at ~7s).
                    var colors = SSHUtil.GetCameraTexturesColors(
                        cameras, shot, TextureFormat.ARGB32, Depth);
                    // unbox
                    string shotFileName = fileName;
                    var sshot = shot;

                    SSHUtil.CombineTexturesThread(colors, (result) => {
                        unityThreadQueue.Enqueue(() =>
                        {
                            var tempText = new Texture2D(
                                sshot.width, sshot.height, textureFormat, false);
                            tempText.SetPixels(result);
                            tempText.Apply();
                            SSHUtil.SaveTexture(tempText, savePath, shotFileName);
                            shotCounter--;

                            if (shotCounter == 0)
                                RunFinishRoutine(currentIndex, timeScaleStart);
                        });
                    });
#else
                    // Non-threaded with 2 cameras takes ~1.5sec per shot (13 shots at 19.51s).
                    tex = SSHUtil.CombineCamerasNonThreaded(
                        cameras, shot, TextureFormat.ARGB32, Depth);
                    if (tex == null)
                        SSHDebug.LogError("Something went wrong! Texture is null");
                    else
                    {
                        SSHUtil.SaveTexture(tex, savePath, fileName);
                        shotCounter--;
                        if (shotCounter == 0)
                            RunFinishRoutine(currentIndex, timeScaleStart);
                    }
#endif
                }
                else
				{
                    // Non-render texture method with bilinear scaling.
                    tex = SSHUtil.GetScreenBilinearScaling(textureFormat, shot);
                    if (tex != null)
                    {
                        SSHUtil.SaveTexture(tex, savePath, fileName);
                        shotCounter--;
                        if (shotCounter == 0)
                            RunFinishRoutine(currentIndex, timeScaleStart);
                    }
                    else
                    {
                        SSHDebug.LogError("Something went wrong! Texture is null");
                    }
                }
			}
		}

        void RunFinishRoutine(int currentIndex, float timeScaleStart)
        {
            StartCoroutine(FinishRoutine(currentIndex, timeScaleStart));
        }

        IEnumerator FinishRoutine(int currentIndex, float timeScaleStart)
        {
#if UNITY_EDITOR
            // Don't maximize game view... causes crash
            //GameViewUtils.MaximizeGameView(false);
            GameViewUtils.SetSize(currentIndex);
            if (OnScreenChanged != null)
            {
                yield return new WaitForEndOfFrame();
                OnScreenChanged();
                yield return new WaitForEndOfFrame();
            }
            RemoveAllCustomSizes();

#else
	        Screen.SetResolution(initialRes.width, initialRes.height, false);
#endif

#if SSH_DIAG
            Debug.LogFormat("Total shots: {0}  Total time: {1:N2}s  Avg time: {2:N2}s",
                shotInfo.Count, 
                (double)stopwatch.ElapsedMilliseconds / 1000f, 
                (double)stopwatch.ElapsedMilliseconds / (1000f * shotInfo.Count));
#endif
            SSHDebug.Log("Screenshots saved to " + savePath);
            if (openFolderWhenDone)
            {
                Application.OpenURL(savePath);
            }
            IsTakingShots = false;

            yield return new WaitForEndOfFrame();

            Time.timeScale = timeScaleStart;
            OnComplete.Invoke();
        }

		private void RemoveAllCustomSizes()
		{
#if UNITY_EDITOR
			foreach (ShotSize shot in shotInfo)
			{
				GameViewUtils.RemoveCustomSize(shot.width, shot.height);
			}
#endif
		}

		public string GetScreenShotName(ShotSize shot)
		{
			string ext = shot.GetFileNameBase(); 
            //shot.width.ToString() + "x" + shot.height.ToString();
			int num = GetFileNum(ext);

			string pre = "";
            if (!string.IsNullOrEmpty(fileNamePrefix))
                pre = fileNamePrefix + "_";

			if (!string.IsNullOrEmpty(shot.label))
				pre += shot.label + "_";
			return pre + ext + "_" + num.ToString() + ".png";
		}

		public delegate void DefaultsSetDelegate();
		public DefaultsSetDelegate DefaultsSet;
		public void SetDefaults()
		{
			orientation = SSHOrientation.portrait;
            openFolderWhenDone = true;
			shotInfo.Clear();
			shotInfo.Add(new ShotSize(640, 960)); // iPhone 4 - 3.5" retina
			shotInfo.Add(new ShotSize(640, 1136)); // iPhone 5S and SE - 4" retina
			shotInfo.Add(new ShotSize(750, 1334)); // iPhone 6, 7, and 8 - 4.7" retina
			shotInfo.Add(new ShotSize(946, 2048)); // iPhone X - 5.8" retina
            shotInfo.Add(new ShotSize(1080, 1920)); // Nexus 5 / Pixel 2, iPhone 7, 7, and 8 PLUS (5.5")
			shotInfo.Add(new ShotSize(1080, 2160)); // Pixel 3
            shotInfo.Add(new ShotSize(1440, 2880)); // Pixel 2 XL
			shotInfo.Add(new ShotSize(1440, 2960)); // Pixel 3 XL
            shotInfo.Add(new ShotSize(1600, 2560)); // Galaxy Tab Pro
			shotInfo.Add(new ShotSize(1200, 1920)); // Nexus 7
			shotInfo.Add(new ShotSize(1242, 2208)); // iPhone 6 Plus - 5.5" retina
			shotInfo.Add(new ShotSize(1536, 2048)); // iPad 3 / 4 (OK for iPad mini)
			shotInfo.Add(new ShotSize(1125, 2436)); // 5.8" super retina
			shotInfo.Add(new ShotSize(2048, 2732)); // iPad Pro - 12.9" retina
			shotInfo.Add(new ShotSize(1668, 2224)); // 10.5" retina
			shotInfo.Add(new ShotSize(1496, 2048)); // 9.7" retina

            shotInfo = shotInfo.OrderBy(shot => shot.width).ToList();

			savePath = "";

			buildSavePathExtra = "screenshots";
			buildSavePathRoot = Environment.SpecialFolder.MyPictures;

			if (DefaultsSet != null)
				DefaultsSet();
		}
	}
}
