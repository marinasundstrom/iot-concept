# GPIO Server with GUI

This project was initally created to experiment with the Raspberry Pi hardware running Windows 10 IoT.

The app consists of a Graphical User Interface (GUI), built for the Universal Windows Platform (UWP), from which you can toggle Pin 4 (hardcoded).

It also runs a custom quick-and-dirty HttpServer, built on the Windows Runtime API:s, that can receive REST-requests to update or clear a given pin.

## Patterns

This app shows off some important patterns:

* Model-View-ViewModel (MVVM) - using MVVM Light Toolkit
* Service pattern - with abstractions
* Inversion of Control (IoC) and Dependency Injection

## Points of interest

### HttpServer class

This class mimics the HttpServer class that is available in the .NET Framework (desktop version). 

Using concepts similar to those found in .NET, it allows for easy handling of HTTP requests and responses, but using Windows Runtime API:s instead.

## REST API

The REST API exposes the functionality and allows for easy integration of devices with the server.

### Set pin value

Valid values for "state":

* false - sets to Low.
* true - sets to High.
* "toggle" - toggles between High and Low.

#### Request

```
POST <address>/led/<id>
```

Body:

```JSON
{ "state": true }
```

#### Response

```JSON
{ "id": 4, "state": true }
```

### Clear pin value

#### Request

```
DELETE <address>/led/<id>
```

#### Response

```JSON
{ "id": 4, "state": false }
```