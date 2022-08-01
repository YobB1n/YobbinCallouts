
# YobbinCallouts
[![Downloads](https://img.shields.io/github/downloads/YobB1n/YobbinCallouts/total.svg)](https://github.com/YobB1n/YobbinCallouts/releases)

YobbinCallouts by YobB1n copyright (c) 2020-2022 YobB1n.
This project in its entirety is open-source. Please provide appropriate credit when portions of this code are used as reference or in verbatim.

### Quick Links: <br/>
**Download the callouts:** https://www.lcpdfr.com/downloads/gta5mods/scripts/29467-yobbin-callouts/ <br/>
**Join my Discord:** https://discord.com/invite/Wj522qa5mT  <br/>
**Website:** https://www.yobbinmods.com/ (callout coding tutorials are also on my website!)

## Introduction

YobbinCallouts is a project I started in summer of 2020 to provide a wide range of unique callouts all around the map, that have never been done before. The project started with just two callouts with extremely poor code efficiency and style, to now including over a dozen callouts that have improved greatly as my coding skills have improved. Of course, it is well known that I am a pretty bad programmer (lmao), so please excuse poor style or code efficiency.

As of July 29, 2022, the project is now entirely open-source to help others with their own LSPDFR plugins, as well as for interested (and lovely) people to contribute to the project. If you'd like to contribute, please continue reading! Thanks!

## Quick guide around the project

### Main.cs

This is the EntryPoint for the LSPDFR plugin. Not very interesting, there is an update checker (thanks albo!) that still works even though I haven't changed it in years xD.

### CallHandler

This class is probably the most interesting if you're looking through my code to use for your own. The class contains a variety of
useful helper functions that you can easily incorporate into your own LSPDFR/RPH plugins by simply copy/pasting. Functions include:

**`void locationChooser(ArrayList list, float maxdistance = 600f, float mindistance = 25f)`**
Creates a list of all the Vector3 locations within `mindistance` and `maxdistance` contained in `list`. Returns `locationReturned = false` if no corresponding locations are found in `list`, and a random corresponding location if a location is found.

**`void Dialogue(List<string> dialogue, Ped animped = null, String animdict = "missfbi3_party_d", String animname = "stand_talk_loop_a_male1", float animspeed = -1, AnimationFlags animflag = AnimationFlags.Loop)`**
Plays a dialogue in `List<string>` form. Optionally, specify a Ped and animation to play while the dialogue is progressing. The dialgoue will be progress when the player presses the `MainInteractionKey` key in the `Config.ini` file.

