
import sys
import pathlib
import os
import subprocess
import time
import io
import yaml
import signal
import copy
import numpy as np

parent_dir = str(pathlib.Path(__file__).resolve().parents[1])
autogenerated_config_dir = os.path.join(
    parent_dir, "train_configs/autogenerated")
output_files_dir = os.path.join(parent_dir, "output_files")


class TrainInstance:
    def __init__(self, env_path, port):
        # Public
        self.__reset()
        self.env_path = env_path
        self.port = port

        # Private
        template_yaml_path = os.path.join(
            parent_dir, "train_configs/sac_template.yaml")
        with open(template_yaml_path, 'r') as f:
            self.template_params = yaml.load(f)

        # Ensure output_files_dir exisits 
        pathlib.Path(output_files_dir).mkdir(
            parents=True, exist_ok=True)

        # Ensure autogenerated_config_dir exists
        pathlib.Path(autogenerated_config_dir).mkdir(
            parents=True, exist_ok=True)

    def __reset(self):
        self.inactive = True
        self.process = None
        self.id = None

    def train(self, meta_params):
        """ Launch training instance
        """
        self.inactive = False
        self.meta_params = meta_params
        self.id = self.__dict_to_string(meta_params)
        self.__create_config_file()
        command = self.__get_command()
        self.output_file = os.path.join(output_files_dir, str(self.id) + ".out")
        log = open(self.output_file, 'a')
        self.process = subprocess.Popen(command, stdout=log, stderr=log, shell=True)
        print("Training started with PID: " + str(self.process.pid))

    def is_done(self):
        """ True if training has finished / converged / not promising
        """
        status = self.process.poll()
        if status is not None and status == 0:
            return True
        # TODO: early stopping heuristics
        return False

    def kill(self):
        """ Send SIGINT signal to process
        """
        print("Killing", self.process.pid)
        try:
            self.process.send_signal(signal.SIGINT)
        except Exception as e:
            print(e)
        self.__reset()

    def get_val(self):
        """ Return training result (for the optimizer)
        """
        last_values = self.__get_last_n_values(n=50)
        if len(last_values) > 0:
            return np.average(last_values)
        return 0

    def __get_last_n_values(self, n):
        last_values = []
        with open(self.output_file, 'r') as f:
            lines = f.read().splitlines()
            for i in range(n):  # Check the last n lines for a mean reward
                try:
                    last_line = lines[-i]
                    if "Mean Reward:" in last_line:
                        # TODO some fancy heuristic instead of last value
                        val = float(last_line.split("Mean Reward: ")[1].split(". Std of Reward")[0])
                        last_values.append(val)
                except Exception as e:
                    print(e)
                    pass
        return last_values
            

    def __create_config_file(self):
        self.config_dir = os.path.join(
            autogenerated_config_dir, 'config_' + str(self.id) + '.yaml')
        # Join dictionaries
        params = copy.deepcopy(self.template_params)
        for key in self.meta_params.keys():
            v = self.meta_params[key]
            if not isinstance(v, (int, float, str)):  # Hack to convert np vars to native python
                v = v.item()
            params["default"][key] = v
        with open(self.config_dir, 'w') as outfile:
            yaml.dump(params, outfile, default_flow_style=False)

    def __get_command(self):
        # "mlagents-learn train_configs/config.yaml --env=Builds/multiple_instances.x86_64 --run-id=test_mi --time-scale=100 --no-graphics --base-port=5100 --train >> output.nohup"
        # command = "cd " + parent_dir + "; workon py3;"
        command = "cd " + parent_dir + ";"
        command += " mlagents-learn "  # I hope this works if you dont have python environments
        command += str(self.config_dir)
        command += " --env=" + self.env_path
        command += " --run-id=" + self.id
        command += " --base-port=" + str(5000 + self.port)
        command += " --time-scale=100 --no-graphics --train"
        # command += str(self.id) + ".out"  # Where to save outputs
        return command

    def __dict_to_string(self, dictionary):
        s = str(dictionary)
        s = s.replace(" ", "")
        s = s.replace("{", "")
        s = s.replace("}", "")
        s = s.replace("'", "")
        s = s.replace(",", "-")
        return s
