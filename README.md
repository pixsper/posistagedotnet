<img src="/resources/PosiStageDotNet-Alpha.png" width="256" alt="PosiStageDotNet">
PosiStageDotNet is a C# library for implementing the PosiStageNet protocol. The protocol is used to pass position, speed, orientation and other automation data between entertainment control systems, for example, between an automation controller and a lighting desk or media server.

<img src="/resources/psn%20logos/PSN_Black.svg" width="128" alt="PSN Logo">

See <http://www.posistage.net/> for more information on PosiStageNet.

## NuGet

![Nuget](https://img.shields.io/nuget/v/Pixsper.PosiStageDotNet?logo=nuget)

The library is available from NuGet.org as [Pixsper.PosiStageDotNet](https://www.nuget.org/packages/Pixsper.PosiStageDotNet).

## License

The library is release under the [MIT license](https://opensource.org/licenses/MIT), allowing use in commerical/non-commerical projects providing the copyright notice is reproduced.


## Copyright

Copyright (c) 2022 Pixsper Ltd.


## Simple Examples
### Sending Data
```C#
// Set this to the IP of the network interface you want to send PSN packets on
var adapterIp = IPAddress.Parse("10.0.0.1");

var psnServer = new PsnServer("Test PSN Server", adapterIp);

var trackers = new []
{
    new PsnTracker(0, "Tracker 0", 
        position: Tuple.Create(0f, 0f, 0f), 
        speed: Tuple.Create(0f, 0f, 0f), 
        orientation: Tuple.Create(0f, 0f, 0f)),
    new PsnTracker(1, "Tracker 1", 
        position: Tuple.Create(10.4f, 0f, 0f), 
        speed: Tuple.Create(1.23f, 0f, 0f), 
        orientation: Tuple.Create(0f, 85.34f, 0f)),
    new PsnTracker(2, "Tracker 2", 
        position: Tuple.Create(5.232f, 2.654f, 13.765f), 
        speed: Tuple.Create(1f, 3f, 0f), 
        orientation: Tuple.Create(23.3f, 43.3f, 76.2f))
};

psnServer.SetTrackers(trackers);

psnServer.StartSending();

// Take some more readings...

// PsnTrackers are immutable, use the 'with' methods to create a copy with mutated values
var tracker2Update = trackers[2].WithPosition(Tuple.Create(6.345f, 2.23f, 13.098f));

// We can update values for individual trackers, replacing any tracker data with the same index
psnServer.UpdateTrackers(tracker2Update);

// When you're finished...
psnServer.StopSending();

// Don't forget to dispose!
psnServer.Dispose();
```

### Receiving Data
```C#
// Set this to the IP of the network interface you want to listen for PSN packets on
var adapterIp = IPAddress.Parse("10.0.0.1");

var psnClient = new PsnClient(adapterIp);

psnClient.TrackersUpdated += (s, e) =>
{
    foreach (var t in e.Values)
        Console.WriteLine(t);
};

psnClient.StartListening();

// Do something with the tracker data

// When you're finished...
psnClient.StopListening();

// Don't forget to dispose!
psnClient.Dispose();
```
