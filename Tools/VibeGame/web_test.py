"""Test the isolated Phaser sample. Does not imply Unity Runtime API support."""
import json
import os
from pathlib import Path
import socket
import subprocess
import sys
import time
import urllib.request
from PIL import Image

ROOT = Path(__file__).resolve().parent
DEMO = Path(os.environ.get('KAIT_VIBEGAME_DEMO', ROOT / 'workspace/web-demo'))
UPSTREAM = Path(os.environ.get('KAIT_VIBEGAME_UPSTREAM', ROOT / 'upstream'))
EVIDENCE = Path(os.environ.get('KAIT_VIBEGAME_EVIDENCE', ROOT / 'evidence'))
EVIDENCE.mkdir(parents=True, exist_ok=True)
RESULTS = []
FLAGS = subprocess.CREATE_NO_WINDOW if os.name == 'nt' else 0


def port():
    with socket.socket() as sock:
        sock.bind(('127.0.0.1', 0))
        return sock.getsockname()[1]


def run(*args, timeout=40):
    result = subprocess.run([sys.executable, '-X', 'utf8', str(ROOT / 'compat_main.py'), *map(str, args)],
                            cwd=DEMO, capture_output=True, text=True, encoding='utf-8', timeout=timeout,
                            creationflags=FLAGS)
    with (EVIDENCE / 'web-commands.log').open('a', encoding='utf-8') as log:
        log.write(f'{args}\nexit={result.returncode}\n{result.stdout}\n{result.stderr}\n')
    assert result.returncode == 0, (result.stdout + result.stderr)[-2000:]
    return result.stdout


def record(name, operation):
    try:
        detail = operation()
        RESULTS.append({'name': name, 'status': 'PASS', 'detail': detail or ''})
    except Exception as error:
        RESULTS.append({'name': name, 'status': 'FAIL', 'detail': str(error)})
    print(f'{RESULTS[-1]["status"]}: {name}', flush=True)


record('Web project schema / asset validation', lambda: run('check', '.'))
runtime_port = port()
runtime_log = (EVIDENCE / 'web-runtime.log').open('w', encoding='utf-8')
runtime = subprocess.Popen([sys.executable, '-X', 'utf8', '-u', str(ROOT / 'compat_main.py'), 'run', '.',
                            '--headless', '--activate', '--port', str(runtime_port)], cwd=DEMO,
                           stdout=runtime_log, stderr=subprocess.STDOUT, creationflags=FLAGS)


def play(*args):
    return run('play', '--port', runtime_port, *args)


try:
    def ready():
        deadline = time.monotonic() + 45
        while time.monotonic() < deadline:
            if runtime.poll() is not None:
                raise AssertionError('Runtime exited; see web-runtime.log')
            try:
                snap = json.loads(play('snapshot'))
                assert snap['mode'] == 'paused' and snap['frame'] == 0
                return f'Paused at frame 0; {len(snap["nodes"])} nodes'
            except Exception:
                time.sleep(0.4)
        raise AssertionError('Runtime not ready within 45 seconds')
    record('Headless runtime startup and state snapshot', ready)

    def input_test():
        play('input', '-a', 'start')
        advanced = json.loads(play('continue', '-f', '60'))
        assert advanced['frame'] == 60
        snap = json.loads(play('snapshot'))
        state = next(n['runtime'] for n in snap['nodes'].values() if n['name'] == 'GameRoot')
        assert state['gameState'] == 'playing'
        assert state['spawnedCount'] >= 1
        assert state['missingTextureKeys'] == []
        (EVIDENCE / 'web-snapshot.json').write_text(json.dumps(snap, indent=2), encoding='utf-8')
        return {'frame': snap['frame'], 'gameState': state['gameState'], 'spawnedCount': state['spawnedCount']}
    record('Input injection / 60-frame advance / actual game response', input_test)

    def screenshot():
        target = EVIDENCE / 'web-runtime.png'
        play('screenshot', '-o', str(target))
        with Image.open(target) as image:
            assert image.width > 100 and image.height > 100
            assert len(image.convert('RGB').getcolors(image.width * image.height)) > 10
        return str(target)
    record('Runtime screenshot (nonblank)', screenshot)

    def edit_state():
        snap = json.loads(play('snapshot'))
        node = next(key for key, n in snap['nodes'].items() if n['name'] == 'GameRoot')
        play('set', '-n', node, '-p', 'config.missLimit', '-v', '5')
        updated = json.loads(play('snapshot'))
        assert updated['nodes'][node]['config']['missLimit'] == 5
        return 'Changed sample missLimit to 5 and read it back'
    record('Runtime property editing', edit_state)
    record('Recording API', lambda: play('record', 'integration-check'))
    record('Recorded frame advance', lambda: play('continue', '-f', '2'))
    record('Stop recording', lambda: play('record', '--stop'))
    record('Runtime refresh', lambda: play('refresh'))
    if os.name != 'nt':
        def replay():
            trace = DEMO / '.vibegame/traces/integration-check/play.sh'
            script = trace.read_text(encoding='utf-8')
            # Only execute this test's generated recording, not arbitrary scripts.
            commands = [line for line in script.splitlines() if line.startswith('run ')]
            expected = [f'run vibegame play --port {runtime_port} activate',
                        f'run vibegame play --port {runtime_port} continue -f 2',
                        f'run vibegame play --port {runtime_port} deactivate']
            assert commands == expected, commands
            # The upstream script lacks pipefail; enable it so failed calls fail the test.
            result = subprocess.run(['bash', '-o', 'pipefail', str(trace)], cwd=DEMO,
                                    capture_output=True, text=True, timeout=45)
            (EVIDENCE / 'replay.log').write_text(result.stdout + result.stderr, encoding='utf-8')
            assert result.returncode == 0, (result.stdout + result.stderr)[-2000:]
            # deactivate removes the snapshot, so verify each replay response
            # rather than requesting a frame after the trace has deactivated it.
            decoder = json.JSONDecoder()
            responses = []
            remaining = result.stdout.strip()
            while remaining:
                value, end = decoder.raw_decode(remaining)
                responses.append(value)
                remaining = remaining[end:].lstrip()
            assert len(responses) == 3, responses
            assert responses[0] == {'status': 'ok', 'frame': 0}, responses
            assert responses[1] == {'status': 'completed', 'frame': 2}, responses
            assert responses[2] == {'status': 'ok'}, responses
            return 'Generated Bash recording replayed: activated at frame 0, advanced to frame 2, deactivated'
        record('Bash recording replay', replay)
