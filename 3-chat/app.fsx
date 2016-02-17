#r "packages/Suave/lib/net40/Suave.dll"
open System
open System.IO
open Suave
open Suave.Web
open Suave.Http
open Suave.Filters
open Suave.Operators
open Suave.Successful

// ------------------------------------------------------------------
// TASK #1: Implement a chat room agent to keep the messages
// which handles the following two kinds of commands:

type ChatMessage =
  | Add of name:string * text:string
  | Get of AsyncReplyChannel<string>

let chat = MailboxProcessor<ChatMessage>.Start(fun inbox ->
  let rec loop lines = async { 
    // TODO: Receive message and handle 'Add' and 'Get'
    return () }
  loop [] )

// ------------------------------------------------------------------
// TASK #2: Call your agent from the following two functions 
// (or WebParts) that handle "/chat" and "/post" requests
// in the HTTP server. The first should return all text and
// the second should add a new message to the agent

let getMessages room ctx = async {
  // TODO: Change the following line 
  let html = "<ul><li><strong>System:</strong> In a room " + room + "!</li></ul>"
  return! Successful.OK html ctx }

let postMessage room ctx = async {
  let name = 
    match ctx.request.queryParam "name" with
    | Choice1Of2 n -> n
    | _ -> "Anonymous"
  use ms = new MemoryStream(ctx.request.rawForm)
  use sr = new StreamReader(ms)
  let text = sr.ReadToEnd()  
  // TODO: Change the following line 
  printfn "%s says: %s" name text
  return! ACCEPTED "OK" ctx }

// ------------------------------------------------------------------
// BONUS: If you implemented the routing agent in the previous
// task, then you can use it here to add support for multiple
// chat rooms in our agent - just use the 'room' parameter!


// ------------------------------------------------------------------
// DEMO: Building a web server that handles the requests
// ------------------------------------------------------------------

let index = File.ReadAllText(__SOURCE_DIRECTORY__ + "/web/chat.html")

let noCache =
  Writers.setHeader "Cache-Control" "no-cache, no-store, must-revalidate"
  >=> Writers.setHeader "Pragma" "no-cache"
  >=> Writers.setHeader "Expires" "0"

let app =
  choose
    [ // Routing for default URLs without room names
      path "/" >=> Writers.setMimeType "text/html" >=> OK index
      path "/chat" >=> GET >=> noCache >=> getMessages "default"
      path "/post" >=> POST >=> noCache >=> postMessage "default"

      // Routing for URLs that start with room name
      pathScan "/%s/" (fun _ -> Writers.setMimeType "text/html" >=> OK index)
      pathScan "/%s/chat" (fun room -> GET >=> noCache >=> getMessages room)
      pathScan "/%s/post" (fun room -> POST >=> noCache >=> postMessage room)
      RequestErrors.NOT_FOUND "Found no handlers" ]
