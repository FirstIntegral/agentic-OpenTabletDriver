#!/usr/bin/env bash
# Assert generate-rules.sh emits Bluetooth HOGP KERNELS (uppercase) and
# lowercase id/vendor libinput ignore for G930L BLE PID 8251.
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"
tmp="$(mktemp)"
trap 'rm -f "$tmp"' EXIT
bash generate-rules.sh >"$tmp"

grep -Fq 'KERNELS=="0005:256C:8251.*"' "$tmp" || {
  echo "Failure: missing uppercase HOGP KERNELS rule for 256C:8251" >&2
  exit 1
}
grep -Fq 'ATTRS{id/vendor}=="256c", ATTRS{id/product}=="8251"' "$tmp" || {
  echo "Failure: missing lowercase id/vendor libinput ignore for 256c:8251" >&2
  exit 1
}
if grep -Fq 'KERNELS=="0005:256c:8251.*"' "$tmp"; then
  echo "Failure: lowercase KERNELS would miss uhid HID parent names" >&2
  exit 1
fi
if grep -Fq 'ATTRS{id/vendor}=="256C"' "$tmp"; then
  echo "Failure: uppercase id/vendor would miss evdev sysfs" >&2
  exit 1
fi
echo "OK: generate-rules.sh HOGP 8251 lines"
