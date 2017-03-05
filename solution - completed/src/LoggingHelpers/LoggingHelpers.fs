namespace LoggingHelpers

module SerilogHelpers =

    open Serilog

    let logInformation (log: ILogger) messageTemplate =
        log.Information(messageTemplate)

    let logInformationWith (log: ILogger) messageTemplate (arguments:obj seq) =
        log.Information(messageTemplate, arguments |> Seq.toArray)

