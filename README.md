# FlowGraph

Flow Graph is a tool for authoring sequences of events that take place over time.
It's not intended to be a visual coding tool or to replace writing code in C#. You will need to be able to write and read code to make use of this tool.

You can use built-in events or write your own, both of which can execute instantly or over time.

----------------------------------------------------------------------------------------------------------

As a preface, here are some concepts that will help you understand and use FlowGraph:

UniqueObjectDatas - Rather than references gameObjects in your scene, FlowGraph will lookup items through references to UniqueObjectDatas. 
Your prefabs and scene objects can use the UniqueObject component to allow FlowGraph to find them via this lookup.

Items / PropertyItem / ItemData - FlowGraph is built around thinking of entities in your game as Items each with a set of PropertyItems to compose how that item behaves.
We defined a sandwich as an Item with an EdibleItem, PickupableItem, and CookableItem. ItemDatas are assets referenced by Item and FlowGraph can use to compare against.
Items and PropertyItems have built in logic for saving and loading fields marked with the [SaveLoad] attribute. Items with the PersistentItem component will automatically
load their state and the state of any items attached to them. PropertyItems can implement the OnLoaded method to respond when save data is deserialized onto them.

Goals - Goals are assets that define moments of progression through your game. At runtime you can through code or FlowGraph mark goals complete on the GoalManager
and a callback will fire so you can respond to progression events.

----------------------------------------------------------------------------------------------------------

OK into the meat. 

FlowGraph - Each FlowGraph asset defines a tree of nodes which will execute starting at a definded start node (right click to change this).

FlowNode - Each subwindow inside a FlowGraph is called a FlowNode. These can be linked to from FlowEffects and from FlowNodeReference fields in code.

FlowEffect - Each FlowNode contains a list of FlowEffects. An effect is similar to a C# function but it's execution can happen instantly or over time. Each of these
commands is authored in C# either as part of this package or by a programmer on your team. FlowEffects are populated automatically via reflection based on methods
defined in scripts that inherit from FlowModule.

FlowEffect Timing - FlowEffect timing is controlled by the setting to the left of each row:
'After' means the effect is timed after the start of the containing FlowNode. 
'After Prev' means the effect is timed after the previous FlowEffect.
'With Prev' means the effect is timed with the previous FlowEffect.

FlowModule - All the commands exposed to FlowGraph come from scripts that derive from FlowModule. This is true of the built-in functions that come with this package
and of user defined functions. To make your own FlowModule and FlowEffects, simply add a new script with the name 'FlowModule_' followed by a category name for the
functions you'll be adding. Specific is usually better as the number of functions exposed adds up very quickly.

FlowRunner - To run a FlowGraph, simply add the FlowRunner component anywhere in a scene and give it a reference to an existing FlowGraph. When you hit play you'll see
flashes of yellow and green in the FlowGraph window where the graph is actively executing.

Custom Argument Types - Out of the box, FlowGraph functions support most built-in value types and UnityEngine.Object reference types but not many custom types.
To add your own types you'll need to add classes implementing the completed version of the generic type ArgumentBase<T>. You can look at Argument_FlowNodeReference as an example.

----------------------------------------------------------------------------------------------------------------
  
These notes are incomplete, but I'll let users ask me questions to help me get a sense of where more information is needed!

Good luck getting into flow and please ask questions or make pull requests!
  
Cheers
  - Chris
