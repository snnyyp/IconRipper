# IconRipper

<img src="IconRipper.Gui/Assets/icon.png" width="25%" alt="IconRipper Logo" />

## Preface

This project was initially started while I was looking for a tool that could extract icons from PE files. I thought it was a pretty simple request, but after searching high and low, I was still unable to find a "perfect" free tool. The main catch is that, even though you can achieve that simply by using `ExtractIconEx` and `PrivateExtractIconsW`, you can't really know how many resolutions a particular icon has in the PE file. And if you specify a resolution that the PE file doesn't contain, the win32 API function will find the closest match and automatically interpolate it for you, which could be very handy for `explorer.exe`, but what I wanted is more like a "PE resource extractor", one that extracts resources exactly as they are, without any interpolation or any other tricks behind the scenes.

Meanwhile, I was learning the WPF framework and desperately in need of a real-life project to gain some hands-on experience. Thus, this project was born.

Interestingly, the project ended up using the Avalonia framework instead. Because while I was testing it, I thought I had found a memory leak issue caused by the large number of bitmap objects the program created, which I wasn't able to solve back then, plus the ginormous file size of the final published executables. Then I somehow stumbled upon Avalonia. I thought to myself: hmm, similar (A)XAML syntax, more modern binding system, and awesome CSS-inspired style selector, trimmable, cross-platform (though in case doesn't matter at all), maybe the memory leak will go away if I switch from WPF to Avalonia and I won't need to rewrite much code, GREAT.

The OG WPF project was almost complete, so it took me around 2 days to migrate the whole codebase. After some testing, sadly, the bug persisted. So, right now, I have two copies of the same buggy tool. Regardless of my temptation of cursing both UI frameworks, I know there has to be something wrong within my own codebase (I've already checked that every bitmap objects was properly disposed of after the view / view model got unloaded, and I've already set up `DecodePixelWidth`). After some thorough research, `VirtualizingStackPanel` came up. It will only load elements that users can actually see (plus some buffer room), and unload them when they are far out of bounds, kind of like a video game. However, what I want is a Grid-like container, not a stack panel. And Avalonia doesn't have `VirtualizingGrid` or `VirtualizingUniformGrid`, so I have to use the bare-bones `ItemsRepeater`, which means I have to implement the selection mechanism myself. And you thought problem solved, right?

Judging by my tone of voice, the answer cannot be more obvious. Despite `ItemsRepeater` being able to load / unload visual elements, it can't automatically create / dispose of bitmap objects. So I have to implement my own loading / unloading mechanism for  unmanaged resources, FK! FINALLY, all hard work paid off, the memory leak issue was gone for good. Looking back now, maybe that wasn't really a memory leak, maybe that's just how C#'s GC works, and I became too annoyed and paranoid after seeing the memory usage in the Task Manager stay high when I unloaded the file.

Anyway, although the above mechanisms might be overkill, it for sure overloaded my tiny little newbie brain at least once during the whole optimization process. I'm glad I learned A LOT in my very first WPF / Avalonia project. This unique experience would definitely be very valuable for my future bigger projects.

Hope you can enjoy!

P.S. I once considered WinUI 3 as an alternative to WPF before choosing Avalonia, but quickly gave up because it took forever to build.

## Some useful functions, includes but not limited to...

- Built-in translation for Simplified Chinese / Traditional Chinese / English
- Export as PNG / JPG / WebP / ICO (single / multi resolution)
- Copy to clipboard (may lose transparency)
- Detailed meta information for each icon
- No resolution interpolation whatnot, directly reads the PE Resource Directory
- Load common DLLs (shell32.dll / user32.dll, imageres.dll, etc.) with one click (File -> Load Common...)