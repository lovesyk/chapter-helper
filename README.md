# chapter-helper
Chapter Helper is a tool to simplify handling basic encoding tasks around chapters.

Features:
- import scene ranges from AviSynth trim commands
- split files (like audio) based on scene ranges
- export Matroska chapter files based on specified chapters
- export timecode v1 files based on specified scene ranges
- export x264 qp files based on specified chapters

![Alt text](/screenshot.png?raw=true "Screeshot")

An MKVToolNix installation is required for file splitting. Its path will be automatically set if
- it was installed by the official installer or an compatible App-V package
- it was put into the same directory as this tool
- the working directory was set to its path

Alternatively, it's possible to set the path manually. As this setting isn't being saved for now, it is required to do so every time the tool is run.

Things to note:
- input checking isn't reliable yet so invalid chapters which result in errors can be specified
- frame rate inputs are being carried over from the previous chapter and in case of output frame rate also from input frame rate
- the current project / build is of alpha quality
