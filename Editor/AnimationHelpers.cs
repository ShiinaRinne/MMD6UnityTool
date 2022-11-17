using System;
using MMDExtensions.Tools;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AnimationHelpers
{
    [MenuItem("Assets/MMD/Create Morph Animation")]
    public static void CreateMorphAnimation()
    {
        System.GC.Collect();
        string path =
            AssetDatabase.GetAssetPath(Selection.GetFiltered<DefaultAsset>(SelectionMode.Assets).FirstOrDefault());

        if (Path.GetExtension(path).ToUpper().Contains("VMD"))
        {
            var stream = File.Open(path, FileMode.Open);

            var vmd = VMDParser.ParseVMD(stream);

            var animationClip = new AnimationClip() {frameRate = 30};

            var delta = 1 / animationClip.frameRate;

            var keyframes = from keys in vmd.Morphs.ToLookup(
                    k => k.MorphName,
                    v => new Keyframe(v.FrameIndex * delta, v.Weight * 100))
                select keys;

            foreach (var package in keyframes)
            {
                var name = package.Key;

                var curve = new AnimationCurve(package.ToArray());
                var gameobject = Selection.GetFiltered<GameObject>(SelectionMode.TopLevel).FirstOrDefault();
                var gameObjectName = gameobject.name;
                var parentName = gameobject.transform.parent.name;

                var mesh = gameobject.GetComponent<SkinnedMeshRenderer>().sharedMesh;
                var bsCounts = mesh.blendShapeCount;
                var blendShapeNames = Enumerable.Range(0, bsCounts).ToList()
                    .ConvertAll(index => mesh.GetBlendShapeName(index));

                // ================================================================================================================
                // ym change
                // to support group blendshapes
                // ================================================================================================================
                string registerName = "";

                try
                {
                    if (name == "まばたき")
                    {
                        registerName = blendShapeNames.Where(x => x.Split('.').Last() == "ウィンク２").First();
                        animationClip.SetCurve($"{parentName}/{gameObjectName}", typeof(SkinnedMeshRenderer),
                            $"blendShape.{registerName}", curve);
                        registerName = blendShapeNames.Where(x => x.Split('.').Last() == "ｳｨﾝｸ２右").First();
                        animationClip.SetCurve($"{parentName}/{gameObjectName}", typeof(SkinnedMeshRenderer),
                            $"blendShape.{registerName}", curve);
                        continue;
                    }

                    if (name == "笑い")
                    {
                        registerName = blendShapeNames.Where(x => x.Split('.').Last() == "ウィンク").First();
                        animationClip.SetCurve($"{parentName}/{gameObjectName}", typeof(SkinnedMeshRenderer),
                            $"blendShape.{registerName}", curve);
                        registerName = blendShapeNames.Where(x => x.Split('.').Last() == "ウィンク右").First();
                        animationClip.SetCurve($"{parentName}/{gameObjectName}", typeof(SkinnedMeshRenderer),
                            $"blendShape.{registerName}", curve);
                        continue;
                    }

                    if (name == "ウィンク２" || name == "ｳｨﾝｸ２右" || name == "ウィンク" || name == "ウィンク右")
                    {
                        if (package.ToArray().Length <= 1)
                        {
                            continue;
                        }
                    }

                    registerName = blendShapeNames.Where(x => x.Split('.').Last() == name).First();
                    animationClip.SetCurve($"{parentName}/{gameObjectName}", typeof(SkinnedMeshRenderer),
                        $"blendShape.{registerName}", curve);
                }
                catch (Exception e)
                {
                    // Debug.Log($"Error: {e.Message}");
                }
                // ================================================================================================================
            }

            AssetDatabase.CreateAsset(animationClip, path.Replace("vmd", "anim"));
        }
    }

    [MenuItem("Assets/MMD/Create Camera Animation")]
    public static void ExportCameraVmdToAnim()
    {
        var selected = Selection.activeObject;
        string selectPath = AssetDatabase.GetAssetPath(selected);
        if (!string.IsNullOrEmpty(selectPath))
        {
            CameraVmdAgent camera_agent = new CameraVmdAgent(selectPath);
            camera_agent.CreateAnimationClip();
            Debug.LogFormat("[{0}]:Export Camera Vmd Success!", System.DateTime.Now);
        }
        else
        {
            Debug.LogError("没有选中文件或文件夹");
        }
    }
}