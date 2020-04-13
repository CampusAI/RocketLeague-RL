# RocketLeague-RL

Code for training RL agents in a simplified version of Rocket League in [Unity](https://unity.com/) game engine.
We use [ML agents](https://github.com/Unity-Technologies/ml-agents) to setup the environment and 
[stable-baselines](https://github.com/hill-a/stable-baselines) to train our models.


## Installation

1. Install virtual environment library:

`sudo -H pip3 install virtualenv virtualenvwrapper`

2. Export paths to bash file:

`sudo atom ~/.bashrc`

Add those three lines:

`export WORKON_HOME=$HOME/.virtualenvs`

`export PROJECT_HOME=$HOME/Devel`

`source /usr/local/bin/virtualenvwrapper.sh`

3. Rebash:

`source ~/.bashrc`

4. Create new virtual environment:

`mkvirtualenv --python=python3 rl`

5. Activate it:

`workon rl`

6. Clone this repo:

`https://github.com/CampusAI/RocketLeague-RL.git`

7. Change directory and branch:

`cd RocketLeague-RL; git checkout gcp`

8. Install dependencies

`pip install -r requirements.txt`

9. SSH Build and train.