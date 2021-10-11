VERSION 3.3 - 2019-05-16

Screenshot Helper is a simple tool that will allow you to take all the screenshots you need for Google Play and iOS App Store at the click of a button (or call of a function)!

Simply attach the "ScreenshotHelper.cs" script onto an empty game object then press the "Take Shots!" button in the inspector.

If you're using Unity Pro or Unity 5 or newer then use Render to Texture for the best resolution possible. If you do not use Render to Texture the images will be scaled with a bilinear scaling method.

You can easily switch orientations and all of the sizes and their file names will be automatically updated. You can add / remove sizes, save the presets to a file to use in other projects, and set a prefix for the file names. 
This utility can run from the Editor or from a Standalone build.

You can also call ScreenshotHelper.instance.GetScreenShots() from any other script so that you can code in where to have the screenshots taken for a more automated process.

You can use the ScreenshotHelper delegate "OnScreenChanged" to detect when the resolution has changed so that you can adjust the positions of objects in your game.
You can use the ScreenshotHelper action "OnComplete" to detect when a batch of screenshots are done.

The ScreenshotHelper class is a persistent singleton so if you put it in your game's first scene it will remain loaded in all other scenes (i.e. it does not get destroyed on level load). You can safely place it in multiple scenes and only one instance will persist.

You can see this all at work in the provided samplescene. Just press "Take Shots!" in the inspector and watch the magic!

You can save your presets to an XML file for portability between projects!

USAGE NOTES:
- If you aren't using Unity 5 or Unity Pro then you won't be able to "use render texture" option since it is not supported. 
- In the non render texture method the images are scaled up with bilinear scaling. So make sure to have Unity maximized and your game view window maximized for best resolution.
- If you are using render texture you will have to make sure that all of you GUI canvases have the main camera attached as the Render Camera otherwise the render texture cannot capture them. You can use the non render texture method to capture everything on screen at the cost of some quality (depending on your monitor's resolution).
- If you use placement of objects on your screen that is based on screen dimensions then make sure to adjust those by using the OnScreenChanged delegate method.
- If using this in a standalone build then for best results launch the game in windowed mode.
- If for some reason you're having issues with multiple camera textures combining when using the THREADED option then you can change this by commenting out "#define USE_THREADING" in the top of the ScreenshotHelper.cs file.

KNOWN ISSUES:
- Sometimes there's issues with the GUI match width/height setting that leaves an offest on the left side of the image. If this happens to you please send naplandgames@gmail.com any info that you can so I can try to work around this. The only currently known workaround is to set the canvas scalar to "expand".
- With some beta versions of Unity they do not include all of the assembly for UnityEditor.GameViewSize (or maybe the meta data). This means that GameViewUtils.AddCustomSize method may fail. I've added some workarounds for this, but cannot guaranty that it works since it's very difficult to reproduce the issue. However, I've placed some catches to attempt to work around the issue. If you still see this issue let me know at naplandgames@gmail.com. Please note that I do not support or test against Unity beta or alpha versions.

REVISION NOTES:
3.3
- Fix to inspector not showing "cameras" array properly
- Fix to non-render to texture mode only running one screenshot.

3.2
- FIX to build compiler issues (missing members due to preprocessor defines - Sorry!)

3.1
- FIX Should now be able to support multiple Unity versions with different constructors for GameViewSize.

3.0
- NEW! No longer limited to a single camera. Multiple cameras will be rendered in depth order then textures will be composited into a single texture. This is done on a separate thread so it is as fast as possible!
- Lots of cleanup and refactoring. Current users it is suggested to fully remove any existing copy of this plugin from your project before importing this version.
- Improved the Inspector so that it's more user friendly. 
	- You can now take shots from the inspector without pressing Play.
	- You can now select "both" to take portrait and landscape shots.
	- You can now add a prefix to all of the files as well as each individual file.
	- You can now turn off the automatic opening of the shots folder if you wish.

2.5
- Encapsulated classes into namespace (NG) and conformed to C# styling.
- Added more sizes for newer devices to the defaults.
- Added ability to assign specific camera.

2.4
- Added recording of view size indices that are created. This fixes the tool from removing custom screen sizes that match the ones created by the tool.
- Added iPad Pro resolution. Fixed resolution for iPhone 6. Labeled them all in ScreenshotHelper.cs.
- Added Texture Format option to easily disallow creating textures with alpha channels.

2.3
- Better management of screenshot hotkey. Can now be set from the custom inspector via dropdowns.
- Better management of save locations (build and in-editor). Build save locations can be selected from a dropdown of standard system locations that are accessible to your game.

2.2
- Fixed issue with scripts not working in Standalone builds. 
- Better management of screen sizes and you can now add a prefix to the individual file name for each size.
- Save and Load Presets from a file.
- You now select the save location from the editor.
- Prefab has been removed so that the new custom inspector works well.
- Updated package so that it is compatible with Unity 4.6 or newer.

2.1 
- Added render to texture with Unity 5 or Unity Pro for higher quality screenshots. 
- You can now run the script from the Editor in Play Mode. 
- You can now select the folder you want to save to.
