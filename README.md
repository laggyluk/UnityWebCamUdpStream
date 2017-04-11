This is an attempt to establish one way video stream via LAN between two devices in Unity.
Should also work over the internet with some additional effort.

using:
udp library https://github.com/RevenantX/LiteNetLib
lzf compression by Agent_007 and mrbroshkin

For best visual results tune it to spam packets as fast as possible,
unfortunately some devices can not keep up hence the various handicapping settings.

known issues: 
tested and confirmed working between 2 android devices butandorid <> PC connection doesn' seem to work
enabling LZF compression causes memory fragmentation
