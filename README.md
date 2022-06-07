# BikeBalance


## What is it
In this Unity project, the balance of the bike is achieved solely by changing the steering angle. The user sets the target steering angle, then PID controller sets current steering angle. Finally, the system comes into equilibrium at the required steering angle and the corresponding tilt angle.

Video:

[![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/51N4ieE62lc/0.jpg)](https://www.youtube.com/watch?v=51N4ieE62lc)

## How to use
Download this project, then import the root folder (BikeBalance) into a new Unity project.

There are two input methods:
* AWSD
* Mouse 
 
 To select mouse input, check the "Use Mouse" checkbox in the BikeController script.
 
 The user can control steer angle or lean angle. To select a control method, check the "Use Steer" checkbox in the BikeController script.
