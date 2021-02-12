namespace Shared

module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

 type LoggedInMessage = LoggedInMessage of string
 type AuthChallengeMessage = AuthChallengeMessage of string

 type IMessageRequiringLoggedInApi = {
     getMessageRequiringLoggedIn: unit -> Async<LoggedInMessage>
 }
 type IMessageRequiringAuthChallengeApi = {
     getMessageRequiringAuthChallenge: unit -> Async<AuthChallengeMessage>
 }

