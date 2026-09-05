"""Small Unity-sidecar adapter. No Unity runtime or global agent settings are changed."""
from __future__ import annotations

import argparse
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parent
PROJECT = ROOT.parent.parent
STAGING = ROOT / 'workspace' / 'output'
DESTINATION = PROJECT / 'Assets' / 'Art' / 'VibeGame'
IMAGE_EXTENSIONS = {'.png', '.jpg', '.jpeg'}


def import_asset(source: Path, dry_run: bool = False) -> Path:
    source = source.resolve(strict=True)
    if not source.is_relative_to(STAGING.resolve()):
        raise ValueError('Only reviewed images from Tools/VibeGame/workspace/output can be imported.')
    if source.suffix.lower() not in IMAGE_EXTENSIONS:
        raise ValueError('Only PNG/JPG images are supported. Atlas slicing remains an explicit Unity step.')
    from PIL import Image
    with Image.open(source) as img:
        img.verify()
    destination = DESTINATION / source.name
    if destination.exists():
        raise FileExistsError(f'Refusing to overwrite an existing Unity asset: {destination}')
    if not dry_run:
        destination.parent.mkdir(parents=True, exist_ok=True)
        # Exclusive creation: a second import must never overwrite an existing asset.
        with source.open('rb') as incoming, destination.open('xb') as outgoing:
            shutil.copyfileobj(incoming, outgoing)
    return destination


def status() -> dict:
    from dotenv import dotenv_values
    # Check only this integration's explicit configuration; never print secrets.
    values = dotenv_values(ROOT / '.env') if (ROOT / '.env').exists() else {}
    keys = ['IMAGE_API_KEY', 'VLM_API_KEY', 'VIDEO_API_KEY', 'QWEN_SERVER_URL']
    return {
        'mode': 'Unity sidecar; Phaser tools do not control the Unity game',
        'unityProject': str(PROJECT),
        'unityVersion': (PROJECT / 'ProjectSettings/ProjectVersion.txt').read_text().splitlines()[0],
        'upstreamPresent': (ROOT / 'upstream/pyproject.toml').exists(),
        'python': sys.version.split()[0],
        'tmuxAvailable': shutil.which('tmux') is not None,
        'localServiceConfigurationPresent': {k: bool(values.get(k)) for k in keys},
        'staging': str(STAGING),
        'importDestination': str(DESTINATION),
        'automaticAssetReplacement': False,
    }


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    sub = parser.add_subparsers(dest='command', required=True)
    sub.add_parser('status')
    imp = sub.add_parser('import-asset')
    imp.add_argument('path', type=Path)
    imp.add_argument('--dry-run', action='store_true')
    sub.add_parser('test')
    sub.add_parser('web-test')
    dashboard = sub.add_parser('dashboard', help='Local asset dashboard only; no agent team is started')
    dashboard.add_argument('--port', type=int, default=18766)
    args = parser.parse_args()
    if args.command == 'status':
        print(json.dumps(status(), ensure_ascii=False, indent=2))
    elif args.command == 'import-asset':
        print(import_asset(args.path, args.dry_run))
    elif args.command == 'test':
        result = subprocess.run([sys.executable, '-X', 'utf8', str(ROOT / 'smoke_test.py')], cwd=ROOT)
        raise SystemExit(result.returncode)
    elif args.command == 'web-test':
        result = subprocess.run([sys.executable, '-X', 'utf8', str(ROOT / 'web_test.py')], cwd=ROOT)
        raise SystemExit(result.returncode)
    elif args.command == 'dashboard':
        evidence = Path(os.environ.get('KAIT_VIBEGAME_EVIDENCE', ROOT / 'evidence'))
        evidence.mkdir(parents=True, exist_ok=True)
        upstream = Path(os.environ.get('KAIT_VIBEGAME_UPSTREAM', ROOT / 'upstream'))
        demo = Path(os.environ.get('KAIT_VIBEGAME_DEMO', ROOT / 'workspace/web-demo'))
        print('Assets/Objects/Play refer to the isolated web demo, NOT Unity. Chat agents are not started.', flush=True)
        subprocess.run([sys.executable, '-X', 'utf8', str(upstream / 'src/web/server.py'),
                        'server', '--host', '127.0.0.1', '--port', str(args.port),
                        '--project', str(demo),
                        '--db', str(evidence / 'dashboard.db'), '--log-file', str(evidence / 'dashboard.log')], cwd=ROOT)


if __name__ == '__main__':
    main()
