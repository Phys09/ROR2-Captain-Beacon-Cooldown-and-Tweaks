# ROR2-Captain-Beacon-Cooldown

A mod for risk of rain 2 that gives cooldowns to captain's beacons.

Bandoliers do not work with them however, the beacons seem to have weird
stopwatch behaviour that I wasn't able to enable/fix.
In the future maybe it could be fixed.

Has config support also if you wish to change the default values.

# Default Beacon Cooldowns

- Healing Beacon: 40s
- Shocking Beacon: 40s
- Equipment Beacon: 60s
- Hacking Beacon: 180s

Note to source code readers:

The source code will look like it's been hacked together because it has been.

This is because trying to just change the cooldowns, max stack size,
stack size, etc... of the beacons loaded in seemed to give weird behaviour, likely
due to how the beacons were not meant to be given cooldowns
so there is no stopwatch ticking for them.

The mod otherwise works given the current implementation,
but it's still possible that the implementation could have been better.
