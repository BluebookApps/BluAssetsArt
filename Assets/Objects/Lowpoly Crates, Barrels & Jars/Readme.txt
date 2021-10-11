Notes on usage:
- Crates and barrels come in three versions:
	- Closed versions consisting of a single mesh
	- Loose lid versions with a lid that can be (re)moved
	- Broken versions that can be used when destroyed (by the player)
- Pottery comes in two versions:
	- Whole versions with an optional cork stopper
	- Cracked versions that can be used when destroyed (by the player)
- In order to destroy props, destroy whole prefabs and replace with broken/cracked version. Make sure to allign the broken/cracked prefabs with the whole ones. Additionally a (explosion) force can be applied to the newly instatiated broken/cracked version. 
- For performance reasons it may be desirable to deactivate/remove physics on broken pieces after X seconds.