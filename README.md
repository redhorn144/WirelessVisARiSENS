# Wireless Visualization AR App
The unity AR project meant to visualize wireless path is augmented reality

This project uses Unity 2022.3.11f1 and needs build support for whichever OS of mobile device is being compiled for, so either iOS or Android build support.

## Compiling
Compiling this project involves opening this project in the correct version of unity with the correct build support, clicking 'file' in the unity app and 'build settings'. One must have the device they are building for plugged in to your computer and shows up in the device selection. One can the click "build and run" in order to build on the target device. For android it is important to have the build device in developer mode. Building to iOS has not been tested but requires a macOS device to build from (with xCode).

## Notes

One of the files that is important to this project is the "one.png" image. This is the image that is used as the tracked image for this project because it is a unity default image. It may be necessary to print this image out or display it on another device. This file may be found in Assets/trackedImages. When using this app the image must be placed at the origin of the scanned room.

The code for this app is still very much a work in progress so comments are poor and the code itself may be opaque in some places so dont hesitate to reach out to me (Alex Shaffer).

## Scenes
This unity project has one scene currently. This is the main AR scene that the app loads into when launched on a mobile device. This scene waits until the AR camera views the tracked "one.png" image in order to align the coordinate system. Once the image is in view, the wireless path are rendered.


## Scripts
This project features a number of C# scripts .The only scripts detailed in this README are custom scripts for the purpose of this project.

### CSVReader.cs 

This is a C# script that reads in the chosen CSV data file. These files are precessed CSV files from wireless insight simulations using Saif's python script. An exampe of such a file may be found in the Assets\csv folder. This file defines the ```Path()``` class used to store information for individual wireless paths. This script is linked to the Options.cs script as it needs to know the name of the data file to read in.

### Colormap.cs

This is a C# script that handles the colormaps in the visualizaion. Most of this file is made up of lookup tables for the popular and good colormaps viridis, magma, inferno, and plasma. There is one public member function ```Color GetColor(float val, float minVal, float maxVal, string name)```. It takes in the number to be represented in a color, the minimum and maximum data values, and the name of the colormap to be used. It returns the resulting unity Color class object.

### TrackedImageInfo.cs

This is the primary C# script that handles the actual visualization of the wireless paths. It defines the ```Packet``` class that holds the information for the dynamic visualization where the spheres travel along the wireless paths. This script handles identifiying the tracked image, coordinate conversions from the CSV files (which do not agree with Unities coordinate system for the house), and the colorscale that is visible in the scene. The code that handles the tracked images is built off of the code detailed on https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.1/manual/tracked-image-manager.html. If you have questions about this script please contact Alex Shaffer (host of this repository).