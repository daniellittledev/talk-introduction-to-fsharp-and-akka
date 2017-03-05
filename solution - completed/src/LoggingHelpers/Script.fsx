#r @"..\..\packages\Serilog\lib\net45\Serilog.dll"
#load "LoggingHelpers.fs"

open LoggingHelpers.SerilogHelpers
open Serilog
open System

let log = 
    Serilog.LoggerConfiguration()
        .WriteTo.Sink({ new Core.ILogEventSink with
            member this.Emit(logEvent) =
                Console.WriteLine(logEvent.RenderMessage())
        })
        .CreateLogger()

let doesSomeWork log =
    logInformation log "Hello"

doesSomeWork log
