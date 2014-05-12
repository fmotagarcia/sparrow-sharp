Sparrow-Sharp
=========

Sparrow-Sharp is a C# port of the [Sparrow]/[Starling] framework for mobiles made with Xamarin. Note that the project is currently is alpha state, so we do not recommend it for production code.

**Cross platform**

Runs on iOS and Android. You need to write your code just once, Sparrow and Mono take care of the underlying platform differences.  
Plus it runs on Desktop (Windows only currently, OSX planned) too so you can use Visual Studio's debugging and profiling tools to fine tune performance and iterate fast.


**Engineered for performance**

Currently it has approximately the same performance as native iOS and Android apps. It uses OpenGL ES 2.0 with all kinds of tricks like batching draw calls for the best possible performance.

**Easy to use API**

Sparrow-Sharp borrows display concepts from Flash's display tree representation. Even if you are not familiar with Flash you will find the API very easy to work with. It hides away the OpenGL implementation but its very easy to extend in case you want to roll your own stuff.

**Open source**

Apache 2.0 license.

How to use
----------

Just include the iOS/Android/Windows project in you app and you are ready to go. For an example project see the SparrowSwarp.Samples directories.


[Sparrow]:http://gamua.com/sparrow/
[Starling]:http://gamua.com/starling/
