# Improved Colony Simulation
This is going to be an improved version of [this old project](https://github.com/JohnnyDeeee/colony-simulation)

## Why?
Mainly because i was using Unity the wrong way. I wasn't making use of the script binding to a prefab and other stuff.
Also i was using the Physics Engine in Unity and thus the performance was really bad (in combination with multiple Neural Nets running every frame)

## What is going to be different?
I'm going to try and write my own physics to reduce overhead and increase performance (i hope). Also i'm going to focus more on core features instead of making everything big and fancy.

## TODO List
- [ ] Implement Neural Net **or** Genetic Algorithm
  - [ ] Neural Network
    - [ ] Input (3): Vision; color of the thing we are seeing, format int[3] {R, G, B}
    - [ ] Output (1): Direction to move to, format float { Degrees }
    - [ ] Layers (1): ??
    - [ ] Nodes (3): ??
  - [ ] Genetic Algorithm
    - [ ] Genomes
      - [ ] Mass (fat parents get fat kids)
      - [ ] Vision length
      - [ ] Color (aesthetic, to see which creatures are from the same familly)
    - [ ] Fitness function f(age)
    - [ ] Selection: https://en.wikipedia.org/wiki/Selection_(genetic_algorithm)
    - [ ] Single point crossover: https://en.wikipedia.org/wiki/Crossover_(genetic_algorithm)
    - [ ] Mutation: https://en.wikipedia.org/wiki/Mutation_(genetic_algorithm)
- [x] Make creatures "look" for food, instead of following the mouse
- [x] A way to represent objectives like food (maybe tile-based)
- [x] Basic creatures with basic physics and movement
- [x] Camera movement (zoom and drag)
