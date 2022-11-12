
# Reverse Engineering ToDos

For the project to progress, further research has to be conducted.
The following list provides an overview over the missing aspects.

## Packets

The following section will describe how unknown details about packets should be documented.

*Hint:* All packets are to be taken from AeroMessages, justify the usage of custom packages in a dedicated heading if not applicable ("AeroMessages doesn't contain this package", for example)

**Note:** Fully qualified names (FQN) **must** be used because there are packets sharing the same name (e.g. `BaseController`, which is present in `AeroMessages.GSS.V66.Character.Controller`, `AeroMessages.GSS.V66.Vehicle.Controller`and `AeroMessages.GSS.V66.Turret.Controller`)

## Name of the packet, eg. "MovementInput" (without the quotation marks)
### FQN
FQN of the packet.
*Example:* `AeroMessages.GSS.V66.Character.Command.MovementInput`
### Used in 
FQN of the controller(s) which use this packet.
*Example:*
- `GameServer.Controllers.Character.BaseController`
- `...`
- `...`
### Assumed Responses 
List both the simple and FQN of the responses (packets) you assume the server is expected to send after processing of the the packet named above.
*Example:*
  - `ConfirmedPoseUpdate` `(AeroMessages.GSS.V66.Character.Event.ConfirmedPoseUpdate)`
  - `...` `(...)`
  - `...` `(...)`

*If there are no responses, just leave this to `NONE`*

*Example:*
`NONE`
### Assumed Response to 
Packets sent to the client which can trigger it to send an instance of this packet here. Add the respective conditions as well. 

*Example:*

- `ConfirmedPoseUpdate`
	- `AeroMessages.GSS.V66.Character.Event.ConfirmedPoseUpdate`
	- Sent after the timestamp provided in `NextShort` expires *(Hint: `NextShort` is a field in `ConfirmedPoseUpdate`)*
- ...
	- `...`
	- ...
- ...
	- `...`
	- ...

### Notes 
Explain **what**, **why** and if possible **how** research should be done on this packet.

*Example (why):* 
As per its documentation in `AeroMessages`, `MovementInput.PoseData.HaveMoreData` (in `MovementPoseData`) appears to control a lot of things.
For the moment, movement is functional, but this doesn't mean that there isn't something important hidden within `MovementPoseData.UnkData` (`MovementPoseMoreData`).

*Example (how):*
Reverse engineering the respective part of the client is one option, but if `HaveMoreData` is set, the content of `UnkData` (`MovementPoseMoreData`) should be persisted with the contents of the whole package so it can be inspected within its context.

# Packet Example
The following is an example of what a well documented entry looks like

## MovementInput
### FQN
`AeroMessages.GSS.V66.Character.Command.MovementInput`
### Used in 
- `GameServer.Controllers.Character.BaseController`
### Assumed Responses 
- `ConfirmedPoseUpdate` `(AeroMessages.GSS.V66.Character.Event.ConfirmedPoseUpdate)`
### Assumed Response to 
- `ConfirmedPoseUpdate`
	- `AeroMessages.GSS.V66.Character.Event.ConfirmedPoseUpdate`
	- Sent after the timestamp provided in `ConfirmedPoseUpdate.NextShort` expires 

### Notes 

As per its documentation in `AeroMessages`, `MovementInput.PoseData.HaveMoreData` (in `MovementPoseData`) appears to control a lot of things.
For the moment, movement is functional, but this doesn't mean that there isn't something important hidden within `MovementPoseData.UnkData` (`MovementPoseMoreData`).


Reverse engineering the respective part of the client is one option, but if `HaveMoreData` is set, the content of `UnkData` (`MovementPoseMoreData`) should be persisted with the contents of the whole package so it can be inspected within its context.

# Packet Template
Use the following template for the creation of a new template

    ## [packet_name]
    ### FQN
    `packet_fqn`
    ### Used in 
    - `controller_fqn`
    ### Assumed Responses 
    - `response_packet_name` `(response_packet_fqn)`
    ### Assumed Response to 
    - `response_to_packet_name`
	    - `response_to_packet_fqn`
	    - [Why the server responds with this packet ...]

    ### Notes 
    [What, why...]
    
    [How...]

# Packets to Investigate

## MovementInput
### FQN
`AeroMessages.GSS.V66.Character.Command.MovementInput`
### Used in 
- `GameServer.Controllers.Character.BaseController`
### Assumed Responses 
- `ConfirmedPoseUpdate` `(AeroMessages.GSS.V66.Character.Event.ConfirmedPoseUpdate)`
### Assumed Response to 
- `ConfirmedPoseUpdate`
	- `AeroMessages.GSS.V66.Character.Event.ConfirmedPoseUpdate`
	- Sent after the timestamp provided in `ConfirmedPoseUpdate.NextShort` expires 

### Notes 

#### `HaveMoreData`
As per its documentation in `AeroMessages`, `MovementInput.PoseData.HaveMoreData` (in `MovementPoseData`) appears to control a lot of things.
For the moment, movement is functional, but this doesn't mean that there isn't something important hidden within `MovementPoseData.UnkData` (`MovementPoseMoreData`).


Reverse engineering the respective part of the client is one option, but if `HaveMoreData` is set, the content of `UnkData` (`MovementPoseMoreData`) should be persisted with the contents of the whole package so it can be inspected within its context.

#### `MovementState`

Currently, we do not really understand what `MovementPoseData.MovementState` really contains.
Logs from the communication between client and server show that the client sends a value of `1288`, which does not correspond to any values present in our enum fpr this purpose (``)