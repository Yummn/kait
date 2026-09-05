# Sourced only by this integration. Do not modify system shell profiles.
# Playwright 1.58 has no Ubuntu 26.04 download mapping. Its 24.04 Chromium
# build is used here, with compatibility checked by the local browser tests.
if [ -r /etc/os-release ]; then
    DISTRO_ID=$(. /etc/os-release; printf '%s' "$ID")
    DISTRO_VERSION=$(. /etc/os-release; printf '%s' "$VERSION_ID")
    if [ "$DISTRO_ID" = ubuntu ] && [ "$DISTRO_VERSION" = 26.04 ] && [ "$(uname -m)" = x86_64 ]; then
        export PLAYWRIGHT_HOST_PLATFORM_OVERRIDE=ubuntu24.04-x64
    fi
fi
