#r "System.Xml.Linq.dll"
#r "packages/Suave/lib/net40/Suave.dll"
#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#r "packages/DotLiquid/lib/NET40/DotLiquid.dll"
#r "packages/Suave.DotLiquid/lib/net40/Suave.DotLiquid.dll"
open System
open System.Web
open System.IO
open FSharp.Data
open Suave
open Suave.Web
open Suave.Http
open Suave.Filters
open Suave.Operators

DotLiquid.setTemplatesDir (__SOURCE_DIRECTORY__ + "/templates")

// -------------------------------------------------------------------------------------------------
// STEP #1: Hello world and composing web parts
// -------------------------------------------------------------------------------------------------

let app_1 = 
  Successful.OK "Hello Krakow!"


let app_1b =
  choose
    [ path "/foo" >=> Successful.OK "Hello Krakow!!!!!!" 
      pathScan "/add/%d/%d" (fun (a, b) -> 
        Successful.OK(string (a + b)) )
      Successful.OK "Hello rest of the world!" ]
















let app_2 =
  choose
    [ path "/" >=> Successful.OK "See <a href=\"/add/40/2\">40 + 2</a>"
      pathScan "/add/%d/%d" (fun (a,b) -> Successful.OK(string (a + b)))
      RequestErrors.NOT_FOUND "404: Not found" ]

// TODO: Write an echo service that will print your name
// For example, given "/echo/Tomas", it should print <h1>Hello Tomas</h1>
// To accept strings in the path, you can use "%s"

// -------------------------------------------------------------------------------------------------
// STEP #2: Records and templating with DotLiquid
// -------------------------------------------------------------------------------------------------

type News = 
  { Title : string 
    DesCription : string
    Link : string }

// Create a value of type News
let newsItem id =
  { Title = sprintf "Suave workshop %d!" id
    DesCription = "Suave workshop is happening today, do not miss it!"
    Link = "http://www.fsharpworks.com" }
    
// Working with collections using higher-order functions
let demo3 = 
  [ 1 .. 10 ]
  |> Seq.map (fun i -> newsItem i)
  |> Seq.filter (fun n -> n.Title.Contains "5")
  |> List.ofSeq

let app_3 = 
  DotLiquid.page "home.html" demo3

// Working with collections using list comprehensions
let demo4 = 
  [ for i in 1 .. 10 do
      yield newsItem i ]
  
let app_4 = 
  DotLiquid.page "home.html" demo4

// -------------------------------------------------------------------------------------------------
// STEP #3: Calling HTTP based REST services
// -------------------------------------------------------------------------------------------------

// TODO: Show weather forecast for the next few days:
// "http://api.openweathermap.org/data/2.5/forecast/daily?units=metric&q=Krakow&APPID=cb63a1cf33894de710a1e3a64f036a27"

type Weather = JsonProvider<"http://api.openweathermap.org/data/2.5/forecast/daily?units=metric&q=Krakow&APPID=cb63a1cf33894de710a1e3a64f036a27">
type RssFeed = XmlProvider<"http://fpish.net/rss/blogs/tag/1/f~23">

let rss = RssFeed.Load("http://feeds.bbci.co.uk/news/rss.xml")
let w = Weather.GetSample()

let data =
  rss.Channel.Items 
  |> Seq.map (fun i ->
      { Title = i.Title
        DesCription = i.Description
        Link = i.Link } )

let app_w = 
  DotLiquid.page "home.html" data










let news = 
  [ for item in rss.Channel.Items do
      yield item ]
  
let app_5 = 
  DotLiquid.page "home.html" news

// -------------------------------------------------------------------------------------------------
// ENTRY POINT: Choose which of the demos to run here!
// -------------------------------------------------------------------------------------------------

let app = app_w


// -------------------------------------------------------------------------------------------------
// To run the web site, you can use `build.sh` or `build.cmd` script, which is nice because it
// automatically reloads the script when it changes. But for debugging, you can also use run or
// run with debugger in VS or XS. This runs the code below.
// -------------------------------------------------------------------------------------------------

#if INTERACTIVE
#else
let cfg =
  { defaultConfig with
      bindings = [ HttpBinding.mkSimple HTTP  "127.0.0.1" 8011 ]
      homeFolder = Some __SOURCE_DIRECTORY__ }
let _, server = startWebServerAsync cfg app
Async.Start(server)
System.Diagnostics.Process.Start("http://localhost:8011")
System.Console.ReadLine() |> ignore
#endif
