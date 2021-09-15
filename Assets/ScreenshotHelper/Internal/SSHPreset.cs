using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace NG
{
	[XmlRoot("SSHPreset")]
	public class SSHPreset
	{
		public SSHOrientation orientation = SSHOrientation.portrait;
		public string lastSavePath = "";
		public ScreenshotHelper.tSSHTextureFormat textureFormat = 
            ScreenshotHelper.tSSHTextureFormat.RGB24;
		public KeyCode keyToPress = KeyCode.S;
		public KeyCode keyToHold = KeyCode.LeftShift;
		public Environment.SpecialFolder buildPathRoot;
		public string buildPathExtra;

		[XmlArray("sizes")]
		[XmlArrayItem("size")]
		public List<ShotSize> sizes = new List<ShotSize>();

		public void Save(ScreenshotHelper ssh)
		{
			string fileName = DefaultSavePath();
			Save(fileName, ssh);
		}

		public void Save(string fileName, ScreenshotHelper ssh)
		{
			if (string.IsNullOrEmpty(fileName))
				return;
			if (ssh.shotInfo.Count <= 0)
			{
				var tempDelegate = ssh.DefaultsSet;
				ssh.DefaultsSet = null;
				ssh.SetDefaults();
				ssh.DefaultsSet = tempDelegate;
			}

			sizes = ssh.shotInfo;
			orientation = ssh.orientation;
			lastSavePath = ssh.savePath;
			buildPathRoot = ssh.buildSavePathRoot;
			buildPathExtra = ssh.buildSavePathExtra;
			textureFormat = ssh.SSHTextureFormat;

			var serializer = new XmlSerializer(typeof(SSHPreset));
			using (var stream = new FileStream(fileName, FileMode.Create))
				serializer.Serialize(stream, this);
		}

		public static SSHPreset Load(string fileName)
		{
			if (File.Exists(fileName))
			{
				var serializer = new XmlSerializer(typeof(SSHPreset));
				using (var stream = new FileStream(fileName, FileMode.Open))
					return serializer.Deserialize(stream) as SSHPreset;
			}

			return null;
		}

		public static string DefaultSavePath()
		{
			string path = Application.persistentDataPath + @"/sshdefaults.xml";
			return path;
		}
	}
}