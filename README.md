# Processor v0.1
## Architecture
The processor v0.1 consists of three main components - **Processor**, **Memory**, and **Disk**. Each has a specific function and manages a certain set of data.
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
|=========== Program ======|File|Display/|======= System ======|  |  Memory  >
|===========  Space  ======| IO |Keyboard|=======  Data  ======|  |    ??    >
+--------------------------+----+--------+---------------------+  +---------->
````
 - **8k program space**: free for any program to use, no persistent data should be stored here.
 - **2k file I/O**: Files streamed from disk can be buffered for up to 2k of space in this region.
 - **4k display/keyboard**: The current keyboard/peripheral IO memory is stored here, as well as the memory used by the display adapter to send queries to the output display terminal or (possibly?) graphics system.
 - **10k system data**: OS/standard libraries are cached here, as well as the current state of the system and the stack. Certain areas of memory are reserved for the program running with root access (the OS or bootloader).

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

## Commands
Commands are provided with the syntax: `List|Of|Many|Calls`, where the [calls](#calls) are broadcasts that are sent in order of execution. A list of commands, a brief description, and their enclosing calls are included below.

Running commands consists of the `Prog` register, which keeps track of the current memory address of the command pointer. As each command is run, the `Prog` counter is (usually) incremented to move to the next command.

### FETCH Cycle
These calls are stored in an environment constant and run by the Scratch processor every time a new command is requested. The processor then runs whatever command that has been set using the `Cmd` call. 
````C#
// Called from Scratch when a new command can be run...
Prog>Addr
MemGet
Cmd
IncProg
// At this point, we have set the command we want to execute. The Prog register has already been incremented, so arguments can simply call Prog>Addr and then IncProg after retrieving input. 
// Run some calls dependent on the command name...
````

In the chart, `arg` lists will also be provided. These would be specified as the next value(s) in memory after the command.  

### `LoadA`
Loads the `arg1` address of memory into the `A` register.
````C#
Prog>Addr
MemGet
// Address loaded into Val register, and the A register is going to be overwritten in this command.
Val>A
A>Addr
MemGet
// Now set the value into the A.
Val>A
````

### `SetA`
Sets the value of the `A` register to `arg1`.
````C#
Prog>Addr
MemGet
// Value loaded into Val register.
Val>A
````

### `AddA`
Adds the `arg1` address of memory into the `A` register, overwriting the `B` register in the process.
````C#
Prog>Addr
MemGet
// Address loaded into Val register
Val>B
Sum
````