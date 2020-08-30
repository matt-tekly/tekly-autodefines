using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using System.Linq;
using System.IO;
using System.Text;

namespace Tekly.AutoDefines
{
	[Serializable]
	public class AutoDefineJson
	{
		public AutoDefine[] Defines;
		public bool WarningsAsErrors;
	}
	
	[Serializable]
	public class AutoDefine
	{
		public string Define;
		public bool Enabled = true;
	}
	
	[InitializeOnLoad]
	public static class AutoDefinesProcessor
	{
		public const string AUTO_DEFINES_FILE_NAME = "defines.auto.json";
		
		private const string CSC_FILE = "Assets/csc.rsp";
		private const string AUTO_DEFINES_SEARCH = "defines.auto";
		private const string WARNINGS_AS_ERRORS = "-warnaserror+";
		
		static AutoDefinesProcessor()
		{
			CompilationPipeline.compilationStarted += OnCompilationStarted;
			BuildCscFile();
		}
		
		[MenuItem("Tools/Tekly/Build Auto Defines")]
		public static void BuildCscFile()
		{
			var autoDefines = GatherAutoDefines();

			if (autoDefines.Length == 0) {
				return;
			}

			var defines = autoDefines.SelectMany(autoDefine => autoDefine.Defines)
				.Where(x => x.Enabled)
				.Select(x => x.Define)
				.ToArray();
			
			var content = new StringBuilder();
			if (defines.Length > 0) {
				content.AppendLine("-define:" + string.Join(";", defines));	
			}
			
			var warningsAsErrors = autoDefines.Any(autoDefine => autoDefine.WarningsAsErrors);
			if (warningsAsErrors) {
				content.AppendLine(WARNINGS_AS_ERRORS);
			}
			
			var originalCscFile = string.Empty;

			if (File.Exists(CSC_FILE)) {
				originalCscFile = File.ReadAllText(CSC_FILE);
			}

			var newCscFile = content.ToString();

			if (newCscFile != originalCscFile) {
				File.WriteAllText(CSC_FILE, newCscFile);
				AssetDatabase.Refresh(ImportAssetOptions.Default);	
			}
		}
		
		private static void OnCompilationStarted(object context)
		{
			BuildCscFile();
		}

		private static AutoDefineJson[] GatherAutoDefines()
		{
			var assetPaths = AssetDatabase.FindAssets(AUTO_DEFINES_SEARCH, new[] { "Assets", "Packages" })
				.Select(AssetDatabase.GUIDToAssetPath)
				.ToArray();

			if (assetPaths.Length == 0) {
				return new AutoDefineJson[0];
			}

			return assetPaths.Select(LoadAutoDefineJson)
				.Where(x => x != null)
				.ToArray();
		}

		private static AutoDefineJson LoadAutoDefineJson(string path)
		{
			try {
				var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
				return JsonUtility.FromJson<AutoDefineJson>(textAsset.text);
			} catch (Exception exception) {
				Debug.LogError($"Failed to load Auto Define Json at path: [{path}]");
				Debug.LogException(exception);
			}

			return null;
		}
	}
}