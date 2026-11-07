"""Build setup programs without loading them into Unity or running license operations."""
from pathlib import Path
import argparse
import hashlib
import json
import subprocess
import xml.etree.ElementTree as ET

from generate_language import render
from prepare_sources import prepare
from verify_sources import verify

PINS = {
    '.': 'f0c1034b1e9d6863e37e3b27c7f97d1ca3c3df25',
    'Assets/InstallerSource/SimpleJson~': '4183278b75c3056aaa47f5b437e047abcf5bc8c3',
    'Assets/InstallerSource/semver.net~': 'cbd74616bb01348f596715d6d22479106d04dda7',
}


def project(path, assembly, sources, managed, platform=None):
    root = ET.Element('Project', Sdk='Microsoft.NET.Sdk')
    props = ET.SubElement(root, 'PropertyGroup')
    values = dict(TargetFramework='netstandard2.1', EnableDefaultCompileItems='false',
                  GenerateAssemblyInfo='false', GenerateTargetFrameworkAttribute='false',
                  AssemblyName=assembly, LangVersion='latest', Deterministic='true')
    if platform:
        values['DefineConstants'] = platform
    for name, value in values.items():
        ET.SubElement(props, name).text = value
    items = ET.SubElement(root, 'ItemGroup')
    for source in sources:
        ET.SubElement(items, 'Compile', Include=str(source))
    for reference in ['UnityEditor.dll', 'UnityEditor.*.dll', 'UnityEngine/*.dll']:
        node = ET.SubElement(items, 'Reference', Include=str(managed / reference))
        ET.SubElement(node, 'Private').text = 'false'
    if platform == 'UNITY_EDITOR_WIN':
        ET.SubElement(items, 'PackageReference', Include='Microsoft.Win32.Registry',
                      Version='5.0.0', PrivateAssets='all')
    ET.indent(root)
    ET.ElementTree(root).write(path, encoding='utf-8', xml_declaration=True)


def build(upstream, inputs, output, unity, dotnet):
    if not (unity / 'Data/Managed/UnityEditor.dll').is_file():
        raise ValueError('--unity must be the Unity Editor directory containing Unity.exe and Data')
    for relative, expected in PINS.items():
        actual = subprocess.check_output(['git', '-C', str(upstream / relative), 'rev-parse', 'HEAD'], text=True).strip()
        dirty = subprocess.check_output(['git', '-C', str(upstream / relative), 'status', '--porcelain', '--untracked-files=no'], text=True).strip()
        if dirty:
            raise ValueError('Upstream tracked sources have local changes: ' + relative)
        if actual != expected:
            raise ValueError(f'Unexpected upstream revision: {relative}: {actual}')
    output.mkdir(parents=True, exist_ok=True)
    render()
    sources = prepare(upstream, inputs, output / 'Sources')
    verify(inputs, sources)
    installer = upstream / 'Assets/InstallerSource'
    builds = output / 'Build'
    builds.mkdir(exist_ok=True)
    # Projects share a directory, so keep their intermediate assets separate.
    (builds / 'Directory.Build.props').write_text('<Project><PropertyGroup><BaseIntermediateOutputPath>obj/$(MSBuildProjectName)/</BaseIntermediateOutputPath></PropertyGroup></Project>', encoding='utf-8')
    managed = unity / 'Data/Managed'
    common = [sources / 'SetupLanguage.cs']
    projects = []
    path = builds / 'Installer.csproj'
    project(path, 'com.anatawa12.vpm-package-auto-installer', common + [
        sources / 'VPMPackageAutoInstaller.cs', installer / 'BurstPatch.cs',
        installer / 'SyncedTask.cs', installer / 'SimpleJson~/SimpleJson.cs',
        installer / 'VrcGetCs/*.cs', installer / 'semver.net~/src/SemanticVersioning/*.cs'], managed)
    projects.append(path)
    for platform in ['WIN', 'OSX', 'LINUX']:
        path = builds / ('License' + platform + '.csproj')
        project(path, 'UnisakiStudio.LicenseSetup.Check.' + platform,
                common + sorted(sources.glob('*LicenseActivator.cs')), managed, 'UNITY_EDITOR_' + platform)
        projects.append(path)
    for path in projects:
        log = output / (path.stem + '-build.log')
        with log.open('w', encoding='utf-8') as stream:
            result = subprocess.run([str(dotnet), 'build', str(path), '-c', 'Release', '-v', 'quiet'],
                                    stdout=stream, stderr=subprocess.STDOUT)
        if result.returncode:
            raise RuntimeError('Build failed; see ' + str(log))
        print(path.stem + ': compiled')
    binary = builds / 'bin/Release/netstandard2.1/com.anatawa12.vpm-package-auto-installer.dll'
    checks = output / 'Checks'
    checks.mkdir(exist_ok=True)
    (checks / 'Program.cs').write_bytes((Path(__file__).parent / 'AssemblyChecks.cs.in').read_bytes())
    (checks / 'Checks.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net9.0</TargetFramework></PropertyGroup></Project>', encoding='utf-8')
    # Loading a helper from a console process does not run Unity's InitializeOnLoad dispatcher.
    # Do not replace this with imports into an Editor: the production DLL installs on load.
    assemblies = [binary] + sorted(binary.parent.glob('UnisakiStudio.LicenseSetup.Check.*.dll'))
    with (output / 'assembly-checks.log').open('w', encoding='utf-8') as stream:
        result = subprocess.run([str(dotnet), 'run', '--project', str(checks / 'Checks.csproj'),
                                 '-c', 'Release', '--', str(managed)] + [str(p) for p in assemblies],
                                stdout=stream, stderr=subprocess.STDOUT)
    if result.returncode:
        raise RuntimeError('Standalone checks failed; see ' + str(output / 'assembly-checks.log'))
    print('Standalone assembly checks passed (no installers executed)')
    manifest = dict(upstream=PINS, unity=str(unity), license_operations_executed=False,
                    installer_executed=False, input_source_hashes={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in inputs.glob('*LicenseActivator.cs')}, binary_sha256=hashlib.sha256(binary.read_bytes()).hexdigest())
    (output / 'build.json').write_text(json.dumps(manifest, indent=2) + '\n', encoding='utf-8')
    print(binary)


if __name__ == '__main__':
    p = argparse.ArgumentParser(description=__doc__)
    for name in ['upstream', 'inputs', 'output', 'unity', 'dotnet']:
        p.add_argument('--' + name, type=Path, required=True)
    args = p.parse_args()
    build(*(getattr(args, name).resolve() for name in ['upstream', 'inputs', 'output', 'unity', 'dotnet']))
