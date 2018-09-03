# LuckyDev

A .NET WPF application from 2009.

In `original` branch you will find the original source code for this application. In `master` an upgraded, refactored version.

> More details about why I published this project in [this blog post](https://mamcer.github.io/2018-09-02-i-cleaned-up-my-virtual-basement/)

## Description

In 2009 I used to work in a project which uses TFS as source control and issue tracker. In particular we started to implement code reviews (still not supported built-in by the tool) we manually defined who will be the code reviewer for every check in. The application  connects to TFS and define the best option for your code review based on the amount of code reviews already assigned to every developers. The developer with less code reviews at that moment is the next 'Lucky Dev'.

The code was initially hosted in a TFS 2008 instance then moved to Visual Studio Team Services (TFS Preview at that moment) 

## Screenshot

![screenshot](https://raw.githubusercontent.com/mamcer/lucky-dev/master/doc/screenshot.png)

## Technologies

- Visual Studio 2008
- .NET Framework 3.5
