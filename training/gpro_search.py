
import sys, pathlib, os
parent_dir = str(pathlib.Path(__file__).resolve().parents[1])
sys.path.append(parent_dir)
sys.path.append(parent_dir + "/training/HyperParameter-Optimizer")

import subprocess
import time
import io
import signal

from skopt.space import Real, Integer, Categorical
from gaussian_process import GaussianProcessSearch
from train_instance import TrainInstance

env_dir = os.path.join(parent_dir, "Builds")

def signal_handler(sig, frame):
    print('\nSIGINT signal received: killing instances...')
    for instance in instances:
        instance.kill()
    sys.exit(0)
signal.signal(signal.SIGINT, signal_handler)

search_space = [
    Integer(low=128, high=2048, name='batch_size'),
    Categorical(categories=[1e5, 5e5, 1e6], name='buffer_size'),
    Real(low=0.5, high=1., name='init_entcoef'),
    Integer(low=1, high=5, name='train_interval'),
    Real(low=0.005, high=0.01, name='tau'),
    Integer(low=1, high=3, name='num_layers'),
    Integer(low=128, high=512, name='hidden_units'),
    ]

if __name__ == "__main__":
    num_instances = 2
    gpro_input_file = None  # Use None to start from zero
    env_path = os.path.join(env_dir, "multiple_instances.x86_64")

    gp_search = GaussianProcessSearch(search_space=search_space,
                                    fixed_space={},
                                    evaluator=None,
                                    input_file=gpro_input_file,
                                    output_file='test.csv')
    
    # Instantiate training instances
    instances = []
    for i in range(num_instances):
        instances.append(TrainInstance(env_path=env_path, port=i))
    
    # Start training all instances
    candidates = []
    if gpro_input_file is None:  # If first points, sample random
        candidates = gp_search.get_random_candidate(num_instances)
    else:
        candidates = gp_search.get_next_candidate(num_instances)
    for i in range(num_instances):
        instances[i].train(candidates[i])

    while(True):
        time.sleep(60)  # refresh rate in seconds
        for i in range(num_instances):
            instance = instances[i]
            if instance.inactive:
                candidate = gp_search.get_next_candidate(1)[0]
                instance.train(candidate)
            elif instance.is_done():
                instance_params = instance.meta_params
                instance_result = instance.get_val()
                print("instance_params:", instance_params)
                print("instance_result:", instance_result)
                gp_search.add_point_value(instance_params, instance_result)
                gp_search.save_values()
                instance.kill()
            # print("Instance", i, "has value:", instance.get_val())



