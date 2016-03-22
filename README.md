# IoT concept app for UWP with iOS client app

This project was initally created to experiment with the Raspberry Pi hardware running Windows 10 IoT.

The app consists of a Graphical User Interface (GUI), built for the Universal Windows Platform (UWP), from which you can toggle Pin 4 (hardcoded).

It also runs a custom quick-and-dirty HttpServer, built on the Windows Runtime API:s, that can receive REST-requests (JSON) to update or clear a given pin.

This REST API allows for the iOS client app to connect to the server and control the GPIO-pins.

## Patterns

This app shows off some patterns that are popular in app development:

* Model-View-ViewModel (MVVM) - using MVVM Light Toolkit
* Service pattern - with abstractions
* Inversion of Control (IoC) and Dependency Injection

## Setup

The tested setup consists of:

* Raspberry Pi 2 with Windows 10 IoT
* Touchscreen Display (the one from Raspberry Pi Foundation)
* Breadboard (based on the standard Blinky example)
	* LED
	* Resistor

The components on the breadboard are connected to Pin 4 and Ground (any) on the Raspberry Pi device.

The iOS app can either run in the simulator or be deployed to an actual device.

Make sure to change all addresses in the application so that the apps and devices work with your network setup.

In IotDemo, check NetService, verify that the port is available, and in IotDemo.iOS, check IotClient, and modify the BaseURL.

## Points of interest

Here are some notable things about this app.

### HttpServer class

This class mimics the HttpServer class that is available in the .NET Framework (desktop version). 

Using concepts similar to those found in .NET, it allows for easy handling of HTTP requests and responses, but using Windows Runtime API:s instead.

### iOS app

Basic app, built with Xamarin.iOS, that lets the user toggle the hardcoded pin 4.

Uses the same structure and patterns as the UWP app does.

## REST API

The REST API exposes the functionality and allows for easy integration of devices with the server. 

The data format used is JSON.

### Get pin value

Gets the current value of the pin with specified id.

#### Request

```
GET <address>/led/<id>
```

#### Response

```JSON
{ "id": 4, "state": true }
```


### Set pin value

Sets the value for the pin with specified id.

Valid values for "state":

* false - sets to Low.
* true - sets to High.
* "toggle" - toggles between High and Low.

#### Request

```
POST <address>/led/<id>
```

##### Body (JSON)

```JSON
{ "state": true }
```

#### Response

```JSON
{ "id": 4, "state": true }
```

### Clear pin value

Clears the value (sets to low) for the pin with specified id.

#### Request

```
DELETE <address>/led/<id>
```

#### Response

```JSON
{ "id": 4, "state": false }
```
