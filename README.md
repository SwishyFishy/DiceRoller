# DiceRoller

This is a console application designed for use in TTRPG settings. The app allows the creation of dice with any number of any-valued faces, plus a static modifier to the dice roll. Furthermore, dice can be combined into dice collections which can themselves be assigned a static modifier. Thus, any combination of dice, with any passive modifiers can be stored and executed as a single `roll` command.

I created the app after playing a Bladesinger Wizard in a 5e D&D game. I'll use that character as an example. 

Suppose the character is 5th level with a magic Flametongue Rapier and 20 DEX, casting Booming Blade. The damage dice required are 1d8 thunder, 2d6 fire, and 1d8 piercing, +5.

![diceroller dmeo](https://github.com/user-attachments/assets/968d9e11-bb1d-40aa-b7ef-32c146ff7ed7)

The saved dice bag `BB_Flametongue` contains the Booming Blade d8, and a `Flametongue` dice collection. That dice collection contains the 2d6 and 1d8 required by the Flametongue item, and has a +5 modifier. With the `BB_Flametongue` object selected, executing `roll` recursively rolls the contained dice and each contained dice collection, and adds any static modifiers.

Using the `help` command displays this syntax menu.

```
The loaded file is displayed on the command line. When you quit DiceRoller, your dice will be saved to that file.
The selected item is displayed on the command line in [square brackets]. Commands apply primarily to the selected item.
Separate each command, parameter, and sequence indicator with whitespace. Multiple sequential whitespace characters are redundant.
'|' is a sequence indicator. Commands separated by a sequence indicator will run in the order entered when the command line is submitted.
Parameters in this menu are constructed of two parts: the parameter in <carats>, and the type in (brackets).
The parameter type indicates the value type accepted. (strings) are sequences of characters. (ints) are integers.
Optional parameters are supplied in this menu in [square brackets]. These parameters may be omitted from commands.
Strict paramters are supplied in this menu inside 'single quotations'. These parameters take the value listed. If there are multiple possible values, they are separated by a /.
Repeat parameters are supplied in this menu proceeded by '...'. These parameters can be supplied in any amount no less than 1.

DICE MANAGEMENT
add        <(string)name/(int)value>                                               -> Depending on whether the selected item is a Collection or a Die, adds the item or face value.
create     <(string)name> <['+'/'-'(int)modifier]>                                 -> Creates a Collection with the given name and modifier (defaults to +0).
           <(string)name> <['+'/'-'(int)modifier]> <(int)faces>                    -> Creates a Die with the given name, modifier (defaults to +0), and number of faces valued 1, 2, ...
           <(string)name> <['+'/'-'(int)modifier]> <(int)value> <(int)value> ...   -> Creates a Die with the given name, modifier (defaults to +0), and faces with the given values.
delete                                                                             -> Deletes the selected item.
list                                                                               -> Depending on whether the selected item is a Collection, a Die, or null, lists all contents, face values, or all items.
modifier   <'+'/'-'(int)modifier>                                                  -> Sets the selected item's modifier to the given value.
remove     <(string)name/(int)value>                                               -> Depending on whether the selected item is a Collection or a Die, removes the item or face value.
rename     <(string)name>                                                          -> Sets the selected item's name to the given name.
roll       <['quick']>                                                             -> Rolls the selected item. If 'quick' parameter supplied, prints only the final result.
select     <(string)name>                                                          -> Sets the selected item to the item with the given name.


FILE MANAGEMENT
f_delete   <[(string)name]>                                                        -> Deletes the loaded file, or the file with the given name if supplied.
f_list                                                                             -> Lists all files.
f_load     <(string)name>                                                          -> Saves the current file if one is loaded, then loads the file with the given name.
f_rename   <(string)name>                                                          -> Sets the loaded file's name to the given name.
           <(string)oldname> <(string)newname>                                     -> Sets the name of the file with given oldname to newname.
f_save     <[(string)name]>                                                        -> Saves the loaded file, or saves to a file with the given name if supplied.


MISCELLANEOUS
exit       <['nosave']>                                                            -> Exit DiceRoller. Saves automatically to loaded file unless 'nosave' parameter supplied.
help                                                                               -> Display this menu.
```

