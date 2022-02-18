# Hackman Contest
Hangman, but it's a 3rd person shooter for some reason.

Submission for the Hackman contest hosted by the Clemson School of Computing discord. The goal was to make a hangman related program in 2 weeks, provided that we utilize their custom API to receive information from a server during gameplay. 

This project was built using Unity 2021.2.7f1. You do not need this exact version to fully open the source/editor files, but any relatively close version will do.

Finally, the current version has the keys used for the API hidden from the git branch, so the game may not load words when playing the editor!

To run the game, simply download the most recent build from the Releases tab and run the HackmanContest executable.
The current build should support Linux, MAC, and Windows, but the only version I know works for sure is windows.

# How To Play
Run and jump around the arena while using your trusty gun to play Hangman on the big screen. Simply shoot the letters on the large keyboard to enter them in. You receive more points the faster you complete the word! Get a word wrong, though, and you'll take damage. After 3 failed rounds, the game is over!

The game uses standard 3rd person shooter controls:

Aim - Mouse

Move - WASD

Shoot - Left Click

Aim down sights - Right click

Jump - Spacebar

Sprint - Left Shift

Due to it not being a priority, the game only supports keyboard and mouse. This will be fixed in a later update.

# Code Architecture
In terms of architecture, I essentially tried to make most gameplay-relative components as reusable as possible.
While it's completely overkill in some aspects for a game of this scope, I tried to structure most things as if any larger scope
3rd person shooter could be created using what exists as a base (adding enemies, weapons, etc)


# Notes
Since I'm not a very good animator/3d modeler, none of the 3d assets and animations were created by me.
The packages used were Synty Low Poly asset package, as well as Unity's Standard Character assets https://github.com/Unity-Technologies/Standard-Assets-Characters
However, not all animations in the game are from those packages. Inverse Kinematics were used for more dynamic animations that occur in real time, like running with your weapon, shooting and holding the weapon, etc. While the aforementioned packages come with some starter scripts and camera controls, the only significant package used for gameplay was Cinemachine, which was just for camera collision and aiming

All 2D sprites and UI images were drawn by me.
I plan to add more visuals, as well as custom models and animations in the future in order to practice those skills.



