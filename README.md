# EjectPlayer

<img src=".\assets\ejectPlayer.png" alt="Eject Player Screenshot">

Eject 1 or more unlucky employee(s) out of the ship. This only works when the ship is in space. 
Everyone needs the mod for it to work.

You can download it on thunderstore as well: https://thunderstore.io/c/lethal-company/p/syntax_z/ejectPlayer/ 

<img src=".\assets\ejectClip.gif" alt="Eject Player example">


# Commands list
| Command     | Example Usage | Description |
| ----------- | ----------- | ----------- |
| list      | `list` | Get a list of everyone's player IDs
| eject      | `eject (playerID) (message)` | Eject a specific player off the ship
| rnd_eject | `rnd_eject`| Eject a random player off the ship



# Changelog

## 1.2.0

### Features
- Added `rnd_eject` which ejects a random person from the ship


### Minor
- Changed `eject` command and `list` to iterate using foreach loop instead of regular for-loop

## 1.1.0

- Messages can now be more than just 1 word
- Fixed bug that only allowed the host to vote everyone and the other clients only being able to vote themselves

## 1.0.0

- Initial release


