#!/bin/sh
set -eu
INTEGRATION_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
. "$INTEGRATION_DIR/wsl-environment.sh"
LINUX_ROOT=${KAIT_VIBEGAME_WSL_ROOT:-"$HOME/.local/share/kait-vibegame"}
PYTHON="$LINUX_ROOT/.venv/bin/python"
test -x "$PYTHON" || { echo 'Linux environment missing; run install-wsl.sh first.'; exit 1; }
export PATH="$LINUX_ROOT/.venv/bin:$LINUX_ROOT/bin:$PATH"
export KAIT_VIBEGAME_UPSTREAM="$LINUX_ROOT/upstream"
export KAIT_VIBEGAME_EVIDENCE="$INTEGRATION_DIR/evidence/wsl"
export KAIT_VIBEGAME_DEMO="$LINUX_ROOT/workspace/web-demo"
cd "$INTEGRATION_DIR/workspace"
case "${1:-}" in
    bridge) shift; exec "$PYTHON" -X utf8 "$INTEGRATION_DIR/bridge.py" "$@" ;;
    web) shift; cd "$KAIT_VIBEGAME_DEMO" ;;
esac
exec "$PYTHON" -X utf8 "$INTEGRATION_DIR/compat_main.py" "$@"
