// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open Microsoft.ServiceBus.Messaging
open System.Threading
open FSharp.Configuration
open System
open System.Threading.Tasks

type TestConfig = YamlConfig<"Config.yaml">

type System.Threading.Tasks.Task with
  static member WaitAll(ts) =
    Task.WaitAll [| for t in ts -> t :> Task |]

[<EntryPoint>]
let main argv = 
    let config = TestConfig()
    let connectionString = config.Azure.ConnectionString
    let iotHubD2cEndpoint = "messages/events"
    let eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, iotHubD2cEndpoint)
    
    let cts = new CancellationTokenSource()

    let actionOnCancel s (e : System.ConsoleCancelEventArgs) =
        e.Cancel <- true
        cts.Cancel()
        System.Console.WriteLine("Exiting...")
        
    System.Console.CancelKeyPress.Add(fun arg ->arg.Cancel <- true; cts.Cancel(); printfn "CancelKeyPress";  ) 
    
    let ReceiveMessagesFromDeviceAsync (partition:string) (ct:CancellationToken)=async{
        let eventHubReceiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.UtcNow)
        let mutable continueLooping = true
        //this feels wrong
        while continueLooping do
            if ct.IsCancellationRequested then
                continueLooping <- false
            let! eventData = eventHubReceiver.ReceiveAsync() |> Async.AwaitTask
            match eventData with
                | null -> ()
                | _ -> printfn "%s" (System.Text.Encoding.UTF8.GetString(eventData.GetBytes()))
            }
    let mutable tasks = []
    let d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds
    for part in d2cPartitions do
        let tsk=ReceiveMessagesFromDeviceAsync part cts.Token |> Async.StartAsTask
        tasks<-List.append tasks [tsk]
    
    printfn "Waiting on events...."
    Task.WaitAll tasks
        
    
    0 // return an integer exit code
