using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CameraVmdAgent
{
    string _vmdFile;
    MMD.VMD.VMDFormat format_;

	public CameraVmdAgent(string filePath)
    {
        _vmdFile = filePath;

    }

    public void CreateAnimationClip()
    {
		if (null == format_)
		{
			format_ = VMDLoaderScript.Import(_vmdFile);
		}

		AnimationClip animation_clip = VMDCameraConverter.CreateAnimationClip(format_);
		if (animation_clip == null)
		{
			throw new System.Exception("Cannot create AnimationClip");
		}

		string vmd_folder = Path.GetDirectoryName(_vmdFile);
		string anima_file = Path.Combine(vmd_folder, animation_clip.name + ".anim");

		if (File.Exists(anima_file))
			AssetDatabase.DeleteAsset(anima_file);

		AssetDatabase.CreateAsset(animation_clip, anima_file);
		
		AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(animation_clip));

		//AssetDatabase.Refresh();
	}
}
