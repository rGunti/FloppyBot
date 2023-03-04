# Debug Commands

## `debugpriv`

Returns the privilege level of the author.

?> Your privilege level is: Administrator

## `unitdebug`

Allows debugging of the unit conversion engine used by the `unit` command.

_This command uses subcommands._

### `unitdebug convert`

Determines how the unit engine will convert from the input to the output unit and prints it.

Syntax: `unitdebug convert <Input Unit> <Output Unit>`

```
> User:      unitdebug convert m ft
< FloppyBot: Can convert from Unit metre [m] to Unit foot [ft] using conversion Invert(Factor(0.3048))
```


### `unitdebug parse`

Tests the unit parsing and returns what FloppyBot understood the input unit and value to be.

Syntax: `unitdebug parse <Input Value & Unit>`

```
> User:      unitdebug parse 2.5m
< FloppyBot: Parsed to 2.5 m
             Detected unit was Unit metre [m] (Expr: ^(\d{1,}(\.?\d{1,})?)m$)
```

### `unitdebug unit`

Prints detailed information about a given unit.

Syntax: `unitdebug unit <Unit>`

```
> User:      unitdebug unit m
< FloppyBot: About Unit m
             Full Name: metre
             Symbol: m
             Parsing Expression: ^(\d{1,}(\.?\d{1,})?)m$
```

### `unitdebug units`

Prints a detailed list of all supported units.

Syntax: `unitdebug unit [<Page>]`

```
> User:      unitdebug units
< FloppyBot: The following units are known:
             - Unit Centilitre [cl] (Expr: ^(\d{1,}(\.?\d{1,})?)cl$)
             - Unit centimetre [cm] (Expr: ^(\d{1,}(\.?\d{1,})?)cm$)
             [...]
             Page 1 of 4
```
