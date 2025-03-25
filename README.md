# OpenGL Graphics Lab Assignments

This repository contains various OpenGL lab assignments, implementing 3D graphics concepts, shading techniques, and interactive elements.

---

## Lab1: Static 3D Cube
A cube is rendered using precomputed points, appearing three-dimensional. The cube is static and consists of three colors.

## Lab1_3P: Enhanced Static Cube
Similar to Lab1, but with specific colors:
- Top: White
- Right Side: Blue
- Left Side: Red
- Black static lines separate the faces

This was an extra homework task.

---

## Lab2_2: Interactive 3D Rubik's Cube
A fully colored Rubik's cube:
- **Top:** White
- **Sides:** Blue, Red, Green, Orange
- **Bottom:** Yellow
- Hidden parts are black.

### Controls:
- Rotate **top horizontal layer**: `Q` (clockwise), `W` (counterclockwise)
- Move camera:
  - **Arrow keys**: Move
  - **Mouse movement**: Look around
  - `Tab`: Move up (Y-axis)
  - `LeftShift`: Move down (Y-axis)

---

## Lab2_3P: Enhanced Rubik's Cube
An extension of Lab2_2, adding:
- **Completion Effect:** When solved, the cube pulses and rotates.
- **Shuffling:** `R` key applies 30 random moves.
- **Full Layer Rotation Controls:**
  - `Q - W`: Top horizontal
  - `A - S`: Middle horizontal
  - `Z - X`: Bottom horizontal
  - `F - V`: Left vertical back
  - `G - B`: Left vertical middle
  - `H - N`: Left vertical front
  - `U - J`: Right vertical front
  - `I - K`: Right vertical middle
  - `O - L`: Right vertical back
  - `Enter`: Stop pulsing animation
  - **Numpad for movement:**
    - `5`: Forward
    - `2`: Backward
    - `1`: Left
    - `3`: Right
    - `7`: Up (Y-axis)
    - `4`: Down (Y-axis)
    - **Mouse movement:** Look around

---

## Lab3_1: Phong Shading - Circular Barrel Model
- 18 vertical rectangles form a closed **barrel shape**.
- Model is defined in the X-Y plane with:
  - **Height:** 2 units
  - **Width:** 1 unit
- Placement ensures proper alignment using 20-degree rotations.
- **Two versions:**
  1. **Normals perpendicular to surfaces**
  2. **Normals calculated with 10-degree outward shift**
- Lighting placed outside the circle, possibly at the camera position.
- **Comparison of both barrels placed one above the other.**

---

## Lab3_2: Phong Shading with Adjustable Properties
Enhancements over Lab3_1:
- Lighting coefficients (`ambientStrength`, `specularStrength`, `diffuseStrength`) are now **adjustable 3D vectors** instead of hardcoded values.
- UI elements added:
  - Sliders to adjust **illumination components**
  - Sliders to change **background light color**
  - Dropdown menu to change the **color of a cube face**
- **Animation:**
  - Press `Space` to make the **small cube spin and orbit** around the large cube.
  - The **large cube pulses in size.**

---

## [Lab3_3](https://github.com/ZoltaniSzabolcs/Rubick-Cube-OpenGL): Rubik's Cube with Phong Shading
Extends Lab2-2 or Lab2-3 by adding:
- **Phong shading model.**
- **Graphical UI for lighting adjustments:**
  - 3 sliders for **light color components**
  - 3 text fields to set **light source position**
- **All previous movement and rotation controls remain unchanged.**

---

## Lab4_1: OBJ Model Loading & Lighting
- Implements an **OBJ file loader** supporting normal vectors (`vn`).
- Loads a **flower model** with lighting and a **skybox**.
- **Navigation:**
  - **Arrow keys:** Move while always looking at the start position.
  - **Move away / left / right**

---

## Aim Trainer Project
A separate project implementing an **aim trainer** with an **AK-47 rifle**, where the player shoots at spheres to practice aiming.

### GitHub Repository:
[UNI-AIM](https://github.com/ZoltaniSzabolcs/UNI-AIM)

[Lab3_3](https://github.com/ZoltaniSzabolcs/Rubick-Cube-OpenGL)
