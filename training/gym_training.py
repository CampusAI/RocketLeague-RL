import gym
import tensorflow as tf
# from gym_unity.envs import UnityEnv
from gym_unity.envs import UnityEnv
# from mlagents.envs import UnityEnvironment
# from stable_baselines.deepq.policies import MlpPolicy
from stable_baselines.common.policies import MlpPolicy
from stable_baselines.ppo1 import PPO1
from stable_baselines.ppo2 import PPO2
import mlagents.trainers.tensorflow_to_barracuda as tf2bc
import numpy as np
from model_saver import *
import os
import shutil

# behavior_name="CarBehavior"

def generate_checkpoint_from_model(model, checkpoint_name):
    with model.graph.as_default():
        if os.path.exists(checkpoint_name):
            shutil.rmtree(checkpoint_name)

        tf.saved_model.simple_save(model.sess, checkpoint_name,
                                    inputs={"obs": model.act_model.obs_ph},
                                    outputs={"action": model.action_ph})

if __name__ == "__main__":
    print("ok")
    env = UnityEnv(
        environment_filename="../Builds/Test.x86_64",
        worker_id=5026,
        no_graphics=False,
        multiagent=False)
    print("ok")

    policy_kwargs = dict(
        # n_env=1,
        act_fun=tf.nn.tanh,
        net_arch=[512, 512])  # All the layers are shared
    # https://stable-baselines.readthedocs.io/en/master/guide/custom_policy.html
    print("creating model....")

    # model = PPO1(
    #     # Setting environment and Policy
    #     env=env,
    #     policy=MlpPolicy,
    #     policy_kwargs=policy_kwargs,

    #     # Training params
    #     timesteps_per_actorbatch=200,  # Time horizon (TODO: TEST ON THIS)
    #     # Epsilon (how much different we allow the new policy to be)
    #     clip_param=0.1,
    #     # Beta (Weight of the entropy in the loss) (TODO: TEST ON THIS)
    #     entcoeff=1e-4,
    #     # Higher means bigger update steps (number of training epochs)
    #     optim_epochs=3,
    #     optim_stepsize=0.001,  # No idea (TODO: Test on this)
    #     optim_batchsize=50,  # Batch size (TODO: Test on this)
    #     gamma=0.99,  # Discount factor (TODO: THINK ABOUT THIS)
    #     lam=0.97,
    #     adam_epsilon=1E-5,
    #     schedule='linear',
    #     _init_setup_model=True,

    #     # Misc. Params
    #     tensorboard_log='./logs/',
    #     full_tensorboard_log=False,
    #     seed=0,
    #     n_cpu_tf_sess=None,
    #     verbose=1)

    model = PPO2(
        # Setting environment and Policy
        env=env,
        policy=MlpPolicy,
        policy_kwargs=policy_kwargs)
    print("training model ...")

    model.learn(total_timesteps=400,
                log_interval=210,
                tb_log_name="test_2",
                # callback=[],
                )
    print("saving")

    attrs = vars(model)
    # {'kids': 0, 'name': 'Dog', 'color': 'Spotted', 'age': 10, 'legs': 2, 'smell': 'Alot'}
    # now dump this in some way or another
    print(', '.join("%s: %s" % item for item in attrs.items()))

    # model.save("ppo_reacher")
    save_to_pb(model, 'temp.pb')
    # generate_checkpoint_from_model(model, "temp")
    tf2bc.convert("temp.pb", "learned_behavior.nn")
