'''
Freezing a stable-baselines to a frozen protocol buffer file to be served.
https://github.com/hill-a/stable-baselines
Some code taken from this lovely blog series
https://blog.metaflow.fr/tensorflow-how-to-freeze-a-model-and-serve-it-with-a-python-api-d4f3596b3adc
'''

import tensorflow as tf
import os
import shutil


def make_checkpoint(graph, folder):
    '''Creates a series of checkpoint files of all variables
    of the `graph` in the `folder`.'''
    checkpoint = os.path.join(folder, 'model.ckpt')
    
    with graph.as_default():

        saver = tf.train.Saver()

        with tf.Session(graph=graph) as sess:

            sess.run(tf.global_variables_initializer())

            saver.save(sess, checkpoint)

def freeze_graph(model_dir, output_graph, output_node_names):
    """Extract the sub graph defined by the output nodes and convert 
    all its variables into constant 
    Args:
        model_dir: the root folder containing the checkpoint state file
        output_node_names: a string, containing all the output node's names, 
                            comma separated
    """
    if not tf.gfile.Exists(model_dir):
        raise AssertionError(
            "Export directory doesn't exists. Please specify an export "
            "directory: %s" % model_dir)

    if not output_node_names:
        print("You need to supply the name of a node to --output_node_names.")
        return -1

    # We retrieve our checkpoint fullpath
    checkpoint = tf.train.get_checkpoint_state(model_dir)
    input_checkpoint = checkpoint.model_checkpoint_path
    
    # We precise the file fullname of our freezed graph
    absolute_model_dir = "/".join(input_checkpoint.split('/')[:-1])

    # We clear devices to allow TensorFlow to control on which device it will load operations
    clear_devices = True

    # We start a session using a temporary fresh Graph
    with tf.Session(graph=tf.Graph()) as sess:
        # We import the meta graph in the current default Graph
        saver = tf.train.import_meta_graph(input_checkpoint + '.meta', clear_devices=clear_devices)

        # We restore the weights
        saver.restore(sess, input_checkpoint)

        # We use a built-in TF helper to export variables to constants
        output_graph_def = tf.graph_util.convert_variables_to_constants(
            sess, # The session is used to retrieve the weights
            tf.get_default_graph().as_graph_def(), # The graph_def is used to retrieve the nodes 
            output_node_names.split(",") # The output node names are used to select the usefull nodes
        ) 

        # Finally we serialize and dump the output graph to the filesystem
        with tf.gfile.GFile(output_graph, "wb") as f:
            f.write(output_graph_def.SerializeToString())
        print("%d ops in the final graph." % len(output_graph_def.node))

def save_to_pb(model, filename):
    '''Saves a stable-baselines model to protocol buffer format
       ready to be served'''
    # get graph
    graph = model.graph

    # find output node name
    output_node = model.act_model.action.name[:-2]

    # Get parent folder name
    folder = os.path.dirname(filename)
    
    # Store files in temp directory
    temp_folder = os.path.join(folder, 'temp')
    if not os.path.exists(temp_folder):
        os.mkdir(temp_folder)
    
    # Make checkpoint
    make_checkpoint(graph, temp_folder)
    
    # Freeze graph
    freeze_graph(temp_folder, filename, output_node)
    
    # Delete checkpoint folder
    shutil.rmtree(temp_folder)

def load_graph(frozen_graph_filename):
    # We load the protobuf file from the disk and parse it to retrieve the 
    # unserialized graph_def
    with tf.gfile.GFile(frozen_graph_filename, "rb") as f:
        graph_def = tf.GraphDef()
        graph_def.ParseFromString(f.read())

    # Then, we import the graph_def into a new Graph and returns it 
    with tf.Graph().as_default() as graph:
        # The name var will prefix every op/nodes in your graph
        # Since we load everything in a new graph, this is not needed
        tf.import_graph_def(graph_def, name="")
    return graph