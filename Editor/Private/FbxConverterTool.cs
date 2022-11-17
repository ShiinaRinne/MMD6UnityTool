using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class FbxConverterTool
{
    //将Fbx内的动画文件分离出来
    public static void SpliteFbxMotion2Anim(string fbxPath)
    {
        Object[] objects = AssetDatabase.LoadAllAssetsAtPath(fbxPath);        //加载FBX里所有物体
        if (objects != null && objects.Length > 0)
        {
            string targetPath = Application.dataPath + "/AnimationClip";          //目录AnimationClip
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);     //如果目录不存在就创建一个
            }

            foreach (Object obj in objects)     //遍历选择的物体
            {
                AnimationClip fbxClip = obj as AnimationClip;
                if (fbxClip != null)
                {
                    AnimationClip clip = new AnimationClip();      //new一个AnimationClip存放生成的AnimationClip
                    EditorUtility.CopySerialized(fbxClip, clip);    //复制
                    AssetDatabase.CreateAsset(clip, "Assets/AnimationClip/" + fbxClip.name + ".anim");    //生成文件
                }
                else
                {
                    Debug.Log("当前选择的文件不是带有AnimationClip的FBX文件");
                }
            }

        }

        
    }

}
