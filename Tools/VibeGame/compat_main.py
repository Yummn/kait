"""Windows entry point. Keep upstream checkout unmodified; patch one platform boundary."""
import os
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parent
os.environ['PATH'] = str(Path(sys.executable).parent) + os.pathsep + os.environ.get('PATH', '')
from dotenv import load_dotenv
load_dotenv(ROOT / '.env', override=False)

from cli.main import main

if os.name == 'nt':
    import ctypes
    from ctypes import wintypes
    import cli.run as runtime_module

    kernel = ctypes.WinDLL('kernel32', use_last_error=True)
    kernel.OpenProcess.argtypes = [wintypes.DWORD, wintypes.BOOL, wintypes.DWORD]
    kernel.OpenProcess.restype = wintypes.HANDLE
    kernel.GetExitCodeProcess.argtypes = [wintypes.HANDLE, ctypes.POINTER(wintypes.DWORD)]
    kernel.GetExitCodeProcess.restype = wintypes.BOOL
    kernel.CloseHandle.argtypes = [wintypes.HANDLE]

    def is_process_alive(pid):
        # Upstream parses localized tasklist text using Python's UTF-8 locale.
        # Chinese Windows returns GBK for missing PIDs, causing a decoding error.
        if pid <= 0:
            return False
        handle = kernel.OpenProcess(0x1000, False, int(pid))
        if not handle:
            return ctypes.get_last_error() == 5  # access denied is not proof of exit
        try:
            code = wintypes.DWORD()
            return bool(kernel.GetExitCodeProcess(handle, ctypes.byref(code))) and code.value == 259
        finally:
            kernel.CloseHandle(handle)

    runtime_module._is_process_alive = is_process_alive

if __name__ == '__main__':
    main()
