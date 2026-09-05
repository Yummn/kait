#!/bin/sh
# Run as the regular Ubuntu user; system libraries are installed separately.
set -eu
INTEGRATION_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
. "$INTEGRATION_DIR/wsl-environment.sh"
LINUX_ROOT=${KAIT_VIBEGAME_WSL_ROOT:-"$HOME/.local/share/kait-vibegame"}
REVISION=cab478bf2dafe93bd586aa1043a1e2182f4da197
UV="$LINUX_ROOT/bin/uv"
test -x "$UV" || { echo 'Install uv into the isolated Linux bin directory first.'; exit 1; }
command -v tmux >/dev/null
export UV_PYTHON_INSTALL_DIR="$LINUX_ROOT/python"
export UV_CACHE_DIR="$LINUX_ROOT/cache"
if [ ! -d "$LINUX_ROOT/upstream" ]; then
    git clone --no-hardlinks "$INTEGRATION_DIR/upstream" "$LINUX_ROOT/upstream"
    git -C "$LINUX_ROOT/upstream" checkout --detach "$REVISION"
fi
test "$(git -C "$LINUX_ROOT/upstream" rev-parse HEAD)" = "$REVISION" || {
    echo 'Unexpected upstream revision; nothing was reset.'; exit 1;
}
if [ ! -x "$LINUX_ROOT/.venv/bin/python" ]; then
    "$UV" venv --python 3.12.14 "$LINUX_ROOT/.venv"
fi
"$UV" pip install --python "$LINUX_ROOT/.venv/bin/python" -e "$LINUX_ROOT/upstream" -c "$LINUX_ROOT/upstream/requirements.txt"
"$UV" pip check --python "$LINUX_ROOT/.venv/bin/python"
"$LINUX_ROOT/.venv/bin/python" -m playwright install chromium
mkdir -p "$LINUX_ROOT/workspace"
DEMO="$LINUX_ROOT/workspace/web-demo"
if [ ! -d "$DEMO" ]; then
    cp -R "$LINUX_ROOT/upstream/src/skeletons/swipe-slice-arcade" "$DEMO"
    "$LINUX_ROOT/.venv/bin/vibegame" init "$DEMO" --lang zh-CN --choice skip --no-commit
fi
echo 'Linux sidecar installed. No agent sessions or paid services were started.'
