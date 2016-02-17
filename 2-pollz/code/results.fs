module Pollz.Results
open Pollz.Data
open FSharp.Data
open Suave
open Suave.Http
open Suave.Filters
open XPlot.GoogleCharts

// ----------------------------------------------------------------------------
// STEP #2: Implement code to display the poll results. To do this, you first
// need to complete STEP #2 in `data.fs`. Then you'll need to implement the
// `getResults` function below using the `Data.loadPoll` helper and using
// `Data.votes` (which contains the votes)
// ----------------------------------------------------------------------------

/// For each answer, we show the answer and the number of votes
type Answer = 
  { Option : string 
    Votes : int }

/// The model contains basic poll info and a sequence of results
type Poll =
  { Title : string
    Question : string 
    Results : seq<Answer> }

let ``TUTORIAL DEMO #2`` () = 
  // Say we have a map (which is what `Data.votes` will give you)
  let testVotes = Map.ofSeq [ ("testpoll", [0;1;0;0;0;1]) ]
  // You can get the data for a given key using `TryFind`
  let optVotes = testVotes.TryFind "testpoll"
  // And you can provide a default value using `defaultArg`
  // (by default, we record no votes)
  let votes = defaultArg optVotes []

  // To count how many votes for a certain option are there, you
  // then need to count how many times a certain value occurs in 
  // the votes list. You can do that using `List.filter` and `List.length`
  [ 0;1;0;1;0 ]
  |> List.filter (fun x -> x = 0)
  |> List.length


// You can delee the `TUTORIAL DEMO #1` function and implement the following:
let getResults pollName =
  { Title = "Testing"
    Question = "What is your favourite color?"
    Results = 
        [ { Option = "Yo"; Votes = 2 }
          { Option = "Nae"; Votes = 5 } ] }

let part1 =
  pathScan "/results/%s" (fun name ->
    DotLiquid.page "results.html" (getResults name) )

// ----------------------------------------------------------------------------
// STEP #6: Plot the results on a chart. To do this add `Chart` property of 
// type `string` to the `Poll` (and modify `results.html` to display this).
// To get the HTML, just call `Chart.Pie` (or whichever chart you like) and
// get the `InlineHtml`.
// ----------------------------------------------------------------------------

let ``TUTORIAL DEMO #6`` () = 
  // Make a chart using XPlot
  let chart = Chart.Pie [ ("Good",12); ("Bad",1) ]
  // Get the HTML of the chart
  chart.InlineHtml


// ----------------------------------------------------------------------------
// STEP #9: Sending live updates with WebSockets! The `socket { .. }`
// computation is like `async { .. }` except that you can send messages
// to the client using `webSocket.send`. Here, we want to send JSON
// updates with new data when someone votes. To do this, we'll also need
// to modify `data.fs` to trigger an event when voting happens.
// ----------------------------------------------------------------------------


open Suave.Utils
open Suave.Sockets
open Suave.WebSocket
open Suave.Sockets.Control

// Defines a type `LiveUpdate.Root` that you can use to produce JSON values
type LiveUpdate = JsonProvider<"""
  { "chart":"<div>...</div>", "votes":[1,2,3] }""">

let ``TUTORIALO DEMO #9`` () =
  // Create a new JSON value
  let obj = LiveUpdate.Root("<div>New HTML</div>", [| 10; 10 |])
  // Use F# Data to format it as string
  obj.JsonValue.ToString()
  

let sendLiveUpdates pollName (webSocket:WebSocket) ctx = socket {
  while true do
    // Once you add `votesUpdated` event to `data.fs`, you can wait
    // until it is triggered using the following:
    //
    //   let! update = 
    //     votesUpdated.Publish 
    //     |> Async.AwaitEvent |> SocketOp.ofAsync
    //
    // This will give you the name of the poll that has been changed.
    // If this is the same as current `pollName`, then create a JSON
    // string with the new data (see DEMO #9) and send it to the client!
    // (You can reuse `getResults` to get the data).

    let update = "Hello client!"
    do! webSocket.send Text (ASCII.bytes update) true 

    // Let's sleep for 10 seconds before another update
    // (You will not need this once you implement the event!)
    do! Async.Sleep(10000) |> SocketOp.ofAsync }
  
let part2 = 
  pathScan "/results/%s/updates" (fun name ->
    
    ServerErrors.INTERNAL_ERROR "Not implemented...."
    // handShake (sendLiveUpdates name)

  ) 
