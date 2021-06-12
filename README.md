# Compiler/Processor for Scratch v0.5
This is the companion repository for [this Scratch project](https://scratch.mit.edu/projects/539686245/), which is a basic 'computer' built in Scratch. It's inspired by [Ben Eater's hardware projects](https://eater.net/8bit) and is essentially a way for assembly-language-like code to be executed in the context of a Scratch project. **TL;DR**: This is all basically a side project, with no practical benefits whatsoever, but here's the documentation for it anyway.

## Architecture
The processor v0.5 consists of the following components:
 - [**Cores**](#cores) 
 - [**Memory**](#memory)
 - [**Disks**](#disks)
 - [**Display**](#display)
 - [**Keyboard**](#keyboard)
 - [**Bootloader**](#bootloader)

Each has a specific function and manages a certain set of data.
### Cores
These are all clones of a single sprite, which has the capability to run code and delegate memory space. They are responsible for executing [commands](#commands) by providing the correct [signals](#signals).

Each core has its own stack, which is a first-in first-out data collection. This means that when you `pull` a value onto the stack, `push`ing a value off of the stack at a later time will retreive that last value (and expose any previous values underneath). Thinking about it like a physical 'stack' from which you add and remove items will help you visualize this.

> **Note:** The number of Cores running on the processor is defined by the `RequestedCores` variable in the `Configuration` sprite. All Cores have their own memory, and can execute instructions in parallel. 

#### Main Core
The first core (core **0**) is special in that it is the only core that is started when the processor first boots. The [`Bootloader`](#bootloader) device memory is mounted to the core, and program execution starts at [memory address `$0000`](#memory-map).

### Memory
'Memory' is an abstract term for the multiple ways in which various data is stored in the project attached to a unique key - the `Address`. Memory stores are managed by multiple components, which use a `Request` system internally to access memory on different components (the details are not relevant to programming the processor):

#### Memory Locations
 - Each [`Core`](#cores) has its own memory, which it breaks up into contiguous blocks called 'chunks'. This memory can be copies of existing memory (clones) or unique memory which this `Core` will then serve to other components upon request.
 - Devices (such as the [`Display`](#display)) have their own special-purpose memory addresses which contain device-specific data. By querying these addresses, the processor can collect and send I/O information.

> Memory can be split into two types - **local** and **remote** memory. **Local** memory means a copy is available on the [`Core`](#cores) that is executing the current command, and **remote** memory means that a request has to be made to another device to get or set data.

#### Value Types
Memory locations can contain the following values:

 - A numeric or decimal value: `3`, `-7.75` etc.
 - A single character: `s`, `|`, etc.
 - `true` or `false` bool values.
 - The null object `" "` (this memory cell is considered 'empty').
 - Command names (such as `Pull#`) - these are *not* strings, and cannot be treated as strings.
 - Some enum values (such as [`Keyboard`](#keyboard)'s `{Up}`, `{Down}`, etc.) which cannot be turned into or parsed as strings but are stored in memory similarly to commands.

While strings are *technically* supported, dealing with strings in a non-character-based way can lead to issues and inability to search through the string (at least in version v0.5).

#### Memory Map
This is the planned memory map for v0.5 of the processor. `k` refers to 1,000 items, and the each 'item' contains a [single Scratch value](#Value-Types).

|Start Address|End Address|Description|
|---|---|---|
|`0000`|`0499`|[`Bootloader`](#bootloader) memory, containing the first instructions that will be executed by [Core **0**](#main-core).|
|`0500`|`0999`|Core **0** cache (core-specific memory).|
|`1000`|`1499`|Core **1** cache (core-specific memory).|
|`1500`|`1999`|Core **2** cache (core-specific memory).|
|`2000`|`2499`|Core **3** cache (core-specific memory).|
|`2500`|`7999`|*Unallocated* (free) memory, available for any device to manage. Program data and multi-core caches may be stored here.|
|`8000`|`8002`|[`Mouse`](#mouse) memory: `MouseX`,`MouseY`,`IsClicked`|
|`8003`|`8003`|[`Keyboard`](#keyboard) memory. **get:** `IsPressed`, **set:** `Key` |
|`8004`|`8009`|[`Display`](#display) memory: `Cmd`, `X`,`Y`,`Pen`,`Size`,`Id`.|
|`8010`|`9999`|Files streamed from [disks](#disks), additional devices and peripherals, and system information are stored here on demand.|

### Disks
This is not currently implemented, but will provide methods for streaming named pieces of data into memory for programs to use. While not in general usage yet, the [`Bootloader`](#bootloader) component is an example of how disks can be used with the processor as of version `v0.5`.

### Bootloader
This sprite is called during initialization and is in charge of loading in the system [microcode](#microcode) as well as whatever programs are being initialized in the first chunk of memory ([`$0000-$0499`](#memory-map)). It then acts as a [`Disk`](#disks) that the main [`Core`](#main-core) will mount and read before executing the instructions contained on it.

> To change the contents of the bootloader, edit the `Microcode` and `BootVolume` variables in the `Configuration` sprite.

### Display
The display controls a vector-based pen graphics driver, which can draw shapes and lines on the screen. It uses the memory range from `$8004` to `$8009` to store the following information:

 - `Cmd`: The ID of the command that is being sent to the display.
 - `X` and `Y`: Encodes a single 2D point. Point co-ordinates can be negative.
 - `Pen`: The pen type, as an index between `0` and `8`. 
 - `Size`: The size of the pen stroke, in pixels.
 - `Id`: A unique ID used to refer to the command when editing/adding information.

These values can be loaded into memory, and then will be loaded when a command `Pokes` any memory address on the device, which tells the display to execute the command in memory.

#### Pen Types

|Index|Pen|Description|
|---|---|---|
|`0`|None|Nothing will be drawn *(only moves the pen)*.|
|`1`|White|`#FFFFFF`|
|`2`|Red|`#FF0000`|
|`3`|Green|`#00FF00`|
|`4`|Blue|`#0000FF`|
|`5`|Yellow|`#FFFF00`|
|`6`|Cyan|`#00FFFF`|
|`7`|Magenta|`#FF00FF`|
|`8`|Black *(Eraser)*|`#000000`|

#### Command Ids
|ID|Name|Description|
|---|---|---|
|`0`|Update/Add|Updates the move/draw command with the given `Id` with the provided values.|
|`1`|Update and Draw|Identical to `0`, but also updates the screen.|
|`2`|Remove|Removes the command with the given `Id` from the list of commands.|
|`3`|Clear|Completely clears the display.|

### Keyboard
The keyboard allows requests to be made to check if a user has a certain key pressed. It uses the memory location `$8003` to function:

 - `$8003` may be **set** to a `char` or one of the keyboard enum values - `{Up}`, `{Down}`,`{Left}`, `{Right}`, or `{Space}`.
 - If **get** is then called to the memory address, a `bool` is returned indicating whether the desired key is pressed.

## Signals
Signals are individual broadcasts sent to a specific part of the processor to tell it to perform a given action. They store no state and do not involve multiple components of the project. Executed in order, they form the basis of [`Microcode`](#microcode).

## Microcode
Compiled microcode is written in JSON, where each **command** is made up of a name (`"Name"`), documentation snippet (`"Help"`), and a list of [`signals`](#signals) that make up the command (`"Signals"`). Defining commands allows you to write programs to memory (see [Writing Programs](#writing-programs)) rather than having to hard-code some collection of control signals, and thus the microcode on the system essentially defines the machine-code-esque language that you'll use to write more complex programs. 

### Pointer
Running microcode involves the `pointer`, which keeps track of the current memory address executing code in each [`Core`](#cores). As each command is run, the `pointer` counter is (usually) incremented by 1 to move to the next command.

### Syntax
The C# console application `ScratchCompiler` provides a way to 'compile' these `.mcs` microcode files into Scratch-readable JSON. The syntax is as follows:

**MyCommand.mcs**
````
CommandName{$/#/?}: Description
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

### Microcode documentation
Compilation also generates a `.mcd` file, which contains all of the documentation pieces that are omitted from the final result. This is used by the program compiler in the `ScratchCompiler` project to provide certain features (such as automatic input-mode switching) and run syntax checks on the resulting code.

**MyCommand.mcd**
```JSON
[
    {
    "CommandName": "MyNewCommand",
    "InputMode": [],
    "Description": "Does some stuff I made up!",
    }
]
```

### Fetch Cycle
This command is run by the Scratch processor every time a new command is requested. The processor then runs whatever command that has been set using the `Cmd` call. Any custom microcode that is added to the system *must* include a definition for the `Fetch` command. 

**Fetch.mcs**
````
Fetch: Fetches the next command from memory and prepares the pointer for execution.
    GetPointer
    IncPointer
    Get
    Cmd
// At this point, we have set the command we want to execute. The pointer has already been incremented, so other commands can simply call GetPointer and then IncPointer when getting inputs.
````

### Default Commands
Each one of these commands has been defined in the system microcode, whose source can be found in `/Source/Microcode.mcs` of this repo. It has been compiled and then included in the Scratch project without you needing to add it yourself. Some **commands** will have the same, or similar, names and functionality to specific **control signals**. This allows for a program written in memory to trigger the functionality of that signal through a dedicated command.

> Documentation on the commands is included in the [documentation](#microcode-documentation) for the `/Source/Microcode.mcs` source code.

Note that in many cases, just like the control signals, these commands are named in predictable ways - making compilation much easier. If a command supports input modes, this means that the command name will be appended by one or more of `$`, `#`, or `?`. These correspond to whether the inputs to that command should be treated as a memory address (`$`), an immediate value (`#`), or an address reference (`?`). In the `.mcs` code, these commands are defined separately.

## Writing Programs
Writing programs for this system is the process of defining commands and parameters in [memory](#memory) that will execute some desired action when run. Memory locations that have a command name (e.g. `Pull`) in them will make that command execute when the [`Fetch` cycle](#fetch-cycle) reads that memory location. Around and between these command names can be arguments. Many commands will pull a value or memory address from the next location in memory (using the [`pointer`](#pointer)) and use that as an input to the command.

### Aren't Control Signals 'Programs' too?
The control signals might be written out in `.mcs` code, but they're the equivalent of the EEPROM or combinational logic in hardware - they store no state, and aren't editable after the system starts. Sure, you *could* use them to write code, but you could also use Scratch itself to write code - the point is that the design is for them to be static definitions of machine code functions. Thus commands are the *real* programming language for this Turing-complete 'processor'.

### How do I write a program to memory?
Writing programs directly to the memory list is possible, but it's tricky and very time-consuming. That's why the `ScratchCompiler` project and CLI interface provides a compiler for the `.ccs` code files, which will generate a memory map from more human-readable 'assembly language.'

### Compiling .ccs
Again, the `ScratchCompiler` will compile .ccs files into a JSON blob that the processor can use to directly set memory values. The syntax below explains some of the ways in which writing programs are made easier:

#### **`#` and `$` Inputs**
`#` and `$` can be used as prefixes to input values, and the compiler will select for you the correct command. The `$` indicates the value is a location in memory, and thus a memory command (such as `Pull`) should be programmed. Using the `#` prefix indicates that the value is a literal, and will call the immediate-mode command (such as `Pull#`) instead.
````
Pull $14
Pull #14
````
The first call would have a command name `Pull$` and the `Pull` command would expect the address `14` in memory to correspond to a memory location. The second call would have a command name of `Pull#` and would simply load the value `14` (as an [`int`](#value-types)) onto the stack.

> Using the `$` or `#` inputs to help you out when writing code - even if the command you're looking at *doesn't support multiple input modes* - is supported by the `ScratchCompiler` project.

Note also that the following syntax applies for values of certain types:
 - **Characters** (`char` values) should be enclosed inside of single quotes (`'`).
 - **Enum** values should be enclosed in braces (`{ }`).
Both `char` and `enum` types use immediate mode (`#`) execution.

#### Input Syntax
Note that inputs can be written directly after the command being called, for example `Pull $12` will put the `Pull$` command in the first memory address, and then `12` in the following address. Easy as pie!

#### `.` and `?` Directives
Using a `.` allows you to define named spots in memory. As your program changes in size, these values will be updated accordingly.

**Example:**
````
Define .MyLoop
...
(Run Some Code)
...
Goto .MyLoop
````
In this example, the loop will function as intended regardless of whether the code (and memory addresses) of the commands change as you work. In addition, the `.` prefix of the input to the `Goto` command will make sure that address-mode commands are used (this is identical to the `$` input-mode, as the `.` directives will be replaced by `$` addresses at compile-time)

If in the code there is a line such as:
````
Pull ?MyVar
````

The directive is resolved by the compiler as usual, but the type of the data is set to `?`, for a memory address reference.

These are the compiler directives at this time:
 - `Define` - names the location in memory, does not move forward address for next line of code.
 - `Var` - names the location in memory, sets it to empty, and moves the memory address forward.
 - `Constant` - writes the immediate value at the current location in memory and moves the memory address forward.

> Using the `Var` compiler command instead of the `Define` command will allocate that space in memory as empty, allowing you to store and get values from that space. `Define` allows the first line of code that follows the declaration to be stored in the now-named section of memory, which is useful for creating loops, setting constants with the `Constant` command, etc.

> Note that there is a special use of the `Define` command - with two inputs - that allows you to define a position in memory that you do not have access to. For example, `Define .Display $8004` will replace all of the `.Display` references in the compiled code with memory address `$8004`, even though the bootloader/cores don't have write access. 

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

> The output of the `ScratchCompiler` CLI can be copied directly into the `Microcode` and `BootVolume` variables in the `Configuration` sprite of the processor.
