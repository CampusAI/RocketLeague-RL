import os
import subprocess
import time
import io

import sys, pathlib
path = str(pathlib.Path(__file__).resolve().parents[0])
path_1 = os.path.join(path, "one.py")
path_2 = os.path.join(path, "two.py")

out_1 = os.path.join(path, "one.out")
out_2 = os.path.join(path, "two.out")

p1 = subprocess.Popen('python "' + path_1 + '" >> "' + out_1 + '"', shell=True)
p2 = subprocess.Popen('python "' + path_2 + '" >> "' + out_2 + '"', shell=True)

while True:
    time.sleep(1)
    for proc in [p1, p2]:
        status = proc.poll()
        print("status:", status)
    print("it")