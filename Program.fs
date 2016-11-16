﻿open Suave
open System.Net

type CmdArgs = { IP: System.Net.IPAddress; Port: Sockets.Port }

[<EntryPoint>]
let main argv = 

    // parse arguments
    let args =
        let parse f str = match f str with (true, i) -> Some i | _ -> None

        let (|Port|_|) = parse System.UInt16.TryParse
        let (|IPAddress|_|) = parse System.Net.IPAddress.TryParse

        //default bind to 127.0.0.1:8083
        let defaultArgs = { IP = System.Net.IPAddress.Loopback; Port = 8083us }

        let rec parseArgs b args =
            match args with
            | [] -> b
            | "--ip" :: IPAddress ip :: xs -> parseArgs { b with IP = ip } xs
            | "--port" :: Port p :: xs -> parseArgs { b with Port = p } xs
            | invalidArgs ->
                printfn "error: invalid arguments %A" invalidArgs
                printfn "Usage:"
                printfn "    --ip ADDRESS   ip address (Default: %O)" defaultArgs.IP
                printfn "    --port PORT    port (Default: %i)" defaultArgs.Port
                exit 1

        argv |> List.ofArray |> parseArgs defaultArgs

    let log x = printfn "%A" x; x

    let getHostName() = 
        Dns.GetHostName()

    let getHostEntry (hostName:string) = 
        Dns.GetHostEntryAsync(hostName).Result

    let getIPs (hostEntry:IPHostEntry) = 
        hostEntry.AddressList

    let getIPString (ip:IPAddress) =
        if ip.AddressFamily = Sockets.AddressFamily.InterNetwork
        then Some (ip.ToString())
        else None

    let getIPStrings() = 
        getHostName()
        |> log
        |> (getHostEntry >> log)
        |> (getIPs >> log)
        |> Array.choose getIPString
        |> Array.toList


    // start suave
    startWebServer
        { defaultConfig with
            bindings = [ HttpBinding.create HTTP args.IP args.Port ] }
        (Successful.OK (sprintf "Hello world: %A, |%s|" (System.Guid.NewGuid()) (getHostName())))

    0
