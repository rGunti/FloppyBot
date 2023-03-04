# Conversion Commands

## `money`

Converts a given amount of money into another currency.
The international three letter currency codes ([ISO 4217][iso4217]) are to be provided.

_Alias: `currency`_

Syntax: `money <Input> <Currency> [in|to] <Target Currency>`

```
> User:      money 25 USD to EUR
< FloppyBot: 25 USD are about 23.49 EUR
```

[iso4217]: https://en.wikipedia.org/wiki/ISO_4217

## `unit`

This command can be used to convert one unit to another.

Syntax: `unit <Source> in <Target Unit>`

```
> User:      unit 1.5m in ft
< FloppyBot: 1.5 m are about 4.92 ft
```

_This command uses subcommands._

### `unit help`

Shows a help message showing how to use this command. It also lists all units that are supported.
