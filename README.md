# LuckyDev

A .NET WPF application from 2009.

In `original` branch you will find the original source code for this application. In `master` an upgraded, refactored version.

## Description

In 2009 I used to work in a project which uses TFS as source control and issue tracker. In particular we started to implement code reviews (still not supported built-in by the tool) we basically define who will be the code reviewer for every commit. The application basically connects to TFS and define the best option for your code review (based on the amount of code reviews already assigned to all the developers) the developer with less code reviews is the next Lucky Dev.

The code was initially hosted in a TFS 2008 instance then moved to Visual Studio Team Services (TFS Preview at that moment) 

## Screenshot

![screenshot](https://raw.githubusercontent.com/mamcer/lucky-dev/master/doc/screenshot.png)

## Technologies

- Visual Studio 2008
- .NET Framework 3.5
