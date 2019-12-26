In order for the water to work correctly, 
you need to add the script "FastLowPolyWater_Camera.cs " 
on camera. 
This is necessary for some platforms, such as IOS or Android.

Color water  - the color of the water near the shore, on the surface of the water.
Color Depth -the color of the water depth, responsible for the lower
 part of the water, where there is no landscape or other models.
Color Foam - foam color for the polygon.
Depth -responsible for the amount of color of the top of the water.
OpasityIntensity -if you set 1 the near landscape water will transparent.
SIzeFoam -the distance where the foam will work from the model.
Gloss -smoothness of water.
Specular - the intensity of the reflections.
Noise -texture for waves.
Speed - the speed at which the waves will move.
NoiseIntensity - the height of the waves.
ScaleNoise - tiling of the texture in world coordinates.
To install the water, just drag the prefab from the
 folder "PrefabsWater".
If you want to use water on their models
 in the settings of the model 
  in the "Normals" variable, change
 the value to "Calculate" and set the Smoothing Angle to 0 to make your 
model angular. Then use water material on your model.
If you have water does not look the same as in the screenshots,
 then add post effects to the camera.
Or change the color space to Linear in the Player settings.