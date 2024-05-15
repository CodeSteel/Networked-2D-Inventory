# Networked-2D-Inventory
A fully networked &amp; optimized 2 dimensional inventory system. 

* Unity 2022.3.261f
* FishNet 4.1.6
* SteelBox Extensions

# Notes:
* Inventory Container & Controller can be added to any Network Object.
* HotbarController should only be added to Player objects.
* GearController should only be added to Player objects.
* 2d array to hold item slot states.
* 'Listening system' - Connections can be added as a listener to any inventory controller. When listening, those connections are sent all slot updates that occur. They are only sent the slot changes, not the whole inventory every time. This also has some security checks which means no cheating.
