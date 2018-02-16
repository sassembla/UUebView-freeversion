# UUebView-freeversion
Standalone WebView for Unity.  
Unity 2017.x or later.

## demo scene

open Assets/Sample/SampleScene in Unity.

or,

open **./WebGL/index.html** in your web browser. you can see html browser running on the Unity WebGL contents.

## presentation

[gitpitch here.](https://gitpitch.com/sassembla/UUebview-freeversion/master?grs=github&t=moon#)

## status

### enabled:
* parse html5 ruled html.
* rendering uGUI interface from html.
* generate original tags from uGUI.
* layout uGUI parts from html.
* original DOCTYPE declaration "uuebview" is enabled.
* some special html attributes supported. (**id**, **listen**, **hidden**, **button**)
* some tags enabled.(a, img, and more ... check [default tags](https://github.com/sassembla/UUebView-freeversion/tree/master/Assets/InformationResources/Resources/Views/Default))
* the uGUI event receiver system is implemented.

### not yet
* reduce amount of the GameObject.
* generating original tags with rich Editor GUI.
* no benchmark yet.

## license 
[LICENSE](./LICENSE)