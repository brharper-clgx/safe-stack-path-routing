module Client.Index

(*******************************************
*               TYPES
*******************************************)
open Client.Pages
open Client.Urls

type Page =
    | About
    | Blog of Blog.State
    | BlogEntry of BlogEntry.State
    | Todo of Todo.State
    | NotFound

type State = { CurrentPage: Page }

type Msg =
    | Blog of Blog.Msg
    | BlogEntry of BlogEntry.Msg
    | Todo of Todo.Msg


(*******************************************
*               INIT
*******************************************)
open Elmish

let initFromUrl url =
    match url with
    | Url.About -> { CurrentPage = About }, Cmd.none
    | Url.Blog ->
        let s, c = Blog.init ()
        { CurrentPage = Page.Blog s }, Cmd.map Msg.Blog c
    | Url.BlogEntry slug ->
        let s, c = BlogEntry.init slug
        { CurrentPage = Page.BlogEntry s }, Cmd.map Msg.BlogEntry c
    | Url.Todo ->
        let s, c = Todo.init ()
        { CurrentPage = Page.Todo s }, Cmd.map Msg.Todo c
    | Url.NotFound -> { CurrentPage = NotFound }, Cmd.none

let init (url: Option<Url>): State * Cmd<Msg> =
    match url with
    | Some url -> initFromUrl url
    | _ ->
        let s, c = Blog.init ()
        { CurrentPage = Page.Blog s }, Cmd.map Msg.Blog c

(*******************************************
*               UPDATE
*******************************************)
let update (msg: Msg) (state: State): State * Cmd<Msg> =
    match msg, state.CurrentPage with
    | Msg.Blog (Blog.Msg.EntryClicked slug), _ ->
        let s, c = BlogEntry.init slug
        { CurrentPage = Page.BlogEntry s }, Cmd.map Msg.BlogEntry c
    | Msg.Blog msg', Page.Blog state' ->
        let s, c = Blog.update msg' state'
        { state with CurrentPage = Page.Blog s }, Cmd.map Msg.Blog c
    | Msg.BlogEntry msg', Page.BlogEntry state' ->
        let s, c = BlogEntry.update msg' state'

        { state with
              CurrentPage = Page.BlogEntry s },
        Cmd.map Msg.Todo c
    | Msg.Todo msg', Page.Todo state' ->
        let s, c = Todo.update msg' state'
        { state with CurrentPage = Page.Todo s }, Cmd.map Msg.Todo c
    | _ -> state, Cmd.none


(*******************************************
*               RENDER
*******************************************)
open Feliz
open Client.Components

let getActivePage dispatch currentPage =
    match currentPage with
    | About -> About.render
    | Page.Blog state' -> Blog.render state' (Msg.Blog >> dispatch)
    | Page.BlogEntry state' -> BlogEntry.render state' (Msg.BlogEntry >> dispatch)
    | Page.Todo state' -> Todo.render state' (Msg.Todo >> dispatch)
    | Page.NotFound -> NotFound.render

let render (state: State) (dispatch: Msg -> unit) =
    [
      Navbar.render
      getActivePage dispatch state.CurrentPage
    ]
    |> Html.div
