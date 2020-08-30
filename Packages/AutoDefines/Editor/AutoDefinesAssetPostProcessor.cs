using System.Linq;
using UnityEditor;

namespace Tekly.AutoDefines
{
	public class AutoDefinesAssetPostProcessor : AssetPostprocessor
	{
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			bool needsProcessing = importedAssets.Any(x => x.EndsWith(AutoDefinesProcessor.AUTO_DEFINES_FILE_NAME));
			needsProcessing = needsProcessing || deletedAssets.Any(x => x.EndsWith(AutoDefinesProcessor.AUTO_DEFINES_FILE_NAME));
			if (needsProcessing) {
				AutoDefinesProcessor.BuildCscFile();
			}
		}
	}
}