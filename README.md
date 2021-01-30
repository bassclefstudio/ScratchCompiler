# Compiler/Processor for Scratch v0.1
This is the companion repository for [this](Add-Scratch-Link) Scratch project, which is a basic 'computer' built in Scratch. It's inspired by [Ben Eater's hardware projects](https://eater.net/8bit) and is essentially a way for assembly-language-like code to be executed in the context of a Scratch project. **TL;DR**: This is all basically a side project, with no practical benefits whatsoever, but I'll be using it to learn a little more about designing my own low-level programs.

## Architecture
The processor v0.1 consists of three main sprites - **Processor**, **Memory**, and **Disk**. Each has a specific function and manages a certain set of data.
### Processor
The core component. This is responsible for managing the registers (named, stored data variables) and executing [commands](#commands) by providing the correct [calls](#calls).

The available registers in v0.1 are as follows:
|Name|Description|
|---|---|
|**A**|General-purpose register.|
|**B**|General-purpose register.|
|**Prog**|Designed for the current program address position.|
|**MemoryAddress** (**Addr**)|The current address in memory - shared with the [Memory](#memory) component via a global variable.|
|**MemoryValue** (**Val**)|The current value to/from memory - shared with the [Memory](#memory) component via a global variable.|

### Memory
Allocates, reads, and writes to and from the `Memory` list - a fixed-size list of data values identifiable by their key/index in the list. Performant and suited for program variables and data.

Each list item can store any Scratch value, but the reccomended values for the processor architecture are as follows:

 - A numeric or decimal value: `3`, `-7.75` etc.
 - A single character: `s`, `|`, etc.
 - `true` or `false` bool values.
 - The null object `" "` (this memory cell is considered 'empty').
 - Command names (such as `LoadA`) - these are *not* strings, and cannot be treated as strings.

While strings *are* supported, dealing with strings in a non-character-based way can lead to issues and inability to search through the string (at least in version v0.1).

#### Memory Map
This is the planned memory map for v0.1 of the processor, in ASCII art form. `k` refers to 1,024 items, and the units for memory are the cells which contain Scratch values.

````
+--------------------------+----+--------+---------------------+  +---------->
|===========   8k    ======| 2k |   4k   |=======  10k   ======|  |   Disk   >
|=========== Program ======|File|Display/|======= System ======|->|  Memory  >
|===========  Space  ======| IO |Keyboard|=======  Data  ======|  |    ??    >
+--------------------------+----+--------+---------------------+  +---------->
````
 - **8k program space**: free for any program to use, no persistent data should be stored here.
 - **2k file I/O**: Files streamed from disk can be buffered for up to 2k of space in this region.
 - **4k display/keyboard**: The current keyboard/peripheral IO memory is stored here, as well as the memory used by the display adapter to send queries to the output display terminal or (possibly?) graphics system.
 - **10k system data**: OS/standard libraries are cached here, as well as the current state of the system and the stack. Certain areas of memory are reserved for the program running with root access (the OS or bootloader).

### Disk
This is not currently implemented, but will provide methods for streaming named pieces of data into memory for programs to use. It will have its own set of [system calls](#calls), and likely require interrupt handling in order to stream larger chunks of data at a slower speed to [memory](#memory).

## Calls
Calls are individual broadcasts sent to a specific part of the processor to tell it to perform a given action. They store no state and do not involve multiple components of the project. Each one is called by a broadcast of type `call_{CallName}`, where the `CallName` is the specific name for that action. The beginning `call_` is ommitted from the following documentation, as it is added by the command processor at runtime.
|Call Name|Component|Action|
|---|---|---|
|`MemGet`|Memory|Sets `MemoryValue` to the value stored in memory address `MemoryAddress`.|
|`MemSet`|Memory|Sets the value stored in memory address `MemoryAddress` to `MemoryValue`.|
|`A>B`|Processor|Transfers data from the first register into the second register of the [processor](#processor).|
|`B>A`|Processor|Transfers data from the first register into the second register of the [processor](#processor).|
|`A>Addr`|Processor|Transfers data from the first register into the second register of the [processor](#processor).|
|`Prog>Addr`|Processor|Transfers data from the first register into the second register of the [processor](#processor).|
|`A>Val`|Processor|Transfers data from the first register into the second register of the [processor](#processor).|
|`B>Val`|Processor|Transfers data from the first register into the second register of the [processor](#processor).|
|`Prog>Val`|Processor|Transfers data from the first register into the second register of the [processor](#processor).|
|`Addr>A`|Processor|Transfers data from the first register into the second register of the [processor](#processor).|
|`Addr>Prog`|Processor|Transfers data from the first register into the second register of the [processor](#processor).|
|`Val>A`|Processor|Transfers data from the first register into the second register of the [processor](#processor).|
|`Val>B`|Processor|Transfers data from the first register into the second register of the [processor](#processor).|
|`Val>Prog`|Processor|Transfers data from the first register into the second register of the [processor](#processor).|
|`IncA`|Processor|Increments the value of the `A` register by 1.|
|`IncProg`|Processor|Increments the value of the `Prog` register by 1.|
|`Sum`|Processor|Adds the values of the `A` and `B` registers together, and saves the result into the `A` register.|
|`Sub`|Processor|Subtracts the values of the `A` and `B` registers from each other, and saves the result into the `A` register.|
|`Prod`|Processor|Multiples the values of the `A` and `B` registers together, and saves the result into the `A` register.|
|`Div`|Processor|Divides the values of the `A` and `B` registers from each other, and saves the result into the `A` register.|
|`Cmd`|Processor|Sets the value in `MemoryValue` as the current command in the processor. This call is used only in the [FETCH cycle](#fetch-cycle), which determines the next command that should be run.|

## Microcode
Compiled microcode is written in the syntax: `CommandName:Desc:List|Of|Many|Calls:`, where each **command** is made up of a name, documentation snippet, and a list of [calls](#calls)/broadcasts that make up the command. A list of commands included by default on the system, along with their documentation, is included [below](#list-of-commands).

Running microcode involves the `Prog` register, which keeps track of the current memory address executing code. As each command is run, the `Prog` counter is (usually) incremented by 1 to move to the next command.

### Syntax
The C# console application `ScratchCompiler` provides a way to 'compile' the `.mcs` microcode files into a Scratch-readable form. The syntax is as follows:

**MyCommand.mcs**
````C#
CommandName: Description
Call1
Call2
// Comments are not included at compile-time
Call3
...
````
The result after compilation would look like this: `CommandName:Description:Call1|Call2|Call3:`. Overall this isn't a huge change, but it does allow for comments in the source code and readable line breaks (which Scratch wouldn't understand).

### FETCH Cycle
This microcode is stored in an environment constant and run by the Scratch processor every time a new command is requested. The processor then runs whatever command that has been set using the `Cmd` call.

**FETCH.mcs**
````C#
// Called from Scratch when a new command can be run...
Prog>Addr
MemGet
Cmd
IncProg
// At this point, we have set the command we want to execute. The Prog register has already been incremented, so arguments can simply call Prog>Addr and then IncProg after retrieving input. 
// Run some calls dependent on the command name...
````

### List of Commands
Each one of these commands has been defined in the system microcode, whose source can be found in `/Source/Microcode.mcs` of this repo. It has been compiled and then included in the Scratch project without you needing to add it yourself. Arguments (denoted `arg*`) are adjancent memory spaces used in a program as inputs to some commands. This is explained more fully in the [Writing Programs](#writing-programs) section.

|Command|Args|Description|
|---|---|---|
|`LoadA`|address `arg1`|Loads the `arg1` address of memory into the `A` register.|
|`SetA`|value `arg1`|Sets the value of the `A` register to `arg1`.|
|`AddA`|address `arg1`|Adds the `arg1` address of memory into the `A` register, overwriting the `B` register in the process.|

## Writing Programs
