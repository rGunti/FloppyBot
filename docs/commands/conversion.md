# Conversion Commands

## `dectime`

_Alias: `dt`_

Returns the current time in [Decimal time][dectime].
If specified, will get the current time in a given time zone.

> See the `time` command for further information.

Syntax: `dectime [<Time Zone>]`

[dectime]: https://en.wikipedia.org/wiki/Decimal_time

## `money`

_Alias: `currency`_

Converts a given amount of money into another currency.
The international three letter currency codes ([ISO 4217][iso4217]) are to be provided.

Syntax: `money <Input> <Currency> [in|to] <Target Currency>`

```
> User:      money 25 USD to EUR
< FloppyBot: 25 USD are about 23.49 EUR
```

[iso4217]: https://en.wikipedia.org/wiki/ISO_4217

## `time`

Returns the current time. If specified, will get the current time in a given time zone.

The time zone name used follows the [tz database][tz]. The schema is usually `<Continent>/<City>`,
the city denoted is usually a capital or other notable population center in the region.

Syntax: `time [<Time Zone>]`

```
> User:      time
< FloppyBot: The current time is 20:54 in Coordinated Universal Time

> User:      time Europe/Berlin
< FloppyBot: The current time is 21:54 in Central European Standard Time

> User:      time Asia/Tokyo
< FloppyBot: The current time is 05:54 in Japan Standard Time
```

[tz]: https://en.wikipedia.org/wiki/List_of_tz_database_time_zones

## `timeset`

Allows the user to store their own time zone. This can be used by `time` and `dectime` to
always feed the users time zone instead of a default one (which is usually UTC).

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
