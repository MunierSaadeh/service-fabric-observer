# Using FabricObserver - Scenarios

**CPU Usage - CPU Time**  
FO makes it really easy for you to monitor the CPU usage behavior of any or your Windows Service Fabric Applications' service processes.
Remember that a Service Fabric Application is really just a logical "container" of related services, an abstract encapsulation or versioned configuration and code.
With this definition in mind, FO enables you to observe the overall CPU usage of your Application (so, the cumulative impact on CPU Time of all of its services)
or you can just monitor specific services and ask FO to Warn when some threshold is reached or exceeded. 

***Problem***: I want to know how much CPU my App is using, specifically two of the 5 services that I know
tend to eat more CPU then the rest of the family, and emit a warning when a specified threshold breached... 

***Solution***: The apt-named AppObserver is your friend. You can do exactly this, plus more. :)

AppObserver requires a JSON config file that contains an array of objects that represent an App (the target), its services, and 
a number of threshold settings that are all optional, of course. Basically, that was a long-winded way of saying: there's a config for that.

Let's first answer the question "How much CPU Time is too much?". Then, we use the answer to supply the following 
configuration for MyApp and the two services we care about (the service include list):
``` 
[
  {
    "target": "fabric:/MyApp",
    "serviceIncludeList": "ILikeCpuService, IAlsoLikeCpuService",
    "cpuWarningLimitPct": 65
  }
]
```

Now, let's say you have more then one App deployed (a common scenario) and only want to watch one or more of the services in a specific set of apps. 
You would do this:

``` 
[
 {
    "target": "fabric:/MyApp",
    "serviceIncludeList": "ILikeCpuService, ILikeCpuTooService",
    "cpuWarningLimitPct": 45
  },
  {
    "target": "fabric:/MyOtherApp",
    "serviceIncludeList": "ILoveCpuService",
    "cpuWarningLimitPct": 65
  }
]
```

Example Output in SFX: 

Cluster level:  

![alt text](/Documentation/Images/AppCpuWarnCluster.jpg "Logo Title Text 1")  


App level:  


![alt text](/Documentation/Images/CPUWarnApp.jpg "Logo Title Text 1")  



**Disk Usage - Space**  
Running out of disk space is a bad place to be in any virtual or non-virtual computing environment. When your logical disks
can handle no more data, then your operating system and all of the things running inside of it begin to fail, including Service Fabric and your services...

***Problem:*** I want to know when my local disks are filling up well before my cluster goes down, along with the VM.

***Solution:*** DiskObserver to the rescue. You simply tell DiskObserver what percentage of unavailable disk space is too much, and make sure
to give yourself plenty of breathing room so you have time to fix the problem before everything burns down.

DiskObserver's Threshold settings are housed in the usual place: PackageRoot/Config/Settings.xml.

Here is the magical setting to have DiskObserver warn you when disk space consumption reaches 80%:

```
  <Section Name="DiskObserverConfiguration">
    <Parameter Name="Enabled" Value="True" />
    <Parameter Name="EnableVerboseLogging" Value="False" />
    <Parameter Name="DiskSpacePercentWarningThreshold" Value="80" />
  </Section>
```  

Example Output in SFX: 

Node Level:  

![alt text](/Documentation/Images/DiskWarnNode.jpg "Logo Title Text 1")  

Cluster Level: 

![alt text](/Documentation/Images/ClusterDiskMemory.jpg "Logo Title Text 1") 

 
**Memory Usage** 

``` 
Without working set, there can be no work.
- Hercule Poirot (Not really, but if you like mysteries, then you'll love debugging memory abuse...)
```
Memory is always an important resource in today's data and compute heavy workloads. Sure, just rent more virtual RAM... However, you definitely 
need to keep an eye on how much memory your services are consuming as part of their Happy Place (under load) and determine the Bad Place 
in order to define meaningful memory use Warning thresholds. 

***Problem:*** I want to know how much memory some or all of my services are using and warn when they hit some meaningful percent-used thresold.  

***Solution:*** AppObserver is your friend.  

The first two JSON objects below tell AppObserver to warn when any of the services under MyApp app reach 30% memory use (as a percentage of total memory). 
 
The third one scopes to all services _but_ 3 (a new wrinkle!) and asks AppObserver to warn when any of them hit 40% memory use on the machine (virtual or not).

```
  {
    "target": "fabric:/MyApp",
    "memoryWarningLimitPercent": 30
  },
  {
    "target": "fabric:/AnotherApp",
    "memoryWarningLimitPercent": 30
  },
  {
    "target": "fabric:/SomeOtherApp",
    "serviceExcludeList": "WhoNeedsMemoryService, NoMemoryNoProblemService, Service42"
    "memoryWarningLimitPercent": 40
  }
```   


**Different thresholds for different services belonging to the same app**  

