#!/usr/bin/env python3
# SPDX-License-Identifier: GPL-3.0-only
"""Build API inputs and preview documentation using the shared Docker image."""

import os
import subprocess
import sys
from pathlib import Path


def run(*args, capture=False):
    result = subprocess.run(
        args, check=True, text=True, stdout=subprocess.PIPE if capture else None
    )
    return result.stdout.strip() if capture else None


if __name__ == "__main__":
    os.chdir(Path(__file__).resolve().parents[1])
    run(sys.executable, "scripts/prepare.py")
    run(
        "dotnet",
        "build",
        "src/MrKWatkins.OakIO.ZXSpectrumNext",
        "-c",
        "Release",
        "--nologo",
        "-p:CopyLocalLockFileAssemblies=true",
    )
    image = os.environ.get("DOCS_IMAGE", "ghcr.io/mrkwatkins/docs-dotnet:latest")
    run("docker", "pull", image)
    resolved = run(
        "docker",
        "image",
        "inspect",
        "--format",
        "{{index .RepoDigests 0}}",
        image,
        capture=True,
    )
    source_ref = run("git", "rev-parse", "HEAD", capture=True)
    print(
        f"Documentation image: {resolved}\nPreview: http://localhost:5173/OakIO/",
        flush=True,
    )
    run(
        "docker",
        "run",
        "--rm",
        "--init",
        "--user",
        f"{os.getuid()}:{os.getgid()}",
        "--mount",
        f"type=bind,source={Path.cwd()},target=/workspace",
        "-w",
        "/workspace",
        "-p",
        "127.0.0.1:5173:5173",
        resolved,
        "serve",
        "--source-ref",
        source_ref,
    )
