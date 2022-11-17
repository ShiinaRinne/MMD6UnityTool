using System;
using System.Collections;
using System.Collections.Generic;
using MMD.VMD;
using UnityEditor;
using UnityEngine;

public class VMDCameraConverter
{
	public static AnimationClip CreateAnimationClip(VMDFormat format)
	{
		VMDCameraConverter converter = new VMDCameraConverter();
		return converter.CreateAnimationClip_(format);
	}

	private AnimationClip CreateAnimationClip_(VMDFormat format)
	{

		AnimationClip clip = new AnimationClip()
		{
			frameRate = 30,
		};
		clip.name = format.name;

		CreateKeysForCamera(format, clip);

		return clip;
	}

	void CreateKeysForCamera(VMDFormat format, AnimationClip clip)
	{
		const float tick_time = 1f / 30f;
		const float mmd4unity_unit = 0.08f;

		Keyframe[] posX_keyframes = new Keyframe[format.camera_list.camera_count];
		Keyframe[] posY_keyframes = new Keyframe[format.camera_list.camera_count];
		Keyframe[] posZ_keyframes = new Keyframe[format.camera_list.camera_count];

		Keyframe[] rotX_keyframes = new Keyframe[format.camera_list.camera_count];
		Keyframe[] rotY_keyframes = new Keyframe[format.camera_list.camera_count];
		Keyframe[] rotZ_keyframes = new Keyframe[format.camera_list.camera_count];
		Keyframe[] rotW_keyframes = new Keyframe[format.camera_list.camera_count];

		Keyframe[] fov_keyframes = new Keyframe[format.camera_list.camera_count];

		Keyframe[] dis_keyframes = new Keyframe[format.camera_list.camera_count];

		//模拟一个相机的变换,用矩阵变换也可以,从世界坐标转局部坐标会很麻烦
		//var cameraWorldObj = new GameObject();
		//var cameraLocalObj = new GameObject();
		//var cameraWorldTrans = cameraWorldObj.transform;
		//var cameraLocalTrans = cameraLocalObj.transform;
		//cameraLocalTrans.SetParent(cameraWorldTrans);

		for (int i = 0; i < format.camera_list.camera_count; i++)
		{
			VMDFormat.CameraData cameraData = format.camera_list.camera[i];

			//位置
			/*cameraWorldTrans.localPosition = new Vector3(
				cameraData.location.x * mmd4unity_unit,
				cameraData.location.y * mmd4unity_unit,
				cameraData.location.z * mmd4unity_unit);*/

			//旋转
			//localEulerAngles取值后将角度设置为绝对值,
			//https://docs.unity3d.com/cn/2017.4/ScriptReference/Transform-localEulerAngles.html
			/*Vector3 rotationAsix = new Vector3(
				-(cameraData.rotation.x) * Mathf.Rad2Deg,
				-(cameraData.rotation.y) * Mathf.Rad2Deg,
				-(cameraData.rotation.z) * Mathf.Rad2Deg);
			cameraWorldTrans.localEulerAngles = rotationAsix;*/

			Quaternion Quaternion = Quaternion.Euler(new Vector3(
									cameraData.rotation.x * Mathf.Rad2Deg,
									-cameraData.rotation.y * Mathf.Rad2Deg,
									cameraData.rotation.z * Mathf.Rad2Deg));

			//距离,修改局部坐标的Z值
			//cameraLocalTrans.localEulerAngles = Vector3.zero;
			//cameraLocalTrans.localPosition = new Vector3(0, 0, (cameraData.length) * mmd4unity_unit);//Z相反轴,但这里实际值已经经过取反处理

			float frameTime = cameraData.frame_no * tick_time;
			posX_keyframes[i] = new Keyframe(frameTime, -cameraData.location.x * mmd4unity_unit);
			posY_keyframes[i] = new Keyframe(frameTime, cameraData.location.y * mmd4unity_unit);
			posZ_keyframes[i] = new Keyframe(frameTime, -cameraData.location.z * mmd4unity_unit);

			//做动画时最好用原值,localEulerAngles取值后将角度设置为绝对值,在做补间曲线会出问题
			rotX_keyframes[i] = new Keyframe(frameTime, Quaternion.x);
			rotY_keyframes[i] = new Keyframe(frameTime, Quaternion.y);
			rotZ_keyframes[i] = new Keyframe(frameTime, Quaternion.z);
			rotW_keyframes[i] = new Keyframe(frameTime, Quaternion.w);

			//视角fov
			fov_keyframes[i] = new Keyframe(frameTime, cameraData.viewing_angle);

			//摄像机距离
			dis_keyframes[i] = new Keyframe(frameTime, -cameraData.length * mmd4unity_unit);
		}

		//UnityEngine.Object.DestroyImmediate(cameraWorldObj);

		//NOTE:这里"距离"已经与position融合了,所以没法做补间
		AnimationCurve posX_curve = ToAnimationCurveWithTangentMode(1, AnimationUtility.TangentMode.Free, posX_keyframes, format.camera_list);
		AnimationCurve posY_curve = ToAnimationCurveWithTangentMode(2, AnimationUtility.TangentMode.Free, posY_keyframes, format.camera_list);
		AnimationCurve posZ_curve = ToAnimationCurveWithTangentMode(3, AnimationUtility.TangentMode.Free, posZ_keyframes, format.camera_list);
		AnimationCurve rotX_curve = ToAnimationCurveWithTangentMode(4, AnimationUtility.TangentMode.Free, rotX_keyframes, format.camera_list);
		AnimationCurve rotY_curve = ToAnimationCurveWithTangentMode(4, AnimationUtility.TangentMode.Free, rotY_keyframes, format.camera_list);
		AnimationCurve rotZ_curve = ToAnimationCurveWithTangentMode(4, AnimationUtility.TangentMode.Free, rotZ_keyframes, format.camera_list);
		AnimationCurve rotW_curve = ToAnimationCurveWithTangentMode(4, AnimationUtility.TangentMode.Free, rotW_keyframes, format.camera_list);
		AnimationCurve dis_curve = ToAnimationCurveWithTangentMode(5, AnimationUtility.TangentMode.Free, dis_keyframes, format.camera_list);
		AnimationCurve fov_curve = ToAnimationCurveWithTangentMode(6, AnimationUtility.TangentMode.Free, fov_keyframes, format.camera_list);


		AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "localPosition.x"), posX_curve);
		AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "localPosition.y"), posY_curve);
		AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "localPosition.z"), posZ_curve);
		AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "localRotation.x"), rotX_curve);   //采用欧拉角插值方式
		AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "localRotation.y"), rotY_curve);
		AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "localRotation.z"), rotZ_curve);
		AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "localRotation.w"), rotW_curve);

		AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("Distance", typeof(Transform), "localPosition.z"), dis_curve);

		AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("Distance/Camera", typeof(Camera), "field of view"), fov_curve);
	}

	//经过观察得知interpolation前四位分别是(x1,x2)(y1,y2)
	//总共6*4点,分别是[X轴,Y轴,Z轴,旋转,距离,视角]
	//都是以左下角为原点
	Tuple<Vector2, Vector2> GetInterpolationPoints(byte[] interpolation, int type)
	{
		int row = (type - 1) * 4;
		Vector2 p1 = new Vector2(interpolation[row + 0], interpolation[row + 2]);
		Vector2 p2 = new Vector2(interpolation[row + 1], interpolation[row + 3]);

		return new Tuple<Vector2, Vector2>(p1, p2);
	}

	//把MMD补间曲线中p1,p2的点映射回Curve中去
	Tuple<Vector2, Vector2> ConvertToFramekeyControllerPoint(Vector2 p1, Vector2 p2, Keyframe outKeyframe, Keyframe inKeyframe)
	{
		var dX = inKeyframe.time - outKeyframe.time;
		var dY = inKeyframe.value - outKeyframe.value;

		var newP1 = new Vector2(outKeyframe.time + p1.x / 127 * dX, outKeyframe.value + p1.y / 127 * dY);
		var newP2 = new Vector2(outKeyframe.time + p2.x / 127 * dX, outKeyframe.value + p2.y / 127 * dY);

		//因为不存在90度的情况,这里要趋近,但是也不能太趋近,不然补间前几帧会变的陡峭
		if (Mathf.Approximately(outKeyframe.time, newP1.x)) newP1.x += 0.1f;
		if (Mathf.Approximately(inKeyframe.time, newP2.x)) newP2.x -= 0.1f;

		return new Tuple<Vector2, Vector2>(newP1, newP2);
	}

	private static float Tangent(in Vector2 from, in Vector2 to)
	{
		Vector2 vec = to - from;
		return vec.y / vec.x;
	}

	private static float Weight(in Vector2 from, in Vector2 to, float length)
	{
		return (to.x - from.x) / length;
	}

	//根据四个控制点计算三次贝塞尔曲线的系数
	//插值:贝塞尔曲线插值(四个点)
	//https://blog.csdn.net/seizeF/article/details/96368503
	//return {outTangent,outWeight,inTangent,inWeight)
	float[] CalculateBezierCoefficient(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
	{
		float p30Length = (p3.x - p0.x);

		float outTangent = Tangent(p0, p1);
		float outWeight = Weight(p0, p1, p30Length);

		float inTangent = Tangent(p2, p3);
		float inWeight = Weight(p2, p3, p30Length);

		return new float[] { outTangent, outWeight, inTangent, inWeight };
	}

	void SetKeyfreamTweenCurve(int index, int type, Keyframe[] keyframes, VMDFormat.CameraList cameraList)
	{
		if (index <= 0)
			return;

		VMDFormat.CameraData curCameraData = cameraList.camera[index];
		VMDFormat.CameraData lastCameraData = cameraList.camera[index - 1];

		Keyframe outKeyframe = keyframes[index - 1];
		Keyframe inKeyframe = keyframes[index];

		var dX = inKeyframe.time - outKeyframe.time;
		var dY = inKeyframe.value - outKeyframe.value;

		outKeyframe.weightedMode = WeightedMode.Both;
		inKeyframe.weightedMode = WeightedMode.Both;
		if (Mathf.Approximately(dY, 0f) || Mathf.Approximately(dX, 1 / 30f))    //没有变化的就不需要补间插值了
		{
			outKeyframe.outTangent = 0;
			outKeyframe.outWeight = 0;

			inKeyframe.inTangent = 0;
			inKeyframe.inWeight = 0;
		}
		else
		{
			//插值计算[0~127]
			//参考https://www.jianshu.com/p/ae312fb53fc3
			Vector2 p0 = new Vector2(outKeyframe.time, outKeyframe.value);
			Vector2 p3 = new Vector2(inKeyframe.time, inKeyframe.value);
			Vector2 p1 = Vector2.zero;
			Vector2 p2 = Vector2.zero;
			var intTuple = GetInterpolationPoints(curCameraData.interpolation, type);
			var ptTuple = ConvertToFramekeyControllerPoint(intTuple.Item1, intTuple.Item2, outKeyframe, inKeyframe);    //转化为keyFrame的控制点
			p1 = ptTuple.Item1;
			p2 = ptTuple.Item2;

			float[] coeffs = CalculateBezierCoefficient(p0, p1, p2, p3);
			outKeyframe.outTangent = coeffs[0];
			outKeyframe.outWeight = coeffs[1];
			inKeyframe.inTangent = coeffs[2];
			inKeyframe.inWeight = coeffs[3];
		}

		//因为是结构体,所以需要重新赋值
		keyframes[index - 1] = outKeyframe;
		keyframes[index] = inKeyframe;
	}

	AnimationCurve ToAnimationCurveWithTangentMode(int type, AnimationUtility.TangentMode mode, Keyframe[] keyframes, VMDFormat.CameraList cameraList)
	{
		if (mode == AnimationUtility.TangentMode.Free)
		{
			for (int i = 0; i < keyframes.Length; i++)
			{
				SetKeyfreamTweenCurve(i, type, keyframes, cameraList);
			}
		}

		var newKeyFrames = OptimizedCurves(type, keyframes, cameraList);

		AnimationCurve curve = new AnimationCurve(newKeyFrames);
		for (int i = 0; i < curve.keys.Length; i++)
		{
			if (mode == AnimationUtility.TangentMode.Free)
				AnimationUtility.SetKeyBroken(curve, i, true);

			AnimationUtility.SetKeyLeftTangentMode(curve, i, mode);
			AnimationUtility.SetKeyRightTangentMode(curve, i, mode);

		}
		for(int j = 0; j < keyframes.Length; j++)
		{
			if(j<keyframes.Length - 1)
			{
				int StartFrame = (int)(keyframes[j].time * 30f);
				int EndFrame = (int)(keyframes[j+1].time * 30f);

				if(EndFrame == StartFrame+1)
				{
					//Debug.Log(StartFrame);
					AnimationUtility.SetKeyRightTangentMode(curve, j, AnimationUtility.TangentMode.Constant);
				}
			}
		}
		return curve;
	}

	////由于Unity自带的插值曲线平滑效果不是很好,可以通过插入关键帧的方式平滑曲线
	//三次贝塞尔曲线插值
	Vector2 EvaluteThire(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
	{
		return Mathf.Pow(1 - t, 3) * p0 + 3 * Mathf.Pow(1 - t, 2) * t * p1 + 3 * (1 - t) * Mathf.Pow(t, 2) * p2 + Mathf.Pow(t, 3) * p3;
	}

	//补间插值插入关键帧
	List<Keyframe> CalculateInterpolationKeyframe(int type, byte[] interpolation, Keyframe outKeyframe, Keyframe inKeyframe, float density = 0.05f)
	{
		density = Mathf.Min(1f, density);
		density = Mathf.Max(0f, density);
		var p0 = new Vector2(outKeyframe.time, outKeyframe.value);
		var p3 = new Vector2(inKeyframe.time, inKeyframe.value);
		var intTuple = GetInterpolationPoints(interpolation, type);
		var ptTuple = ConvertToFramekeyControllerPoint(intTuple.Item1, intTuple.Item2, outKeyframe, inKeyframe);    //转化为keyFrame的控制点
		var p1 = ptTuple.Item1;
		var p2 = ptTuple.Item2;

		var dX = inKeyframe.time - outKeyframe.time;
		var dY = inKeyframe.value - outKeyframe.value;
		float stepTime = (1 / 30f) / density;
		List<Keyframe> interpolationKeyframes = new List<Keyframe>();
		if (!Mathf.Approximately(dY, 0f))
		{
			if (stepTime > 0f && stepTime >= (1 / 30f))
			{
				for (float time = outKeyframe.time + stepTime; time < inKeyframe.time; time += stepTime)
				{
					float t = (time - outKeyframe.time) / dX;
					var pt = EvaluteThire(t, p0, p1, p2, p3);

					var newKeyframe = new Keyframe(pt.x, pt.y);
					interpolationKeyframes.Add(newKeyframe);
				}
			}

		}

		return interpolationKeyframes;
	}

	Keyframe[] OptimizedCurves(int type, Keyframe[] keyframes, VMDFormat.CameraList cameraList)
	{
		List<Keyframe> framesList = new List<Keyframe>();

		//曲线优化
		for (int i = 0; i < keyframes.Length; i++)
		{
			//if (i > 0)
			//{
			//    var lastKeyFrame = keyframes[i - 1];
			//    var curKeyFrame = keyframes[i];

			//    VMDFormat.CameraData curCameraData = cameraList.camera[i];
			//    var exKeyFrames = CalculateInterpolationKeyframe(type, curCameraData.interpolation, lastKeyFrame, curKeyFrame);
			//    framesList.AddRange(exKeyFrames);
			//}
			var keyframe = keyframes[i];
			framesList.Add(keyframe);
		}

		//针对只有一帧的进行优化
		if (framesList.Count == 1)
		{
			Keyframe[] newKeyframes = new Keyframe[2];
			newKeyframes[0] = keyframes[0];
			newKeyframes[1] = keyframes[0];
			newKeyframes[1].time += 0.001f / 60f;//1[ms]
			newKeyframes[0].outTangent = 0f;
			newKeyframes[1].inTangent = 0f;

			framesList.Clear();
			framesList.AddRange(newKeyframes);
		}

		return framesList.ToArray();
	}
}
