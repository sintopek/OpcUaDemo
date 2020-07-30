# OpcUa Demo

Testing OPC UA stack

## Description 

This is a GUI of a trivially simple OPC UA browser.
Connects to an OPC UA server. Displays server tags and enables user to change their values.

This browser is tested together with a demo OPC UA server. See
```https://github.com/sintopek/OpcUaDemoServer```

(Note: The project is ongoing! Come back later for more features)

## Technologies used :gear:

- C# 
- .Net UI framework (Blazor)
- Official OPC UA C# stack from OPC UA Foundation.

## Setting up the development environment :wrench:
Clone the project repo to your preferred location
```https://github.com/sintopek/OpcUaDemo.git```

Open the project in your preffered Visual Studio edition

Run the app in development mode by pressing

```$  f5  or CTL + f5```

Navigate to OPC ua Demo page and Enter your server endpoint in Server url field, then hit CONNECT button.

Build Docker container on the root folder
```docker build .```

## Application deployed on 🚀

- [Azure Websites](https://highhillsoftwareopcuademo.azurewebsites.net/)
(Invites only)

## Dependencies :page_with_curl:

- https://docs.microsoft.com/en-us/samples/azure-samples/iot-edge-opc-client/azure-iot-opc-client/
- https://github.com/OPCFoundation/UA-.NETStandard

## Acknowledgements:
Stack Overflow topic on how to browse root node of OPC UA server
- https://stackoverflow.com/questions/30573689/opc-ua-minimal-code-that-browses-the-root-node-of-a-server
- [Author](https://stackoverflow.com/users/2071258/laurent-la-rizza)

## Author :man_artist:

High Hill Software team

Pekka Sintonen

Emmy Karangwa

