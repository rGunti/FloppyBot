# Tools

## `define`

_Alias: `urbandictionary`_

Looks up a definition for the given word in [Urban Dictionary](https://www.urbandictionary.com/).

Syntax: `define <Term>`

```
> User:      define hello world
< FloppyBot: hello world: The easiest, and first program any [newbie] would write.
             Applies for any language. Also what you would see in the first [chapter]
             of most [programming] books.
             (Source: http://hello-world.urbanup.com/2052280)
```

## `timer`

Starts a new timer that will send a chat message after it has elapsed.

Syntax: `timer <Time Expression> <Message>`

The time expression follows this format: `[_d][_h][_m][_s]`
All sections are optional (though at least one has to be present) and units have to be declared.

```
> User:      timer 15m20s Hello, very specific timer
< FloppyBot: Created new timer. Your message should be there in about 15 minutes.
# Note: The command above will create a timer that runs for 15 minutes and 20 seconds.
```

## `translate`

Translates a given text from one langauage to another using [DeepL Translator](https://www.deepl.com/).

Syntax: `translate <Input Text> [from <Input Language>] to <Output Language>`

?> **Note**: If the input language is ommitted, DeepL will guess what the input language is.
This might lead to unexpected results.

```
> User:      translate Hello World from English to Swedish
< FloppyBot: Hej, världen
             (translated from English)
```
