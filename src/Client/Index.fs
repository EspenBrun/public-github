module Index

open Elmish
open Fable.Remoting.Client
open Shared
open Fable.React
open Fable.React.Props
open Fulma

type Model =
    { LoggedInMessageText: LoggedInMessage
      AuthChallengeText: AuthChallengeMessage }

type Msg =
    | Login
    | GotLoggedInMessage of LoggedInMessage
    | GotAuthChallengeMessage of AuthChallengeMessage

let requiringLoggedInApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IMessageRequiringLoggedInApi>

let requireAuthChallengeApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IMessageRequiringAuthChallengeApi>

let init () =
    let model = {
        LoggedInMessageText = LoggedInMessage "Init require logged in, should be text from server"
        AuthChallengeText = AuthChallengeMessage "Init auth challenge text, should be text from server"
    }
    let cmd = Cmd.OfAsync.perform requireAuthChallengeApi.getMessageRequiringAuthChallenge () GotAuthChallengeMessage
    model, cmd

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with
    | Login ->
        let cmd = Cmd.OfAsync.perform requiringLoggedInApi.getMessageRequiringLoggedIn () GotLoggedInMessage
        model, cmd
    | GotLoggedInMessage m -> { model with LoggedInMessageText = m }, Cmd.none
    | GotAuthChallengeMessage m -> { model with AuthChallengeText = m }, Cmd.none

let navbar dispatch=
    Navbar.navbar [ ]
        [ Navbar.Brand.div [ ]
            [ Navbar.Item.a [  ]
                [ img [ Src "/favicon.png" ] ] ]
          Navbar.End.div [ ]
            [ Navbar.Item.div [ ]
                [ Button.button [ Button.OnClick (fun _ -> dispatch Login); Button.Color IsSuccess ]
                    [ str "Login" ] ] ] ]


let view (model : Model) (dispatch : Msg -> unit) =
    Hero.hero [
        Hero.Color IsLight
        Hero.IsFullHeight
        ] [
        Hero.head [ ] [
            navbar dispatch
        ]

        Hero.body [ ] [
            Container.container [ ] [
                Column.column [
                    Column.Width (Screen.All, Column.Is6)
                    Column.Offset (Screen.All, Column.Is3)
                ] [
                    Heading.p [ Heading.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ] [ str (model.AuthChallengeText.ToString ()) ]
                    Heading.p [ Heading.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ] [ str (model.LoggedInMessageText.ToString ()) ]
                ]
            ]
        ]
    ]
