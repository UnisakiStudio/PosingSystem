"""Build-local overlays. Does not execute installers or modify a distribution folder."""
from pathlib import Path
import argparse,json,re,hashlib,shutil
HERE=Path(__file__).resolve().parent

def csharp(value):return json.dumps(value,ensure_ascii=False)
def prepare(upstream,inputs,output):
    output.mkdir(parents=True,exist_ok=True)
    repo=HERE.parents[1]
    helper=repo/'Packages/jp.unisakistudio.posingsystem/Editor/Setup/SetupLanguage.cs'
    shutil.copy2(helper,output/'SetupLanguage.cs')
    src=upstream/'Assets/InstallerSource/VPMPackageAutoInstaller.cs'
    text=src.read_text(encoding='utf-8-sig').replace('using UnityEngine;','using UnityEngine;\nusing SetupLanguage = UnisakiStudio.Setup.Language;')
    text=text.replace('EditorUtility.DisplayDialog(', 'SetupLanguage.Dialog(').replace('EditorUtility.DisplayProgressBar(', 'SetupLanguage.Progress(')
    # Translate only human-readable pieces of compound confirmation messages.
    for phrase in ["You're installing the following packages:","\\n\\nThis will add following repositories:","\\n\\nYou're also deleting the following files/folders:","\\n\\nYou're also deleting the following legacy Packages:","The following packages are incompatible with current unity!\\n","\\nPlease change unity version before installing packages!","Installing package failed due to conflicts:\\n","Please see console for more details"," version "]:
        literal='"'+phrase+'"'
        text=text.replace(literal,'SetupLanguage.T('+literal+')')
    start=text.index('        private static string MinimumUnityVersionMessage(')
    end=text.index('        internal static void ResolveUnityPackageManger()',start)
    text=text[:start]+'''        private static string MinimumUnityVersionMessage(ushort major, byte minor) =>
            SetupLanguage.Format("This tool requires Unity {0}.{1} or later. Upgrade Unity before installing.", major, minor);

'''+text[end:]
    text=text.replace('''return "You're installing tools that require VRCSDK for Unity 2019.x " +
                           "but you're using other versions of VRCSDK!\\n" +
                           "Please use older VRCSDK or wait for tool updates!";''','''return SetupLanguage.T("You're installing tools that require VRCSDK for Unity 2019.x but you're using other versions of VRCSDK! Please use older VRCSDK or wait for tool updates!");''')
    # Re-render the human-readable parts after the dialog's Language... action.
    # Package IDs, versions, repository URLs and cleanup paths remain the resolved inputs.
    start=text.index('                var confirmMessage = new StringBuilder(SetupLanguage.T("You\'re installing the following packages:"));')
    end=text.index('                if (!SetupLanguage.Dialog("Confirm", confirmMessage.ToString(), "Install", "Cancel"))',start)
    body=text[start:end].rstrip()
    text=text[:start]+'                string ConfirmationMessage()\n                {\n'+''.join('    '+line+'\n' for line in body.splitlines())+'                    return confirmMessage.ToString();\n                }\n\n'+text[end:]
    text=text.replace('SetupLanguage.Dialog("Confirm", confirmMessage.ToString(), "Install", "Cancel")', 'SetupLanguage.Dialog("Confirm", ConfirmationMessage, "Install", "Cancel")')
    # Keep the actual exception visible, including after choosing another language.
    # Full exception/stack logging remains in the upstream catch block.
    error_dialog='SetupLanguage.Dialog("ERROR", "Error installing packages", "ok");'
    if text.count(error_dialog) != 1:
        raise ValueError('Expected one generic package-installation error dialog')
    text=text.replace(error_dialog, 'SetupLanguage.Dialog("ERROR", () => InstallerError.Message(e, SetupLanguage.Current), "ok");')
    text+='\n'+(HERE/'InstallerError.cs.in').read_text(encoding='utf-8')
    (output/'VPMPackageAutoInstaller.cs').write_text(text,encoding='utf-8')
    # Preserve every non-UI part of each license activator. In particular do not change
    # registration keys, activation values, product IDs or cleanup targets.
    licenses=[]
    literal=re.compile(r'"(?:\\.|[^"\\])*"')
    for original in sorted(inputs.glob('*LicenseActivator.cs')):
        lines=[]
        for line in original.read_text(encoding='utf-8-sig').splitlines():
            if '[MenuItem(' not in line and 'const string' not in line and '$"' not in line:
                def replace(m):
                    try:value=json.loads(m[0])
                    except ValueError:return m[0]
                    return 'SetupLanguage.T('+m[0]+')' if re.search('[\u3040-\u30ff\u4e00-\u9fff]',value) else m[0]
                line=literal.sub(replace,line)
            else:
                # Short status words occur inside interpolated expressions.
                for word in ['有効','未登録']:line=line.replace('"'+word+'"','SetupLanguage.T("'+word+'")')
            line=line.replace('EditorUtility.DisplayDialog(', 'SetupLanguage.Dialog(')
            lines.append(line)
        patched='\n'.join(lines)+'\n'
        patched=patched.replace('SetupLanguage.T("ライセンスはソフトウェア用のレジストリ内にインストールされます。")', 'SetupLanguage.StorageNotice')
        patched='using SetupLanguage = UnisakiStudio.Setup.Language;\n'+patched
        (output/original.name).write_text(patched,encoding='utf-8')
        licenses.append(original.name)
    notices=[]
    for title,relative in [('VPMPackageAutoInstaller','LICENSE'),('SimpleJson','Assets/InstallerSource/SimpleJson~/LICENSE'),('semver.net','Assets/InstallerSource/semver.net~/LICENSE')]:
        notices.append(title+'\n'+(upstream/relative).read_text(encoding='utf-8-sig').strip())
    # Keep existing upstream notices and the fixed-caption artwork font attribution.
    notices.append((HERE/'Bootstrap/BootstrapFontNotice.txt').read_text(encoding='utf-8-sig').strip())
    (output/'LICENSE-VPMPackageAutoInstaller.txt').write_text('\n\n'.join(notices)+'\n',encoding='utf-8')
    manifest={'upstream':str(upstream),'source':str(src),'licenses':licenses,'generated_sources':{p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in output.glob('*.cs')}}
    (output/'sources.json').write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf-8')
    return output

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--upstream',type=Path,required=True);p.add_argument('--inputs',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args();print(prepare(a.upstream.resolve(),a.inputs.resolve(),a.output.resolve()))