***Problem:*** I want to monitor and report on different services for different thresholds 
for one app.  

***Solution:*** Easy. You can supply any number of array items in AppObserver's JSON configuration file
regardless of target - there is no requirement for unique target properties in the object array. 

```
  {
    "target": "fabric:/MyApp",
    "serviceIncludeList": "MyCpuEatingService1, MyCpuEatingService2",
    "cpuWarningLimitPct": 45
  },
  {
    "target": "fabric:/MyApp",
    "serviceIncludeList": "MemoryCrunchingService1, MemoryCrunchingService42",
    "memoryWarningLimitPercent": 30
  }
```


If what you really want to do is monitor for different thresholds (like CPU and Memory) for a set of services, you would
just add the threshold properties to one object: 

```
  {
    "target": "fabric:/MyApp",
    "serviceIncludeList": "MyCpuEatingService1, MyCpuEatingService2, MemoryCrunchingService1, MemoryCrunchingService42",
    "cpuWarningLimitPct": 45,
    "memoryWarningLimitPercent": 30
  }
```  

 
The above configuration applies both CPU Time and Memory Usage thresholds to all included services in serviceIncludeList.
It all depends on what you want to do. The world is your configuration. Here is an example of a JSON object
that monitors and reports on a number of properties - including Error thresholds, which you want to be VERY careful using
unless you understand that emitting Error Health Reports to Service Fabric puts the App or Node into a state where updates can't take place, which
impacts the overall health of your cluster, for example. That said, it's totally reasonable to make the call that exceeding
some threshold for some resource property should put the App or Node into an Error state.  


```
{
    "target": "fabric:/MyApp",
    "serviceIncludeList": "MyService42, MyOtherService42",
    "cpuErrorLimitPct": 90,
    "cpuWarningLimitPct": 80,
    "diskIOErrorReadsPerSecMS": 0,
    "diskIOErrorWritesPerSecMS": 0,
    "diskIOWarningReadsPerSecMS": 0,
    "diskIOWarningWritesPerSecMS": 0,
    "dumpProcessOnError": true,
    "memoryErrorLimitPercent": 90,
    "memoryWarningLimitPercent": 70,
    "networkErrorActivePorts": 0,
    "networkWarningActivePorts": 800,
    "networkErrorEphemeralPorts": 0,
    "networkWarningEphemeralPorts": 400
  }
``` 
> You can learn all about the currently implemeted Observers and their supported resource properties aross App and Node level observations [***here***](/Documentation/Observers.md). 



**What about the state of the Machine, as a whole?** 

***Problem:*** I want to know when Total CPU Time and Memory Consumption on the VM (or real machine)
reaches certain points and then emit a Warning.  

***Solution:*** Enter NodeObserver.  

NodeObserver doesn't speak JSON (can you believe it!!??....). So, you simply set the desired warning
thresholds in PackageRoot/Config/Settings.xml:  

```
  <Section Name="NodeObserverConfiguration">
    <Parameter Name="Enabled" Value="True" />
    <Parameter Name="EnableVerboseLogging" Value="False" />
    <Parameter Name="CpuWarningLimitPercent" Value="90" />
    <Parameter Name="MemoryWarningLimitPercent" Value="90" />
  </Section>
```  

Example Output in SFX: 

![alt text](/Documentation/Images/MemoryWarnNode.jpg "Logo Title Text 1") 



**Networking: Endpoint Availability**  

***Problem:*** I want to know when the endpoints I care about are not reachable.  

***Solution:*** NetworkObserver at your service. Let's say you have 3 critical endpoints that 
need to always be available (as if that is possible, but let's pretend, shall we)
and if not, you want to put the impacted app into a Warning state. Here's how you would do that.

Once again, ladies and gentlemen, JSON. 

```
[
  {
    "appTarget": "fabric:/MyApp",
    "endpoints": [
      {
        "hostname": "critical.endpoint.com",
        "port": 443
      },
      {
        "hostname": "another.critical.endpoint.net",
        "port": 443
      },
      {
        "hostname": "eastusserver0042.database.windows.net",
        "port": 1433
      }
    ]
  },
  {
    "appTarget": "fabric:/AnotherApp",
    "endpoints": [
      {
        "hostname": "critical.endpoint42.com",
        "port": 443
      },
      {
        "hostname": "another.critical.endpoint.net",
        "port": 443
      },
      {
        "hostname": "westusserver0007.database.windows.net",
        "port": 1433
      }
    ]
  }
]
```  

Example Output in SFX: 


App Level: 

![alt text](/Documentation/Images/NetworkEndpointWarning.jpg "Logo Title Text 1")   

Cluster Level: 

![alt text](/Documentation/Images/NetworkEndpointWarningCluster.jpg "Logo Title Text 1") 