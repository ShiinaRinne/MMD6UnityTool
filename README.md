# MMD6UnityTool

May be used when you use Unity to make MMD

For **export camera and morph animations** from VMD (Character animations, and pmx => fbx you can export
via [MMD4Mecanim](https://stereoarts.jp/))
<br>
For ease of use, directly copied from the following project and put it in a menu

[MMD2UnityTool](https://github.com/MorphoDiana/MMD2UnityTool)

[MMD4UnityTools](https://github.com/ShiinaRinne/MMD4UnityTools)

(2+4=6)

## Demo
[Miku お気に召すまま](https://www.bilibili.com/video/BV1eY411o7Dd/)


## Known Issue
Sometimes one eye may lose morph animation. <br>
At present, you can copy the key frame of the other eye through the Animation window( Ctrl + 6 ).
## Usage

- For camera animation, just right click on VMD file and select `MMD/Create Camera Anim`<br>
  When you use camera animation via Timeline, please uncheck `Remove Start Offset` in Clip properties<br>
  ![](https://pic.youngmoe.com/1668669688_202211171521480/6375e0f85569a.png)
- For morph animation, you need to select the object in the scene that contains the blendshapes of the face under the
  model
  (If it is generated by MMD4Mecanim, it is usually `your model/U_Char/U_Char_1`)<br>
  and select the VMD with morph (ie: multi selection)<br>
  ![](https://pic.youngmoe.com/1668669748_202211171522697/6375e134917bb.png) <br>
  Then right click and select `MMD/Create Morph Anim`

### [Not Important] When you make post-processing, try this repo
[TimelineExtensions](https://github.com/ShiinaRinne/TimelineExtensions)

## License
[MIT License](https://github.com/ShiinaRinne/MMD6UnityTool/blob/master/LICENSE)

