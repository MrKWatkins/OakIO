#!/usr/bin/env python3
# SPDX-License-Identifier: GPL-3.0-only
"""Prepare the threaded converter and its static documentation assets."""

import json
import os
import shutil
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
WEB = ROOT / "web"
RUNTIME_EXTENSIONS = {".js", ".mjs", ".wasm", ".dll", ".dat", ".json"}


def run(*args, cwd=ROOT):
    subprocess.run(args, cwd=cwd, check=True)


def publish_runtime():
    project = ROOT / "src/MrKWatkins.OakIO.Wasm/MrKWatkins.OakIO.Wasm.csproj"
    installed = subprocess.check_output(
        ["dotnet", "workload", "list"], cwd=ROOT, text=True
    )
    if not any(line.split()[:1] == ["wasm-tools"] for line in installed.splitlines()):
        run("dotnet", "workload", "install", "wasm-tools", "--skip-manifest-update")
    output = ROOT / "artifacts/wasm"
    shutil.rmtree(output, ignore_errors=True)
    arguments = ["dotnet", "publish", project, "-c", "Release", "-o", output]
    if version := os.environ.get("VERSION"):
        arguments.append(f"-p:Version={version}")
    run(*arguments)
    runtime = WEB / "dotnet"
    shutil.rmtree(runtime, ignore_errors=True)
    shutil.copytree(output, runtime)
    for name in ("dotnet.js", "dotnet.native.wasm", "dotnet.native.worker.mjs"):
        if not (runtime / name).is_file():
            raise FileNotFoundError(f"Missing threaded runtime asset: {name}")
    resources = {
        "jsModuleNative": [{"name": "dotnet.native.js"}],
        "jsModuleRuntime": [{"name": "dotnet.runtime.js"}],
        "jsModuleWorker": [{"name": "dotnet.native.worker.mjs"}],
        "wasmNative": [{"name": "dotnet.native.wasm"}],
        "assembly": [
            {"name": p.name, "virtualPath": p.name}
            for p in sorted(runtime.glob("*.dll"))
        ],
        "icu": [
            {"name": p.name, "virtualPath": p.name}
            for p in sorted(runtime.glob("icudt*.dat"))
        ],
    }
    config = {"mainAssemblyName": "MrKWatkins.OakIO.Wasm.dll", "resources": resources}
    (runtime / "dotnet.boot.js").write_text(
        "export const config = " + json.dumps(config, indent=2) + ";\n"
    )


def prepare():
    publish_runtime()
    run("npm", "ci", cwd=WEB)
    run("npm", "test", cwd=WEB)
    run("npm", "exec", "--", "tsc", "-b", cwd=WEB)
    run("npm", "exec", "--", "vite", "build", "--base=./", cwd=WEB)
    runtime = WEB / "dist/dotnet"
    runtime.mkdir(parents=True, exist_ok=True)
    for path in (WEB / "dotnet").iterdir():
        if path.is_file() and path.suffix in RUNTIME_EXTENSIONS:
            shutil.copy2(path, runtime / path.name)
    public = ROOT / "artifacts/docs-public"
    shutil.rmtree(public, ignore_errors=True)
    shutil.copytree(ROOT / "docs/public", public)
    shutil.copytree(WEB / "dist", public / "converter-app")
    print(f"Prepared converter and documentation assets: {public}", flush=True)


if __name__ == "__main__":
    if sys.argv[1:] == ["--runtime-only"]:
        publish_runtime()
    elif not sys.argv[1:]:
        prepare()
    else:
        raise SystemExit("Usage: prepare.py [--runtime-only]")
