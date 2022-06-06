# BikeBalance
There are several bike controllers in Unity that maintain balance with a variety of stabilizers. In the real world, the cyclist balances with the handlebars. I tried to model this natural way of balancing in Unity. I was afraid that the physics of Unity, especially WheelCollider, would not allow me to simulate this delicate process. To my surprise, everything worked and the controller turned out to be very simple.

To avoid falling, the cyclist must turn the handlebars in the direction of the slope. To turn, the cyclist must perform a paradoxical action: He must turn the handlebars in the direction opposite to the target. For example, we need to turn left. We turn the handlebars to the right, the bike gets a tilt to the left, set the handlebars to the balance position and begin the turning maneuver.

For a given speed and lean angle, there is a steering angle that compensates for gravity. It is **balance angle** of steer.
There are two ways to control in this project: Control by lean and steer control. In both cases, I use a PID controller that brings the system to a state of equilibrium at a given angle of inclination or steering angle.

[![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/OlC7LPSNq_E/0.jpg)](https://www.youtube.com/watch?v=OlC7LPSNq_E)
https://youtu.be/OlC7LPSNq_E
