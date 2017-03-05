module TwitterAlerts

open Akka
open Akka.Actor
open Akka.FSharp
open System
open Serilog
open FSharp.Data.Toolbox.Twitter
open WindowsNotifications

type TwitterCreds = 
    {
        Key: string;
        Secret: string;
    }

let loadKeys () =
    let lines = System.IO.File.ReadLines(@"C:\Projects\twitterCreds.txt") |> Seq.toArray
    {
        Key = lines.[0].Trim();
        Secret = lines.[1].Trim();
    }

let logWith (log:ILogger) (prop:string) (value:obj) =
    log.ForContext(prop, value)

let recieve<'m> (mailbox:Actor<'m>) = mailbox.Receive()

type ConsoleMessage = 
    | ReadLine

type FeedMessage = 
    | PollFeed

type WatcherMessage = 
    | StartListening of string

type NewTweet = 
    {
        Text: string
        User: string
    }

let schedule (system: ActorSystem) actor message =
    system
        .Scheduler
        .ScheduleTellRepeatedly(TimeSpan.FromSeconds(0.0), TimeSpan.FromSeconds(5.0), actor, message);

let console log (watcher:IActorRef) =
    let log = logWith log "ActorName" "console"
    (fun (mailbox) ->
        let rec loop() = actor {
            let! message = recieve mailbox

            match message with
            | ReadLine ->
                Console.WriteLine("Listen out for: ")
                let feedTerm = Console.ReadLine()
                watcher <! (StartListening feedTerm)
                mailbox.Self <! ReadLine

            return! loop()
        }
        mailbox.Self <! ReadLine
        loop())

let feed log creds notifierRef (StartListening feedTerm) =
    let log = logWith log "ActorName" "feed"
    (fun (mailbox:Actor<_>) ->
        schedule (mailbox.Context.System) (mailbox.Self) PollFeed
        let twitter = Twitter.AuthenticateAppOnly(creds.Key, creds.Secret)
        let rec loop seenTweets = actor {
            let! message = recieve mailbox

            match message with
            | PollFeed ->
                let tweets = twitter.Search.Tweets(feedTerm, count = 1).Statuses
                let newTweets = tweets |> Seq.filter (fun x -> not (seenTweets |> List.contains x.Id ) )

                for newTweet in newTweets do
                    notifierRef <! { Text = newTweet.Text; User = newTweet.User.Name }

                let seenTweets = tweets |> Seq.map (fun x -> x.Id) |> Seq.toList

                return! loop seenTweets
        }
        loop [])

let watcher log creds notifierRef =
    let log = logWith log "ActorName" "watcher"
    (fun (mailbox) ->
        let rec loop() = actor {
            let! (message:WatcherMessage) = recieve mailbox

            let feedRef =
                spawnOpt mailbox null (feed log creds notifierRef message)
                    [
                        SpawnOption.SupervisorStrategy (Strategy.OneForOne (fun error -> Directive.Restart))
                    ]

            return! loop()
        }
        loop())

let notifier log =
    let log = logWith log "ActorName" "notifier"
    (fun (mailbox) ->
        let rec loop() = actor {
            let! (message:NewTweet) = recieve mailbox

            let notificationManager = NotificationManager("TwitterAlerts")
            notificationManager.Toast("New Tweet", message.Text, message.User)

            return! loop()
        }
        loop())


[<EntryPoint>]
let main argv =

    let creds = loadKeys ()

    let log =
        LoggerConfiguration()
            .WriteTo.Seq("http://localhost:5341")
            .CreateLogger()

    use system = System.create "my-system" (Configuration.load())

    let notifierRef = spawn system "notifer" <| notifier log
    let watcherRef = spawn system "watcher" <| watcher log creds notifierRef
    let consoleRef = spawn system "console" <| console log watcherRef

    system.WhenTerminated
        |> Async.AwaitTask
        |> Async.RunSynchronously

    0 // return an integer exit code
