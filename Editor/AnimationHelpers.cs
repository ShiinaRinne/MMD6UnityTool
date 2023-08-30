using System;
using System.Collections.Generic;
using MMDExtensions.Tools;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MMD6UnityTool
{
    internal class AnimationHelpers
    {
        [MenuItem("Assets/MMD/Create Morph Animation")]
        internal static void CreateMorphAnimation()
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

                var gameobject = Selection.GetFiltered<GameObject>(SelectionMode.TopLevel).FirstOrDefault();
                var mesh = gameobject.GetComponent<SkinnedMeshRenderer>().sharedMesh;
                var bsCounts = mesh.blendShapeCount;
                var blendShapeNames = Enumerable.Range(0, bsCounts).ToList()
                    .ConvertAll(index => mesh.GetBlendShapeName(index));

                foreach (var package in keyframes)
                {
                    var name = package.Key;

                    var curve = new AnimationCurve(package.ToArray());

                    var gameObjectName = gameobject.name;
                    var parentName = gameobject.transform.parent.name;

                    try
                    {
                        string registerName = "";
                        if (name == MorphAnimationNames.Blink)
                        {
                            registerName = blendShapeNames.Where(x => x.Split('.').Last() == MorphAnimationNames.Wink2).FirstOrDefault();
                            AddCurveToAnimationClip(animationClip, parentName, gameObjectName, blendShapeNames, registerName, curve);
                            registerName = blendShapeNames.Where(x => x.Split('.').Last() == MorphAnimationNames.Wink2Right).FirstOrDefault();
                            AddCurveToAnimationClip(animationClip, parentName, gameObjectName, blendShapeNames, registerName, curve);
                        }
                        else if (name == MorphAnimationNames.Smile)
                        {
                            registerName = blendShapeNames.Where(x => x.Split('.').Last() == MorphAnimationNames.Wink).FirstOrDefault();
                            AddCurveToAnimationClip(animationClip, parentName, gameObjectName, blendShapeNames, registerName, curve);
                            var c = package.ToArray();
                            Debug.Log(package.ToArray());
                            registerName = blendShapeNames.Where(x => x.Split('.').Last() == MorphAnimationNames.WinkRight).FirstOrDefault();
                            AddCurveToAnimationClip(animationClip, parentName, gameObjectName, blendShapeNames, registerName, curve);
                        }
                        else if (name == MorphAnimationNames.Wink2 || name == MorphAnimationNames.Wink2Right ||
                                 name == MorphAnimationNames.Wink  || name == MorphAnimationNames.WinkRight)
                        {
                            registerName = blendShapeNames.Where(x => x.Split('.').Last() == name).FirstOrDefault();
                            var existingCurve = AnimationUtility.GetEditorCurve(animationClip, EditorCurveBinding.FloatCurve($"{parentName}/{gameObjectName}", typeof(SkinnedMeshRenderer), $"blendShape.{registerName}"));
                            if (existingCurve != null)
                                curve = MergeAnimationCurves(existingCurve, curve);
                            AddCurveToAnimationClip(animationClip, parentName, gameObjectName, blendShapeNames, registerName, curve);
                        }
                        else
                        {
                            registerName = blendShapeNames.Where(x => x.Split('.').Last() == name).FirstOrDefault();
                            AddCurveToAnimationClip(animationClip, parentName, gameObjectName, blendShapeNames, registerName, curve);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error: {e.Message}");
                    }
                }

                stream.Close();
                AssetDatabase.CreateAsset(animationClip, path.Replace("vmd", "anim"));
            }
        }


        [MenuItem("Assets/MMD/Create Camera Animation")]
        internal static void ExportCameraVmdToAnim()
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

        [MenuItem("GameObject/MMD/Generate MMDCamera")]
        internal static void GenerateMMDCamera()
        {
            var mmdCamera = new GameObject("MMDCamera");
            var distance = new GameObject("Distance");
            var camera = new GameObject("Camera");

            distance.transform.parent = mmdCamera.transform;
            camera.transform.parent = distance.transform;

            distance.transform.rotation = Quaternion.Euler(0, 180, 0);

            mmdCamera.AddComponent<Animator>();
            camera.AddComponent<Camera>();
            camera.AddComponent<AudioListener>();
            
            // Just a reference, you can change this part according to your own needs
            
            // =========== Enable postprocessing for unity urp ===========
            // var uac = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            // uac.renderPostProcessing = true;
            // uac.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            // uac.antialiasingQuality = AntialiasingQuality.High;
        }

        #region Utils

        private static void AddCurveToAnimationClip(AnimationClip animationClip, string parentName,
            string gameObjectName, List<string> blendShapeNames, string registerName, AnimationCurve curve)
        {
            if (blendShapeNames.Contains(registerName))
            {
                animationClip.SetCurve($"{parentName}/{gameObjectName}", typeof(SkinnedMeshRenderer),
                    $"blendShape.{registerName}", curve);
            }
        }

        private static AnimationCurve MergeAnimationCurves(AnimationCurve existingCurve, AnimationCurve newCurve)
        {
            var existingKeyframes = existingCurve.keys.ToList();
            existingKeyframes.AddRange(newCurve.keys);
            return new AnimationCurve(existingKeyframes.ToArray());
        }

        private static class MorphAnimationNames
        {
            public const string Blink = "まばたき";
            public const string Wink2 = "ウィンク２";
            public const string Wink2Right = "ｳｨﾝｸ２右";
            public const string Smile = "笑い";
            public const string Wink = "ウィンク";
            public const string WinkRight = "ウィンク右";
        }

        #endregion
    }
}