module Server

open FSharp.Control.Tasks
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Identity.Web
open Saturn
open Giraffe
open Microsoft.AspNetCore.Http

open Shared

type Storage () =
    let todos = ResizeArray<_>()

    member __.GetMessageRequiringLoggedIn() = LoggedInMessage "Logged in message from server"

    member __.GetMessageRequiringAuthChallenge() = AuthChallengeMessage "Auth challenge message from server"

let storage = Storage()

let messageRequiringLoggedInApi ctx =
    { getMessageRequiringLoggedIn = fun () -> async { return storage.GetMessageRequiringLoggedIn() } }

let messageRequiringAuthChallengeApi ctx =
    { getMessageRequiringAuthChallenge = fun () -> async { return storage.GetMessageRequiringAuthChallenge() } }

let configureApp (app : IApplicationBuilder) =
    app.UseAuthentication()

let configureServices (services : IServiceCollection) =
    let config = services.BuildServiceProvider().GetService<IConfiguration>()

    services
        .AddMicrosoftIdentityWebAppAuthentication(config, openIdConnectScheme = "AzureAD")
        |> ignore

    services

let buildRemotingApi api next ctx = task {
    let handler =
        Remoting.createApi()
        |> Remoting.withRouteBuilder Route.builder
        |> Remoting.fromValue (api ctx)
        |> Remoting.buildHttpHandler
    return! handler next ctx }

let authScheme = "AzureAD"

let httpContextFuncFunc = requiresAuthentication (RequestErrors.UNAUTHORIZED authScheme "My Application" "You must be logged in.")

let requireLoggedIn : HttpFunc -> HttpContext -> HttpFuncResult =
    httpContextFuncFunc

let authChallenge : HttpFunc -> HttpContext -> HttpFuncResult =
    requiresAuthentication (Auth.challenge authScheme)

let routes =
    choose [
        authChallenge >=> buildRemotingApi messageRequiringAuthChallengeApi
        requireLoggedIn >=> buildRemotingApi messageRequiringLoggedInApi
    ]

let app =
    application {
        url "http://0.0.0.0:8085"
        service_config configureServices
        app_config configureApp
        use_router routes
        memory_cache
        use_static "public"
        use_gzip
    }

Application.run app
