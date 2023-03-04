# Quote Commands

Quotes are stored and numbered per channel. The quote with the number 42 on your channel is not the same as quote number 42 on someone elses channel.

## `quote`

_Alias: `q`_

Returns a random quote registered to the channel.

_This command supports subcommands._

### `quote <Quote No.>`

_Alias: `q <Quote No.>`_

Returns a specific quote

### `quote add`

_Alias: `quote +`, `quoteadd`, `q+`_

Adds a new quote to the channel.

Syntax: `quote add <Text>`

### `quote edit`

_Alias: `quote *`, `quoteedit`, `qe`, `q*`_

Edits an existing quote.

Syntax: `quote edit <Quote No.> <New Text>`

### `quote editcontext`

_Alias: `quote ec`, `quoteeditcontext`, `qec`, `q*c`_

Edits the context of an existing quote. (i.e. the game played when the quote was recorded)

Syntax: `quote editcontext <Quote No.> <New Context>`

### `quote delete`

_Alias: `quote del`, `quote -`, `quotedel`, `q-`_

Deletes an existing quote.

Syntax: `quote delete 25`

## `quoteinfo`

_Alias: `qi`_

Returns administrative information about this channels quote database and information about how to link it with one from another channel.

## `quotejoin`

_Alias: `qj`_

Starts the join process for the given channel. On Twitch, your channel ID is for example: `Twitch/<Account Name>`

Syntax: `quotejoin <Channel ID>`

## `quoteconfirm`

_Alias: `qc`_

Confirms the connection between this channel and another one, linking their quote databases together. This is to be executed after "quotejoin".

Syntax: `quoteconfirm <Channel ID> <Join Code>`
