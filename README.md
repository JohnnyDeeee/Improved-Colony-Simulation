# Improved Colony Simulation
This is going to be an improved version of [this old project](https://github.com/JohnnyDeeee/colony-simulation)

## Why?
Mainly because i was using Unity the wrong way. I wasn't making use of the script binding to a prefab and other stuff.
Also i was using the Physics Engine in Unity and thus the performance was really bad (in combination with multiple Neural Nets running every frame)

## What is going to be different?
I'm going to try and write my own physics to reduce overhead and increase performance (i hope). Also i'm going to focus more on core features instead of making everything big and fancy.

## TODO
- [ ] Improve vision, create a "cone" \ / instead of a single straight line (see CreatureBehaviour.cs 125 - vision edges)
  - [ ] Hopefully this will also fix the avoid force so that they wont collide while trying to avoid, if not also fix this
- [ ] Genetic Algorithm
  - [ ] Genomes
    - [ ] Mass (fat parents get fat kids)
    - [ ] Vision length
    - [ ] Color (aesthetic, to see which creatures are from the same familly)
  - [ ] Fitness function f(age)
  - [ ] Selection: https://en.wikipedia.org/wiki/Selection_(genetic_algorithm)
  - [ ] Single point crossover: https://en.wikipedia.org/wiki/Crossover_(genetic_algorithm)
  - [ ] Mutation: https://en.wikipedia.org/wiki/Mutation_(genetic_algorithm)

## DONE
- [x] Added NN input [..., 0-1 Bool wether we see something or not] this is to make a difference between white (RGB,000) and seeing "nothing" (RGB,-1-1-1) which gets translated to 000 by the NN
- [x] Added NN input [... , Velocity.x, Velocity.y] to get some different outputs even when you are seeing nothing the whole time
(otherwise it will always output the same if you keep seeing the same RGB values)
- [x] Improved Neural Network Feedforward()
- [x] Make creatures "Wander" if they see nothing
- [x] Neural Network
  - [x] Input (3): Vision; color of the thing we are seeing, format double[3] {R, G, B}
  - [x] Output (1): Chance to avoid/follow what we see
  - [x] Layers (1): ??
- [x] Make creatures "look" for food, instead of following the mouse
- [x] A way to represent objectives like food (maybe tile-based)
- [x] Basic creatures with basic physics and movement
- [x] Camera movement (zoom and drag)
