// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open Microsoft.Azure.Devices.Client
open Newtonsoft.Json
open FSharp.Configuration
open System

type TestConfig = YamlConfig<"Config.yaml">

type windSpeedMessage = {DeviceId :string; WindSpeed:float }

[<EntryPoint>]
let main argv = 
    let config=TestConfig()
    let iotHubUri = config.Azure.HubHostName
    let deviceKey = config.Azure.DeviceKey
    let getRandomMessageToSend()=
        let avgWindSpeed = 10.0 // m/s
        let rand = new Random()
        let currentWindSpeed = avgWindSpeed + rand.NextDouble() * 4.0 - 2.0;
        let msg={DeviceId=deviceKey;WindSpeed=currentWindSpeed}
        let json=JsonConvert.SerializeObject(msg)
        new Message(System.Text.Encoding.ASCII.GetBytes(json))

    printfn "Simulated device"
    let deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey("mydevid", deviceKey), TransportType.Mqtt);
    while true do
        let task=deviceClient.SendEventAsync(getRandomMessageToSend())
        System.Threading.Tasks.Task.Delay(1000).Wait();
        task.Wait()
    0 // return an integer exit code
     