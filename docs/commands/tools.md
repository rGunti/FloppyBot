# Tools

## `define`

_Alias: `urbandictionary`_

Looks up a definition for the given word in [Urban Dictionary](https://www.urbandictionary.com/).

**Default Settings**: Restricted to "Viewers" and above, Cooldown 30s

Syntax: `define <Term>`

```
> User:      define hello world
< FloppyBot: hello world: The easiest, and first program any [newbie] would write.
             Applies for any language. Also what you would see in the first [chapter]
             of most [programming] books.
             (Source: http://hello-world.urbanup.com/2052280)
```

## `translate`

Translates a given text from one langauage to another using [DeepL Translator](https://www.deepl.com/).

**Default Settings**: Restricted to "Viewers" and above, Cooldown 25s

Syntax: `translate <Input Text> [from <Input Language>] to <Output Language>`

?> **Note**: If the input language is ommitted, DeepL will guess what the input language is.
This might lead to unexpected results.

```
> User:      translate Hello World from English to Swedish
< FloppyBot: Hej, världen
             (translated from English)
```
