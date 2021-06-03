# Compiler/Processor for Scratch v0.2
This is the companion repository for [this](Add-Scratch-Link) Scratch project, which is a basic 'computer' built in Scratch. It's inspired by [Ben Eater's hardware projects](https://eater.net/8bit) and is essentially a way for assembly-language-like code to be executed in the context of a Scratch project. **TL;DR**: This is all basically a side project, with no practical benefits whatsoever, but here's the documentation for it anyway.

## Architecture
The processor v0.1 consists of the following components:
 - [**Processor**](#processor) 
 - [**Memory**](#memory)
 - [**Disk**](#disk)
 - [**Flags**](#flags)
 - [**Display**](#display)
 - [**Keyboard**](#keyboard)
 - [**Parser**](#parser)

Each has a specific function and manages a certain set of data.
### Processor
The core component. This is responsible for managing the registers (named, stored data variables) and executing [commands](#commands) by providing the correct [signals](#signals).

The available registers in v0.1 are as follows:
|Name|Description|Flag|
|---|---|---|
|**A**|General-purpose register.|`0001`|
|**B**|General-purpose register.|`0010`|
|**X**|Temporary cache register, designed for temporary use.|`1000`|
|**Prog**|Designed for the current program address position.|`0100`|
|**MemoryAddress** (**Addr**)|The current address in memory - shared with the [Memory](#memory) component via a global variable.|*N/A*|
|**MemoryValue** (**Val**)|The current value to/from memory - shared with the [Memory](#memory) component via a global variable.|*N/A*|

### Memory
Allocates, reads, and writes to and from the `Memory` list - a fixed-size list of data values identifiable by their key/index in the list. Performant and suited for program variables and data.

Each list item can store any Scratch value, but the reccomended values for the processor architecture are as follows:

 - A numeric or decimal value: `3`, `-7.75` etc.
 - A single character: `s`, `|`, etc.
 - `true` or `false` bool values.
 - The null object `" "` (this memory cell is considered 'empty').
 - Command names (such as `LoadA`) - these are *not* strings, and cannot be treated as strings.
 - Some enum values (such as [keyboard](#keyboard)'s `{Up}`, `{Down}`, etc.) which cannot be turned into or parsed as strings, but remain 

While strings *are* supported, dealing with strings in a non-character-based way can lead to issues and inability to search through the string (at least in version v0.1).

#### Memory Map
This is the planned memory map for v0.1 of the processor. `k` refers to 1,000 items, and the each 'item' contains a single Scratch value.

|Start Address|End Address|Description|
|---|---|---|
|`0000`|`7999`|Free for any program to use, general-purpose memory.|
|`8000`|`8009`|Display memory: `X1`,`Y1`,`X2`,`Y2`,`R`,`G`,`B`,`S`,`Scale`,`Cmd`. See [Display](#display) for more information.|
|`8010`|`8010`|Keyboard memory: `Key` and `IsPressed`. See [Keyboard](#keyboard) for more information.|
|`8011`|`9009`|Files streamed from disk can be buffered for up to 1k of space in this region.|
|`9500`|`9999`|The state of the system and the stack. Certain areas of memory are reserved for the program running with root access (the OS or bootloader).|

### Disk
This is not currently implemented, but will provide methods for streaming named pieces of data into memory for programs to use. It will have its own set of [control signals](#signals), and likely require interrupt handling in order to stream larger chunks of data at a slower speed to [memory](#memory).

### Parser
This sprite is called during initialization and is in charge of loading in the system [microcode](#microcode) as well as whatever programs are being initialized in memory. It has no control signals and does not participate in computations.

### Flags
**Flags** can be triggered by many factors, including the state of the [processor](#processor)'s registers or the completion of an asynchronous task. In this way, they serve as both the 'interrupts' and 'flags' of a physical processor. They will be able to be set and unset by various [control signals](#signals) as explained in the following section.

#### List of flags
|Id|Name|Description|
|---|---|---|
|1|`HLT`|If this flag is ever set, the processor will halt and no longer execute commands.|
|2|`Eq`|Indicates whether the value in the `A` and `B` registers are the same (equal).|

### Display
The display controls a vector-based pen graphics driver, which can draw shapes and lines on the screen. It uses the memory range from `$8000` to `$8009` to store the following information:

 - `X1` and `Y1`: The first point storage. Point co-ordinates can be negative.
 - `X2` and `Y2`: The second point storage. Point co-ordinates can be negative.
 - `R`,`G`,`B`: The color of the pen for this command. Values can range from 0 to 255.
 - `S`: The size of the pen stroke.
 - `Scale`: The current scale factor for pen/screen co-ordinates.
 - `Cmd`: The ID of the command that is being sent to the display.

These values can be loaded into memory, and then will be loaded when a command calls the control signal `DispRef`, which tells the display to execute the command in memory.

### Keyboard
The keyboard allows requests to be made to check if a user has a certain key pressed. It uses the memory locations `$8010` and `$8011` to function:

 - `$8010` may contain a `char` or one of the keyboard enum values - `{Up}`, `{Down}`,`{Left}`, or `{Right}` - for the arrow keys.
 - `$8011` will contain a `bool` value indicating whether the key in `$8010` is currently pressed.
  
 Updating the keyboard `$8011` memory value is done using the `KeyRef` control signal. 

## Signals
Signals are individual broadcasts sent to a specific part of the processor to tell it to perform a given action. They store no state and do not involve multiple components of the project. Each one is called by a broadcast of type `call_{SignalName}`, where the `SignalName` is the specific name for that action. The beginning `call_` is ommitted from the following documentation, as it is added by the command processor at runtime.
|Signal Name|Component|Action|
|---|---|---|
|`MemGet`|Memory|Sets `MemoryValue` to the value stored in memory address `MemoryAddress`.|
|`MemSet`|Memory|Sets the value stored in memory address `MemoryAddress` to `MemoryValue`.|
|`Rf*`|Processor|Sets the 'from' register to `*` (`A`,`B`,`Prog`, etc.).|
|`Rt*`|Processor|Sets the 'to' register to `*` (`A`,`B`,`Prog`, etc.).|
|`TReg`|Processor|Transfers data from the 'from' register into the 'to' register (`Rf*`>`Rt*`).|
|`IncReg`|Processor|Increments the value of the 'from' register (`Rf*`).|
|`DecReg`|Processor|Decrements the value of the 'from' register (`Rf*`).|
|`Sum`|Processor|Adds the values of the `A` and `B` registers together, and saves the result into the `A` register.|
|`Sub`|Processor|Subtracts the values of the `A` and `B` registers from each other, and saves the result into the `A` register.|
|`Prod`|Processor|Multiples the values of the `A` and `B` registers together, and saves the result into the `A` register.|
|`Div`|Processor|Divides the values of the `A` and `B` registers from each other, and saves the result into the `A` register.|
|`Cmd`|Processor|Sets the value in `MemoryValue` as the current command in the processor. This call is used in the [FETCH cycle](#fetch-cycle), or any other command that determines the next command that should be run.|
|`HLT`|Processor|Halts the execution of code and stops all processing.|
|`DispCmd`|Display|Tells the [display](#display) module to execute the command in memory.|
|`KeyRef`|Keyboard|Tells the [keyboard](#keyboard) module to refresh whether the given key is pressed.|

## Microcode
Compiled microcode is written in JSON, where each **command** is made up of a name (`"Name"`), documentation snippet (`"Help"`), and a list of [signals](#signals)/broadcasts that make up the command (`"Signals"`). A list of commands included by default on the system, along with their documentation, is included [below](#list-of-commands). Defining commands allows you to write programs to memory (see [Writing Programs](#writing-programs)) rather than having to hard-code some collection of control signals, and thus the microcode on the system essentially defines the machine-code-esque language that you'll use to write more complex programs. 

Running microcode involves the `Prog` register, which keeps track of the current memory address executing code. As each command is run, the `Prog` counter is (usually) incremented by 1 to move to the next command.

### Syntax
The C# console application `ScratchCompiler` provides a way to 'compile' these `.mcs` microcode files into Scratch-readable JSON. The syntax is as follows:

**MyCommand.mcs**
````
CommandName: Affected,Registers: Description
Signal1
Signal2
// Comments are omitted at compile-time
Signal3
...
````
The result after compilation would look like this:
**MyCommand.mco**
```JSON
[{"Name":"MyNewCommand","Signals":"Signal1|Signal2|Signal3"}]
```
Overall, this isn't a huge change, but it does allow for comments in the source code and readable line breaks (which Scratch wouldn't understand).

Additionally, the compiler supports the following shorthand for register transfers: `A>B`, `A?B`, and `A+`. These are the equivalent of writing `RfA|RtB|TReg`, `RfA|RtB|TEqReg`, and `RfA|IncReg`. They save time when transferring data between registers - however, if you're transferring data to or from the same register multiple times, there *is* a small performance overhead.

Compilation also generates a `.mcd` file, which contains all of the documentation pieces that are omitted from the final result. This is used by the program compiler in the `ScratchCompiler` project to provide certain features (such as automatic input-mode switching) and run syntax checks on the resulting code.

**MyCommand.mcd**
```JSON
[
    {
    "CommandName": "MyNewCommand",
    "InputMode": null,
    "Description": "Does some stuff I made up!",
    "InvolvedRegisters": 2
    }
]
```
(The `InvolvedRegisters` property is an enum with flags - see [Processor](#processor) for a list of registers and corresponding bits). 

### Fetch Cycle
This command is run by the Scratch processor every time a new command is requested. The processor then runs whatever command that has been set using the `Cmd` call. Any custom microcode that is added to the system *must* include a definition for the `FETCH` command. 

**Fetch.mcs**
````
Fetch: Retrieves the next command info from memory.
Prog>Addr
MemGet
Cmd
IncProg
// At this point, we have set the command we want to execute. The Prog register has already been incremented, so arguments can simply call Prog>Addr and then IncProg after retrieving input. 
````

### List of Commands
Each one of these commands has been defined in the system microcode, whose source can be found in `/Source/Microcode.mcs` of this repo. It has been compiled and then included in the Scratch project without you needing to add it yourself. Arguments (denoted `arg*`) are adjancent memory spaces used in a program as inputs to some commands. This is explained more fully in the [Writing Programs](#writing-programs) section.

> Some **commands** will have the same, or similar, names and functionality to specific **control signals**. This allows for a program written in memory to trigger the functionality of that signal through a dedicated command.

|Command|Supports Input Modes|Description|
|---|---|---|
|`Fetch`||See [Fetch Cycle](#fetch-cycle) documentation.|
|`Jump`|`$`|Sets the next line of code to execute (the `Prog` register value) to the `arg` address in memory.|
|`JumpEq`|`$`|Sets the next line of code to execute (the `Prog` register value) to the `arg` address in memory if the `Eq` flag is set.|
|`JumpNotEq`|`$`|Sets the next line of code to execute (the `Prog` register value) to the `arg` address in memory if the `Eq` flag is not set.|
|`Halt`||Halts program execution.|
|`LoadA`|`$`,`#`|Loads `arg` into the `A` register.|
|`StoreA`|`$`|Stores the `A` register into `arg` in memory.|
|`IncA`||Increments the `A` register by 1.|
|`DecA`||Decrements the `A` register by 1.|
|`SumA`|`$`,`#`|Adds `arg` to the value of the `A` register (sets register `B`'s value).|
|`LoadB`|`$`,`#`|Loads `arg` into the `B` register.|
|`StoreB`|`$`|Stores the `B` register into `arg` in memory.|
|`IncB`||Increments the `B` register by 1.|
|`DecB`||Decrements the `B` register by 1.|
|`SumB`|`$`,`#`|Adds `arg` to the value of the `B` register (sets register `A`'s value).|
|`SendDisplay`||Sends the command in memory addresses `$8000-8009` to the display.|
|`RefreshKeys`||Refreshes the keyboard's currently pressed key (address `$8010`).|

Note that in many cases, just like the control signals, these commands are named in predictable ways - making compilation much easier. If a command supports input modes, this means that commands with suffixes exist - one with a suffix `$`, and/or one with a suffix `#`. These correspond to whether the inputs to that command should be treated as a memory address (`$`) or an immediate value (`#`). In the `.mcs` code, these commands are defined separately (at least for now).

## Writing Programs
Writing programs for this system is the process of defining commands and parameters in [memory](#memory) that will execute some desired action when run. Memory locations that have a command name (e.g. `LoadA`) in them will make that command execute when the `FETCH` cycle reads that memory location. Around and between these command names can be arguments. Many commands will pull a value or memory address from the next location in memory (using the `Prog` register) and use that as an input to the command. In the documentation for those commands, these are denoted as `arg*`, where the `*` indicates the offset of the input memory location from the command name memory location.

### Aren't Control Signals 'Programs' too?
The control signals might be written out in `.mcs` code, but they're the equivalent of the EEPROM or combinational logic in hardware - they store no state, and aren't editable after the system starts. Sure, you *could* use them to write code, but you could also use Scratch itself to write code - the point is that the design is for them to be static definitions of machine code functions. Thus commands are the *real* programming language for this Turing-complete 'processor'.

### How do I write a program to memory?
Writing programs directly to the memory list is possible, but it's tricky and very time-consuming. That's why the `ScratchCompiler` project and CLI interface provides a compiler for the `.ccs` code files, which will generate a memory map from more human-readable 'assembly language.'

### Compiling .ccs
Again, the `ScratchCompiler` will compile .ccs files into a JSON blob that the processor can use to directly set memory values. The syntax below explains some of the ways in which writing programs are made easier:

#### **`#` and `$` Inputs**
`#` and `$` can be used as prefixes to input values, and the compiler will select for you the correct command. The `$` indicates the value is a location in memory, and thus a memory command (such as `LoadA`) should be programmed. Using the `#` prefix indicates that the value is a literal, and will call the immediate-mode command (such as `SetA`) instead.
````
LoadA $14
LoadA #14
````
The first call would have a command name `LoadA$` and the `LoadA` command would expect the address `14` in memory to correspond to memory location. The second call would have a command name of `LoadA#` and would simply load the value `14` (as decimal) into the `A` register.

> Using the `$` or `#` inputs to help you out when writing code - even if the command you're looking at *doesn't support multiple input modes* - is supported by the `ScratchCompiler` project.

Note also that the following syntax applies for values of certain types:
 - **Characters** (`char` values) should be enclosed inside of single quotes (`'`).
 - **Enum** values should be enclosed in braces (`{ }`).
Both `char` and `enum` types are values the same way the `#NUM` construct is.

#### **Input Syntax**
Note that inputs can be written directly after the command being called, for example `LoadA $12` will put the `LoadA` command in the first memory address, and then `12` in the following address. Easy as pie!

#### **`.` Directives**
Using a `.` allows you to define named spots in memory. As your program changes in size, these values will be updated accordingly.

**Example:**
````
Define .MyLoop
...
(Run Some Code)
...
Jump .MyLoop
````
In this example, the loop will function as intended regardless of whether the code (and memory addresses) of the commands change as you work. In addition, the `.` prefix of the input to the `Jump` command will make sure that address-mode commands are used (this is identical to the `$` input-mode, as the `.` directives will be replaced by `$` addresses at compile-time).

These are the compiler directives at this time:
 - `Define` - names the location in memory, does not move forward address for next line of code.
 - `Var` - names the location in memory, sets it to empty, and moves the memory address forward.
 - `Position` - sets the current memory address in the `.ccs` code to the defined location.
 - `Constant` - writes the immediate value at the current location in memory and moves the memory address forward.

> Using the `Var` compiler command instead of the `Define` command will allocate that space in memory as empty, allowing you to store and get values from that space. `Define` allows the first line of code that follows the declaration to be stored in the now-named section of memory, which is useful for creating loops, setting constants with the `Constant` command, etc.

## Running the ScratchCompiler CLI
The `ScratchCompiler` project contains a .NET 5 CLI project that supports help and parameter descriptions. The most basic command, which would be to compile a folder of `.mcs` and `.ccs` files to some output directory, is as follows.

````
dotnet ./ScratchCompiler/bin/{Debug/Release}/net5.0/ScratchCompiler.dll --input-dir ./Source --output-dir ./Source/build
````
This will first compile all `.mcs` microcode files, and place the `*.mco` and `*.mcd` files in the `./Source/build` directory. Then, it will build any `.ccs` scripts using the definitions and documentation from the `*.mcd` files that are present in the input or output folder, and create a `*.cco` output file. The output should appear as follows for a folder with one `.mcs` and one `.ccs` file:

````
Found 1 .mcs source file(s).
Compiling Microcode.mcs...
Compilation complete!
Found 1 .ccs source file(s).
Found 1 .mcd documentation file(s).
Compiling Sample.ccs...
Compilation complete!
Compilation complete: 2 out of 2 succeeded.
````
The system's `.mcs` file can be found in the `./Source` directory of this repository, for you to use to build your own `.ccs` scripts.
