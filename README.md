# Compiler/Processor for Scratch v0.1
This is the companion repository for [this](Add-Scratch-Link) Scratch project, which is a basic 'computer' built in Scratch. It's inspired by [Ben Eater's hardware projects](https://eater.net/8bit) and is essentially a way for assembly-language-like code to be executed in the context of a Scratch project. **TL;DR**: This is all basically a side project, with no practical benefits whatsoever, but here's the documentation for it anyway.

## Architecture
The processor v0.1 consists of the following components:
 - [**Processor**](#processor) 
 - [**Memory**](#memory)
 - [**Disk**](#disk)
 - [**Flags**](#flags)
 - [**Compiler**](#compiler)

Each has a specific function and manages a certain set of data.
### Processor
The core component. This is responsible for managing the registers (named, stored data variables) and executing [commands](#commands) by providing the correct [signals](#signals).

The available registers in v0.1 are as follows:
|Name|Description|
|---|---|
|**A**|General-purpose register.|
|**B**|General-purpose register.|
|**X**|Temporary cache register, designed for temporary use.|
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
This is not currently implemented, but will provide methods for streaming named pieces of data into memory for programs to use. It will have its own set of [control signals](#signals), and likely require interrupt handling in order to stream larger chunks of data at a slower speed to [memory](#memory).

### Compiler
This sprite is called during initialization and is in charge of loading in the system [microcode](#microcode) as well as whatever programs are being initialized in memory. It has no control signals and does not participate in computations.

### Flags
**Flags** can be triggered by many factors, including the state of the [processor](#processor)'s registers or the completion of an asynchronous task. In this way, they serve as both the 'interrupts' and 'flags' of a physical processor. They will be able to be set and unset by various [control signals](#signals) as explained in the following section.

#### List of flags
|Id|Name|Description|
|---|---|---|
|1|`HLT`|If this flag is ever set, the processor will halt and no longer execute commands.|
|2|`Eq`|Indicates whether the value in the `A` and `B` registers are the same (equal).|

## Signals
Signals are individual broadcasts sent to a specific part of the processor to tell it to perform a given action. They store no state and do not involve multiple components of the project. Each one is called by a broadcast of type `call_{SignalName}`, where the `SignalName` is the specific name for that action. The beginning `call_` is ommitted from the following documentation, as it is added by the command processor at runtime.
|Signal Name|Component|Action|
|---|---|---|
|`MemGet`|Memory|Sets `MemoryValue` to the value stored in memory address `MemoryAddress`.|
|`MemSet`|Memory|Sets the value stored in memory address `MemoryAddress` to `MemoryValue`.|
|`RfA`|Processor|Sets the 'from' register.|
|`RfB`|Processor|Sets the 'from' register.|
|`RfProg`|Processor|Sets the 'from' register.|
|`RfVal`|Processor|Sets the 'from' register.|
|`RfAddr`|Processor|Sets the 'from' register.|
|`RtA`|Processor|Sets the 'to' register.|
|`RtB`|Processor|Sets the 'to' register.|
|`RtProg`|Processor|Sets the 'to' register.|
|`RtVal`|Processor|Sets the 'to' register.|
|`RtAddr`|Processor|Sets the 'to' register.|
|`TReg`|Processor|Transfers data from the 'from' register into the 'to' register (`Rf*`>`Rt*`).|
|`IncReg`|Processor|Increments the value of the 'from' register (`Rf*`).|
|`Sum`|Processor|Adds the values of the `A` and `B` registers together, and saves the result into the `A` register.|
|`Sub`|Processor|Subtracts the values of the `A` and `B` registers from each other, and saves the result into the `A` register.|
|`Prod`|Processor|Multiples the values of the `A` and `B` registers together, and saves the result into the `A` register.|
|`Div`|Processor|Divides the values of the `A` and `B` registers from each other, and saves the result into the `A` register.|
|`Cmd`|Processor|Sets the value in `MemoryValue` as the current command in the processor. This call is used in the [FETCH cycle](#fetch-cycle), or any other command that determines the next command that should be run.|
|`HLT`|Processor|Halts the execution of code and stops all processing.|

## Microcode
Compiled microcode is written in JSON, where each **command** is made up of a name (`"Name"`), documentation snippet (`"Help"`), and a list of [signals](#signals)/broadcasts that make up the command (`"Signals"`). A list of commands included by default on the system, along with their documentation, is included [below](#list-of-commands). Defining commands allows you to write programs to memory (see [Writing Programs](#writing-programs)) rather than having to hard-code some collection of control signals, and thus the microcode on the system essentially defines the machine-code-esque language that you'll use to write more complex programs. 

Running microcode involves the `Prog` register, which keeps track of the current memory address executing code. As each command is run, the `Prog` counter is (usually) incremented by 1 to move to the next command.

### Syntax
The C# console application `ScratchCompiler` provides a way to 'compile' these `.mcs` microcode files into Scratch-readable JSON. The syntax is as follows:

**MyCommand.mcs**
````
CommandName: Description
Signal1
Signal2
// Comments are omitted at compile-time
Signal3
...
````
The result after compilation would look like this:
```JSON
[{"Name":"CommandName","Help":"Description","Signals":"Signal1|Signal2|Signal3"}]
```
Overall, this isn't a huge change, but it does allow for comments in the source code and readable line breaks (which Scratch wouldn't understand).

Additionally, the compiler supports the following shorthand for register transfers: `A>B`, `A?B`, and `A+`. These are the equivalent of writing `RfA|RtB|TReg`, `RfA|RtB|TEqReg`, and `RfA|IncReg`. They save time when transferring data between registers - however, if you're transferring data to or from the same register multiple times, there *is* a small performance overhead.

### FETCH Cycle
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

|Command|Args|Description|
|---|---|---|
|`FETCH`||See [FETCH Cycle](#fetch-cycle) documentation.|
|`Jump`|address `arg1`|Sets the next line of code to execute (the `Prog` register value) to the `arg1` address in memory.|
|`LoadA`|address `arg1`|Loads the `arg1` address of memory into the `A` register.|
|`SetA`|value `arg1`|Sets the value of the `A` register to `arg1`.|
|`AddA`|address `arg1`|Adds the `arg1` address of memory into the `A` register, overwriting the `B` register in the process.|

## Writing Programs
Writing programs for this system is the process of defining commands and parameters in [memory](#memory) that will execute some desired action when run. Memory locations that have a command name (e.g. `LoadA`) in them will make that command execute when the `FETCH` cycle reads that memory location. Around and between these command names can be arguments. Many commands will pull a value or memory address from the next location in memory (using the `Prog` register) and use that as an input to the command. In the documentation for those commands, these are denoted as `arg*`, where the `*` indicates the offset of the input memory location from the command name memory location.

### Aren't Control Signals 'Programs' too?
The control signals might be written out in `.mcs` code, but they're the equivalent of the EEPROM or combinational logic in hardware - they store no state, and aren't editable. Sure, you could use them to write code, but you could also use Scratch itself to write code - the point is that the design is for them to be static definitions of machine code functions. Thus commands are the *real* programming language, each command triggering the set of signals necessary for the computer to work, but the lists of commands and values in memory the 'programs' for this Turing-complete computer.

### How do I write a program to memory?
Writing programs directly to the memory list is possible, but it's tricky and very time-consuming. That's why the `ScratchCompiler` project and CLI interface provides a compiler for the `.ccs` code files, which will generate a memory map from more human-readable 'assembly language.'