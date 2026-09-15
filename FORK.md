# This fork (FirstIntegral)

Public personal fork of [OpenTabletDriver/OpenTabletDriver](https://github.com/OpenTabletDriver/OpenTabletDriver).
It is **not** an upstream pull request.

A GitHub fork is a **separate repository**. Commits, branches, force-pushes, and
deletes here (`FirstIntegral/OpenTabletDriver`) do not change upstream. Nothing
reaches `OpenTabletDriver/OpenTabletDriver` unless someone opens a pull request
and maintainers merge it. That is not happening for this work.

Default branch is **`main`**: upstream `0.6.x` plus the Linux HOGP / G930L Bluetooth work. One branch, no PR-shaped extra.

## What this fork adds

Upstream already supports the Huion Inspiroy Giano **G930L over USB**
(`256c:0061`). This fork adds the **Bluetooth** path on Linux.

### 1. Linux Bluetooth HOGP hidraw hub

HidSharpCore 1.3.0 on Linux only creates hidraw devices that have a udev parent
`usb` / `usb_device`. Bluetooth HID-over-GATT (HOGP) devices show up as `uhid`
on HID bus `0005` and have **no USB parent**, so they never appear in
`DeviceList.Local.GetHidDevices()`. A tablet JSON file alone cannot detect them.

This fork adds `LinuxHidrawRootHub` (`OpenTabletDriver/Devices/LinuxHidraw/`):

- Enumerates `/dev/hidraw*` whose `HIDIOCGRAWINFO` bustype is **not** `BUS_USB`
  (USB stays on HidSharp)
- Reads the report descriptor from sysfs (`device/report_descriptor`)
- Matches existing OTD `IDeviceEndpoint` / `IDeviceEndpointStream` so
  `Driver.Detect` works as for USB

`generate-rules.sh` also emits, for every tablet VID:PID:

- `KERNEL=="hidraw*", KERNELS=="0005:VID:PID.*", TAG+="uaccess"` —
  uhid hidraw has no USB `idVendor` (match is uppercase, as in sysfs)
- `SUBSYSTEM=="input", ATTRS{id/vendor}` / `ATTRS{id/product}` libinput ignore
  when `libinputoverride` is set — uhid evdev uses `id/vendor`, not USB `idVendor`

USB `ATTRS{idVendor}` rules are unchanged.

### 2. Huion G930L Bluetooth tablet config

Separate config from USB `G930L.json` — the two reports are not the same device
description:

| | USB G930L | This fork: G930L Bluetooth |
| --- | --- | --- |
| PID | `0x0061` | `0x8251` |
| Report | vendor 12-byte | HOGP 10-byte, report ID 2 |
| Axes | Max 69088×43180, 5080 LPI | Max 32767×32767 (15-bit HID) |
| Parser | `GianoReportParser` | `GianoBluetoothReportParser` |
| Pad | existing USB aux | HOGP HID-keyboard chords → 6 aux buttons |

Config: `OpenTabletDriver.Configurations/Configurations/Huion/G930L Bluetooth.json`.

Pad key **bindings** stay in the user's `settings.json` (Auxiliary Settings in
the GUI), same as every other OTD tablet. This fork does not ship personal
Xournal++ shortcuts.

## Verified here

- Tablet: Huion Inspiroy Giano G930L, BLE name `Inspiroy Giano-864`, pen PW517
- HOGP: `0005:256C:8251`
- OS: Omarchy 4.0.3 (Arch), kernel 7.2.3-arch1-3, Hyprland, BlueZ 5.87, Intel AX211
- Pen + pressure + all 6 pad keys in Xournal++ (Artist Mode), 2026-09-09

## Not claimed

- Windows / macOS / other Linux distros / musl
- Vendor GATT `0000FFE0` (present on the tablet, unused)
- USB + BLE connected at once (two tablet trees, different PIDs)
- Packaged AUR `opentabletdriver` udev rules — those are still USB-only; BLE
  needs this tree's `generate-rules.sh`

## Using this fork

1. Pair the tablet over Bluetooth (BlueZ). Kernel should create
   `0005:256C:8251` hidraw nodes.
2. Build this branch (`./build.sh linux` / `./eng/bash/package.sh`).
3. Install udev rules generated from **this** tree, then `udevadm control --reload-rules`.
4. Stop any packaged daemon (`systemctl --user stop opentabletdriver.service`)
   before running the local one. Two daemons fight over the tablet.

## Relation to upstream

Upstream project: <https://github.com/OpenTabletDriver/OpenTabletDriver>

This fork's `main` is that project's `0.6.x` line plus the commits above. It is
kept public as a working copy. **No pull requests, no pushes, no issues to
upstream.** We only **pull** their `0.6.x` updates into `main` and keep our own
development here.

## License and name

Upstream is **LGPL-3.0-or-later**. Forking, modifying, and running a private or
public copy is allowed. This tree stays under the same license (`LICENSE` kept).
This file is the prominent notice of what changed. Source is this GitHub repo.
No claim to be the official OpenTabletDriver project.

The GitHub repo name stays **OpenTabletDriver**. That is normal for a fork
(`forked from OpenTabletDriver/OpenTabletDriver`). A rename is for a separate
product identity, not required by LGPL.

```bash
git fetch origin
git merge origin/0.6.x    # into main
git push fork main        # never git push origin
```