finally:
    try:
        run('close', '.', '--port', runtime_port)
        runtime.wait(timeout=15)
        RESULTS.append({'name': 'Runtime shutdown', 'status': 'PASS', 'detail': ''})
    except Exception as error:
        RESULTS.append({'name': 'Runtime shutdown', 'status': 'FAIL', 'detail': str(error)})
        if runtime.poll() is None:
            runtime.terminate()
            runtime.wait(timeout=10)
    runtime_log.close()

dashboard_port = port()
with (EVIDENCE / 'dashboard-test.log').open('w', encoding='utf-8') as log:
    dashboard = subprocess.Popen([sys.executable, '-X', 'utf8', '-u', str(UPSTREAM / 'src/web/server.py'),
                                  'server', '--host', '127.0.0.1', '--port', str(dashboard_port),
                                  '--project', str(DEMO), '--db', str(EVIDENCE / 'dashboard-test.db'),
                                  '--log-file', str(EVIDENCE / 'dashboard-server.log')],
                                 cwd=ROOT, stdout=log, stderr=subprocess.STDOUT, creationflags=FLAGS)
    try:
        def dashboard_test():
            deadline = time.monotonic() + 20
            url = f'http://127.0.0.1:{dashboard_port}'
            while time.monotonic() < deadline:
                try:
                    with urllib.request.urlopen(url, timeout=2) as response:
                        assert response.status == 200
                    break
                except Exception:
                    if dashboard.poll() is not None: raise AssertionError('Dashboard exited; see log')
                    time.sleep(0.3)
            else:
                raise AssertionError('Dashboard failed to start')
            from playwright.sync_api import sync_playwright
            with sync_playwright() as p:
                browser = p.chromium.launch(headless=True)
                page = browser.new_page(viewport={'width': 1440, 'height': 900})
                page.goto(url, wait_until='networkidle')
                text = page.locator('body').inner_text()
                assert len(text) > 20
                page.screenshot(path=str(EVIDENCE / 'dashboard.png'))
                (EVIDENCE / 'dashboard-text.txt').write_text(text, encoding='utf-8')
                # These are the sample project's trees, not Unity scene data.
                for endpoint in ['/api/assets/tree', '/api/nodes/tree']:
                    response = page.request.get(url + endpoint)
                    assert response.ok, endpoint
                    data = response.json()
                    assert data and not (isinstance(data, dict) and data.get('error')), str(data)
                page.get_by_text('Assets', exact=True).click()
                page.wait_for_load_state('networkidle')
                page.wait_for_function("!document.body.innerText.includes('Loading')", timeout=15000)
                page.screenshot(path=str(EVIDENCE / 'dashboard-assets.png'))
                page.get_by_text('Objects', exact=True).click()
                page.wait_for_load_state('networkidle')
                page.wait_for_function("!document.body.innerText.includes('Loading')", timeout=15000)
                page.screenshot(path=str(EVIDENCE / 'dashboard-objects.png'))
                browser.close()
            return 'Dashboard rendered; Assets/Objects tabs and tree APIs opened; no agent chat was launched'
        record('Dashboard render without tmux agents', dashboard_test)
    finally:
        dashboard.terminate()
        dashboard.wait(timeout=10)

(EVIDENCE / 'web-results.json').write_text(json.dumps(RESULTS, ensure_ascii=False, indent=2), encoding='utf-8')
raise SystemExit(1 if any(row['status'] == 'FAIL' for row in RESULTS) else 0)
