Open your flash animation in Adobe Flash (CS6 or above).
![](./Images/user-guide-1.png)

Click the right button on movie clip which you want to export besides main timeline and go to the Properties.
![](./Images/user-guide-3.png)

Choose "Export for ActionScript" and set name for clip in "Class" field.
![](./Images/user-guide-4.png)

You can add **anchor** frame labels to separate timeline to different named sequences.
![](./Images/user-guide-2.png)

Run export script (_FlashTools/FlashExport/FlashExport.jsfl_) for your flash animation. The script optimizes an animation, rasterizes vector graphics and export .swf file which is compatible with the toolset.
![](./Images/user-guide-5.png)
![](./Images/user-guide-6.png)

Move exported .swf file to your unity project (You may find this .swf file at _export folder next to .fla file).
![](./Images/user-guide-7.png)

.swf file will be automatically converted to unity asset with proper settings. Open them to change texture packing settings (by default it uses settings from _"FlashTools/Resources/SwfSettings"_).
![](./Images/user-guide-8.png)

Now you can select exported clip and add it to scene or create a prefab.
![](./Images/user-guide-9.png)

In instance properties you can change any settings like a sorting layer, play mode, current frame and etc.
![](./Images/user-guide-10.png)

Enjoy!
