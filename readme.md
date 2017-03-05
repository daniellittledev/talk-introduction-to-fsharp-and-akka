# Introduction to FSharp

> High-level overview of that we'll be doing

 - Leaning the basics of F#
 - Building a twitter alert app

-----------------------------

## Part A - High-level overview of FSharp

> High level overview of FSharp bits

 - Starting a project from scratch
 - Setting up Paket
 - Creating Build scripts with FAKE
 - Creating a NuGet package

### The basics of FSharp

> General points about the language

	- About the Fsharp language
	- Language category
	- Good at domain modeling

### Creating a new project

> This section will go over the syntax and basics while creating a logger helper and nuget package

	- Functions
	- Variables
	- Overloads (or lack of)
	- Type Inference
	- FSharp Interactive

Maybe - Units of Measure…

### Creating build scripts

	- Build the project with paket
	- Package the project with paket

-------------------------------

## Part B - Akka and Twitter

> Explains what this part will cover

### Project Setup

	- Creating the console app that will host the service
	- Adding paket
	- Pulling down all dependencies

### Actors

	- Design the actors we'll need
		○ /console
		○ /watchers
		○ /watchers/feed-*
		○ /notifier
	- Talk about designing actor systems
	- Talk about failures 
		○ Trees contain failures
	- Talk about messaging
		○ Trees only pass to roots of other trees

### The twitter API

	- Check out the twitter API
	- Talk about type providers
		○ Dangers
		○ Good use cases
	- Build the actors
		○ Watcher-manager
		○ Feed-watcher

### Windows API to do a Ding or something

	- Look at the API for Windows Notifications
	- Build the actor Windows Notification

------------------------------

## Part C - Wrap up

> The outcomes

- What I like/dislike about F#
- What I like/dislike about Akka.NET, comment about Akkling

### Getting started with F#

	- How to get started with F# (scripts!)
	- FSharp For Fun and Profit
	- VS: Visual F# PowerTools
	- Iodine F#
	- Suave

### Trying out your first functional language

	- For Non .net Developers
		○ Mono
		○ DotNet Core (still coming, in beta)
	- For .NET Developers
		○ Use it for build scripts or in a project
	
### Actors

What I have found
- Composing actors (types and decoupling)
- Libraries (Akkling)

### Questions

1. Is the code avalible?
2. How does F# compare with other languages?
3. Error handling in F#?