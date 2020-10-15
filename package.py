# -*- coding: utf-8 -*-
import re
import subprocess
import zipfile
from itertools import chain
from pathlib import Path

import yaml

# load mod name and version from everest.yaml
with open('./everest.yaml', 'r', encoding='utf-8') as f:
    mod_metadata = yaml.safe_load(f.read())

mod_name = mod_metadata[0]['Name']
mod_version = mod_metadata[0]['Version']

print(mod_name, mod_version)

# update AssemblyVersion in AssemblyInfo.cs to match the mod version
with open('./Properties/AssemblyInfo.cs', 'r+', encoding='utf-8') as f:
    s = f.read()
    f.seek(0)
    f.truncate()

    s = re.sub(r'^(\[assembly: AssemblyVersion\(")(.*?)("\)\])$',
               f'\\g<1>{mod_version}.0\\g<3>',
               s, flags=re.MULTILINE)

    f.write(s)

# build the project
subprocess.run(['dotnet', 'build', '--configuration', 'Release'])

# package
file_list = ['./bin/**/*', './Dialog/**/*', './everest.yaml']

with zipfile.ZipFile(f'{mod_name}_v{mod_version}.zip', 'w', zipfile.ZIP_DEFLATED) as f:
    for file in chain(*[Path('.').glob(i) for i in file_list]):
        # correct the case in file name
        file = file.resolve().relative_to(Path.cwd())
        print(file)
        f.write(file)
