open System.Threading.Tasks
open Microsoft.Azure.Devices.Common.Exceptions
open Microsoft.Azure.Devices
open System
open FSharp.Configuration

type TestConfig = YamlConfig<"Config.yaml">


[<EntryPoint>]
let main argv = 
    
    let rec unwrapExMessage (exn:Exception)=
        match exn.InnerException with
            | null -> exn.Message
            | _ -> unwrapExMessage exn.InnerException

    let config = TestConfig()
    let iotRegManager=
        Microsoft.Azure.Devices.RegistryManager.CreateFromConnectionString(config.Azure.ConnectionString)

    let addDevicesAsync deviceId=
        let myDevice=
            new Microsoft.Azure.Devices.Device(deviceId)
        iotRegManager.AddDeviceAsync(myDevice)
    
    let executeAsync deviceId=async {
                 let! dev= addDevicesAsync deviceId |> Async.AwaitTask |> Async.Catch
                 match dev with
                    | Choice1Of2 device -> printfn "Device %s registered OK" device.Authentication.SymmetricKey.PrimaryKey
                    | Choice2Of2 error ->   printfn "Error! %s" (unwrapExMessage error) 
        }

    match argv with 
        | [|first|] -> executeAsync first |> Async.RunSynchronously
        | _ -> failwith "Must have only one argument."

    0 // return an integer exit code
